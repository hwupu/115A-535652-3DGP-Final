using System.IO;
using UnityEditor;
using UnityEngine;

namespace Project3DGP.EditorTools
{
    /// <summary>
    /// Magic > Prefabs > Create Kenney Prefabs (#74): our prefabs for the Kenney models in Assets/ThirdParty/Kenney
    /// (Platformer Kit, Mini Forest; CC0, only the files used are committed).
    ///   Materials/Kenney/KenneyPlatformer.mat, KenneyMiniForest.mat   URP Lit on each kit's colormap
    ///   Prefabs/Kenney/Platformer/*.prefab, Prefabs/Kenney/MiniForest/*.prefab
    ///   Prefabs/Nature/Tree, Bush, Rock and Course/Fence             placeholder Visual replaced by a Kenney model,
    ///                                                                 colliders refitted; every placed copy updates
    /// Each prefab: root + Visual (the model x Scale, centred on the root in X/Z), colliders by kind (trees: trunk +
    /// canopy; fences, hedges, barrels, crates, signs, tents, rocks: a box; blocks and small decor: none, the level
    /// builder adds the blocks' colliders). Only what is missing: an existing prefab, a placeholder that already uses
    /// a model, or a material is never touched.
    /// </summary>
    public static class KenneyPrefabsMenu
    {
        public const float Scale = 2f;   // Kenney: 1 unit = one block; x2 fits the 1.3 m hamster (a tree is ~4 m)
        public const string Folder = PlaceholderPrefabsMenu.Folder + "/Kenney";
        const string ModelRoot = "Assets/ThirdParty/Kenney";
        const string MaterialFolder = "Assets/_Project/Materials/Kenney";

        public enum Kind { Block, Tree, Solid, Decor }

        public static readonly (string Kit, string Model, Kind Kind)[] Catalog =
        {
            ("Platformer", "block-grass-large", Kind.Block), ("Platformer", "block-grass-large-tall", Kind.Block),
            ("Platformer", "block-grass-low-large", Kind.Block), ("Platformer", "block-grass", Kind.Block),
            ("Platformer", "tree", Kind.Tree), ("Platformer", "tree-pine", Kind.Tree), ("Platformer", "tree-pine-small", Kind.Tree),
            ("Platformer", "fence-straight", Kind.Solid), ("Platformer", "hedge-corner", Kind.Solid),
            ("Platformer", "barrel", Kind.Solid), ("Platformer", "crate", Kind.Solid), ("Platformer", "sign", Kind.Solid),
            ("Platformer", "flowers", Kind.Decor), ("Platformer", "flowers-tall", Kind.Decor), ("Platformer", "grass", Kind.Decor),
            ("Platformer", "plant", Kind.Decor),
            ("MiniForest", "tree", Kind.Tree), ("MiniForest", "tree-high", Kind.Tree), ("MiniForest", "tent", Kind.Solid),
            ("MiniForest", "rocks-low", Kind.Solid), ("MiniForest", "stones", Kind.Decor), ("MiniForest", "patch-dirt", Kind.Decor),
            ("MiniForest", "plant", Kind.Decor),
        };

        // The placeholders placed in L1 (Create Placeholder Prefabs) and the Kenney model that replaces each.
        static readonly (string Prefab, string Kit, string Model, Kind Kind, float Scale)[] Placeholders =
        {
            ("Nature/Tree", "Platformer", "tree", Kind.Tree, 1.2f),
            ("Nature/Bush", "Platformer", "hedge-corner", Kind.Solid, 0.8f),
            ("Nature/Rock", "MiniForest", "rocks-low", Kind.Solid, 0.8f),
            ("Course/Fence", "Platformer", "fence-straight", Kind.Solid, 1f),
        };

        static string KitFolder(string kit) => kit == "Platformer" ? "PlatformerKit" : "MiniForest";
        static string ModelPath(string kit, string model) => $"{ModelRoot}/{KitFolder(kit)}/Models/{model}.fbx";
        public static string PrefabPath(string kit, string model) => $"{Folder}/{kit}/{model}.prefab";
        public static GameObject Load(string kit, string model) => AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath(kit, model));

