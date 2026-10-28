using Project3DGP.Core.Cameras;
using Project3DGP.Game.Controls;
using Project3DGP.Game.Golf;
using Project3DGP.Game.Player;
using Project3DGP.Game.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project3DGP.EditorTools
{
    /// <summary>
    /// Magic > Level > Add Ball, Shot and Avatar (#95, first half): fills Henry's slots of the active level scene
    /// with the shared prefabs, so the level can be played shot by shot:
    ///   Systems/ShotController   ShotController (input, ball, 3 clubs)
    ///   Gameplay/Player          GolfBall on BallSpawn, GolferAvatar, AimCone
    ///   UI                       HUD canvas with the power meter
    /// Plus two stand-ins until the owners' code exists (delete them then):
    ///   TEMP(#19) OrbitFollowCamera on Cameras/Main Camera, following the ball   -> CameraRig (Bahadir)
    ///   TEMP(#27) Systems/AvatarWiring_TEMP (GolferAvatarTestWiring)            -> LevelManager (Pongpat)
    /// Each step runs only if its component is missing; nothing existing is moved or changed. One Undo step.
    /// The scene is marked dirty, not saved. Also adds InvalidZone to the Pond prefab (once).
    /// </summary>
    public static class LevelPlayerSlotsMenu
    {
        public const string WiringName = "AvatarWiring_TEMP";

        [MenuItem(MagicMenuUtil.Root + "Level/Add Ball, Shot and Avatar", priority = MagicMenuUtil.LevelPriority + 3)]
        static void Fill()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (LevelSkeleton.Find(scene, "Gameplay/Player") == null || LevelSkeleton.Find(scene, "Systems/ShotController") == null)
            {
                MagicMenuUtil.Log("Run Magic > Level > Create Level Scene Skeleton first.");
                return;
            }

            // Shared assets first (each is created only if missing).
            ClubProfilesMenu.CreateAll();
            if (GolfInputAssetMenu.Create() == null || GolfBallPrefabMenu.Create() == null || GolferAvatarPrefabMenu.Create() == null)
                return;
            PondInvalidZone.Ensure();

            Undo.SetCurrentGroupName("Add Ball, Shot and Avatar");
            var undoGroup = Undo.GetCurrentGroup();
            var added = 0;

            var ball = Object.FindAnyObjectByType<GolfBall>(FindObjectsInactive.Include);
            if (ball == null)
            {
                ball = AddBall(scene);
                added++;
            }

            var shot = Object.FindAnyObjectByType<ShotController>(FindObjectsInactive.Include);
            if (shot == null)
            {
                shot = AddShotController(scene, ball);
                added++;
            }

            if (Object.FindAnyObjectByType<AimCone>(FindObjectsInactive.Include) == null)
            {
                AddAimCone(scene, shot, ball);
                added++;
            }

            var avatar = Object.FindAnyObjectByType<GolferAvatar>(FindObjectsInactive.Include);
            if (avatar == null)
            {
                avatar = AddAvatar(scene, ball);
                added++;
            }

            if (Object.FindAnyObjectByType<PowerMeterView>(FindObjectsInactive.Include) == null)
            {
                AddHud(scene, shot);
                added++;
            }

            if (AddTemporaryCamera(scene, ball))
                added++;
            if (AddTemporaryWiring(scene, shot, ball, avatar))
                added++;

            Undo.CollapseUndoOperations(undoGroup);
            if (added > 0)
                EditorSceneManager.MarkSceneDirty(scene);
            MagicMenuUtil.Log($"Ball, shot and avatar in {scene.name}: {added} added. Press Play to try it, then save the scene.");
        }

        // The ball sits on the tee (BallSpawn); its prefab keeps physics and the ball material.
        static GolfBall AddBall(Scene scene)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(GolfBallPrefabMenu.PrefabPath);
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            go.transform.SetParent(LevelSkeleton.Find(scene, "Gameplay/Player").transform, false);
            var spawn = LevelSkeleton.Find(scene, "Gameplay/BallSpawn");
            if (spawn != null)
                go.transform.position = spawn.transform.position;
            Undo.RegisterCreatedObjectUndo(go, "Add GolfBall");
            MagicMenuUtil.Log("Added Gameplay/Player/GolfBall on the tee " + go.transform.position);
            return go.GetComponent<GolfBall>();
        }

        // On the existing empty slot. All 3 clubs for testing; which clubs are unlocked comes later (#70, #89).
        static ShotController AddShotController(Scene scene, GolfBall ball)
        {
            var slot = LevelSkeleton.Find(scene, "Systems/ShotController");
            var shot = Undo.AddComponent<ShotController>(slot);
            var so = new SerializedObject(shot);
            so.FindProperty("input").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GolfInput>(GolfInputAssetMenu.InputPath);
            so.FindProperty("ball").objectReferenceValue = ball;
            var clubs = so.FindProperty("clubs");
            clubs.arraySize = ClubProfilesMenu.AllPaths.Length;
            for (var i = 0; i < ClubProfilesMenu.AllPaths.Length; i++)
                clubs.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<ClubProfile>(ClubProfilesMenu.AllPaths[i]);
            so.ApplyModifiedPropertiesWithoutUndo();
            MagicMenuUtil.Log("Added ShotController to Systems/ShotController (3 clubs)");
            return shot;
        }

        static void AddAimCone(Scene scene, ShotController shot, GolfBall ball)
        {
            var go = new GameObject("AimCone");
            SceneManager.MoveGameObjectToScene(go, scene);
            go.transform.SetParent(LevelSkeleton.Find(scene, "Gameplay/Player").transform, false);
            var line = go.AddComponent<LineRenderer>();
            line.widthMultiplier = 0.04f;
            line.sharedMaterial = PlaceholderMaterials.White;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            var cone = go.AddComponent<AimCone>();
            var so = new SerializedObject(cone);
            so.FindProperty("shot").objectReferenceValue = shot;
            so.FindProperty("ball").objectReferenceValue = ball;
            so.ApplyModifiedPropertiesWithoutUndo();
            Undo.RegisterCreatedObjectUndo(go, "Add AimCone");
            MagicMenuUtil.Log("Added Gameplay/Player/AimCone");
        }

        // Placed behind the ball; at Play it teleports next to the ball and faces the aim anyway.
        static GolferAvatar AddAvatar(Scene scene, GolfBall ball)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(GolferAvatarPrefabMenu.AvatarPath);
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            go.transform.SetParent(LevelSkeleton.Find(scene, "Gameplay/Player").transform, false);
            go.transform.position = ball.transform.position + new Vector3(-0.45f, -ball.transform.position.y, -0.75f);
            Undo.RegisterCreatedObjectUndo(go, "Add GolferAvatar");
            MagicMenuUtil.Log("Added Gameplay/Player/GolferAvatar");
            return go.GetComponent<GolferAvatar>();
        }

        // The HUD canvas (#20 adds the timer and strokes later); for now only the power meter.
        static void AddHud(Scene scene, ShotController shot)
        {
            var canvas = ShotTestSceneMenu.BuildPowerMeter(shot);
            canvas.name = "HUD";
            SceneManager.MoveGameObjectToScene(canvas, scene);
            canvas.transform.SetParent(LevelSkeleton.Find(scene, "UI").transform, false);
            Undo.RegisterCreatedObjectUndo(canvas, "Add HUD");
            MagicMenuUtil.Log("Added UI/HUD with the power meter");
        }

        // TEMP(#19): until CameraRig exists, the main camera orbits the ball (right-drag) like the test area.
        // Skipped if the CameraRig slot already has a component (Bahadir's rig is in).
        static bool AddTemporaryCamera(Scene scene, GolfBall ball)
        {
            var rig = LevelSkeleton.Find(scene, "Cameras/CameraRig");
            if (rig != null && rig.GetComponents<Component>().Length > 1)
                return false;
            var cameraObject = LevelSkeleton.Find(scene, "Cameras/Main Camera");
            if (cameraObject == null || cameraObject.GetComponent<OrbitFollowCamera>() != null)
                return false;

            var orbit = Undo.AddComponent<OrbitFollowCamera>(cameraObject);
            var so = new SerializedObject(orbit);
            so.FindProperty("target").objectReferenceValue = ball.transform;
            so.FindProperty("distance").floatValue = 8f;
            so.FindProperty("pitch").floatValue = 25f;
            so.FindProperty("zoomSensitivity").floatValue = 0f;   // the wheel switches clubs
            so.ApplyModifiedPropertiesWithoutUndo();
            MagicMenuUtil.Log("Added TEMP(#19) OrbitFollowCamera to Cameras/Main Camera. Remove it when CameraRig is in.");
            return true;
        }

        // TEMP(#27): until LevelManager exists, the test wiring connects ball, shot and avatar.
        static bool AddTemporaryWiring(Scene scene, ShotController shot, GolfBall ball, GolferAvatar avatar)
        {
            if (Object.FindAnyObjectByType<GolferAvatarTestWiring>(FindObjectsInactive.Include) != null)
                return false;
            var go = new GameObject(WiringName);
            SceneManager.MoveGameObjectToScene(go, scene);
            go.transform.SetParent(LevelSkeleton.Find(scene, "Systems").transform, false);
            var wiring = go.AddComponent<GolferAvatarTestWiring>();
            var so = new SerializedObject(wiring);
            so.FindProperty("shot").objectReferenceValue = shot;
            so.FindProperty("ball").objectReferenceValue = ball;
            so.FindProperty("avatar").objectReferenceValue = avatar;
            so.ApplyModifiedPropertiesWithoutUndo();
            Undo.RegisterCreatedObjectUndo(go, "Add " + WiringName);
            MagicMenuUtil.Log($"Added TEMP(#27) Systems/{WiringName}. Delete it when LevelManager is in.");
            return true;
        }
    }
}
