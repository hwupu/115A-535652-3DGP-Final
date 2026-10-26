using System.IO;
using Project3DGP.Core.Cameras;
using Project3DGP.Core.Interaction;
using Project3DGP.Core.Movement;
using Project3DGP.MiniEx.Henry;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Project3DGP.EditorTools
{
    /// <summary>
    /// Magic > Mini-Exercises > Henry 4.C.4: Set Up Scene (#57). In the 4C4_ObjectInteraction scene, creates
    /// what is missing: materials and the Domino prefab in the exercise folder, the Hamster (walker + pusher),
    /// the walled Arena with its DominoArena spawner, the camera follow and the HUD. Existing objects are
    /// left alone (found by name). The dominoes themselves are spawned in Play mode (ADR-0010). One Undo step.
    /// </summary>
    public static class HenryObjectInteractionSetupMenu
    {
        const string ExerciseFolder = "Assets/_Project/MiniExercises/Henry/4C4_ObjectInteraction";
        const string SceneName = "4C4_ObjectInteraction";
        const string DominoPrefabPath = ExerciseFolder + "/Prefabs/Domino.prefab";

        static readonly Vector3 DominoSize = new Vector3(0.5f, 1f, 0.15f);
        const float DominoMass = 0.5f;
        const float ArenaHalfSize = 12f;

        [MenuItem(MagicMenuUtil.Root + "Mini-Exercises/Henry 4.C.4: Set Up Scene", priority = MagicMenuUtil.MiniExercisesPriority + 20)]
        static void SetUp()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.name != SceneName)
            {
                MagicMenuUtil.Log($"Open {SceneName} first (Henry/4C4_ObjectInteraction/Scenes).");
                return;
            }

            var dominoPrefab = EnsureDominoPrefab();

            Undo.SetCurrentGroupName("Set Up 4.C.4 Scene");
            var undoGroup = Undo.GetCurrentGroup();
            var created = 0;

            var hamster = Find(scene, "Hamster");
            if (hamster == null)
            {
                hamster = CreateHamster();
                created++;
            }

            var arena = Find(scene, "Arena");
            if (arena == null)
            {
                arena = CreateArena(dominoPrefab);
                created++;
            }

            if (Find(scene, "HUD") == null)
            {
                var hud = new GameObject("HUD");
                var hudComponent = hud.AddComponent<ArenaHud>();
                SetReference(hudComponent, "arena", arena.GetComponent<DominoArena>());
                Undo.RegisterCreatedObjectUndo(hud, "Create HUD");
                MagicMenuUtil.Log("Created HUD");
                created++;
            }

            var camera = Find(scene, "Main Camera");
            if (camera != null && !camera.TryGetComponent(out OrbitFollowCamera _))
            {
                var follow = Undo.AddComponent<OrbitFollowCamera>(camera);
                SetReference(follow, "target", hamster.transform);
                SetReference(hamster.GetComponent<ThirdPersonWalker>(), "cameraTransform", camera.transform);
                MagicMenuUtil.Log("Added OrbitFollowCamera to Main Camera (target: Hamster)");
                created++;
            }

            Undo.CollapseUndoOperations(undoGroup);
            if (created > 0)
                EditorSceneManager.MarkSceneDirty(scene);
            MagicMenuUtil.Log($"4.C.4 scene: {created} things created. Save the scene, then press Play.");
        }

        // Hamster: CharacterController on the root, primitive body in a "Visual" child (swap for a model later).
        static GameObject CreateHamster()
        {
            var hamster = new GameObject("Hamster");
            var controller = hamster.AddComponent<CharacterController>();
            controller.height = 0.8f;
            controller.radius = 0.3f;
            controller.center = new Vector3(0f, 0.4f, 0f);
            controller.stepOffset = 0.2f;
            hamster.AddComponent<ThirdPersonWalker>();
            hamster.AddComponent<RigidbodyPusher>();

            var visual = new GameObject("Visual");
            visual.transform.SetParent(hamster.transform, false);
            var fur = GetMaterial("Hamster", new Color(0.85f, 0.6f, 0.35f));
            var dark = GetMaterial("HamsterNose", new Color(0.2f, 0.12f, 0.1f));
            Part(visual, PrimitiveType.Capsule, "Body", new Vector3(0f, 0.4f, 0f), new Vector3(0.6f, 0.4f, 0.6f), fur);
            Part(visual, PrimitiveType.Sphere, "EarL", new Vector3(-0.15f, 0.78f, 0.05f), Vector3.one * 0.15f, fur);
            Part(visual, PrimitiveType.Sphere, "EarR", new Vector3(0.15f, 0.78f, 0.05f), Vector3.one * 0.15f, fur);
            Part(visual, PrimitiveType.Sphere, "Nose", new Vector3(0f, 0.5f, 0.3f), Vector3.one * 0.1f, dark);

            // Face the gap between two domino lines.
            hamster.transform.rotation = Quaternion.Euler(0f, 18f, 0f);
            Undo.RegisterCreatedObjectUndo(hamster, "Create Hamster");
            MagicMenuUtil.Log("Created Hamster (ThirdPersonWalker + RigidbodyPusher)");
            return hamster;
        }

        // Arena: four low walls keep the obstacles in; DominoArena spawns the 10 x 10 dominoes in Play mode.
        static GameObject CreateArena(VanishAfterHit dominoPrefab)
        {
            var arena = new GameObject("Arena");
            var spawner = arena.AddComponent<DominoArena>();
            SetReference(spawner, "dominoPrefab", dominoPrefab);

            var wallMaterial = GetMaterial("Wall", new Color(0.55f, 0.55f, 0.6f));
            var length = ArenaHalfSize * 2f + 0.5f;
            Wall(arena, "Wall_North", new Vector3(0f, 0.5f, ArenaHalfSize), new Vector3(length, 1f, 0.5f), wallMaterial);
            Wall(arena, "Wall_South", new Vector3(0f, 0.5f, -ArenaHalfSize), new Vector3(length, 1f, 0.5f), wallMaterial);
            Wall(arena, "Wall_East", new Vector3(ArenaHalfSize, 0.5f, 0f), new Vector3(0.5f, 1f, length), wallMaterial);
            Wall(arena, "Wall_West", new Vector3(-ArenaHalfSize, 0.5f, 0f), new Vector3(0.5f, 1f, length), wallMaterial);

            Undo.RegisterCreatedObjectUndo(arena, "Create Arena");
            MagicMenuUtil.Log("Created Arena (DominoArena + 4 walls)");
            return arena;
        }

        static void Wall(GameObject arena, string name, Vector3 position, Vector3 scale, Material material)
        {
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.SetParent(arena.transform, false);
            wall.transform.localPosition = position;
            wall.transform.localScale = scale;
            wall.GetComponent<Renderer>().sharedMaterial = material;
        }

        // Domino: BoxCollider + Rigidbody + VanishAfterHit on the root, a cube without collider as "Visual".
        static VanishAfterHit EnsureDominoPrefab()
        {
            var existing = AssetDatabase.LoadAssetAtPath<VanishAfterHit>(DominoPrefabPath);
            if (existing != null)
                return existing;

            MagicMenuUtil.EnsureFolder(Path.GetDirectoryName(DominoPrefabPath));
            var buildScene = EditorSceneManager.NewPreviewScene();
            try
            {
                var root = new GameObject("Domino");
                SceneManager.MoveGameObjectToScene(root, buildScene);

                var box = root.AddComponent<BoxCollider>();
                box.size = DominoSize;
                var body = root.AddComponent<Rigidbody>();
                body.mass = DominoMass;
                body.interpolation = RigidbodyInterpolation.Interpolate;
                root.AddComponent<VanishAfterHit>();

                var visual = new GameObject("Visual");
                visual.transform.SetParent(root.transform, false);
                Part(visual, PrimitiveType.Cube, "Tile", Vector3.zero, DominoSize, GetMaterial("Domino", Color.white));

                var prefab = PrefabUtility.SaveAsPrefabAsset(root, DominoPrefabPath);
                Object.DestroyImmediate(root);
                MagicMenuUtil.Log("Created " + DominoPrefabPath);
                return prefab.GetComponent<VanishAfterHit>();
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(buildScene);
            }
        }

        static void Part(GameObject parent, PrimitiveType type, string name, Vector3 position, Vector3 scale, Material material)
        {
            var part = GameObject.CreatePrimitive(type);
            part.name = name;
            Object.DestroyImmediate(part.GetComponent<Collider>());
            part.transform.SetParent(parent.transform, false);
            part.transform.localPosition = position;
            part.transform.localScale = scale;
            part.GetComponent<Renderer>().sharedMaterial = material;
        }

        // Materials live in the exercise folder so the exercise exports on its own.
        static Material GetMaterial(string name, Color color)
        {
            var path = $"{ExerciseFolder}/Materials/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
                return material;

            MagicMenuUtil.EnsureFolder(Path.GetDirectoryName(path));
            material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard")) { color = color };
            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", color);
            AssetDatabase.CreateAsset(material, path);
            MagicMenuUtil.Log("Created " + path);
            return material;
        }

        static void SetReference(Object component, string field, Object value)
        {
            var serialized = new SerializedObject(component);
            serialized.FindProperty(field).objectReferenceValue = value;
            serialized.ApplyModifiedProperties();
        }

        static GameObject Find(Scene scene, string name)
        {
            foreach (var root in scene.GetRootGameObjects())
                if (root.name == name)
                    return root;
            return null;
        }
    }
}
