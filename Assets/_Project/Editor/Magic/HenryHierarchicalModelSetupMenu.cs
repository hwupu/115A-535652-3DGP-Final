using System.IO;
using Project3DGP.Core.Cameras;
using Project3DGP.Core.Motion;
using Project3DGP.MiniEx.Henry;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Project3DGP.EditorTools
{
    /// <summary>
    /// Magic > Mini-Exercises > Henry 4.A.1: Set Up Scene (#59). In the 4A1_HierarchicalModel scene, creates
    /// what is missing (found by name): the hamster standing on the ground at address, its right arm as the chain
    /// A_Shoulder (about Z) > B_Wrist (about Y) > C_Club (sin aiming wiggle about Z) holding a golf club
    /// and a sledgehammer, the golf ball on the ground, the HUD and the camera orbit. One Undo step.
    /// </summary>
    public static class HenryHierarchicalModelSetupMenu
    {
        const string ExerciseFolder = "Assets/_Project/MiniExercises/Henry/4A1_HierarchicalModel";
        const string SceneName = "4A1_HierarchicalModel";
        const float ModelScale = 2f;
        const float BallRadius = 0.07f;
        // Ball next to the club head, on the target side (-X), in the hamster's local space (before scaling).
        static readonly Vector3 BallLocal = new Vector3(0.08f, 0f, 0.35f);

        [MenuItem(MagicMenuUtil.Root + "Mini-Exercises/Henry 4.A.1: Set Up Scene", priority = MagicMenuUtil.MiniExercisesPriority + 23)]
        static void SetUp()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.name != SceneName)
            {
                MagicMenuUtil.Log($"Open {SceneName} first (Henry/4A1_HierarchicalModel/Scenes).");
                return;
            }

            Undo.SetCurrentGroupName("Set Up 4.A.1 Scene");
            var undoGroup = Undo.GetCurrentGroup();
            var created = 0;

            var ball = Find(scene, "GolfBall");
            if (ball == null)
            {
                ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                ball.name = "GolfBall";
                Object.DestroyImmediate(ball.GetComponent<Collider>());   // the flight is computed, no physics needed
                var position = BallLocal * ModelScale;
                ball.transform.position = new Vector3(position.x, BallRadius, position.z);
                ball.transform.localScale = Vector3.one * (BallRadius * 2f);
                ball.GetComponent<Renderer>().sharedMaterial = GetMaterial("Ball", Color.white);
                ball.AddComponent<FlyingBall>();
                Undo.RegisterCreatedObjectUndo(ball, "Create GolfBall");
                MagicMenuUtil.Log("Created GolfBall");
                created++;
            }

            var hamster = Find(scene, "Hamster");
            if (hamster == null)
            {
                hamster = CreateHamster(ball.GetComponent<FlyingBall>());
                created++;
            }

            if (Find(scene, "HUD") == null)
            {
                var hud = new GameObject("HUD").AddComponent<HierarchyDemoHud>();
                var shoulder = hamster.transform.Find("A_Shoulder");
                var wrist = shoulder.Find("B_Wrist");
                SetReference(hud, "jointA", shoulder.GetComponent<AxisJoint>());
                SetReference(hud, "jointB", wrist.GetComponent<AxisJoint>());
                SetReference(hud, "partC", wrist.Find("C_Club").GetComponent<SineMotion>());
                SetReference(hud, "swing", hamster.GetComponent<SwingAnimator>());
                SetReference(hud, "ball", ball.GetComponent<FlyingBall>());
                SetReference(hud, "tools", hamster.GetComponent<ToolSwitcher>());
                Undo.RegisterCreatedObjectUndo(hud.gameObject, "Create HUD");
                MagicMenuUtil.Log("Created HUD");
                created++;
            }

            // Camera orbit: add it if missing, or fill in its target if the old target was deleted.
            var camera = Find(scene, "Main Camera");
            if (camera != null)
            {
                var orbit = camera.GetComponent<OrbitFollowCamera>();
                if (orbit == null)
                    orbit = Undo.AddComponent<OrbitFollowCamera>(camera);
                var serialized = new SerializedObject(orbit);
                if (serialized.FindProperty("target").objectReferenceValue == null)
                {
                    serialized.FindProperty("target").objectReferenceValue = hamster.transform;
                    serialized.FindProperty("distance").floatValue = 4f;
                    serialized.FindProperty("pitch").floatValue = 15f;
                    serialized.FindProperty("yaw").floatValue = 180f;   // face-on view: the ball flies across the screen
                    serialized.ApplyModifiedProperties();
                    MagicMenuUtil.Log("Main Camera: OrbitFollowCamera targets the Hamster (face-on view)");
                    created++;
                }
            }

            Undo.CollapseUndoOperations(undoGroup);
            if (created > 0)
                EditorSceneManager.MarkSceneDirty(scene);
            MagicMenuUtil.Log($"4.A.1 scene: {created} things created. Save the scene, then press Play.");
        }

        // The model: body parts (>= 5 components) + the right arm as the chain A > B > C > tools (one-handed swing).
        static GameObject CreateHamster(FlyingBall ball)
        {
            var fur = GetMaterial("Hamster", new Color(0.85f, 0.6f, 0.35f));
            var dark = GetMaterial("HamsterNose", new Color(0.2f, 0.12f, 0.1f));

            var hamster = new GameObject("Hamster");
            hamster.transform.localScale = Vector3.one * ModelScale;   // stands on the ground at the origin, facing +Z

            // Body parts, not animated.
            Part(hamster, PrimitiveType.Capsule, "Body", new Vector3(0f, 0.4f, 0f), new Vector3(0.6f, 0.4f, 0.6f), fur);
            Part(hamster, PrimitiveType.Sphere, "EarL", new Vector3(-0.15f, 0.78f, 0.05f), Vector3.one * 0.15f, fur);
            Part(hamster, PrimitiveType.Sphere, "EarR", new Vector3(0.15f, 0.78f, 0.05f), Vector3.one * 0.15f, fur);
            Part(hamster, PrimitiveType.Sphere, "EyeL", new Vector3(-0.09f, 0.6f, 0.26f), Vector3.one * 0.06f, dark);
            Part(hamster, PrimitiveType.Sphere, "EyeR", new Vector3(0.09f, 0.6f, 0.26f), Vector3.one * 0.06f, dark);
            Part(hamster, PrimitiveType.Sphere, "Nose", new Vector3(0f, 0.5f, 0.3f), Vector3.one * 0.08f, dark);
            Part(hamster, PrimitiveType.Sphere, "LegL", new Vector3(-0.15f, 0.06f, 0.08f), Vector3.one * 0.16f, fur);
            Part(hamster, PrimitiveType.Sphere, "LegR", new Vector3(0.15f, 0.06f, 0.08f), Vector3.one * 0.16f, fur);
            // Left arm: resting at the side, not part of the swing.
            var armL = Part(hamster, PrimitiveType.Capsule, "ArmL", new Vector3(-0.33f, 0.38f, 0.05f), new Vector3(0.08f, 0.13f, 0.08f), fur);
            armL.transform.localRotation = Quaternion.Euler(0f, 0f, -15f);

            // A: right shoulder, just in front of the body so the arm can swing across the chest.
            // Turns about its local Z axis (the swing). Key 1 reverses.
            var shoulder = Joint(hamster.transform, "A_Shoulder", new Vector3(0.15f, 0.55f, 0.32f));
            Configure(shoulder.AddComponent<AxisJoint>(), Vector3.forward, Key.Digit1);
            Part(shoulder, PrimitiveType.Capsule, "ArmR", new Vector3(0f, -0.12f, 0f), new Vector3(0.08f, 0.13f, 0.08f), fur);

            // B: wrist (hand), child of A. Turns about its local Y axis = along the club (opens/closes the face). Key 2 reverses.
            var wrist = Joint(shoulder.transform, "B_Wrist", new Vector3(0f, -0.25f, 0f));
            Configure(wrist.AddComponent<AxisJoint>(), Vector3.up, Key.Digit2);
            Part(wrist, PrimitiveType.Sphere, "Hand", Vector3.zero, Vector3.one * 0.1f, fur);

            // C: the club, child of B. Aiming wiggle at address: turns about its local Z by 8 x sin(2 pi 1.2 t) degrees,
            // so the club head moves back and forth behind the ball (PO choice; the handout's "along B's local y-axis"
            // slide is kept commented out in SineMotion). Rest is slightly up so the head clears the ground.
            var club = Joint(wrist.transform, "C_Club", new Vector3(0f, 0.01f, 0f));
            var waggle = club.AddComponent<SineMotion>();
            var serializedC = new SerializedObject(waggle);
            serializedC.FindProperty("axis").vector3Value = Vector3.forward;
            serializedC.FindProperty("amplitude").floatValue = 8f;
            serializedC.FindProperty("frequency").floatValue = 1.2f;
            serializedC.ApplyModifiedProperties();

            // Tools: children of C, so they follow C, B and A. ToolSwitcher shows one at a time (key T).
            var wood = GetMaterial("Wood", new Color(0.45f, 0.3f, 0.18f));
            var metal = GetMaterial("Metal", new Color(0.75f, 0.78f, 0.82f));
            var iron = GetMaterial("Iron", new Color(0.25f, 0.25f, 0.28f));

            var golfClub = Joint(club.transform, "GolfClub", Vector3.zero);
            Part(golfClub, PrimitiveType.Cylinder, "Shaft", new Vector3(0f, -0.15f, 0f), new Vector3(0.02f, 0.15f, 0.02f), metal);
            Part(golfClub, PrimitiveType.Cube, "Head", new Vector3(0f, -0.285f, 0.03f), new Vector3(0.04f, 0.03f, 0.1f), metal);

            var hammer = Joint(club.transform, "Sledgehammer", Vector3.zero);
            hammer.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);   // head across the swing, so its face hits the ball
            Part(hammer, PrimitiveType.Cylinder, "Handle", new Vector3(0f, -0.13f, 0f), new Vector3(0.025f, 0.13f, 0.025f), wood);
            Part(hammer, PrimitiveType.Cube, "Head", new Vector3(0f, -0.26f, 0f), new Vector3(0.08f, 0.08f, 0.16f), iron);

            var switcher = hamster.AddComponent<ToolSwitcher>();
            var serializedTools = new SerializedObject(switcher);
            var list = serializedTools.FindProperty("tools");
            list.arraySize = 2;
            list.GetArrayElementAtIndex(0).objectReferenceValue = golfClub;
            list.GetArrayElementAtIndex(1).objectReferenceValue = hammer;
            serializedTools.ApplyModifiedProperties();

            // The swing animation (F): drives A and B, switches C's waggle, launches the ball.
            var swing = hamster.AddComponent<SwingAnimator>();
            SetReference(swing, "shoulder", shoulder.GetComponent<AxisJoint>());
            SetReference(swing, "wrist", wrist.GetComponent<AxisJoint>());
            SetReference(swing, "waggle", waggle);
            SetReference(swing, "tools", switcher);
            SetReference(swing, "ball", ball);

            Undo.RegisterCreatedObjectUndo(hamster, "Create Hamster");
            MagicMenuUtil.Log("Created Hamster (A_Shoulder > B_Wrist > C_Club > GolfClub / Sledgehammer, SwingAnimator)");
            return hamster;
        }

        static void Configure(AxisJoint joint, Vector3 axis, Key reverseKey)
        {
            var serialized = new SerializedObject(joint);
            serialized.FindProperty("axis").vector3Value = axis;
            serialized.FindProperty("reverseKey").intValue = (int)reverseKey;
            serialized.ApplyModifiedProperties();
        }

        // An empty GameObject used as a joint (pivot): children rotate/move around its origin.
        static GameObject Joint(Transform parent, string name, Vector3 localPosition)
        {
            var joint = new GameObject(name);
            joint.transform.SetParent(parent, false);
            joint.transform.localPosition = localPosition;
            return joint;
        }

        // A primitive without collider (visual only).
        static GameObject Part(GameObject parent, PrimitiveType type, string name, Vector3 localPosition, Vector3 scale, Material material)
        {
            var part = GameObject.CreatePrimitive(type);
            part.name = name;
            Object.DestroyImmediate(part.GetComponent<Collider>());
            part.transform.SetParent(parent.transform, false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = scale;
            part.GetComponent<Renderer>().sharedMaterial = material;
            return part;
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
