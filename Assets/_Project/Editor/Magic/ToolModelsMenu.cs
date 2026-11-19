using System.IO;
using System.Linq;
using Project3DGP.Game.Golf;
using UnityEditor;
using UnityEngine;

namespace Project3DGP.EditorTools
{
    /// <summary>
    /// Magic > Prefabs > Use Tool and Ball Models: Sketchfab models (CREDITS.md), split, turned and resized in Blender
    /// (Art/Tools/BlenderScripts~: golf_club_and_ball.py, tool_models.py), replace the placeholder looks. Only what is missing:
    ///   1. FBX import settings (scale 1, axis conversion baked, no rig/animation, materials remapped to ours).
    ///   2. URP Lit materials next to each FBX, one per source material, with the JPG of the same name.
    ///   3. Prefabs/Tools/{StandardClub,Sledgehammer,Mysterious}.prefab: the placeholder "Visual" is replaced (grip at the root,
    ///      hanging down -Y; club toe toward +Z, hammer/wand head across X), so ClubProfile.ToolPrefab and the hamster's ToolSocket keep working.
    ///   4. Prefabs/Shared/GolfBall.prefab: the root's sphere mesh and white material become the model's
    ///      (a unit sphere, so the 0.2 scale, SphereCollider, Rigidbody and GolfBall stay exactly as they were).
    ///   5. UI/ClubIcons/<Tool>_Icon.png (rendered by tool_icons.py) imported as sprites and set as the club's
    ///      ClubProfile.Icon (HUD club card picture) if it has none.
    /// No scene is touched; every scene follows the prefabs.
    /// </summary>
    public static class ToolModelsMenu
    {
        const string ToolsArt = "Assets/_Project/Art/Tools";
        const string BallArt = "Assets/_Project/Art/Ball";

        // A Unity material: textured from <Name>.jpg (+ <Name>_Normal.jpg if NormalMap) or a plain colour.
        // Plain colours are given as in Blender (linear) and converted to sRGB.
        sealed class Mat
        {
            public string Name; public Color Color = Color.white; public float Metallic; public float Smoothness;
            public bool Textured = true; public bool NormalMap; public float Glow;
            public Mat(string name, float metallic, float smoothness) { Name = name; Metallic = metallic; Smoothness = smoothness; }
            public static Mat Plain(string name, float r, float g, float b, float metallic, float smoothness, float glow = 0f) =>
                new(name, metallic, smoothness) { Textured = false, Color = new Color(r, g, b).gamma, Glow = glow };
        }

        // Tool prefab name, its club profile, its model and materials (material names as exported by the Blender scripts).
        // Its card picture is UI/ClubIcons/<Prefab>_Icon.png (tool_icons.py).
        // CheckToe: the club's head points one way (+Z); the hammer and wand heads are symmetric.
        static readonly (string Prefab, string Club, string Fbx, bool CheckToe, Mat[] Materials)[] Tools =
        {
            ("StandardClub", ClubProfilesMenu.StandardPath, $"{ToolsArt}/StandardClub/StandardClub.fbx", true, new[]
            {
                new Mat("Club_Steel", 0.8f, 0.6f), new Mat("Club_Rubber", 0f, 0.2f), new Mat("Club_Gold", 1f, 0.6f),
                Mat.Plain("Club_Dark", 0.341f, 0.341f, 0.341f, 0.5f, 0.4f),
            }),
            ("Sledgehammer", ClubProfilesMenu.SledgehammerPath, $"{ToolsArt}/Sledgehammer/Sledgehammer.fbx", false, new[]
            {
                new Mat("Hammer", 0.3f, 0.35f) { NormalMap = true },
            }),
            ("Mysterious", ClubProfilesMenu.MysteriousPath, $"{ToolsArt}/Mysterious/Mysterious.fbx", false, new[]
            {
                Mat.Plain("Wand_Pink", 0.8f, 0.22f, 0.382f, 0f, 0.6f), Mat.Plain("Wand_Red", 0.8f, 0.035f, 0.138f, 0f, 0.7f),
                Mat.Plain("Wand_Gold", 0.8f, 0.441f, 0.15f, 1f, 0.7f), Mat.Plain("Wand_Cream", 0.8f, 0.741f, 0.636f, 0f, 0.5f),
                Mat.Plain("Wand_Gem", 1f, 0.237f, 0.691f, 0f, 0.9f, glow: 0.6f),
            }),
        };

