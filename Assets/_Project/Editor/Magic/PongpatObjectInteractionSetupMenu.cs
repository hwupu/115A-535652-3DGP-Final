using System.Collections.Generic;
using System.IO;
using Project3DGP.Core.Interaction;
using Project3DGP.Core.Movement;
using Project3DGP.MiniEx.Pongpat.ObjectInteraction;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project3DGP.EditorTools
{
    /// <summary>Authors Pongpat's independent physics demo; running it again preserves existing work.</summary>
    public static class PongpatObjectInteractionSetupMenu
    {
        const string ExerciseFolder = "Assets/_Project/MiniExercises/Pongpat/4C4_ObjectInteraction";
        const string ScenePath = ExerciseFolder + "/Scenes/4C4_ObjectInteraction.unity";
        const string ArenaName = "Pongpat Object Interaction";
        const int ObstacleCount = 120;

        [MenuItem(MagicMenuUtil.Root + "Mini-Exercises/Pongpat 4.C.4: Set Up Scene",
            priority = MagicMenuUtil.MiniExercisesPriority + 32)]
        public static void SetUpScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                MagicMenuUtil.Log("Stop Play mode before setting up Pongpat 4.C.4.");
                return;
            }

            if (!File.Exists(ScenePath))
            {
                MagicMenuUtil.Log("Pongpat 4.C.4 scene is missing. Run Create All Planned first.");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                MagicMenuUtil.Log("Cancelled: open scenes were not saved.");
                return;
            }

            var scene = SceneManager.GetActiveScene();
            if (scene.path != ScenePath)
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            Undo.IncrementCurrentGroup();
            var undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Set up Pongpat 4.C.4");

            try
            {
                var obstacleMaterial = EnsureMaterial("Movable Cube", new Color(1f, 0.65f, 0.17f));
                var characterMaterial = EnsureMaterial("Character", new Color(0.16f, 0.48f, 0.85f));
                var wallMaterial = EnsureMaterial("Boundary", new Color(0.24f, 0.28f, 0.33f));
                var root = EnsureRoot(scene, ArenaName);
                var boundary = EnsureChild(root.transform, "Boundary");
                EnsureGround(scene, wallMaterial);
                EnsureBlock(boundary.transform, "North Wall", new Vector3(0f, 1f, 15f), new Vector3(30.5f, 2f, 0.5f), wallMaterial);
                EnsureBlock(boundary.transform, "South Wall", new Vector3(0f, 1f, -15f), new Vector3(30.5f, 2f, 0.5f), wallMaterial);
                EnsureBlock(boundary.transform, "West Wall", new Vector3(-15f, 1f, 0f), new Vector3(0.5f, 2f, 30.5f), wallMaterial);
                EnsureBlock(boundary.transform, "East Wall", new Vector3(15f, 1f, 0f), new Vector3(0.5f, 2f, 30.5f), wallMaterial);

                var character = EnsureCharacter(root.transform, characterMaterial);
                var obstacleRoot = EnsureChild(root.transform, "Obstacles");
                var obstacles = new List<ImpactRecycleBody>(ObstacleCount);
                for (var i = 0; i < ObstacleCount; i++)
                    obstacles.Add(EnsureObstacle(obstacleRoot.transform, i, InitialPosition(i), obstacleMaterial));

                EnsureCamera(root.transform);
                var demo = root.GetComponent<ObjectInteractionDemo>();
                if (demo == null)
                {
                    demo = Undo.AddComponent<ObjectInteractionDemo>(root);
                    var settings = new SerializedObject(demo);
                    settings.FindProperty("character").objectReferenceValue = character;
                    var obstacleField = settings.FindProperty("obstacles");
                    obstacleField.arraySize = obstacles.Count;
                    for (var i = 0; i < obstacles.Count; i++)
                        obstacleField.GetArrayElementAtIndex(i).objectReferenceValue = obstacles[i];
                    settings.FindProperty("respawnCenter").vector3Value = new Vector3(0f, 0.53f, 0f);
                    settings.FindProperty("respawnSize").vector2Value = new Vector2(27f, 27f);
                    settings.FindProperty("obstacleHalfExtents").vector3Value = Vector3.one * 0.45f;
                    settings.FindProperty("clearance").floatValue = 0.04f;
                    settings.FindProperty("placementAttempts").intValue = 48;
                    settings.FindProperty("retryInterval").floatValue = 0.15f;
                    settings.FindProperty("collisionWindow").floatValue = 2f;
                    settings.FindProperty("recyclingEnabled").boolValue = false;
                    settings.ApplyModifiedProperties();
                    MagicMenuUtil.Log("Added demo input, recycling and collision counters.");
                }
                else
                {
                    RepairMissingReferences(demo, character, obstacles);
                }

                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                AssetDatabase.SaveAssets();
                Selection.activeGameObject = root;
                MagicMenuUtil.Log("Pongpat 4.C.4 ready: " + obstacles.Count + " obstacles; north pack has 20 cubes. " +
                    "Press Play, walk with WASD/arrows, hold Shift to push through the pack. Existing objects were preserved.");
            }
            finally
            {
                Undo.CollapseUndoOperations(undoGroup);
            }
        }

        static void RepairMissingReferences(ObjectInteractionDemo demo, PlanarBodyMotor character,
            List<ImpactRecycleBody> obstacles)
        {
            var settings = new SerializedObject(demo);
            var characterField = settings.FindProperty("character");
            var repaired = 0;
            if (characterField.objectReferenceValue == null)
            {
                characterField.objectReferenceValue = character;
                repaired++;
                MagicMenuUtil.Log("Repaired missing demo Character reference.");
            }

            var obstacleField = settings.FindProperty("obstacles");
            var referenced = new HashSet<Object>();
            for (var i = 0; i < obstacleField.arraySize; i++)
            {
                var reference = obstacleField.GetArrayElementAtIndex(i).objectReferenceValue;
                if (reference != null)
                    referenced.Add(reference);
            }

            for (var i = 0; i < obstacles.Count; i++)
            {
                var obstacle = obstacles[i];
                if (obstacle == null || referenced.Contains(obstacle))
                    continue;

                // Prefer the original index after an object was deleted and recreated. Other
                // non-null references (including manually assigned objects) remain untouched.
                var slot = i < obstacleField.arraySize &&
                    obstacleField.GetArrayElementAtIndex(i).objectReferenceValue == null ? i : -1;
                if (slot < 0)
                    for (var j = 0; j < obstacleField.arraySize; j++)
                        if (obstacleField.GetArrayElementAtIndex(j).objectReferenceValue == null)
                        {
                            slot = j;
                            break;
                        }

                if (slot < 0)
                {
                    slot = obstacleField.arraySize;
                    obstacleField.arraySize++;
                }
                obstacleField.GetArrayElementAtIndex(slot).objectReferenceValue = obstacle;
                referenced.Add(obstacle);
                repaired++;
                MagicMenuUtil.Log("Repaired demo obstacle reference for " + obstacle.name + ".");
            }

            if (repaired > 0)
                settings.ApplyModifiedProperties();
        }

        static Vector3 InitialPosition(int index)
        {
            // Twenty close cubes make a reachable cascade; the surrounding rings never intersect it.
            if (index < 20)
                return new Vector3((index % 4 - 1.5f) * 0.98f, 0.53f, 2.5f + index / 4 * 0.98f);

            var ringIndex = index - 20;
            var radius = 8f + ringIndex / 20 * 1.2f;
            var angle = ringIndex % 20 * (Mathf.PI * 2f / 20f);
            return new Vector3(Mathf.Sin(angle) * radius, 0.53f, Mathf.Cos(angle) * radius);
        }

        static PlanarBodyMotor EnsureCharacter(Transform parent, Material material)
        {
            var existing = parent.Find("Character");
            GameObject character;
            if (existing == null)
            {
                character = CreatePrimitive(parent, "Character", PrimitiveType.Cube,
                    new Vector3(0f, 0.98f, 0f), new Vector3(1.2f, 1.8f, 1.2f), material);
            }
            else
            {
                character = existing.gameObject;
                MagicMenuUtil.Log("Skipped existing character transform and appearance.");
            }

            var body = character.GetComponent<Rigidbody>();
            if (body == null)
            {
                body = Undo.AddComponent<Rigidbody>(character);
                body.mass = 30f;
                body.constraints = RigidbodyConstraints.FreezeRotation;
                body.interpolation = RigidbodyInterpolation.Interpolate;
                body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            }
            return character.GetComponent<PlanarBodyMotor>() ?? Undo.AddComponent<PlanarBodyMotor>(character);
        }

        static ImpactRecycleBody EnsureObstacle(Transform parent, int index, Vector3 position, Material material)
        {
            var name = "Obstacle " + (index + 1).ToString("000");
            var existing = parent.Find(name);
            var obstacle = existing != null ? existing.gameObject :
                CreatePrimitive(parent, name, PrimitiveType.Cube, position, Vector3.one * 0.9f, material);
            if (existing != null)
                MagicMenuUtil.Log("Skipped existing " + name + ".");

            var body = obstacle.GetComponent<Rigidbody>();
            if (body == null)
            {
                body = Undo.AddComponent<Rigidbody>(obstacle);
                body.mass = 1f;
                body.interpolation = RigidbodyInterpolation.Interpolate;
                body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            }
            return obstacle.GetComponent<ImpactRecycleBody>() ?? Undo.AddComponent<ImpactRecycleBody>(obstacle);
        }

        static void EnsureCamera(Transform parent)
        {
            const string cameraName = "Interaction Overview Camera";
            var existing = parent.Find(cameraName);
            if (existing != null)
            {
                MagicMenuUtil.Log("Skipped existing overview camera.");
                return;
            }
            var cameraObject = new GameObject(cameraName);
            Undo.RegisterCreatedObjectUndo(cameraObject, "Create overview camera");
            cameraObject.transform.SetParent(parent, false);
            cameraObject.transform.position = new Vector3(0f, 27f, -18f);
            cameraObject.transform.LookAt(Vector3.zero);
            var camera = Undo.AddComponent<Camera>(cameraObject);
            camera.orthographic = true;
            camera.orthographicSize = 18f;
            camera.depth = 10f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.12f, 0.15f, 0.18f);
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 100f;
            // Keep the scaffold's AudioListener; a second listener would produce a Console warning.
            MagicMenuUtil.Log("Created overview camera; preserved the scaffold camera and AudioListener.");
        }

        static void EnsureGround(Scene scene, Material material)
        {
            if (FindRoot(scene, "Ground") != null)
            {
                MagicMenuUtil.Log("Skipped existing Ground.");
                return;
            }
            CreatePrimitive(null, "Ground", PrimitiveType.Plane, Vector3.zero,
                new Vector3(5f, 1f, 5f), material);
        }

        static GameObject EnsureRoot(Scene scene, string name)
        {
            var existing = FindRoot(scene, name);
            if (existing != null)
            {
                MagicMenuUtil.Log("Skipped existing " + name + ".");
                return existing;
            }
            var root = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(root, "Create " + name);
            MagicMenuUtil.Log("Created " + name + ".");
            return root;
        }

        static GameObject FindRoot(Scene scene, string name)
        {
            foreach (var root in scene.GetRootGameObjects())
                if (root.name == name)
                    return root;
            return null;
        }

        static GameObject EnsureChild(Transform parent, string name)
        {
            var child = parent.Find(name);
            if (child != null)
                return child.gameObject;
            var created = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(created, "Create " + name);
            created.transform.SetParent(parent, false);
            MagicMenuUtil.Log("Created " + name + ".");
            return created;
        }

        static void EnsureBlock(Transform parent, string name, Vector3 position, Vector3 size, Material material)
        {
            if (parent.Find(name) != null)
                return;
            CreatePrimitive(parent, name, PrimitiveType.Cube, position, size, material);
        }

        static GameObject CreatePrimitive(Transform parent, string name, PrimitiveType primitive,
            Vector3 position, Vector3 scale, Material material)
        {
            var created = GameObject.CreatePrimitive(primitive);
            Undo.RegisterCreatedObjectUndo(created, "Create " + name);
            created.name = name;
            created.transform.SetParent(parent, false);
            created.transform.position = position;
            created.transform.localScale = scale;
            created.GetComponent<Renderer>().sharedMaterial = material;
            MagicMenuUtil.Log("Created " + name + ".");
            return created;
        }

        static Material EnsureMaterial(string name, Color color)
        {
            var path = ExerciseFolder + "/Materials/" + name.Replace(' ', '_') + ".mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null)
                return existing;
            if (File.Exists(path))
                throw new System.InvalidOperationException("Existing file is not a material; preserved " + path);
            MagicMenuUtil.EnsureFolder(ExerciseFolder + "/Materials");
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                shader = Shader.Find("Standard");
            var material = new Material(shader) { name = name, color = color };
            AssetDatabase.CreateAsset(material, path);
            MagicMenuUtil.Log("Created material " + path);
            return material;
        }
    }
}
