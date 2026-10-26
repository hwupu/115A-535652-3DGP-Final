using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Project3DGP.EditorTools
{
    /// <summary>
    /// Magic > Prefabs > Create Placeholder Prefabs (#9): primitive stand-ins for models, made only if missing.
    /// Every prefab is a root with the colliders plus a "Visual" child with the meshes (no colliders).
    /// To use a real model later, open the prefab and replace "Visual"; scene instances update everywhere.
    /// </summary>
    public static class PlaceholderPrefabsMenu
    {
        public const string Folder = "Assets/_Project/Prefabs";

        static readonly (string Path, Action<GameObject> Build)[] Prefabs =
        {
            ("Course/Cup", BuildCup),
            ("Course/Pond", BuildPond),
            ("Course/Fence", BuildFence),
            ("Course/Gate", BuildGate),
            ("Nature/Tree", BuildTree),
            ("Nature/Bush", BuildBush),
            ("Nature/Rock", BuildRock),
            ("Characters/Cat", BuildCat),
        };

        [MenuItem(MagicMenuUtil.Root + "Prefabs/Create Placeholder Prefabs", priority = MagicMenuUtil.PrefabsPriority)]
        static void CreateAll()
        {
            // Build in a preview scene so the open scene isn't touched.
            var buildScene = EditorSceneManager.NewPreviewScene();
            var created = 0;
            try
            {
                foreach (var (path, build) in Prefabs)
                {
                    var assetPath = $"{Folder}/{path}.prefab";
                    if (File.Exists(assetPath))
                    {
                        MagicMenuUtil.Log("Skipped (exists) " + assetPath);
                        continue;
                    }

                    MagicMenuUtil.EnsureFolder(Path.GetDirectoryName(assetPath));
                    var root = new GameObject(Path.GetFileName(path));
                    SceneManager.MoveGameObjectToScene(root, buildScene);
                    build(root);
                    PrefabUtility.SaveAsPrefabAsset(root, assetPath);
                    Object.DestroyImmediate(root);
                    MagicMenuUtil.Log("Created " + assetPath);
                    created++;
                }
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(buildScene);
            }

            AssetDatabase.SaveAssets();
            MagicMenuUtil.Log($"Placeholder prefabs: {created} created, {Prefabs.Length - created} already existed.");
        }

        // Cup + flag. Root: trigger at the hole (LevelGoal/Cup component later, #18). The pin is solid.
        static void BuildCup(GameObject root)
        {
            var visual = Visual(root);
            Part(visual, PrimitiveType.Cylinder, "Hole", new Vector3(0f, 0.005f, 0f), new Vector3(0.5f, 0.005f, 0.5f), PlaceholderMaterials.Hole);
            Part(visual, PrimitiveType.Cylinder, "Pin", new Vector3(0f, 1.25f, 0f), new Vector3(0.04f, 1.25f, 0.04f), PlaceholderMaterials.White);
            Part(visual, PrimitiveType.Cube, "Flag", new Vector3(0.4f, 2.25f, 0f), new Vector3(0.8f, 0.5f, 0.02f), PlaceholderMaterials.Flag);

            var hole = root.AddComponent<BoxCollider>();
            hole.size = new Vector3(0.5f, 0.2f, 0.5f);
            hole.isTrigger = true;
            var pin = root.AddComponent<CapsuleCollider>();
            pin.center = new Vector3(0f, 1.25f, 0f);
            pin.radius = 0.04f;
            pin.height = 2.5f;
        }

        // Water surface; the trigger reaches 1 m down (InvalidZone component later, #14). Scale the root to resize.
        static void BuildPond(GameObject root)
        {
            Part(Visual(root), PrimitiveType.Cube, "Water", Vector3.zero, new Vector3(8f, 0.02f, 8f), PlaceholderMaterials.Water);
            var zone = root.AddComponent<BoxCollider>();
            zone.center = new Vector3(0f, -0.5f, 0f);
            zone.size = new Vector3(8f, 1f, 8f);
            zone.isTrigger = true;
        }

        // A 2 m fence segment along X; duplicate and line them up for boundaries.
        static void BuildFence(GameObject root)
        {
            var visual = Visual(root);
            Part(visual, PrimitiveType.Cube, "PostLeft", new Vector3(-0.95f, 0.4f, 0f), new Vector3(0.1f, 0.8f, 0.1f), PlaceholderMaterials.Wood);
            Part(visual, PrimitiveType.Cube, "PostRight", new Vector3(0.95f, 0.4f, 0f), new Vector3(0.1f, 0.8f, 0.1f), PlaceholderMaterials.Wood);
            Part(visual, PrimitiveType.Cube, "RailTop", new Vector3(0f, 0.65f, 0f), new Vector3(2f, 0.08f, 0.05f), PlaceholderMaterials.Wood);
            Part(visual, PrimitiveType.Cube, "RailBottom", new Vector3(0f, 0.3f, 0f), new Vector3(2f, 0.08f, 0.05f), PlaceholderMaterials.Wood);
            var box = root.AddComponent<BoxCollider>();
            box.center = new Vector3(0f, 0.4f, 0f);
            box.size = new Vector3(2f, 0.8f, 0.1f);
        }

        // Stone arch with a portal surface. Pillars are solid; "PortalTrigger" is the opening (behaviour TBD).
        static void BuildGate(GameObject root)
        {
            var visual = Visual(root);
            Part(visual, PrimitiveType.Cube, "PillarLeft", new Vector3(-1.25f, 1.5f, 0f), new Vector3(0.5f, 3f, 0.5f), PlaceholderMaterials.Stone);
            Part(visual, PrimitiveType.Cube, "PillarRight", new Vector3(1.25f, 1.5f, 0f), new Vector3(0.5f, 3f, 0.5f), PlaceholderMaterials.Stone);
            Part(visual, PrimitiveType.Cube, "Beam", new Vector3(0f, 3.25f, 0f), new Vector3(3f, 0.5f, 0.5f), PlaceholderMaterials.Stone);
            Part(visual, PrimitiveType.Cube, "PortalSurface", new Vector3(0f, 1.5f, 0f), new Vector3(2f, 3f, 0.05f), PlaceholderMaterials.Portal);

            foreach (var x in new[] { -1.25f, 1.25f })
            {
                var pillar = root.AddComponent<BoxCollider>();
                pillar.center = new Vector3(x, 1.5f, 0f);
                pillar.size = new Vector3(0.5f, 3f, 0.5f);
            }

            var trigger = new GameObject("PortalTrigger").AddComponent<BoxCollider>();
            trigger.transform.SetParent(root.transform, false);
            trigger.center = new Vector3(0f, 1.5f, 0f);
            trigger.size = new Vector3(2f, 3f, 0.5f);
            trigger.isTrigger = true;
        }

        static void BuildTree(GameObject root)
        {
            var visual = Visual(root);
            Part(visual, PrimitiveType.Cylinder, "Trunk", new Vector3(0f, 1f, 0f), new Vector3(0.3f, 1f, 0.3f), PlaceholderMaterials.Wood);
            Part(visual, PrimitiveType.Sphere, "Canopy", new Vector3(0f, 2.8f, 0f), new Vector3(2.4f, 2.4f, 2.4f), PlaceholderMaterials.Leaves);
            var trunk = root.AddComponent<CapsuleCollider>();
            trunk.center = new Vector3(0f, 1f, 0f);
            trunk.radius = 0.15f;
            trunk.height = 2f;
            var canopy = root.AddComponent<SphereCollider>();
            canopy.center = new Vector3(0f, 2.8f, 0f);
            canopy.radius = 1.2f;
        }

        static void BuildBush(GameObject root)
        {
            Part(Visual(root), PrimitiveType.Sphere, "Leaves", new Vector3(0f, 0.4f, 0f), new Vector3(1.2f, 0.8f, 1.2f), PlaceholderMaterials.Grass);
            var sphere = root.AddComponent<SphereCollider>();
            sphere.center = new Vector3(0f, 0.4f, 0f);
            sphere.radius = 0.5f;
        }

        static void BuildRock(GameObject root)
        {
            Part(Visual(root), PrimitiveType.Sphere, "Stone", new Vector3(0f, 0.3f, 0f), new Vector3(1f, 0.6f, 0.8f), PlaceholderMaterials.Stone);
            var sphere = root.AddComponent<SphereCollider>();
            sphere.center = new Vector3(0f, 0.3f, 0f);
            sphere.radius = 0.4f;
        }

        // Sleeping cat (Garden hazard, Sir Grumblepaws' minion). Faces +Z.
        static void BuildCat(GameObject root)
        {
            var visual = Visual(root);
            Part(visual, PrimitiveType.Cube, "Body", new Vector3(0f, 0.5f, 0f), new Vector3(1f, 0.9f, 2f), PlaceholderMaterials.Cat);
            Part(visual, PrimitiveType.Sphere, "Head", new Vector3(0f, 0.8f, 1.2f), new Vector3(0.8f, 0.8f, 0.8f), PlaceholderMaterials.Cat);
            Part(visual, PrimitiveType.Cube, "EarLeft", new Vector3(-0.25f, 1.25f, 1.2f), new Vector3(0.15f, 0.25f, 0.1f), PlaceholderMaterials.Cat);
            Part(visual, PrimitiveType.Cube, "EarRight", new Vector3(0.25f, 1.25f, 1.2f), new Vector3(0.15f, 0.25f, 0.1f), PlaceholderMaterials.Cat);
            Part(visual, PrimitiveType.Cube, "Tail", new Vector3(0.3f, 0.2f, -1.4f), new Vector3(0.15f, 0.15f, 1f), PlaceholderMaterials.Cat);
            var box = root.AddComponent<BoxCollider>();
            box.center = new Vector3(0f, 0.6f, 0.2f);
            box.size = new Vector3(1f, 1.2f, 2.4f);
        }

        static Transform Visual(GameObject root)
        {
            var visual = new GameObject("Visual").transform;
            visual.SetParent(root.transform, false);
            return visual;
        }

        static void Part(Transform parent, PrimitiveType type, string name, Vector3 position, Vector3 scale, Material material)
        {
            var go = GameObject.CreatePrimitive(type);
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = scale;
            go.GetComponent<MeshRenderer>().sharedMaterial = material;
        }
    }
}
