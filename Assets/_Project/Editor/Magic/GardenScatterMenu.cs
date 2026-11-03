using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Kind = Project3DGP.EditorTools.NatureModelsMenu.Kind;

namespace Project3DGP.EditorTools
{
    /// <summary>
    /// Magic > Level > Garden: Scatter Vegetation (#74): places the Garden nature prefabs (Use Pandazole Nature Models)
    /// in L1 under Environment/Props/Scatter, grouped by kind. Fixed random seed, so the result is the same every time.
    ///   Forest    trees (broadleaf near the course, conifers further out), bushes, rocks, logs, mushrooms, grass on
    ///             the hills around the course (Garden: Sculpt and Paint Terrain)
    ///   Course    bushes and rocks only along the edge; inside, flower patches, grass tufts and mushrooms (no
    ///             colliders: they never block a shot), never on the fairway, the green or the tee
    ///   Clear     a margin around everything under Gameplay (cup, gate, pond, cat, ball, avatar), the fences and the
    ///             objects already in Environment/Props
    /// Each object sits on the terrain surface. Only if Scatter is missing or empty: delete it to scatter again
    /// (e.g. after moving the layout). One Undo step; the scene is marked dirty, not saved.
    /// </summary>
    public static class GardenScatterMenu
    {
        const string SceneName = "L1_Garden";
        const string ParentPath = "Environment/Props/Scatter";
        const int Seed = 74;
        const float TerrainEdge = GardenTerrainMenu.Size / 2f - 1.5f;

        readonly struct Spot
        {
            public readonly Vector3 Position;
            public readonly float Radius;
            public Spot(Vector3 position, float radius) { Position = position; Radius = radius; }
        }

        [MenuItem(MagicMenuUtil.Root + "Level/Garden: Scatter Vegetation", priority = MagicMenuUtil.LevelPriority + 25)]
        static void Scatter()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.name != SceneName)
            {
                MagicMenuUtil.Log($"Open {SceneName} first (this scatter is for the Garden only).");
                return;
            }
            var terrainObject = LevelSkeleton.Find(scene, LevelSkeleton.TerrainPath);
            if (terrainObject == null || !terrainObject.TryGetComponent(out Terrain terrain)
                || !Mathf.Approximately(terrain.terrainData.size.x, GardenTerrainMenu.Size))
            {
                MagicMenuUtil.Log("Run Magic > Level > Garden: Sculpt and Paint Terrain first (the forest grows on its hills).");
                return;
            }
            var props = LevelSkeleton.Find(scene, "Environment/Props");
            if (props == null)
            {
                MagicMenuUtil.Log("Run Magic > Level > Create Level Scene Skeleton first.");
                return;
            }
            var existing = LevelSkeleton.Find(scene, ParentPath);
            if (existing != null && existing.transform.childCount > 0)
            {
                MagicMenuUtil.Log($"Skipped (exists) {ParentPath}. Delete it to scatter again.");
                return;
            }

            var prefabs = LoadPrefabs();
            if (prefabs.Count == 0)
            {
                MagicMenuUtil.Log("No Garden nature prefabs: run Magic > Prefabs > Use Pandazole Nature Models first.");
                return;
            }

            Undo.SetCurrentGroupName("Scatter Garden Vegetation");
            var undoGroup = Undo.GetCurrentGroup();

            var root = existing;
            if (root == null)
            {
                root = new GameObject("Scatter");
                root.transform.SetParent(props.transform, false);
                Undo.RegisterCreatedObjectUndo(root, "Add Scatter");
            }

            var scatter = new Scatterer(scene, terrain, root.transform, prefabs, GardenTerrainMenu.FairwayPoints(scene), Blockers(scene, props.transform));
            scatter.Run();

