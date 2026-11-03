using System.IO;
using UnityEditor;
using UnityEngine;

namespace Project3DGP.EditorTools
{
    /// <summary>
    /// Magic > Prefabs > Use Pandazole Nature Models (#74): the low-poly nature models of the Pandazole pack (Asset Store,
    /// not committed: docs/asset-store-packs.md) as our own prefabs.
    ///   Materials/Nature/PandaNature.mat     URP Lit on the pack's atlas (the pack's own material is Built-in: pink in URP)
    ///   Prefabs/Nature/Tree, Bush, Rock      the placeholder Visual replaced by a model, colliders fitted to it;
    ///                                        every placed copy (L1 Environment/Props) updates
    ///   Prefabs/Nature/Garden/*.prefab       the Sunny Garden set used by Garden: Scatter Vegetation (Catalog)
    /// Our prefabs reference the pack's meshes by GUID; a member without the pack sees empty objects, nothing breaks.
    /// Only what is missing: an existing prefab or a Visual that already is a model is never touched.
    /// </summary>
    public static class NatureModelsMenu
    {
        public const string GardenFolder = PlaceholderPrefabsMenu.Folder + "/Nature/Garden";
        const string MaterialPath = "Assets/_Project/Materials/Nature/PandaNature.mat";

        public enum Kind { Tree, Conifer, Bush, Rock, Log, Flower, GroundFlower, Mushroom, Grass }

        /// <summary>The Garden set: prefab name (= pack model without "_Spring"), kind, scale.</summary>
        public static readonly (string Name, Kind Kind, float Scale)[] Catalog =
        {
            ("Tree_05", Kind.Tree, 1f), ("Tree_06", Kind.Tree, 1f), ("Tree_09", Kind.Tree, 1f), ("Tree_10", Kind.Tree, 1f),
            ("Tree_17", Kind.Tree, 1f), ("Tree_19", Kind.Tree, 1f), ("Tree_20", Kind.Tree, 1f),
            ("Tree_01", Kind.Conifer, 1.2f), ("Tree_03", Kind.Conifer, 1.1f), ("Tree_04", Kind.Conifer, 1f),
            ("Tree_21", Kind.Conifer, 1f), ("Tree_23", Kind.Conifer, 1.3f),
            ("Bush_04", Kind.Bush, 1f), ("Bush_05", Kind.Bush, 1f), ("Bush_09", Kind.Bush, 1f), ("Bush_10", Kind.Bush, 1f),
            ("Bush_12", Kind.Bush, 1f), ("Bush_13", Kind.Bush, 1f), ("Bush_14", Kind.Bush, 1f), ("Bush_15", Kind.Bush, 1f),
            ("SoftRock_03", Kind.Rock, 1f), ("SoftRock_06", Kind.Rock, 1f), ("SoftRock_08", Kind.Rock, 1f),
            ("SoftRock_12", Kind.Rock, 1f), ("SoftRock_14", Kind.Rock, 1f),
            ("Log_01", Kind.Log, 1f), ("Log_03", Kind.Log, 1f),
            ("Flower_10", Kind.Flower, 1.6f), ("Flower_11", Kind.Flower, 1.6f), ("Flower_14", Kind.Flower, 1.6f),
            ("Flower_15", Kind.Flower, 1.6f), ("Flower_18", Kind.Flower, 1.6f), ("Flower_19", Kind.Flower, 1.6f),
            ("Flower_23", Kind.Flower, 1.6f), ("Flower_26", Kind.Flower, 1.6f),
            ("Flower_01", Kind.GroundFlower, 1.6f), ("Flower_02", Kind.GroundFlower, 1.6f),
            ("Flower_03", Kind.GroundFlower, 1.6f), ("Flower_04", Kind.GroundFlower, 1.6f),
            ("Mashroom_01", Kind.Mushroom, 1.5f), ("Mashroom_06", Kind.Mushroom, 1.5f), ("Mashroom_08", Kind.Mushroom, 1.5f),
            ("Mashroom_09", Kind.Mushroom, 1.5f), ("Mashroom_15", Kind.Mushroom, 1.5f), ("Mashroom_22", Kind.Mushroom, 1.5f),
            ("Grass_19", Kind.Grass, 1.2f), ("Grass_20", Kind.Grass, 1.2f), ("Grass_21", Kind.Grass, 1.2f),
            ("Grass_28", Kind.Grass, 1.2f), ("Grass_29", Kind.Grass, 1.2f), ("Grass_31", Kind.Grass, 1.2f),
            ("Grass_37", Kind.Grass, 1.2f), ("Grass_40", Kind.Grass, 1.2f),
        };

        // The L1 placeholders and the model that replaces each.
        static readonly (string Prefab, string Model, Kind Kind)[] Placeholders =
        {
            ("Nature/Tree", "Tree_10_Spring", Kind.Tree),
            ("Nature/Bush", "Bush_10", Kind.Bush),
            ("Nature/Rock", "SoftRock_08", Kind.Rock),
        };

        public static string ModelName(string prefabName) => prefabName.StartsWith("Tree_") ? prefabName + "_Spring" : prefabName;