        static readonly Mat BallMaterial = new("GolfBall", 0f, 0.5f);
        static string BallFbx => $"{BallArt}/GolfBall.fbx";

        [MenuItem(MagicMenuUtil.Root + "Prefabs/Use Tool and Ball Models", priority = MagicMenuUtil.PrefabsPriority + 6)]
        static void UseModels()
        {
            foreach (var tool in Tools)
            {
                if (!ConfigureImport(tool.Fbx, tool.Materials))
                    continue;
                SwapToolVisual(tool.Prefab, tool.Fbx, tool.CheckToe);
                UseIcon(tool.Prefab, tool.Club);
            }
            if (ConfigureImport(BallFbx, new[] { BallMaterial }))
                UseBallModel();
            AssetDatabase.SaveAssets();
        }

        // ---------- 1 + 2. import settings and materials ----------

        static bool ConfigureImport(string fbxPath, Mat[] materials)
        {
            if (AssetImporter.GetAtPath(fbxPath) is not ModelImporter importer)
            {
                MagicMenuUtil.Log($"Skipped: no {fbxPath}");
                return false;
            }
            var folder = Path.GetDirectoryName(fbxPath)!.Replace('\\', '/');
            var map = importer.GetExternalObjectMap();
            var changed = false;
            foreach (var m in materials)
            {
                var id = new AssetImporter.SourceAssetIdentifier(typeof(Material), m.Name);
                if (map.TryGetValue(id, out var existing) && existing != null)
                    continue;
                importer.AddRemap(id, EnsureMaterial(folder, m));
                changed = true;
            }
            if (!changed)
                return true;
            importer.globalScale = 1f;
            importer.bakeAxisConversion = true;
            importer.animationType = ModelImporterAnimationType.None;
            importer.importAnimation = false;
            importer.importCameras = false;
            importer.importLights = false;
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
            importer.SaveAndReimport();
            MagicMenuUtil.Log($"Import settings of {fbxPath}: no rig, axis conversion baked, materials {string.Join(", ", materials.Select(m => m.Name))}");
            return true;
        }

        static Material EnsureMaterial(string folder, Mat m)
        {
            var path = $"{folder}/{m.Name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
                return material;
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            material = new Material(shader) { name = m.Name };
            if (m.Textured)
            {
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>($"{folder}/{m.Name}.jpg");
                if (texture != null)
                {
                    material.SetTexture("_BaseMap", texture);
                    material.mainTexture = texture;
                }
                else
                    MagicMenuUtil.Log($"{m.Name}: no {folder}/{m.Name}.jpg, plain colour");
                if (m.NormalMap)
                    SetNormalMap(material, $"{folder}/{m.Name}_Normal.jpg");
            }
            if (m.Glow > 0f)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", m.Color * m.Glow);
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }
            material.SetColor("_BaseColor", m.Color);
            material.SetFloat("_Metallic", m.Metallic);
            material.SetFloat("_Smoothness", m.Smoothness);
            AssetDatabase.CreateAsset(material, path);
            MagicMenuUtil.Log("Created " + path);
            return material;
        }

        static void SetNormalMap(Material material, string path)
        {
            if (AssetImporter.GetAtPath(path) is not TextureImporter importer)
            {
                MagicMenuUtil.Log($"{material.name}: no {path}, no normal map");
                return;
            }
            if (importer.textureType != TextureImporterType.NormalMap)
            {
                importer.textureType = TextureImporterType.NormalMap;
                importer.SaveAndReimport();
            }
            material.SetTexture("_BumpMap", AssetDatabase.LoadAssetAtPath<Texture2D>(path));
            material.EnableKeyword("_NORMALMAP");
        }

        // ---------- 3. tool prefab ----------

