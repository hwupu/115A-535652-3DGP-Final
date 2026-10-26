using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Project3DGP.EditorTools
{
    /// <summary>
    /// Magic > Level > Create Level Terrain (#9): adds a 50 x 50 m Terrain at Environment/Course/Terrain
    /// for sculpting hills and dips. The surface starts flat at y = 0 with 2 m of depth below it,
    /// so ponds can be dug with the Lower brush. Layers: Grass, Fairway, Sand, Snow.
    /// Creates only what is missing; the skeleton's Ground plane is disabled (not deleted). One Undo step.
    /// </summary>
    public static class LevelTerrainMenu
    {
        const string DataFolder = "Assets/_Project/Art/Terrain";
        const string LayerFolder = DataFolder + "/Layers";
        const float Size = 50f;        // matches the skeleton's Ground plane
        const float Height = 10f;      // highest possible hill
        const float BaseDepth = 2f;    // deepest possible pond
        const int HeightmapResolution = 129;
        const int AlphamapResolution = 128;

        static readonly (string Name, Color Color)[] Layers =
        {
            ("Grass", new Color(0.36f, 0.62f, 0.24f)),
            ("Fairway", new Color(0.52f, 0.8f, 0.36f)),
            ("Sand", new Color(0.86f, 0.76f, 0.5f)),
            ("Snow", new Color(0.92f, 0.95f, 1f)),
        };

        [MenuItem(MagicMenuUtil.Root + "Level/Create Level Terrain", priority = MagicMenuUtil.LevelPriority + 2)]
        static void CreateTerrain()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (string.IsNullOrEmpty(scene.path))
            {
                MagicMenuUtil.Log("Save the scene first, then run Create Level Terrain again.");
                return;
            }
            if (LevelSkeleton.Find(scene, LevelSkeleton.TerrainPath) != null)
            {
                MagicMenuUtil.Log($"Skipped (exists) {LevelSkeleton.TerrainPath} in {scene.name}.");
                return;
            }
            var course = LevelSkeleton.Find(scene, "Environment/Course");
            if (course == null)
            {
                MagicMenuUtil.Log("Run Magic > Level > Create Level Scene Skeleton first.");
                return;
            }

            Undo.SetCurrentGroupName("Create Level Terrain");
            var undoGroup = Undo.GetCurrentGroup();

            var go = Terrain.CreateTerrainGameObject(GetOrCreateData(scene.name));
            go.name = "Terrain";
            go.transform.SetParent(course.transform, false);
            go.transform.localPosition = new Vector3(-Size / 2f, -BaseDepth, -Size / 2f);
            var terrain = go.GetComponent<Terrain>();
            if (terrain.materialTemplate == null)
                terrain.materialTemplate = PlaceholderMaterials.Get("Terrain", Color.white, "Universal Render Pipeline/Terrain/Lit");
            Undo.RegisterCreatedObjectUndo(go, "Create Terrain");
            MagicMenuUtil.Log("Created " + LevelSkeleton.TerrainPath);

            var ground = LevelSkeleton.Find(scene, "Environment/Course/Ground");
            if (ground != null && ground.activeSelf)
            {
                Undo.RecordObject(ground, "Disable Ground");
                ground.SetActive(false);
                MagicMenuUtil.Log("Disabled the Ground plane (the terrain replaces it; delete it when you like).");
            }

            Undo.CollapseUndoOperations(undoGroup);
            EditorSceneManager.MarkSceneDirty(scene);
            MagicMenuUtil.Log($"Terrain in {scene.name} ready. Save the scene to keep it.");
        }

        static TerrainData GetOrCreateData(string sceneName)
        {
            var path = $"{DataFolder}/{sceneName}_TerrainData.asset";
            var data = AssetDatabase.LoadAssetAtPath<TerrainData>(path);
            if (data != null)
                return data;

            data = new TerrainData
            {
                heightmapResolution = HeightmapResolution, // set before size
                alphamapResolution = AlphamapResolution,
                baseMapResolution = AlphamapResolution,
            };
            data.size = new Vector3(Size, Height, Size);

            var heights = new float[HeightmapResolution, HeightmapResolution];
            for (var y = 0; y < HeightmapResolution; y++)
            for (var x = 0; x < HeightmapResolution; x++)
                heights[y, x] = BaseDepth / Height;
            data.SetHeights(0, 0, heights);

            var layers = new TerrainLayer[Layers.Length];
            for (var i = 0; i < Layers.Length; i++)
                layers[i] = GetOrCreateLayer(Layers[i].Name, Layers[i].Color);
            data.terrainLayers = layers;

            var alphas = new float[AlphamapResolution, AlphamapResolution, layers.Length];
            for (var y = 0; y < AlphamapResolution; y++)
            for (var x = 0; x < AlphamapResolution; x++)
                alphas[y, x, 0] = 1f; // all Grass
            data.SetAlphamaps(0, 0, alphas);

            MagicMenuUtil.EnsureFolder(DataFolder);
            AssetDatabase.CreateAsset(data, path);
            MagicMenuUtil.Log("Created " + path);
            return data;
        }

        static TerrainLayer GetOrCreateLayer(string name, Color color)
        {
            var path = $"{LayerFolder}/{name}.terrainlayer";
            var layer = AssetDatabase.LoadAssetAtPath<TerrainLayer>(path);
            if (layer != null)
                return layer;

            MagicMenuUtil.EnsureFolder(LayerFolder);
            var texture = NoiseTexture(color);
            AssetDatabase.CreateAsset(texture, $"{LayerFolder}/{name}_Albedo.asset");
            layer = new TerrainLayer { diffuseTexture = texture, tileSize = new Vector2(4f, 4f) };
            AssetDatabase.CreateAsset(layer, path);
            MagicMenuUtil.Log("Created " + path);
            return layer;
        }

        // A small colour texture with slight noise, so slopes and tiling read in the Scene view.
        static Texture2D NoiseTexture(Color color)
        {
            const int size = 64;
            var random = new System.Random(1);
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, true) { wrapMode = TextureWrapMode.Repeat };
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var shade = 1f + ((float)random.NextDouble() - 0.5f) * 0.12f;
                texture.SetPixel(x, y, new Color(color.r * shade, color.g * shade, color.b * shade, 1f));
            }
            texture.Apply();
            return texture;
        }
    }
}
