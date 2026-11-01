using System.Collections.Generic;
using Project3DGP.Core.Cameras;
using Project3DGP.Core.Movement;
using Project3DGP.Game.Cameras;
using Project3DGP.Game.Controls;
using Project3DGP.Game.Explore;
using Project3DGP.Game.Flow;
using Project3DGP.Game.Golf;
using Project3DGP.Game.Player;
using Project3DGP.Game.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Project3DGP.EditorTools
{
    /// <summary>
    /// Magic > Level > Add Camera Rig and Explore Mode (#19, Bahadir): sets up the open level scene so the player
    /// walks to the ball instead of teleporting:
    ///   Cameras/CameraRig        CameraRig, moving Cameras/Main Camera (replaces the TEMP(#19) OrbitFollowCamera)
    ///   Gameplay/Player avatar   CharacterController (sized from its model) + CharacterWalker, on this scene's copy
    ///                            only (the avatar prefab is not changed)
    ///   Systems/PlayerModes      PlayerModeController (walk / shoot / ball in flight)
    ///   Systems/AvatarWiring_TEMP  "Teleport To Ball" off
    /// Run Magic > Level > Add Ball, Shot and Avatar first. Each step runs only if its component is missing. One Undo
    /// step; the scene is marked dirty, not saved (to only try it, close the scene without saving).
    /// </summary>
    public static class ExploreModeMenu
    {
        const string ModesName = "PlayerModes";

        [MenuItem(MagicMenuUtil.Root + "Level/Add Camera Rig and Explore Mode", priority = MagicMenuUtil.LevelPriority + 7)]
        static void Add()
        {
            var scene = EditorSceneManager.GetActiveScene();
            var rigSlot = LevelSkeleton.Find(scene, "Cameras/CameraRig");
            var cameraObject = LevelSkeleton.Find(scene, "Cameras/Main Camera");
            var systems = LevelSkeleton.Find(scene, "Systems");
            if (rigSlot == null || cameraObject == null || systems == null)
            {
                MagicMenuUtil.Log("Run Magic > Level > Create Level Scene Skeleton first.");
                return;
            }

            var ball = Object.FindAnyObjectByType<GolfBall>(FindObjectsInactive.Include);
            var shot = Object.FindAnyObjectByType<ShotController>(FindObjectsInactive.Include);
            var avatar = Object.FindAnyObjectByType<GolferAvatar>(FindObjectsInactive.Include);
            var input = AssetDatabase.LoadAssetAtPath<GolfInput>(GolfInputAssetMenu.InputPath);
            if (ball == null || shot == null || avatar == null || input == null)
            {
                MagicMenuUtil.Log("Run Magic > Level > Add Ball, Shot and Avatar first.");
                return;
            }

            Undo.SetCurrentGroupName("Add Camera Rig and Explore Mode");
            var undoGroup = Undo.GetCurrentGroup();
            var changes = 0;

            // 1. The TEMP(#19) stand-in camera goes; the CameraRig moves the camera from now on.
            var temporary = cameraObject.GetComponent<OrbitFollowCamera>();
            if (temporary != null)
            {
                Undo.DestroyObjectImmediate(temporary);
                MagicMenuUtil.Log("Removed TEMP(#19) OrbitFollowCamera from Cameras/Main Camera");
                changes++;
            }

            // 2. The walking player: a CharacterController the size of the avatar's model, plus the walker.
            var height = 1f;
            var controller = avatar.GetComponent<CharacterController>();
            if (controller == null)
            {
                controller = Undo.AddComponent<CharacterController>(avatar.gameObject);
                height = FitToModel(controller, avatar.transform);
                MagicMenuUtil.Log($"Added CharacterController to {avatar.name} (height {height:0.00} m, radius {controller.radius:0.00} m)");
                changes++;
            }
            else
            {
                height = controller.height;
            }

            var walker = avatar.GetComponent<CharacterWalker>();
            if (walker == null)
            {
                walker = Undo.AddComponent<CharacterWalker>(avatar.gameObject);
                // Both start off: PlayerModeController turns the walker (and with it the controller) on once the
                // avatar stands at the tee. A collider that is on too early would trip up the avatar's ground snap.
                Undo.RecordObjects(new Object[] { walker, controller }, "Start the walker off");
                walker.enabled = false;
                controller.enabled = false;
                MagicMenuUtil.Log($"Added CharacterWalker to {avatar.name} (starts off; PlayerModes turns it on)");
                changes++;
            }

            // 3. The camera rig on its slot.
            var rig = rigSlot.GetComponent<CameraRig>();
            if (rig == null)
            {
                rig = Undo.AddComponent<CameraRig>(rigSlot);
                var so = new SerializedObject(rig);
                so.FindProperty("targetCamera").objectReferenceValue = cameraObject.GetComponent<Camera>();
                so.FindProperty("input").objectReferenceValue = input;
                so.FindProperty("ball").objectReferenceValue = ball;
                so.FindProperty("shot").objectReferenceValue = shot;
                so.FindProperty("player").objectReferenceValue = avatar.transform;
                so.FindProperty("startView").enumValueIndex = (int)CameraView.ThirdPerson;
                so.FindProperty("eyeHeight").floatValue = height * 0.3f;            // low, at the hamster's eye level
                so.FindProperty("playerFocusHeight").floatValue = height * 0.7f;
                so.ApplyModifiedPropertiesWithoutUndo();
                MagicMenuUtil.Log("Added CameraRig to Cameras/CameraRig");
                changes++;
            }

            // 4. The state machine that switches between walking, shooting and watching the ball.
            if (Object.FindAnyObjectByType<PlayerModeController>(FindObjectsInactive.Include) == null)
            {
                var go = new GameObject(ModesName);
                Undo.RegisterCreatedObjectUndo(go, "Add " + ModesName);
                go.transform.SetParent(systems.transform, false);
                var modes = go.AddComponent<PlayerModeController>();
                var so = new SerializedObject(modes);
                so.FindProperty("input").objectReferenceValue = input;
                so.FindProperty("shot").objectReferenceValue = shot;
                so.FindProperty("ball").objectReferenceValue = ball;
                so.FindProperty("avatar").objectReferenceValue = avatar;
                so.FindProperty("walker").objectReferenceValue = walker;
                so.FindProperty("cameraRig").objectReferenceValue = rig;
                so.FindProperty("exit").objectReferenceValue = Object.FindAnyObjectByType<LevelExit>(FindObjectsInactive.Include);
                var shootingOnly = ShootingOnlyObjects();
                var list = so.FindProperty("shootingOnly");
                list.arraySize = shootingOnly.Count;
                for (var i = 0; i < shootingOnly.Count; i++)
                    list.GetArrayElementAtIndex(i).objectReferenceValue = shootingOnly[i];
                so.ApplyModifiedPropertiesWithoutUndo();
                MagicMenuUtil.Log($"Added Systems/{ModesName} (shown only while shooting: {shootingOnly.Count} objects)");
                changes++;
            }

            // 5. The TEMP(#27) wiring must stop teleporting the avatar to the ball.
            var wiring = Object.FindAnyObjectByType<GolferAvatarTestWiring>(FindObjectsInactive.Include);
            if (wiring != null)
            {
                var so = new SerializedObject(wiring);
                var teleport = so.FindProperty("teleportToBall");
                if (teleport.boolValue)
                {
                    Undo.RecordObject(wiring, "Turn off Teleport To Ball");
                    teleport.boolValue = false;
                    so.ApplyModifiedProperties();
                    MagicMenuUtil.Log($"Turned off Teleport To Ball on Systems/{LevelPlayerSlotsMenu.WiringName}");
                    changes++;
                }
            }

            Undo.CollapseUndoOperations(undoGroup);
            if (changes > 0)
                EditorSceneManager.MarkSceneDirty(scene);
            MagicMenuUtil.Log($"Camera rig and explore mode in {scene.name}: {changes} changes. Press Play to try it.");
        }

        // Sizes the capsule to the model's renderers. The avatar's origin is at its feet, so the centre is half-height up.
        static float FitToModel(CharacterController controller, Transform avatar)
        {
            var renderers = avatar.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
                return controller.height;
            var bounds = renderers[0].bounds;
            foreach (var renderer in renderers)
                bounds.Encapsulate(renderer.bounds);

            var height = Mathf.Max(0.2f, bounds.max.y - avatar.position.y);
            var radius = Mathf.Clamp(Mathf.Min(bounds.size.x, bounds.size.z) * 0.5f, 0.05f, height * 0.5f);
            controller.height = height;
            controller.radius = radius;
            controller.center = new Vector3(0f, height * 0.5f, 0f);
            controller.stepOffset = Mathf.Min(0.3f, height * 0.4f);   // must stay below the height
            controller.skinWidth = Mathf.Max(0.005f, radius * 0.1f);
            controller.minMoveDistance = 0f;
            return height;
        }

        // The aim cone and the power meter (only the meter itself, never a whole HUD canvas).
        static List<GameObject> ShootingOnlyObjects()
        {
            var result = new List<GameObject>();
            var cone = Object.FindAnyObjectByType<AimCone>(FindObjectsInactive.Include);
            if (cone != null)
                result.Add(cone.gameObject);
            var meter = Object.FindAnyObjectByType<PowerMeterView>(FindObjectsInactive.Include);
            if (meter != null && meter.GetComponent<Canvas>() == null)
                result.Add(meter.gameObject);
            return result;
        }
    }
}
