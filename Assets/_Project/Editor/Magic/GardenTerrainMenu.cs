using System.Collections.Generic;
using Project3DGP.Game.Golf;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Project3DGP.EditorTools
{
    /// <summary>
    /// Magic > Level > Garden: Sculpt and Paint Terrain (#74): dresses the L1 terrain with the hand-painted Chromisu
    /// textures (Asset Store, not committed: docs/asset-store-packs.md).
    ///   Size      100 x 100 m (was 50): the 50 x 50 m course in the middle stays flat at y = 0, so nothing in the
    ///             layout moves; around it rolling hills (up to ~7 m) for the forest.
    ///   Paint     grass; a lighter fairway along Environment/Course/FairwayPath (tee > bends > cup) with a round green
    ///             and tee; darker rough outside the course; dirt on steep slopes and in patches in the rough.
    ///   Bounds    Gameplay/InvalidZones/OutOfBounds_*: four triggers over the hills, so a ball leaving the course is
    ///             out of bounds as before (it used to fall off the edge).
    /// The fairway bends are empty markers created once (Bend_1..3): move them and run this again to repaint.
    /// Overwrites the heights and the paint: it asks first unless the terrain is still flat. One Undo step.
    /// </summary>
    public static class GardenTerrainMenu
    {
        const string SceneName = "L1_Garden";
        const string LayerFolder = "Assets/_Project/Art/Terrain/Layers/Garden";
        public const string PathRoot = "Environment/Course/FairwayPath";

        public const float CourseHalf = 25f;   // the play area: flat, y = 0
        public const float Size = 100f;
        const float Height = 10f;              // highest possible point above the terrain's bottom
        const float BaseDepth = 2f;            // ground (y = 0) is 2 m above the bottom, as in LevelTerrainMenu
        const int HeightmapResolution = 257;
        const int AlphamapResolution = 256;
        public const float FairwayHalfWidth = 3f;
        const float GreenRadius = 5f;
        const float TeeRadius = 2.5f;

        static readonly Vector3[] DefaultBends = { new(-12f, 0f, 2f), new(-5f, 0f, 12f), new(5f, 0f, 18f) };

        // Layer order = alphamap channel.
        static readonly (string Name, string Guid)[] Layers =
        {
            ("Garden_Grass", AssetStorePacks.Ground.GrassNormal),
            ("Garden_Fairway", AssetStorePacks.Ground.GrassLighted),
            ("Garden_Rough", AssetStorePacks.Ground.GrassDarked),
            ("Garden_Dirt", AssetStorePacks.Ground.DirtNormal),
        };
        const int Grass = 0, Fairway = 1, Rough = 2, Dirt = 3;

        [MenuItem(MagicMenuUtil.Root + "Level/Garden: Sculpt and Paint Terrain", priority = MagicMenuUtil.LevelPriority + 24)]
        static void SculptAndPaint()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.name != SceneName)
            {
                MagicMenuUtil.Log($"Open {SceneName} first (this terrain is for the Garden only).");
                return;
            }
            var terrainObject = LevelSkeleton.Find(scene, LevelSkeleton.TerrainPath);
            if (terrainObject == null || !terrainObject.TryGetComponent(out Terrain terrain) || terrain.terrainData == null)
            {
                MagicMenuUtil.Log("Run Magic > Level > Create Level Terrain first.");
                return;
            }
            if (!AssetStorePacks.Require(AssetStorePacks.Ground.Name, AssetStorePacks.Ground.GrassNormal))
                return;
            var data = terrain.terrainData;

            if (!IsFlat(data))
            {
                var ours = Mathf.Approximately(data.size.x, Size);
                var message = ours
                    ? "Sculpt and paint the Garden terrain again? This replaces the heights and the paint (brush work done since the last run is lost). Undo works."
                    : "The L1 terrain has already been sculpted or painted by hand. Replace it with the generated Garden terrain? Undo works.";
                if (!EditorUtility.DisplayDialog("Garden terrain", message, "Replace", "Cancel"))
                {
                    MagicMenuUtil.Log("Cancelled: the terrain was not changed.");
                    return;
                }
            }

            Undo.SetCurrentGroupName("Sculpt and Paint Garden Terrain");
            var undoGroup = Undo.GetCurrentGroup();

            var path = FairwayPoints(scene);
            Undo.RegisterCompleteObjectUndo(data, "Sculpt Garden Terrain");
            Undo.RecordObject(terrainObject.transform, "Move Terrain");
            Undo.RecordObject(terrain, "Terrain Settings");

            data.heightmapResolution = HeightmapResolution;   // resolutions before size
            data.alphamapResolution = AlphamapResolution;
            data.baseMapResolution = 512;
            data.size = new Vector3(Size, Height, Size);
            terrainObject.transform.localPosition = new Vector3(-Size / 2f, -BaseDepth, -Size / 2f);
            terrain.basemapDistance = 250f;
            terrain.heightmapPixelError = 3f;

            data.SetHeights(0, 0, Heights());
            data.terrainLayers = TerrainLayers();
            data.SetAlphamaps(0, 0, Paint(data, path));
            EditorUtility.SetDirty(data);
            MagicMenuUtil.Log($"Sculpted {Size} x {Size} m (course flat at y = 0, hills around it) and painted grass, fairway, rough and dirt");

            var bounds = AddOutOfBounds(scene);

            Undo.CollapseUndoOperations(undoGroup);
            EditorSceneManager.MarkSceneDirty(scene);
            AssetDatabase.SaveAssets();
            MagicMenuUtil.Log($"Garden terrain done ({bounds} out-of-bounds triggers added). Move {PathRoot}/Bend_* and run again to repaint the fairway. Save the scene.");
        }

        // ---------- shape ----------

        /// <summary>Metres outside the course square (0 inside), rounded at the corners.</summary>
        public static float OutsideDistance(float x, float z)
        {
            var dx = Mathf.Max(Mathf.Abs(x) - CourseHalf, 0f);
            var dz = Mathf.Max(Mathf.Abs(z) - CourseHalf, 0f);
            return Mathf.Sqrt(dx * dx + dz * dz);
        }

        static float HillHeight(float x, float z)
        {
            var d = OutsideDistance(x, z);
            if (d <= 0f)
                return 0f;
            var big = Smooth((d - 1f) / 14f) * (2.5f + 4.5f * Fbm(x * 0.035f + 11f, z * 0.035f + 7f));
            var small = Smooth((d - 1f) / 6f) * 0.8f * (Fbm(x * 0.12f + 3f, z * 0.12f + 5f) - 0.5f);
            return Mathf.Max(0f, big + small);
        }

        static float[,] Heights()
        {
            var heights = new float[HeightmapResolution, HeightmapResolution];   // [z, x]
            for (var iz = 0; iz < HeightmapResolution; iz++)
            for (var ix = 0; ix < HeightmapResolution; ix++)
            {
                var x = -Size / 2f + ix * Size / (HeightmapResolution - 1);
                var z = -Size / 2f + iz * Size / (HeightmapResolution - 1);
                heights[iz, ix] = Mathf.Clamp01((BaseDepth + HillHeight(x, z)) / Height);
            }
            return heights;
        }

        // ---------- paint ----------

        static float[,,] Paint(TerrainData data, List<Vector3> path)
        {
            var alphas = new float[AlphamapResolution, AlphamapResolution, Layers.Length];
            var tee = path[0];
            var cup = path[path.Count - 1];
            var weights = new float[Layers.Length];
            for (var iz = 0; iz < AlphamapResolution; iz++)
            for (var ix = 0; ix < AlphamapResolution; ix++)
            {
                var u = (ix + 0.5f) / AlphamapResolution;
                var v = (iz + 0.5f) / AlphamapResolution;
                var x = -Size / 2f + u * Size;
                var z = -Size / 2f + v * Size;
                var p = new Vector3(x, 0f, z);

                var rough = Smooth((OutsideDistance(x, z) + 1.5f) / 4f);
                var fairway = 1f - Smooth((DistanceToPath(p, path) - FairwayHalfWidth) / 1.2f);
                fairway = Mathf.Max(fairway, 1f - Smooth((Flat(p - cup).magnitude - GreenRadius) / 1f));
                fairway = Mathf.Max(fairway, 1f - Smooth((Flat(p - tee).magnitude - TeeRadius) / 0.8f));
                var dirt = Smooth((data.GetSteepness(u, v) - 24f) / 10f);
                dirt = Mathf.Max(dirt, Smooth((Fbm(x * 0.18f + 3f, z * 0.18f + 9f) - 0.62f) / 0.08f) * rough * 0.9f);

                for (var k = 0; k < weights.Length; k++)
                    weights[k] = k == Grass ? 1f : 0f;
                Blend(weights, Rough, rough);
                Blend(weights, Fairway, fairway * (1f - rough));
                Blend(weights, Dirt, dirt);
                for (var k = 0; k < weights.Length; k++)
                    alphas[iz, ix, k] = weights[k];
            }
            return alphas;
        }

        // Moves the weights toward one layer by t (they keep summing to 1).
        static void Blend(float[] weights, int layer, float t)
        {
            for (var k = 0; k < weights.Length; k++)
                weights[k] = weights[k] * (1f - t) + (k == layer ? t : 0f);
        }

        static TerrainLayer[] TerrainLayers()
        {
            MagicMenuUtil.EnsureFolder(LayerFolder);
            var layers = new TerrainLayer[Layers.Length];
            for (var i = 0; i < Layers.Length; i++)
            {
                var path = $"{LayerFolder}/{Layers[i].Name}.terrainlayer";
                var layer = AssetDatabase.LoadAssetAtPath<TerrainLayer>(path);
                if (layer == null)
                {
                    layer = new TerrainLayer
                    {
                        diffuseTexture = AssetStorePacks.Load<Texture2D>(Layers[i].Guid),
                        tileSize = new Vector2(6f, 6f),
                        smoothness = 0f,
                        metallic = 0f,
                    };
                    AssetDatabase.CreateAsset(layer, path);
                    MagicMenuUtil.Log("Created " + path);
                }
                layers[i] = layer;
            }
            return layers;
        }

        // ---------- fairway path ----------

        /// <summary>Tee (BallSpawn), the bend markers under FairwayPath (created once), cup. Ground positions (y = 0).</summary>
        public static List<Vector3> FairwayPoints(UnityEngine.SceneManagement.Scene scene)
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
                MagicMenuUtil.Log($"Added {PathRoot} with 3 bend markers (move them to reshape the fairway)");
            }
            foreach (Transform bend in root.transform)
                points.Add(Flat(bend.position));

            var cup = LevelSkeleton.Find(scene, "Gameplay/Goal/Cup");
            points.Add(Flat(cup != null ? cup.transform.position : new Vector3(18f, 0f, 18f)));
            return points;
        }

        public static float DistanceToPath(Vector3 p, List<Vector3> path)
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

        // ---------- out of bounds ----------

        static int AddOutOfBounds(UnityEngine.SceneManagement.Scene scene)
        {
            var parent = LevelSkeleton.Find(scene, "Gameplay/InvalidZones");
            if (parent == null)
                return 0;
            const float thickness = Size / 2f - CourseHalf - 0.5f;   // from 0.5 m outside the course to the terrain edge
            const float offset = CourseHalf + 0.5f + thickness / 2f;
            var zones = new (string Name, Vector3 Center, Vector3 Size)[]
            {
                ("OutOfBounds_North", new Vector3(0f, 8f, offset), new Vector3(Size, 20f, thickness)),
                ("OutOfBounds_South", new Vector3(0f, 8f, -offset), new Vector3(Size, 20f, thickness)),
                ("OutOfBounds_East", new Vector3(offset, 8f, 0f), new Vector3(thickness, 20f, Size)),
                ("OutOfBounds_West", new Vector3(-offset, 8f, 0f), new Vector3(thickness, 20f, Size)),
            };
            var added = 0;
            foreach (var zone in zones)
            {
                if (parent.transform.Find(zone.Name) != null)
                    continue;
                var go = new GameObject(zone.Name);
                go.transform.SetParent(parent.transform, false);
                go.transform.localPosition = zone.Center;
                var box = go.AddComponent<BoxCollider>();
                box.size = zone.Size;
                box.isTrigger = true;
                go.AddComponent<InvalidZone>();
                Undo.RegisterCreatedObjectUndo(go, "Add " + zone.Name);
                added++;
            }
            return added;
        }

        // ---------- helpers ----------

        static bool IsFlat(TerrainData data)
        {
            var res = data.heightmapResolution;
            var heights = data.GetHeights(0, 0, res, res);
            var first = heights[0, 0];
            foreach (var h in heights)
                if (Mathf.Abs(h - first) > 0.0005f)
                    return false;
            // Painted by hand? Any texel not all the first layer.
            var alphas = data.GetAlphamaps(0, 0, data.alphamapWidth, data.alphamapHeight);
            for (var z = 0; z < data.alphamapHeight; z++)
            for (var x = 0; x < data.alphamapWidth; x++)
                if (alphas[z, x, 0] < 0.999f)
                    return false;
            return true;
        }

        static Vector3 Flat(Vector3 v) => new(v.x, 0f, v.z);

        static float Smooth(float t)
        {
            t = Mathf.Clamp01(t);
            return t * t * (3f - 2f * t);
        }

        /// <summary>Three octaves of Perlin noise, about 0..1.</summary>
        public static float Fbm(float x, float z)
        {
            var sum = 0f;
            var amplitude = 0.5f;
            var frequency = 1f;
            var total = 0f;
            for (var i = 0; i < 3; i++)
            {
                sum += amplitude * Mathf.PerlinNoise(x * frequency, z * frequency);
                total += amplitude;
                amplitude *= 0.5f;
                frequency *= 2f;
            }
            return sum / total;
        }
    }
}
