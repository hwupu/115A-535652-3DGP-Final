using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Project3DGP.EditorTools
{
    /// <summary>
    /// Magic > Level > Garden: Build Kenney Level (#74, PO 2026-11-03: Kenney look for L1): dresses L1_Garden with the
    /// Kenney Platformer Kit and Mini Forest (CC0, Create Kenney Prefabs) under Environment/Kenney.
    ///   Ground    17 x 17 grass blocks (4.16 m, top at y = 0) on a 3.125 m grid: they overlap ~1 m, so the rounded
    ///             corners leave no holes. Each block has a box collider. The old terrain is switched off (not deleted).
    ///   Border    a raised wall of tall blocks all around (2 to 4 m high), with colliders: the ball bounces back.
    ///   Plateaus  a few raised areas (1 or 2 m, one box collider each) away from the fairway, tee, cup and hazards.
    ///   Pond      the blocks under Gameplay/InvalidZones/Pond sit 1 m lower, and the pond moves down 1 m with them.
    ///   Decor     a dirt trail along Environment/Course/FairwayPath (tee > bends > cup; the bends are markers created
    ///             once: move them and rebuild), trees, hedges, rocks, barrels and crates along the edge, flowers,
    ///             grass and stones off the fairway (no colliders), a sign at the tee, floating islands around.
    ///   Look      Kenney day sky, light-blue fog, warm sun with soft shadows.
    /// L1's trees, bushes, rocks and fences are the Nature / Course prefabs (Kenney models after Create Kenney Prefabs).
    /// Fixed random seed. Only if Environment/Kenney is missing: delete it to build again (e.g. after moving the
    /// layout). One Undo step (lighting settings excepted); the scene is marked dirty, not saved.
    /// </summary>
    public static class GardenKenneyLevelMenu
    {
        const string SceneName = "L1_Garden";
        const string RootPath = "Environment/Kenney";
        public const string PathRoot = "Environment/Course/FairwayPath";

        const float CourseHalf = 25f;
        const int Cells = 17;
        const float Pitch = 2f * CourseHalf / (Cells - 1);   // 3.125 m: blocks overlap ~1 m
        const float Block = 4.16f;                           // block-grass-large x 2
        const float BlockHalf = Block / 2f;
        const float BlockHeight = 2f, LowHeight = 1f, TallHeight = 4f;
        const float PondDrop = 1f;
        const float FairwayHalfWidth = 3f;
        const int Seed = 2026;

        static readonly Vector3[] DefaultBends = { new(-12f, 0f, 2f), new(-5f, 0f, 12f), new(5f, 0f, 18f) };
        static readonly Color FogColor = new(0.68f, 0.74f, 0.96f);
        static readonly Color SunColor = new(1f, 0.94f, 0.8f);

        [MenuItem(MagicMenuUtil.Root + "Level/Garden: Build Kenney Level", priority = MagicMenuUtil.LevelPriority + 24)]
        static void Build()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.name != SceneName)
            {
                MagicMenuUtil.Log($"Open {SceneName} first (this level look is for the Garden only).");
                return;
            }
            if (LevelSkeleton.Find(scene, RootPath) != null)
            {
                MagicMenuUtil.Log($"Skipped (exists) {RootPath}. Delete it to build again.");
                return;
            }
            var environment = LevelSkeleton.Find(scene, "Environment");
            if (environment == null || LevelSkeleton.Find(scene, "Gameplay") == null)
            {
                MagicMenuUtil.Log("Run Magic > Level > Create Level Scene Skeleton first.");
                return;
            }
            KenneyPrefabsMenu.Create();
            if (KenneyPrefabsMenu.Load("Platformer", "block-grass-large") == null)
            {
                MagicMenuUtil.Log("The Kenney prefabs are missing: see the messages above.");
                return;
            }

            Undo.SetCurrentGroupName("Build Kenney Garden");
            var undoGroup = Undo.GetCurrentGroup();

            var builder = new Builder(scene, environment.transform);
            builder.Run();
            Look(scene);

            Undo.CollapseUndoOperations(undoGroup);
            EditorSceneManager.MarkSceneDirty(scene);
            MagicMenuUtil.Log($"Kenney Garden built: {builder.Total} objects ({builder.Summary}). Save the scene.");
        }

        static void Look(UnityEngine.SceneManagement.Scene scene)
        {
            var sky = KenneySky.Get("day");
            if (sky != null)
            {
                RenderSettings.skybox = sky;
                RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
            }
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 45f;
            RenderSettings.fogEndDistance = 160f;
            RenderSettings.fogColor = FogColor;

            var sunObject = LevelSkeleton.Find(scene, "Environment/Lighting/Directional Light");
            if (sunObject != null && sunObject.TryGetComponent(out Light sun) && sun.shadows != LightShadows.Soft)
            {
                Undo.RecordObject(sun, "Garden Sun");
                Undo.RecordObject(sun.transform, "Garden Sun");
                sun.shadows = LightShadows.Soft;
                sun.color = SunColor;
                sun.intensity = 1.25f;
                sun.transform.rotation = Quaternion.Euler(45f, -35f, 0f);
                RenderSettings.sun = sun;
            }
            DynamicGI.UpdateEnvironment();
            MagicMenuUtil.Log("Look: Kenney day sky, fog 45 to 160 m, warm sun with soft shadows");
        }

        // ---------- the fairway path ----------

        /// <summary>Tee (BallSpawn), the bend markers under FairwayPath (created once), cup. Ground positions (y = 0).</summary>
        static List<Vector3> FairwayPoints(UnityEngine.SceneManagement.Scene scene)
        {
            var points = new List<Vector3>();
            var spawn = LevelSkeleton.Find(scene, "Gameplay/BallSpawn");
            points.Add(Flat(spawn != null ? spawn.transform.position : new Vector3(-18f, 0f, -15f)));

            var root = LevelSkeleton.Find(scene, PathRoot);
            if (root == null)
            {
                var course = LevelSkeleton.Find(scene, "Environment/Course");
                root = new GameObject("FairwayPath");
                root.transform.SetParent(course != null ? course.transform : null, false);
                for (var i = 0; i < DefaultBends.Length; i++)
                {
                    var bend = new GameObject($"Bend_{i + 1}");
                    bend.transform.SetParent(root.transform, false);
                    bend.transform.localPosition = DefaultBends[i];
                }
                Undo.RegisterCreatedObjectUndo(root, "Add Fairway Path");
                MagicMenuUtil.Log($"Added {PathRoot} with 3 bend markers (move them to reshape the trail)");
            }
            foreach (Transform bend in root.transform)
                points.Add(Flat(bend.position));

            var cup = LevelSkeleton.Find(scene, "Gameplay/Goal/Cup");
            points.Add(Flat(cup != null ? cup.transform.position : new Vector3(18f, 0f, 18f)));
            return points;
        }

        static float DistanceToPath(Vector3 p, List<Vector3> path)
        {
            var best = float.MaxValue;
            for (var i = 0; i + 1 < path.Count; i++)
            {
                var a = path[i];
                var ab = path[i + 1] - a;
                var t = ab.sqrMagnitude > 0f ? Mathf.Clamp01(Vector3.Dot(p - a, ab) / ab.sqrMagnitude) : 0f;
                best = Mathf.Min(best, Flat(p - (a + ab * t)).magnitude);
            }
            return best;
        }

        static Vector3 Flat(Vector3 v) => new(v.x, 0f, v.z);
        static float Cell(int k) => -CourseHalf + k * Pitch;

        readonly struct Spot
        {
            public readonly Vector3 Position;
            public readonly float Radius;
            public Spot(Vector3 position, float radius) { Position = position; Radius = radius; }
        }

        sealed class Builder
        {
            readonly UnityEngine.SceneManagement.Scene _scene;
            readonly Transform _environment;
            readonly System.Random _random = new(Seed);
            readonly List<Spot> _blockers = new();
            readonly List<Spot> _solid = new();
            readonly List<Spot> _decor = new();
            readonly List<(Rect Area, float Top)> _plateaus = new();
            readonly Dictionary<string, Transform> _groups = new();
            readonly Dictionary<string, int> _counts = new();
            List<Vector3> _path;
            Rect _pond;   // empty if no pond
            Transform _root;

            public int Total { get; private set; }
            public string Summary => string.Join(", ", _counts.Select(c => $"{c.Value} {c.Key.ToLower()}"));

            public Builder(UnityEngine.SceneManagement.Scene scene, Transform environment)
            {
                _scene = scene;
                _environment = environment;
            }

            public void Run()
            {
                _path = FairwayPoints(_scene);
                FindBlockers();
                _root = new GameObject("Kenney").transform;
                _root.SetParent(_environment, false);
                Undo.RegisterCreatedObjectUndo(_root.gameObject, "Add Kenney Garden");

                HideTerrain();
                Pond();
                Ground();
                Border();
                Plateaus();
                Trail();
                Decor();
                FloatingIslands();
            }

            // ---------- what stays clear ----------

            void FindBlockers()
            {
                var gameplay = LevelSkeleton.Find(_scene, "Gameplay");
                foreach (Transform slot in gameplay.transform)
                {
                    if (slot.name is "Effects" or "Player")
                        continue;
                    if (slot.childCount == 0 || PrefabUtility.IsAnyPrefabInstanceRoot(slot.gameObject))
                        _blockers.Add(new Spot(slot.position, Footprint(slot) + 1.5f));
                    else
                        foreach (Transform item in slot)
                            _blockers.Add(new Spot(item.position, Footprint(item) + 1.5f));
                }
                foreach (var path in new[] { "Environment/Props", "Environment/Boundary" })
                {
                    var group = LevelSkeleton.Find(_scene, path);
                    if (group != null)
                        foreach (Transform item in group.transform)
                            _blockers.Add(new Spot(item.position, 1.5f));
                }
            }

            static float Footprint(Transform t)
            {
                var renderers = t.GetComponentsInChildren<Renderer>();
                if (renderers.Length == 0)
                    return 1f;
                var bounds = renderers[0].bounds;
                foreach (var r in renderers)
                    bounds.Encapsulate(r.bounds);
                return Mathf.Clamp(Mathf.Max(bounds.extents.x, bounds.extents.z), 0.5f, 6f);
            }

            void HideTerrain()
            {
                foreach (var path in new[] { LevelSkeleton.TerrainPath, "Environment/Course/Ground" })
                {
                    var go = LevelSkeleton.Find(_scene, path);
                    if (go != null && go.activeSelf)
                    {
                        Undo.RecordObject(go, "Hide " + go.name);
                        go.SetActive(false);
                        MagicMenuUtil.Log($"Switched off {path} (the Kenney blocks are the ground now)");
                    }
                }
            }

            // ---------- ground ----------

            void Pond()
            {
                var pond = LevelSkeleton.Find(_scene, "Gameplay/InvalidZones/Pond");
                if (pond == null)
                    return;
                var renderers = pond.GetComponentsInChildren<Renderer>();
                var bounds = renderers.Length > 0 ? renderers[0].bounds : new Bounds(pond.transform.position, Vector3.one * 4f);
                foreach (var r in renderers)
                    bounds.Encapsulate(r.bounds);
                _pond = Rect.MinMaxRect(bounds.min.x, bounds.min.z, bounds.max.x, bounds.max.z);
                if (pond.transform.position.y > -PondDrop / 2f)
                {
                    Undo.RecordObject(pond.transform, "Lower Pond");
                    pond.transform.position += Vector3.down * PondDrop;
                    MagicMenuUtil.Log($"Lowered the pond by {PondDrop} m into its basin");
                }
            }

            // A cell is in the basin if its block would overlap the pond.
            bool InBasin(int i, int j) => _pond.width > 0f
                && Cell(i) + BlockHalf > _pond.xMin && Cell(i) - BlockHalf < _pond.xMax
                && Cell(j) + BlockHalf > _pond.yMin && Cell(j) - BlockHalf < _pond.yMax;

            void Ground()
            {
                var block = KenneyPrefabsMenu.Load("Platformer", "block-grass-large");
                for (var i = 0; i < Cells; i++)
                for (var j = 0; j < Cells; j++)
                {
                    var top = InBasin(i, j) ? -PondDrop : 0f;
                    var go = Spawn(InBasin(i, j) ? "Basin" : "Ground", block, new Vector3(Cell(i), top - BlockHeight, Cell(j)), 90f * _random.Next(4), 1f);
                    AddBox(go, BlockHeight);
                }
                // Tapered underside: rings below the border, one cell smaller each step (no colliders).
                for (var layer = 1; layer <= 3; layer++)
                {
                    int lo = -1 + layer, hi = Cells - layer;
                    for (var i = lo; i <= hi; i++)
                    for (var j = lo; j <= hi; j++)
                        if (i == lo || i == hi || j == lo || j == hi)
                            Spawn("Underside", block, new Vector3(Cell(i), -BlockHeight * (layer + 1), Cell(j)), 90f * _random.Next(4), 1f);
                }
            }

            // A wall of tall blocks one cell outside the course; some columns one or two metres higher.
            void Border()
            {
                var tall = KenneyPrefabsMenu.Load("Platformer", "block-grass-large-tall");
                var low = KenneyPrefabsMenu.Load("Platformer", "block-grass-low-large");
                var block = KenneyPrefabsMenu.Load("Platformer", "block-grass-large");
                for (var i = -1; i <= Cells; i++)
                for (var j = -1; j <= Cells; j++)
                {
                    if (i != -1 && i != Cells && j != -1 && j != Cells)
                        continue;
                    var p = new Vector3(Cell(i), -BlockHeight, Cell(j));
                    var column = Spawn("Border", tall, p, 90f * _random.Next(4), 1f);
                    var height = TallHeight;   // from -2: top at +2
                    var roll = _random.NextDouble();
                    if (roll < 0.2)
                    {
                        Spawn("Border", block, p + Vector3.up * TallHeight, 90f * _random.Next(4), 1f);
                        height += BlockHeight;
                    }
                    else if (roll < 0.55)
                    {
                        Spawn("Border", low, p + Vector3.up * TallHeight, 90f * _random.Next(4), 1f);
                        height += LowHeight;
                    }
                    AddBox(column, height);
                }
            }

            // Raised areas off the fairway: 2-3 x 2-3 cells, 1 m (low blocks) or 2 m, one collider each.
            void Plateaus()
            {
                var low = KenneyPrefabsMenu.Load("Platformer", "block-grass-low-large");
                var block = KenneyPrefabsMenu.Load("Platformer", "block-grass-large");
                var taken = new List<RectInt>();
                for (var attempt = 0; attempt < 600 && _plateaus.Count < 5; attempt++)
                {
                    int w = _random.Next(2, 4), d = _random.Next(2, 4);
                    int i0 = _random.Next(1, Cells - 1 - w), j0 = _random.Next(1, Cells - 1 - d);
                    var cells = new RectInt(i0, j0, w, d);
                    if (taken.Any(t => new RectInt(t.x - 1, t.y - 1, t.width + 2, t.height + 2).Overlaps(cells)))
                        continue;
                    var ok = true;
                    for (var i = i0; i < i0 + w && ok; i++)
                    for (var j = j0; j < j0 + d && ok; j++)
                    {
                        var c = new Vector3(Cell(i), 0f, Cell(j));
                        ok = DistanceToPath(c, _path) > FairwayHalfWidth + 3.5f
                             && Flat(c - _path[0]).magnitude > 7f && Flat(c - _path[^1]).magnitude > 8f
                             && !InBasin(i, j) && Clear(c, BlockHalf, _blockers);
                    }
                    if (!ok)
                        continue;

                    var high = _random.NextDouble() < 0.35;
                    var top = high ? BlockHeight : LowHeight;
                    for (var i = i0; i < i0 + w; i++)
                    for (var j = j0; j < j0 + d; j++)
                        Spawn("Plateaus", high ? block : low, new Vector3(Cell(i), 0f, Cell(j)), 90f * _random.Next(4), 1f);

                    var area = Rect.MinMaxRect(Cell(i0) - BlockHalf, Cell(j0) - BlockHalf, Cell(i0 + w - 1) + BlockHalf, Cell(j0 + d - 1) + BlockHalf);
                    var collider = new GameObject($"PlateauCollider_{_plateaus.Count + 1}");
                    collider.transform.SetParent(Group("Plateaus"), false);
                    collider.transform.position = new Vector3(area.center.x, top / 2f, area.center.y);
                    collider.AddComponent<BoxCollider>().size = new Vector3(area.width, top, area.height);
                    _plateaus.Add((area, top));
                    taken.Add(cells);
                }
            }

            /// <summary>Ground height at p: a plateau's top, the basin floor, or 0.</summary>
            float Height(Vector3 p)
            {
                foreach (var (area, top) in _plateaus)
                    if (area.Contains(new Vector2(p.x, p.z)))
                        return top;
                return InPondArea(p) ? -PondDrop : 0f;
            }

            bool InPondArea(Vector3 p) => _pond.width > 0f && Rect.MinMaxRect(_pond.xMin - BlockHalf, _pond.yMin - BlockHalf,
                _pond.xMax + BlockHalf, _pond.yMax + BlockHalf).Contains(new Vector2(p.x, p.z));

            bool OnPlateauEdge(Vector3 p) => _plateaus.Any(pl =>
            {
                var outer = new Rect(pl.Area.x - 0.6f, pl.Area.y - 0.6f, pl.Area.width + 1.2f, pl.Area.height + 1.2f);
                var inner = new Rect(pl.Area.x + 0.6f, pl.Area.y + 0.6f, pl.Area.width - 1.2f, pl.Area.height - 1.2f);
                var q = new Vector2(p.x, p.z);
                return outer.Contains(q) && !inner.Contains(q);
            });

            // ---------- decor ----------

            void Trail()
            {
                var patch = KenneyPrefabsMenu.Load("MiniForest", "patch-dirt");
                var cup = _path[^1];
                for (var i = 0; i + 1 < _path.Count; i++)
                {
                    var a = _path[i];
                    var b = _path[i + 1];
                    var length = Flat(b - a).magnitude;
                    var yaw = Mathf.Atan2(b.x - a.x, b.z - a.z) * Mathf.Rad2Deg;
                    for (var d = 0f; d < length; d += 1.8f)
                    {
                        var p = Vector3.Lerp(a, b, d / length) + new Vector3(Range(-0.3f, 0.3f), 0f, Range(-0.3f, 0.3f));
                        if (Flat(p - cup).magnitude < 3.5f || InPondArea(p) || OnPlateauEdge(p))
                            continue;
                        p.y = Height(p) + 0.005f;
                        var go = Spawn("Trail", patch, p, yaw + Range(-12f, 12f), 1f);
                        go.transform.localScale = new Vector3(Range(0.8f, 1f), 0.12f, Range(0.8f, 1f));
                    }
                }
            }

            void Decor()
            {
                var tee = _path[0];
                var cup = _path[^1];
                var trees = new[] { ("Platformer", "tree"), ("Platformer", "tree-pine"), ("Platformer", "tree-pine-small"), ("MiniForest", "tree"), ("MiniForest", "tree-high") };

                bool Edge(Vector3 p) => EdgeBand(p, 0.8f, 3.5f) && OffFairway(p, 2.5f) && !InPondArea(p) && !OnPlateauEdge(p);
                Place("Trees", 16, 3.5f, true, Edge, () => Pick(trees), 1f, 1.3f);
                Place("Hedges", 6, 3f, true, Edge, () => KenneyPrefabsMenu.Load("Platformer", "hedge-corner"), 0.8f, 1f);
                Place("Rocks", 4, 3f, true, Edge, () => KenneyPrefabsMenu.Load("MiniForest", "rocks-low"), 0.7f, 1f);
                for (var c = 0; c < 6; c++)   // little barrel and crate stacks
                {
                    var center = RandomPoint(CourseHalf);
                    if (!Edge(center) || !Clear(center, 2f, _blockers) || !Clear(center, 3f, _solid))
                        continue;
                    for (var i = 0; i < 3; i++)
                    {
                        var p = center + new Vector3(Range(-0.9f, 0.9f), 0f, Range(-0.9f, 0.9f));
                        if (Clear(p, 0.9f, _solid))
                            Spawn("Props", KenneyPrefabsMenu.Load("Platformer", Chance(0.5f) ? "barrel" : "crate"), At(p), Range(0f, 360f), 1f, true);
                    }
                }

                // Open ground: never on the fairway, the green, the tee, the basin or a plateau's edge; no colliders.
                bool Open(Vector3 p) => Inside(p, 1f) && OffFairway(p, 1.5f) && Flat(p - cup).magnitude > 7f
                    && Flat(p - tee).magnitude > 4f && !InPondArea(p) && !OnPlateauEdge(p);
                var patches = new[] { ("Platformer", "flowers"), ("Platformer", "flowers-tall"), ("Platformer", "plant"), ("MiniForest", "plant") };
                for (var n = 0; n < 30; n++)
                {
                    var center = RandomPoint(CourseHalf);
                    if (!Open(center) || !Clear(center, 1.5f, _blockers))
                        continue;
                    var model = Pick(patches);
                    for (var i = _random.Next(3, 6); i > 0; i--)
                    {
                        var p = center + new Vector3(Range(-1.3f, 1.3f), 0f, Range(-1.3f, 1.3f));
                        if (Open(p) && Clear(p, 0.5f, _decor) && Clear(p, 0.6f, _solid))
                            Spawn("Flowers", model, At(p), Range(0f, 360f), Range(0.9f, 1.2f), false);
                    }
                }
                Place("Grass", 80, 1.2f, false, Open, () => KenneyPrefabsMenu.Load("Platformer", "grass"), 0.9f, 1.3f);
                Place("Stones", 10, 2f, false, Open, () => KenneyPrefabsMenu.Load("MiniForest", "stones"), 0.6f, 0.9f);

                // A sign at the tee, facing along the first leg.
                var first = _path[1] - tee;
                var side = Vector3.Cross(Vector3.up, first.normalized) * 2.2f;
                Spawn("Props", KenneyPrefabsMenu.Load("Platformer", "sign"), At(tee + side), Mathf.Atan2(first.x, first.z) * Mathf.Rad2Deg + 180f, 1f, true);
            }

            void FloatingIslands()
            {
                var block = KenneyPrefabsMenu.Load("Platformer", "block-grass-large");
                var small = KenneyPrefabsMenu.Load("Platformer", "block-grass");
                var trees = new[] { ("Platformer", "tree"), ("Platformer", "tree-pine"), ("MiniForest", "tree"), ("MiniForest", "tree-high") };
                for (var k = 0; k < 9; k++)
                {
                    var angle = (k * 40f + Range(-12f, 12f)) * Mathf.Deg2Rad;
                    var radius = Range(46f, 66f);
                    var top = Range(-5f, 7f);
                    var center = new Vector3(Mathf.Cos(angle) * radius, top, Mathf.Sin(angle) * radius);
                    int w = _random.Next(1, 4), d = _random.Next(1, 4);
                    var origin = center - new Vector3((w - 1) * Pitch / 2f, 0f, (d - 1) * Pitch / 2f);
                    for (var i = 0; i < w; i++)
                    for (var j = 0; j < d; j++)
                        Spawn("FloatingIslands", block, origin + new Vector3(i * Pitch, -BlockHeight, j * Pitch), 90f * _random.Next(4), 1f);
                    Spawn("FloatingIslands", small, center + Vector3.down * (BlockHeight * 2f), 0f, 1f);

                    for (var n = Mathf.Max(1, w * d / 2); n > 0; n--)
                    {
                        var p = origin + new Vector3(Range(0f, (w - 1) * Pitch), 0f, Range(0f, (d - 1) * Pitch));
                        var go = k % 4 == 1 && n == 1 ? KenneyPrefabsMenu.Load("MiniForest", "tent")
                            : k % 4 == 3 && n == 1 ? KenneyPrefabsMenu.Load("MiniForest", "rocks-low")
                            : Pick(trees);
                        Spawn("FloatingIslands", go, p, Range(0f, 360f), Range(0.9f, 1.3f));
                    }
                    Spawn("FloatingIslands", KenneyPrefabsMenu.Load("Platformer", "flowers"), center + new Vector3(Range(-1f, 1f), 0f, Range(-1f, 1f)), Range(0f, 360f), 1f);
                }
            }

            // ---------- helpers ----------

            void Place(string group, int count, float spacing, bool solid, System.Func<Vector3, bool> where,
                System.Func<GameObject> prefab, float minScale, float maxScale)
            {
                var placed = 0;
                for (var attempt = 0; attempt < 2000 && placed < count; attempt++)
                {
                    var p = RandomPoint(CourseHalf);
                    if (!where(p) || !Clear(p, spacing * 0.5f, _blockers) || !Clear(p, spacing, solid ? _solid : _decor)
                        || (!solid && !Clear(p, 0.4f, _solid)))
                        continue;
                    if (Spawn(group, prefab(), At(p), Range(0f, 360f), Range(minScale, maxScale), solid) != null)
                        placed++;
                }
            }

            Vector3 At(Vector3 p) => new(p.x, Height(p), p.z);

            GameObject Spawn(string group, GameObject prefab, Vector3 position, float yaw, float scale, bool? solid = null)
            {
                if (prefab == null)
                    return null;
                var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, _scene);
                go.transform.SetParent(Group(group), false);
                go.transform.SetPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));
                go.transform.localScale = Vector3.one * scale;
                if (solid == true) _solid.Add(new Spot(position, 0f));
                if (solid == false) _decor.Add(new Spot(position, 0f));
                _counts[group] = _counts.TryGetValue(group, out var n) ? n + 1 : 1;
                Total++;
                return go;
            }

            // A box collider over a block column of the given height (the block's pivot is at its bottom centre).
            static void AddBox(GameObject block, float height)
            {
                var box = block.AddComponent<BoxCollider>();
                box.center = new Vector3(0f, height / 2f, 0f);
                box.size = new Vector3(Block, height, Block);
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

            GameObject Pick((string Kit, string Model)[] options)
            {
                var (kit, model) = options[_random.Next(options.Length)];
                return KenneyPrefabsMenu.Load(kit, model);
            }

            static bool Inside(Vector3 p, float margin) => Mathf.Abs(p.x) < CourseHalf - margin && Mathf.Abs(p.z) < CourseHalf - margin;

            static bool EdgeBand(Vector3 p, float min, float max)
            {
                var toEdge = CourseHalf - Mathf.Max(Mathf.Abs(p.x), Mathf.Abs(p.z));
                return toEdge >= min && toEdge <= max;
            }

            bool OffFairway(Vector3 p, float margin) => DistanceToPath(p, _path) > FairwayHalfWidth + margin;

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
        }
    }
}