        [MenuItem(MagicMenuUtil.Root + "Prefabs/Use Pandazole Nature Models", priority = MagicMenuUtil.PrefabsPriority + 6)]
        static void Use()
        {
            if (!AssetStorePacks.Require(AssetStorePacks.Pandazole.Name, AssetStorePacks.Pandazole.AtlasTexture))
                return;
            var material = EnsureMaterial();

            var swapped = 0;
            foreach (var p in Placeholders)
                if (SwapPlaceholder($"{PlaceholderPrefabsMenu.Folder}/{p.Prefab}.prefab", p.Model, p.Kind, material))
                    swapped++;

            MagicMenuUtil.EnsureFolder(GardenFolder);
            int created = 0, missing = 0;
            foreach (var item in Catalog)
            {
                var path = $"{GardenFolder}/{item.Name}.prefab";
                if (File.Exists(path))
                    continue;
                var model = AssetStorePacks.Pandazole.Model(ModelName(item.Name));
                if (model == null)
                {
                    missing++;
                    continue;
                }
                var root = new GameObject(item.Name);
                try
                {
                    var visual = AddVisual(root.transform, model, material, item.Scale, CastsShadow(item.Kind));
                    AddColliders(root, visual, item.Kind);
                    GameObjectUtility.SetStaticEditorFlags(root, StaticEditorFlags.BatchingStatic);
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                    created++;
                }
                finally
                {
                    Object.DestroyImmediate(root);
                }
            }
            if (missing > 0)
                MagicMenuUtil.Log($"{missing} Pandazole models not found: import the whole pack (see {AssetStorePacks.Guide}).");
            AssetDatabase.SaveAssets();
            MagicMenuUtil.Log($"Nature models: {swapped} placeholders replaced, {created} Garden prefabs created in {GardenFolder}.");
        }

        static Material EnsureMaterial()
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (material != null)
                return material;
            MagicMenuUtil.EnsureFolder(Path.GetDirectoryName(MaterialPath));
            material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = "PandaNature" };
            material.SetTexture("_BaseMap", AssetStorePacks.Load<Texture2D>(AssetStorePacks.Pandazole.AtlasTexture));
            material.SetFloat("_Smoothness", 0.1f);
            material.SetFloat("_Metallic", 0f);
            material.enableInstancing = true;
            AssetDatabase.CreateAsset(material, MaterialPath);
            MagicMenuUtil.Log("Created " + MaterialPath);
            return material;
        }

        static bool SwapPlaceholder(string prefabPath, string modelName, Kind kind, Material material)
        {
            if (!File.Exists(prefabPath))
            {
                MagicMenuUtil.Log($"Skipped: no {prefabPath} (run Magic > Prefabs > Create Placeholder Prefabs first)");
                return false;
            }
            var model = AssetStorePacks.Pandazole.Model(modelName);
            if (model == null)
                return false;
            var root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                var old = root.transform.Find("Visual");
                if (old != null && PrefabUtility.IsPartOfPrefabInstance(old.gameObject))
                {
                    MagicMenuUtil.Log($"Skipped (uses a model) {prefabPath}");
                    return false;
                }
                if (old != null)
                    Object.DestroyImmediate(old.gameObject);
                foreach (var collider in root.GetComponents<Collider>())
                    Object.DestroyImmediate(collider);   // placeholder sizes; refitted to the model below

                var visual = AddVisual(root.transform, model, material, 1f, true);
                AddColliders(root, visual, kind);
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                MagicMenuUtil.Log($"{prefabPath}: placeholder Visual replaced by {modelName}");
                return true;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        // Small ground decor casts no shadow (hundreds of them; the shadow would hardly show).
        static bool CastsShadow(Kind kind) => kind is Kind.Tree or Kind.Conifer or Kind.Bush or Kind.Rock or Kind.Log;

        static GameObject AddVisual(Transform root, GameObject model, Material material, float scale, bool castShadows)
        {
            var visual = (GameObject)PrefabUtility.InstantiatePrefab(model);
            visual.name = "Visual";
            visual.transform.SetParent(root, false);
            visual.transform.localScale = Vector3.one * scale;
            foreach (var renderer in visual.GetComponentsInChildren<MeshRenderer>(true))
            {
                var materials = renderer.sharedMaterials;
                for (var i = 0; i < materials.Length; i++)
                    materials[i] = material;
                renderer.sharedMaterials = materials;
                if (!castShadows)
                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            return visual;
        }

        // Trees: trunk capsule + canopy sphere. Bushes, rocks, logs: a box. Flowers, grass, mushrooms: none (the ball
        // rolls through them).
        static void AddColliders(GameObject root, GameObject visual, Kind kind)
        {
            var renderers = visual.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
                return;
            var bounds = renderers[0].bounds;
            foreach (var r in renderers)
                bounds.Encapsulate(r.bounds);
            var center = root.transform.InverseTransformPoint(bounds.center);
            var size = bounds.size;

            switch (kind)
            {
                case Kind.Tree:
                case Kind.Conifer:
                {
                    var trunk = root.AddComponent<CapsuleCollider>();
                    trunk.radius = 0.25f;
                    trunk.height = size.y * 0.5f;
                    trunk.center = new Vector3(center.x, size.y * 0.25f, center.z);
                    var canopy = root.AddComponent<SphereCollider>();
                    canopy.radius = Mathf.Min(size.x, size.z) * (kind == Kind.Tree ? 0.4f : 0.3f);
                    canopy.center = new Vector3(center.x, size.y * (kind == Kind.Tree ? 0.68f : 0.5f), center.z);
                    break;
                }
                case Kind.Bush:
                case Kind.Rock:
                case Kind.Log:
                {
                    var box = root.AddComponent<BoxCollider>();
                    box.center = center;
                    box.size = size * 0.9f;
                    break;
                }
            }
        }
    }
}
