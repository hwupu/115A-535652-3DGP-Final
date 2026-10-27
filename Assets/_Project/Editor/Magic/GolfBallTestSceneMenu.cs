using Project3DGP.Core.Cameras;
using Project3DGP.Game.Controls;
using Project3DGP.Game.Golf;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Project3DGP.EditorTools
{
    /// <summary>
    /// Magic > Game > Open Ball Test Area (#14): opens a new, UNSAVED scene to try the GolfBall: a 60 m green
    /// with open edges (fall off = kill height), a ramp, a wall, a pond (InvalidZone), the ball prefab,
    /// an orbit camera and the GolfBallDebugHitter. Nothing is saved; close it without saving.
    /// </summary>
    public static class GolfBallTestSceneMenu
    {
        [MenuItem(MagicMenuUtil.Root + "Game/Open Ball Test Area", priority = MagicMenuUtil.GamePriority + 11)]
        static void Open()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                MagicMenuUtil.Log("Cancelled: open scenes were not saved.");
                return;
            }

            if (GolfInputAssetMenu.Create() == null || GolfBallPrefabMenu.Create() == null)
                return;

            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            // Load the assets AFTER NewScene: opening a scene unloads unused assets, which would empty
            // references taken before it (the hitter's Input field ended up None).
            var input = AssetDatabase.LoadAssetAtPath<GolfInput>(GolfInputAssetMenu.InputPath);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(GolfBallPrefabMenu.PrefabPath);

            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);   // 10 m x 10 m at scale 1
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(6f, 1f, 6f);
            ground.GetComponent<Renderer>().sharedMaterial = PlaceholderMaterials.Grass;

            Block("Ramp", new Vector3(0f, 0.5f, 10f), new Vector3(4f, 0.2f, 6f), Quaternion.Euler(-12f, 0f, 0f), PlaceholderMaterials.Stone);
            Block("Wall", new Vector3(-8f, 0.5f, 6f), new Vector3(0.4f, 1f, 8f), Quaternion.identity, PlaceholderMaterials.Wood);

            // Pond: a thin water surface + a trigger box around it (from below the ground to 1 m above).
            var pond = new GameObject("Pond (InvalidZone)");
            pond.transform.position = new Vector3(7f, 0f, 8f);
            var trigger = pond.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(6f, 2f, 6f);
            pond.AddComponent<InvalidZone>();
            var water = GameObject.CreatePrimitive(PrimitiveType.Cube);
            water.name = "Water";
            Object.DestroyImmediate(water.GetComponent<Collider>());
            water.transform.SetParent(pond.transform, false);
            water.transform.localPosition = new Vector3(0f, 0.01f, 0f);
            water.transform.localScale = new Vector3(6f, 0.02f, 6f);
            water.GetComponent<Renderer>().sharedMaterial = PlaceholderMaterials.Water;

            var ball = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            ball.transform.position = new Vector3(0f, GolfBallPrefabMenu.Diameter / 2f, 0f);

            var camera = Camera.main;
            var orbit = camera.gameObject.AddComponent<OrbitFollowCamera>();
            var orbitSo = new SerializedObject(orbit);
            orbitSo.FindProperty("target").objectReferenceValue = ball.transform;
            orbitSo.FindProperty("distance").floatValue = 8f;
            orbitSo.FindProperty("pitch").floatValue = 25f;
            orbitSo.ApplyModifiedPropertiesWithoutUndo();

            var hitter = new GameObject("GolfBallDebug").AddComponent<GolfBallDebugHitter>();
            var hitterSo = new SerializedObject(hitter);
            hitterSo.FindProperty("input").objectReferenceValue = input;
            hitterSo.FindProperty("ball").objectReferenceValue = ball.GetComponent<GolfBall>();
            hitterSo.FindProperty("aimCamera").objectReferenceValue = camera.transform;
            hitterSo.ApplyModifiedPropertiesWithoutUndo();

            Selection.activeGameObject = ball;
            MagicMenuUtil.Log("Opened an unsaved ball test area. Press Play; close it without saving.");
        }

        static void Block(string name, Vector3 position, Vector3 scale, Quaternion rotation, Material material)
        {
            var block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = name;
            block.transform.SetPositionAndRotation(position, rotation);
            block.transform.localScale = scale;
            block.GetComponent<Renderer>().sharedMaterial = material;
        }
    }
}
