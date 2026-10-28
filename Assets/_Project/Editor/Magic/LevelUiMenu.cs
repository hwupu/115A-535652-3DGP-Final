using Project3DGP.Game.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Project3DGP.EditorTools
{
    /// <summary>
    /// Magic > Level > Add HUD and Menus (#20, architecture §4.8): the shared level UI, for every level scene.
    ///   UI/HUD         HudView: time bar + stage icon, strokes, menu button (on the power meter's canvas if it exists)
    ///   UI/Menus       LevelMenus: pause, save confirm, settings and time-up dialogs
    ///   UI/EventSystem if the scene has none (Input System UI module)
    /// Both connect to the level's ILevelFlow (LevelManager, #27) at Start; until it is in the scene the timer and
    /// strokes stay hidden and the menu does nothing (try it in Magic > Game > Open UI Test Area).
    /// Only what is missing; one Undo step; the scene is marked dirty, not saved.
    /// </summary>
    public static class LevelUiMenu
    {
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
    }
}
