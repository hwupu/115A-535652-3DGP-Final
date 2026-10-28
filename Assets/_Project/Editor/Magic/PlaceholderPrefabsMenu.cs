using System;
using System.IO;
using Project3DGP.Game.Golf;
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
    /// #88 adds the hero hamster, the princess and the three tools (Prefabs/Characters, Prefabs/Tools).
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
            ("Characters/Hamster", BuildHamster),
            ("Characters/Princess", BuildPrincess),
            ("Tools/StandardClub", BuildStandardClub),
            ("Tools/Sledgehammer", BuildSledgehammer),
            ("Tools/Mysterious", BuildMysterious),
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

        // Water surface + InvalidZone trigger from 1 m below to 0.5 m above it, so a rolling ball touches it. Scale the root to resize.
        static void BuildPond(GameObject root)
        {
            Part(Visual(root), PrimitiveType.Cube, "Water", Vector3.zero, new Vector3(8f, 0.02f, 8f), PlaceholderMaterials.Water);
            var zone = root.AddComponent<BoxCollider>();
            zone.center = PondInvalidZone.TriggerCenter;
            zone.size = PondInvalidZone.TriggerSize;
            zone.isTrigger = true;
            root.AddComponent<InvalidZone>();   // the ball goes back to its last valid spot (#14)
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
            CatPrefabEyes.Add(visual);   // closed / open by GardenCat (#21)
            var box = root.AddComponent<BoxCollider>();
            box.center = new Vector3(0f, 0.6f, 0.2f);
            box.size = new Vector3(1f, 1.2f, 2.4f);
        }

        // ---- Hamsters (#88). Shapes borrowed from mini-exercise 4.A.1; built in "hamster units" inside a Visual
        // scaled x HamsterScale (about 1.3 m tall), facing +Z, feet at the root. No collider: the ball passes through.
        // To use the Blender model later, replace the Visual child (keep a "ToolSocket" in the right hand).
        const float HamsterScale = 1.5f;
        static readonly Color Fur = new(0.85f, 0.6f, 0.35f);

        // The hero: plain hamster holding a tool in the right hand (ShoulderR > HandR > ToolSocket).
        static void BuildHamster(GameObject root) => HamsterBody(root, PlaceholderMaterials.Get("Hamster", Fur));

        // The princess: lighter fur, a gold crown and a pink skirt.
        static void BuildPrincess(GameObject root)
        {
            var visual = HamsterBody(root, PlaceholderMaterials.Get("HamsterPrincess", new Color(0.95f, 0.85f, 0.7f)));
            Part(visual, PrimitiveType.Cylinder, "Crown", new Vector3(0f, 0.86f, 0.03f), new Vector3(0.16f, 0.04f, 0.16f),
                PlaceholderMaterials.Get("Gold", new Color(1f, 0.8f, 0.2f)));
            Part(visual, PrimitiveType.Cylinder, "Skirt", new Vector3(0f, 0.18f, 0f), new Vector3(0.66f, 0.1f, 0.66f),
                PlaceholderMaterials.Get("Pink", new Color(1f, 0.55f, 0.75f)));
        }

        static Transform HamsterBody(GameObject root, Material fur)
        {
            var dark = PlaceholderMaterials.Get("HamsterDark", new Color(0.2f, 0.12f, 0.1f));
            var visual = Visual(root);
            visual.localScale = Vector3.one * HamsterScale;
            Part(visual, PrimitiveType.Capsule, "Body", new Vector3(0f, 0.4f, 0f), new Vector3(0.6f, 0.4f, 0.6f), fur);
            Part(visual, PrimitiveType.Sphere, "EarL", new Vector3(-0.15f, 0.78f, 0.05f), Vector3.one * 0.15f, fur);
            Part(visual, PrimitiveType.Sphere, "EarR", new Vector3(0.15f, 0.78f, 0.05f), Vector3.one * 0.15f, fur);
            Part(visual, PrimitiveType.Sphere, "EyeL", new Vector3(-0.09f, 0.6f, 0.26f), Vector3.one * 0.06f, dark);
            Part(visual, PrimitiveType.Sphere, "EyeR", new Vector3(0.09f, 0.6f, 0.26f), Vector3.one * 0.06f, dark);
            Part(visual, PrimitiveType.Sphere, "Nose", new Vector3(0f, 0.5f, 0.3f), Vector3.one * 0.08f, dark);
            Part(visual, PrimitiveType.Sphere, "LegL", new Vector3(-0.15f, 0.06f, 0.08f), Vector3.one * 0.16f, fur);
            Part(visual, PrimitiveType.Sphere, "LegR", new Vector3(0.15f, 0.06f, 0.08f), Vector3.one * 0.16f, fur);
            var armL = Part(visual, PrimitiveType.Capsule, "ArmL", new Vector3(-0.33f, 0.38f, 0.05f), new Vector3(0.08f, 0.13f, 0.08f), fur);
            armL.localRotation = Quaternion.Euler(0f, 0f, -15f);

            // Right arm as pivots (like 4.A.1's A_Shoulder > B_Wrist), so a swing can rotate the shoulder later (#36).
            var shoulder = Pivot(visual, "ShoulderR", new Vector3(0.15f, 0.55f, 0.32f));
            Part(shoulder, PrimitiveType.Capsule, "ArmR", new Vector3(0f, -0.12f, 0f), new Vector3(0.08f, 0.13f, 0.08f), fur);
            var hand = Pivot(shoulder, "HandR", new Vector3(0f, -0.25f, 0f));
            Part(hand, PrimitiveType.Sphere, "Hand", Vector3.zero, Vector3.one * 0.1f, fur);
            Pivot(hand, "ToolSocket", new Vector3(0f, 0.01f, 0f));   // GolferAvatar.SetTool puts the tool here
            return visual;
        }

        // ---- Tools (#16, #88): held at the grip (the root), pointing down -Y, sized in hamster units
        // (the hamster's Visual scales them with it). ClubProfile.ToolPrefab points here.

        static void BuildStandardClub(GameObject root)
        {
            var metal = PlaceholderMaterials.Get("Metal", new Color(0.75f, 0.78f, 0.82f));
            var visual = Visual(root);
            Part(visual, PrimitiveType.Cylinder, "Shaft", new Vector3(0f, -0.15f, 0f), new Vector3(0.02f, 0.15f, 0.02f), metal);
            Part(visual, PrimitiveType.Cube, "Head", new Vector3(0f, -0.285f, 0.03f), new Vector3(0.04f, 0.03f, 0.1f), metal);
        }

        static void BuildSledgehammer(GameObject root)
        {
            var visual = Visual(root);
            visual.localRotation = Quaternion.Euler(0f, 90f, 0f);   // head across the swing, so its face hits the ball
            Part(visual, PrimitiveType.Cylinder, "Handle", new Vector3(0f, -0.13f, 0f), new Vector3(0.025f, 0.13f, 0.025f), PlaceholderMaterials.Wood);
            Part(visual, PrimitiveType.Cube, "Head", new Vector3(0f, -0.26f, 0f), new Vector3(0.08f, 0.08f, 0.16f),
                PlaceholderMaterials.Get("Iron", new Color(0.25f, 0.25f, 0.28f)));
        }

        // Codename tool (real name/look TBD): a purple wand with a glowing orb (unlit = looks like it glows).
        static void BuildMysterious(GameObject root)
        {
            var visual = Visual(root);
            Part(visual, PrimitiveType.Cylinder, "Wand", new Vector3(0f, -0.14f, 0f), new Vector3(0.018f, 0.14f, 0.018f), PlaceholderMaterials.Portal);
            Part(visual, PrimitiveType.Sphere, "Orb", new Vector3(0f, -0.3f, 0f), Vector3.one * 0.07f,
                PlaceholderMaterials.Get("MagicGlow", new Color(0.85f, 0.7f, 1f), "Universal Render Pipeline/Unlit"));
        }

        static Transform Pivot(Transform parent, string name, Vector3 position)
        {
            var pivot = new GameObject(name).transform;
            pivot.SetParent(parent, false);
            pivot.localPosition = position;
            return pivot;
        }

        static Transform Visual(GameObject root)
        {
            var visual = new GameObject("Visual").transform;
            visual.SetParent(root.transform, false);
            return visual;
        }

        static Transform Part(Transform parent, PrimitiveType type, string name, Vector3 position, Vector3 scale, Material material)
        {
            var go = GameObject.CreatePrimitive(type);
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = scale;
            go.GetComponent<MeshRenderer>().sharedMaterial = material;
            return go.transform;
        }
    }
}