        static void SwapToolVisual(string prefabName, string fbxPath, bool checkToe)
        {
            var prefabPath = $"{PlaceholderPrefabsMenu.Folder}/Tools/{prefabName}.prefab";
            if (!File.Exists(prefabPath))
            {
                MagicMenuUtil.Log($"Skipped: no {prefabPath} (run Magic > Prefabs > Create Placeholder Prefabs first)");
                return;
            }
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
            var root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                var old = root.transform.Find("Visual");
                if (old != null && PrefabUtility.IsPartOfPrefabInstance(old.gameObject))
                {
                    MagicMenuUtil.Log($"Skipped (uses a model) {prefabPath}");
                    return;
                }
                if (old != null)
                    Object.DestroyImmediate(old.gameObject);

                var visual = (GameObject)PrefabUtility.InstantiatePrefab(model);
                visual.name = "Visual";
                visual.transform.SetParent(root.transform, false);
                visual.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                foreach (var collider in visual.GetComponentsInChildren<Collider>(true))
                    Object.DestroyImmediate(collider);   // tools are looks only (placeholder had none either)
                CheckToolPose(root.transform, visual, prefabName, checkToe);

                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                MagicMenuUtil.Log($"{prefabPath}: placeholder Visual replaced by the {prefabName} model");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        // Expected (Blender script): grip at the root, head down at about -0.3, the toe toward +Z.
        static void CheckToolPose(Transform root, GameObject visual, string name, bool checkToe)
        {
            var renderers = visual.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
                return;
            var b = renderers[0].bounds;
            foreach (var r in renderers)
                b.Encapsulate(r.bounds);
            var min = root.InverseTransformPoint(b.min);
            var max = root.InverseTransformPoint(b.max);
            if (max.y > -min.y)
                MagicMenuUtil.Log($"{name}: the head is above the grip: check the FBX axis settings");
            if (checkToe && -min.z > max.z)
            {
                visual.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                MagicMenuUtil.Log($"{name}: turned the model 180° so the head points +Z");
            }
            MagicMenuUtil.Log($"{name}: model spans y {min.y:0.00}..{max.y:0.00} m, z {Mathf.Min(min.z, -max.z):0.00}..{Mathf.Max(max.z, -min.z):0.00} m");
        }

        // ---------- 5. club card icon ----------

        const string IconFolder = "Assets/_Project/UI/ClubIcons";

        static void UseIcon(string toolName, string clubPath)
        {
            var path = $"{IconFolder}/{toolName}_Icon.png";
            if (AssetImporter.GetAtPath(path) is not TextureImporter importer)
            {
                MagicMenuUtil.Log($"Skipped: no {path}");
                return;
            }
            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
                MagicMenuUtil.Log($"{path}: imported as a sprite");
            }
            var club = AssetDatabase.LoadAssetAtPath<ClubProfile>(clubPath);
            if (club == null)
            {
                MagicMenuUtil.Log($"Skipped: no {clubPath} (run Magic > Game > Create Club Profiles)");
                return;
            }
            if (club.Icon != null)
                return;
            club.Icon = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            EditorUtility.SetDirty(club);
            MagicMenuUtil.Log($"{clubPath}: Icon = {toolName}_Icon");
        }

        // ---------- 4. golf ball ----------

        static void UseBallModel()
        {
            var prefabPath = GolfBallPrefabMenu.PrefabPath;
            if (!File.Exists(prefabPath))
            {
                MagicMenuUtil.Log($"Skipped: no {prefabPath} (run Magic > Game > Create Golf Ball Prefab first)");
                return;
            }
            var mesh = AssetDatabase.LoadAllAssetsAtPath(BallFbx).OfType<Mesh>().FirstOrDefault();
            var material = AssetDatabase.LoadAssetAtPath<Material>($"{BallArt}/{BallMaterial.Name}.mat");
            if (mesh == null || material == null)
            {
                MagicMenuUtil.Log($"Skipped: {BallFbx} has no mesh or {BallMaterial.Name}.mat is missing");
                return;
            }
            var root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                if (!root.TryGetComponent(out MeshFilter filter) || !root.TryGetComponent(out MeshRenderer renderer))
                {
                    MagicMenuUtil.Log($"Skipped: {prefabPath} has no MeshFilter/MeshRenderer on its root");
                    return;
                }
                if (filter.sharedMesh == mesh && renderer.sharedMaterial == material)
                {
                    MagicMenuUtil.Log($"Skipped (uses the model) {prefabPath}");
                    return;
                }
                filter.sharedMesh = mesh;
                renderer.sharedMaterial = material;
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                MagicMenuUtil.Log($"{prefabPath}: sphere replaced by the golf ball model (size and physics unchanged)");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }
    }
}