        [MenuItem(MagicMenuUtil.Root + "Prefabs/Create Kenney Prefabs", priority = MagicMenuUtil.PrefabsPriority + 6)]
        public static void Create()
        {
            int created = 0, missing = 0;
            foreach (var item in Catalog)
            {
                var path = PrefabPath(item.Kit, item.Model);
                if (File.Exists(path))
                    continue;
                var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath(item.Kit, item.Model));
                if (model == null)
                {
                    missing++;
                    continue;
                }
                MagicMenuUtil.EnsureFolder(Path.GetDirectoryName(path));
                var root = new GameObject(item.Model);
                try
                {
                    var bounds = AddVisual(root, model, item.Kit, item.Kind, 1f);
                    AddColliders(root, bounds, item.Kind);
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
                MagicMenuUtil.Log($"{missing} Kenney models missing under {ModelRoot} (pull the branch).");

            var swapped = 0;
            foreach (var p in Placeholders)
                if (SwapPlaceholder(p.Prefab, p.Kit, p.Model, p.Kind, p.Scale))
                    swapped++;

            AssetDatabase.SaveAssets();
            MagicMenuUtil.Log($"Kenney prefabs: {created} created in {Folder}, {swapped} placeholders replaced.");
        }

        static bool SwapPlaceholder(string prefab, string kit, string modelName, Kind kind, float scale)
        {
            var path = $"{PlaceholderPrefabsMenu.Folder}/{prefab}.prefab";
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath(kit, modelName));
            if (!File.Exists(path) || model == null)
                return false;
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var old = root.transform.Find("Visual");
                if (old != null && PrefabUtility.IsPartOfPrefabInstance(old.gameObject))
                    return false;   // already a model
                if (old != null)
                    Object.DestroyImmediate(old.gameObject);
                foreach (var collider in root.GetComponents<Collider>())
                    Object.DestroyImmediate(collider);   // placeholder sizes; refitted below
                var bounds = AddVisual(root, model, kit, kind, scale);
                AddColliders(root, bounds, kind);
                PrefabUtility.SaveAsPrefabAsset(root, path);
                MagicMenuUtil.Log($"{path}: placeholder Visual replaced by Kenney {modelName}");
                return true;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        // Visual = the model x Scale x scale, URP material, centred on the root in X/Z. Returns its bounds (root space).
        static Bounds AddVisual(GameObject root, GameObject model, string kit, Kind kind, float scale)
        {
            var visual = (GameObject)PrefabUtility.InstantiatePrefab(model);
            visual.name = "Visual";
            visual.transform.SetParent(root.transform, false);
            visual.transform.localScale = Vector3.one * (Scale * scale);
            var material = EnsureMaterial(kit);
            foreach (var renderer in visual.GetComponentsInChildren<MeshRenderer>(true))
            {
                var materials = renderer.sharedMaterials;
                for (var i = 0; i < materials.Length; i++)
                    materials[i] = material;
                renderer.sharedMaterials = materials;
                if (kind == Kind.Decor)
                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            var bounds = LocalBounds(root.transform, visual);
            var offset = new Vector3(bounds.center.x, 0f, bounds.center.z);   // Kenney fences sit on a tile edge
            visual.transform.localPosition -= offset;
            bounds.center -= offset;
            return bounds;
        }

        static Material EnsureMaterial(string kit)
        {
            var path = $"{MaterialFolder}/Kenney{kit}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
                return material;
            MagicMenuUtil.EnsureFolder(MaterialFolder);
            material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>($"{ModelRoot}/{KitFolder(kit)}/Models/Textures/colormap.png"));
            material.SetFloat("_Smoothness", 0.15f);
            material.SetFloat("_Metallic", 0f);
            material.enableInstancing = true;
            AssetDatabase.CreateAsset(material, path);
            MagicMenuUtil.Log("Created " + path);
            return material;
        }

        /// <summary>Bounds of the renderers under <paramref name="go"/>, in <paramref name="space"/>'s local space.</summary>
        public static Bounds LocalBounds(Transform space, GameObject go)
        {
            var renderers = go.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
                return new Bounds(Vector3.zero, Vector3.one);
            Bounds? result = null;
            foreach (var r in renderers)
            {
                var b = r.bounds;
                foreach (var corner in new[] { b.min, b.max, new Vector3(b.min.x, b.min.y, b.max.z), new Vector3(b.max.x, b.max.y, b.min.z) })
                {
                    var local = space.InverseTransformPoint(corner);
                    if (result == null)
                        result = new Bounds(local, Vector3.zero);
                    else
                    {
                        var copy = result.Value;
                        copy.Encapsulate(local);
                        result = copy;
                    }
                }
            }
            return result.Value;
        }

        static void AddColliders(GameObject root, Bounds bounds, Kind kind)
        {
            var size = bounds.size;
            switch (kind)
            {
                case Kind.Tree:
                    var trunk = root.AddComponent<CapsuleCollider>();
                    trunk.radius = 0.2f;
                    trunk.height = size.y * 0.4f;
                    trunk.center = new Vector3(0f, size.y * 0.2f, 0f);
                    var canopy = root.AddComponent<SphereCollider>();
                    canopy.radius = Mathf.Min(size.x, size.z) * 0.45f;
                    canopy.center = new Vector3(0f, size.y * 0.62f, 0f);
                    break;
                case Kind.Solid:
                    var box = root.AddComponent<BoxCollider>();
                    box.center = bounds.center;
                    box.size = size * 0.95f;
                    break;
            }
        }
    }
}
