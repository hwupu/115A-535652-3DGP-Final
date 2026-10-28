using Project3DGP.Core.Cameras;
using Project3DGP.Core.UI;
using Project3DGP.Game.Golf;
using Project3DGP.Game.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Project3DGP.EditorTools
{
    /// <summary>
    /// Magic > Level > Add HUD and Menus (#20, architecture §4.8): the shared level UI, for every level scene.
    ///   UI/HUD         HudView: time bar + stage icon, strokes, menu button (on the power meter's canvas if it exists)
    ///                  DialogBar: tutorial hints and story lines at the bottom (#28, #51)
    ///                  XpBar: level + XP under the strokes (#78, reads GameSession), LevelUpNotice in the dialog bar (#70)
    ///                  ClubPanel: the usable clubs on the right, current one highlighted + sound (#78)
    ///                  Minimap: upper right (#78), from Cameras/Minimap Camera following the ball, with icons for
    ///                  ball, avatar, cup, exit and each object under Gameplay/Hazards (run again after adding hazards)
    ///   UI/Menus       LevelMenus: pause, save confirm, settings and time-up dialogs
    ///   UI/EventSystem if the scene has none (Input System UI module)
    /// Both connect to the level's ILevelFlow (LevelManager, #27) at Start; until it is in the scene the timer and
    /// strokes stay hidden and the menu does nothing (try it in Magic > Game > Open UI Test Area).
    /// Only what is missing; one Undo step; the scene is marked dirty, not saved.
    /// </summary>
    public static class LevelUiMenu
    {
        const string SelectSoundPath = "Assets/ThirdParty/Freesound/sfx_platform_leave_663859.mp3";   // CC0, docs/assets.md

        [MenuItem(MagicMenuUtil.Root + "Level/Add HUD and Menus", priority = MagicMenuUtil.LevelPriority + 5)]
        static void Add()
        {
            var scene = EditorSceneManager.GetActiveScene();
            var ui = LevelSkeleton.Find(scene, "UI");
            if (ui == null)
            {
                MagicMenuUtil.Log("Run Magic > Level > Create Level Scene Skeleton first.");
                return;
            }

            Undo.SetCurrentGroupName("Add HUD and Menus");
            var undoGroup = Undo.GetCurrentGroup();
            var added = 0;

            if (Object.FindAnyObjectByType<HudView>(FindObjectsInactive.Include) == null)
            {
                // On the power meter's canvas (Add Ball, Shot and Avatar) if there is one, else a new UI/HUD canvas.
                var canvas = LevelSkeleton.Find(scene, "UI/HUD");
                var isNew = canvas == null || canvas.GetComponent<Canvas>() == null;
                if (isNew)
                {
                    canvas = GameUiBuilder.NewCanvas("HUD", 0);
                    canvas.transform.SetParent(ui.transform, false);
                }
                var hud = GameUiBuilder.BuildHud(canvas, null);
                Undo.RegisterCreatedObjectUndo(isNew ? canvas : hud.gameObject, "Add HUD");
                MagicMenuUtil.Log($"Added the HUD (time bar, stage icon, strokes, menu button) to UI/{canvas.name}");
                added++;
            }

            if (Object.FindAnyObjectByType<DialogBar>(FindObjectsInactive.Include) == null)
            {
                var hud = Object.FindAnyObjectByType<HudView>(FindObjectsInactive.Include);
                var bar = GameUiBuilder.BuildDialogBar(hud.GetComponentInParent<Canvas>().gameObject);
                Undo.RegisterCreatedObjectUndo(bar.gameObject, "Add DialogBar");
                MagicMenuUtil.Log("Added the dialog bar (tutorial hints, story lines) to the HUD");
                added++;
            }

            var hudCanvas = Object.FindAnyObjectByType<HudView>(FindObjectsInactive.Include).GetComponentInParent<Canvas>().gameObject;
            if (Object.FindAnyObjectByType<XpBarView>(FindObjectsInactive.Include) == null)
            {
                var xp = GameUiBuilder.BuildXpBar(hudCanvas);
                Undo.RegisterCreatedObjectUndo(xp.gameObject, "Add XP Bar");
                MagicMenuUtil.Log("Added the XP bar (level + XP) to the HUD");
                added++;
            }

            if (Object.FindAnyObjectByType<LevelUpNotice>(FindObjectsInactive.Include) == null)
            {
                var xpBar = Object.FindAnyObjectByType<XpBarView>(FindObjectsInactive.Include);
                Undo.AddComponent<LevelUpNotice>(xpBar.gameObject);
                MagicMenuUtil.Log("Added LevelUpNotice to the XP bar (\"Level 3! New: ...\" in the dialog bar)");
                added++;
            }

            if (Object.FindAnyObjectByType<ClubPanelView>(FindObjectsInactive.Include) == null)
            {
                var clubs = GameUiBuilder.BuildClubPanel(hudCanvas, ClubProfilesMenu.AllPaths.Length,
                    AssetDatabase.LoadAssetAtPath<AudioClip>(SelectSoundPath));
                Undo.RegisterCreatedObjectUndo(clubs.gameObject, "Add Club Panel");
                MagicMenuUtil.Log("Added the club panel (one card per usable club) to the HUD");
                added++;
            }

            added += AddMinimap(scene, hudCanvas);

            if (Object.FindAnyObjectByType<LevelMenus>(FindObjectsInactive.Include) == null)
            {
                var menus = GameUiBuilder.BuildMenus(null);
                menus.transform.SetParent(ui.transform, false);
                Undo.RegisterCreatedObjectUndo(menus.gameObject, "Add Menus");
                MagicMenuUtil.Log("Added UI/Menus (pause, save confirm, settings, time-up dialogs)");
                added++;
            }

            var eventSystem = GameUiBuilder.EnsureEventSystem(ui.transform);
            if (eventSystem != null)
            {
                Undo.RegisterCreatedObjectUndo(eventSystem, "Add EventSystem");
                MagicMenuUtil.Log("Added UI/EventSystem");
                added++;
            }

            Undo.CollapseUndoOperations(undoGroup);
            if (added > 0)
                EditorSceneManager.MarkSceneDirty(scene);
            MagicMenuUtil.Log($"HUD and menus in {scene.name}: {added} added. They connect to LevelManager (#27) once it is in the scene. Save the scene.");
        }

        // Minimap camera + HUD frame once the ball is in (Add Ball, Shot and Avatar); icons for whatever is missing one.
        static int AddMinimap(UnityEngine.SceneManagement.Scene scene, GameObject hudCanvas)
        {
            var ball = Object.FindAnyObjectByType<GolfBall>(FindObjectsInactive.Include);
            if (ball == null)
            {
                MagicMenuUtil.Log("No GolfBall yet: run Magic > Level > Add Ball, Shot and Avatar, then this again for the minimap.");
                return 0;
            }
            var layer = LevelMinimap.Layer;
            if (layer < 0)
                return 0;

            var added = 0;
            var texture = LevelMinimap.EnsureTexture();
            var minimap = Object.FindAnyObjectByType<MinimapCamera>(FindObjectsInactive.Include);
            var camera = minimap != null ? minimap.GetComponent<Camera>() : null;
            if (minimap == null)
            {
                camera = LevelMinimap.AddCamera(scene, ball.transform, texture);
                added++;
            }
            LevelMinimap.HideLayerFromOtherCameras(layer, camera);

            if (hudCanvas.transform.Find("Minimap") == null)
            {
                var frame = GameUiBuilder.BuildMinimapFrame(hudCanvas, texture);
                Undo.RegisterCreatedObjectUndo(frame, "Add Minimap Frame");
                MagicMenuUtil.Log("Added the minimap frame (upper right) to the HUD");
                added++;
            }
            return added + LevelMinimap.AddLevelIcons(scene, layer);
        }
    }
}