            Undo.CollapseUndoOperations(undoGroup);
            EditorSceneManager.MarkSceneDirty(scene);
            MagicMenuUtil.Log($"Scattered {scatter.Total} objects under {ParentPath}: {scatter.Summary}. Save the scene.");
        }

        static Dictionary<Kind, List<GameObject>> LoadPrefabs()
        {
            var result = new Dictionary<Kind, List<GameObject>>();
            foreach (var item in NatureModelsMenu.Catalog)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{NatureModelsMenu.GardenFolder}/{item.Name}.prefab");
                if (prefab == null)
                    continue;
                if (!result.TryGetValue(item.Kind, out var list))
                    result[item.Kind] = list = new List<GameObject>();
                list.Add(prefab);
            }
            return result;
        }

        // Keep-clear discs: gameplay objects (not the out-of-bounds triggers), fences, props placed before.
        static List<Spot> Blockers(UnityEngine.SceneManagement.Scene scene, Transform props)
        {
            var spots = new List<Spot>();
            var gameplay = LevelSkeleton.Find(scene, "Gameplay");
            if (gameplay != null)
            {
                foreach (Transform slot in gameplay.transform)
                {
                    if (slot.childCount == 0)
                        spots.Add(new Spot(slot.position, 2.5f));   // e.g. BallSpawn
                    foreach (Transform item in slot)
                        if (!item.name.StartsWith("OutOfBounds"))
                            spots.Add(new Spot(item.position, FootprintRadius(item) + 1.5f));
                    if (slot.childCount > 0 && slot.GetComponent<Renderer>() != null)
                        spots.Add(new Spot(slot.position, FootprintRadius(slot) + 1.5f));
                }
            }
            var boundary = LevelSkeleton.Find(scene, "Environment/Boundary");
            if (boundary != null)
                foreach (Transform fence in boundary.transform)
                    spots.Add(new Spot(fence.position, 1.5f));
            foreach (Transform prop in props)
                if (prop.name != "Scatter")
                    spots.Add(new Spot(prop.position, 2f));
            return spots;
        }

        static float FootprintRadius(Transform t)
        {
            var renderers = t.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
                return 1f;
            var bounds = renderers[0].bounds;
            foreach (var r in renderers)
                bounds.Encapsulate(r.bounds);
            return Mathf.Clamp(Mathf.Max(bounds.extents.x, bounds.extents.z), 0.5f, 6f);
        }

        sealed class Scatterer
        {
            readonly UnityEngine.SceneManagement.Scene _scene;
            readonly Terrain _terrain;
            readonly Transform _root;
            readonly Dictionary<Kind, List<GameObject>> _prefabs;
            readonly List<Vector3> _path;
            readonly List<Spot> _blockers;
            readonly List<Spot> _solid = new();   // placed objects with colliders (trees, bushes, rocks, logs)
            readonly List<Spot> _decor = new();   // placed flowers, grass, mushrooms
            readonly Dictionary<string, Transform> _groups = new();
            readonly Dictionary<string, int> _counts = new();
            readonly System.Random _random = new(Seed);

            public int Total { get; private set; }
            public string Summary => string.Join(", ", _counts.Select(c => $"{c.Value} {c.Key.ToLower()}"));

            public Scatterer(UnityEngine.SceneManagement.Scene scene, Terrain terrain, Transform root,
                Dictionary<Kind, List<GameObject>> prefabs, List<Vector3> path, List<Spot> blockers)
            {
                _scene = scene;
                _terrain = terrain;
                _root = root;
                _prefabs = prefabs;
                _path = path;
                _blockers = blockers;
            }

            public void Run()
            {
                var cup = _path[_path.Count - 1];
                var tee = _path[0];

                // Forest on the hills: broadleaf trees near the course, conifers further out.
                Place("Trees", 110, 4f, 3000, true, p => Ring(p, 2f, 24f),
                    p => GardenTerrainMenu.OutsideDistance(p.x, p.z) < 9f ? (Chance(0.75f) ? Kind.Tree : Kind.Conifer)
                        : (Chance(0.3f) ? Kind.Tree : Kind.Conifer), 0.9f, 1.3f);
                Place("Bushes", 45, 2f, 1500, true, p => Ring(p, 0.5f, 20f), _ => Kind.Bush, 0.8f, 1.3f);
                Place("Rocks", 14, 2.5f, 800, true, p => Ring(p, 1f, 22f), _ => Chance(0.7f) ? Kind.Rock : Kind.Log, 0.8f, 1.5f);
                Place("Mushrooms", 30, 1f, 800, false, p => Ring(p, 1f, 20f), _ => Kind.Mushroom, 0.8f, 1.3f);
                Place("Grass", 50, 1f, 800, false, p => Ring(p, 0f, 20f), _ => Kind.Grass, 0.8f, 1.3f);

                // Course edge: bushes and rocks only along the boundary, clear of the fairway.
                Place("Bushes", 18, 2.5f, 1500, true, p => EdgeBand(p, 0.8f, 4f) && OffFairway(p, 2.5f), _ => Kind.Bush, 0.8f, 1.2f);
                Place("Rocks", 6, 3f, 800, true, p => EdgeBand(p, 0.8f, 4f) && OffFairway(p, 2.5f), _ => Kind.Rock, 0.7f, 1.1f);

                // Inside the course: flower patches and grass tufts (no colliders), never on fairway, green or tee.
                bool OpenGround(Vector3 p) => Inside(p, 1f) && OffFairway(p, 1.5f)
                    && Flat(p - cup).magnitude > 7f && Flat(p - tee).magnitude > 4f;
                var centers = new List<Vector3>();
                for (var attempt = 0; attempt < 1500 && centers.Count < 26; attempt++)
                {
                    var c = RandomPoint(GardenTerrainMenu.CourseHalf);
                    if (OpenGround(c) && Clear(c, 1.5f, _blockers) && centers.All(o => Flat(o - c).magnitude > 4f))
                        centers.Add(c);
                }
                foreach (var c in centers)
                {
                    var n = _random.Next(3, 7);
                    var kind = Chance(0.7f) ? Kind.Flower : Kind.GroundFlower;
                    for (var i = 0; i < n; i++)
                    {
                        var p = c + new Vector3(Range(-1.5f, 1.5f), 0f, Range(-1.5f, 1.5f));
                        if (OpenGround(p) && Clear(p, 0.4f, _decor))
                            Spawn("Flowers", kind, p, 0.8f, 1.2f, false);
                    }
                }
                Place("Grass", 70, 1.2f, 1500, false, OpenGround, _ => Kind.Grass, 0.8f, 1.3f);

                // Mushrooms at the foot of the props trees already in the level (Tree_01..).
                foreach (var tree in _blockers.Where(b => b.Radius == 2f).ToList())
                    for (var i = 0; i < 2; i++)
                    {
                        var angle = Range(0f, Mathf.PI * 2f);
                        var p = tree.Position + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * Range(1.2f, 2f);
                        if (Inside(p, 0.5f) && OffFairway(p, 1f) && Clear(p, 0.5f, _decor))
                            Spawn("Mushrooms", Kind.Mushroom, p, 0.8f, 1.2f, false);
                    }
            }

            void Place(string group, int count, float spacing, int attempts, bool solid, System.Func<Vector3, bool> where,
                System.Func<Vector3, Kind> kind, float minScale, float maxScale)
            {
                var placed = 0;
                for (var attempt = 0; attempt < attempts && placed < count; attempt++)
                {
                    var p = RandomPoint(TerrainEdge);
                    if (!where(p) || !Clear(p, spacing * 0.5f, _blockers))
                        continue;
                    if (!Clear(p, spacing, solid ? _solid : _decor) || (!solid && !Clear(p, 0.3f, _solid)))
                        continue;
                    if (Spawn(group, kind(p), p, minScale, maxScale, solid))
                        placed++;
                }
            }

            bool Spawn(string group, Kind kind, Vector3 p, float minScale, float maxScale, bool solid)
            {
                if (!_prefabs.TryGetValue(kind, out var list) || list.Count == 0)
                    return false;
                var prefab = list[_random.Next(list.Count)];
                var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, _scene);
                go.transform.SetParent(Group(group), false);
                p.y = _terrain.SampleHeight(p) + _terrain.transform.position.y;
                go.transform.position = p;
                go.transform.rotation = Quaternion.Euler(0f, Range(0f, 360f), 0f);
                go.transform.localScale = Vector3.one * Range(minScale, maxScale);
                (solid ? _solid : _decor).Add(new Spot(p, 0f));
                _counts[group] = _counts.TryGetValue(group, out var n) ? n + 1 : 1;
                Total++;
                return true;
            }

            Transform Group(string name)
            {
                if (_groups.TryGetValue(name, out var group))
                    return group;
                group = new GameObject(name).transform;
                group.SetParent(_root, false);
                _groups[name] = group;
                return group;
            }

            static bool Ring(Vector3 p, float min, float max)
            {
                var d = GardenTerrainMenu.OutsideDistance(p.x, p.z);
                return d >= min && d <= max;
            }

            static bool Inside(Vector3 p, float margin) =>
                Mathf.Abs(p.x) < GardenTerrainMenu.CourseHalf - margin && Mathf.Abs(p.z) < GardenTerrainMenu.CourseHalf - margin;

            // Inside the course, between min and max metres from its edge.
            static bool EdgeBand(Vector3 p, float min, float max)
            {
                var toEdge = GardenTerrainMenu.CourseHalf - Mathf.Max(Mathf.Abs(p.x), Mathf.Abs(p.z));
                return toEdge >= min && toEdge <= max;
            }

            bool OffFairway(Vector3 p, float margin) =>
                GardenTerrainMenu.DistanceToPath(p, _path) > GardenTerrainMenu.FairwayHalfWidth + margin;

            static bool Clear(Vector3 p, float distance, List<Spot> spots)
            {
                foreach (var s in spots)
                    if (Flat(p - s.Position).magnitude < distance + s.Radius)
                        return false;
                return true;
            }

            Vector3 RandomPoint(float half) => new(Range(-half, half), 0f, Range(-half, half));
            float Range(float min, float max) => min + (float)_random.NextDouble() * (max - min);
            bool Chance(float p) => _random.NextDouble() < p;
            static Vector3 Flat(Vector3 v) => new(v.x, 0f, v.z);
        }
    }
}
