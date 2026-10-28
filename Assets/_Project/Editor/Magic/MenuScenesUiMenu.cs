using Project3DGP.Game.Flow;
using Project3DGP.Game.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Project3DGP.EditorTools
{
    /// <summary>
    /// Magic > Scenes > Add Main Menu UI / Add Ending UI (#30, architecture §4.8): fills the open MainMenu or Ending
    /// scene with its placeholder screen (GameUiBuilder), an EventSystem and the GameSession if missing.
    /// Only what is missing; one Undo step; the scene is marked dirty, not saved.
    /// </summary>
    public static class MenuScenesUiMenu
    {
        [MenuItem(MagicMenuUtil.Root + "Scenes/Add Main Menu UI", priority = MagicMenuUtil.ScenesPriority + 3)]
        static void AddMainMenu() => Fill(GameScenes.MainMenu, () =>
            Object.FindAnyObjectByType<MainMenuView>(FindObjectsInactive.Include) == null
                ? GameUiBuilder.BuildMainMenu().gameObject : null);

        [MenuItem(MagicMenuUtil.Root + "Scenes/Add Ending UI", priority = MagicMenuUtil.ScenesPriority + 4)]
        static void AddEnding() => Fill(GameScenes.Ending, () =>
            Object.FindAnyObjectByType<EndingView>(FindObjectsInactive.Include) == null
                ? GameUiBuilder.BuildEnding().gameObject : null);

        static void Fill(string sceneName, System.Func<GameObject> build)
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.name != sceneName)
            {
                MagicMenuUtil.Log($"Open Scenes/{sceneName} first (the open scene is {scene.name}).");
                return;
            }

            Undo.SetCurrentGroupName("Add " + sceneName + " UI");
            var undoGroup = Undo.GetCurrentGroup();
            var added = 0;

            var screen = build();
            if (screen != null)
            {
                Undo.RegisterCreatedObjectUndo(screen, "Add " + screen.name);
                MagicMenuUtil.Log($"Added the {screen.name} screen");
                added++;
            }
            var eventSystem = GameUiBuilder.EnsureEventSystem(null);
            if (eventSystem != null)
            {
                Undo.RegisterCreatedObjectUndo(eventSystem, "Add EventSystem");
                MagicMenuUtil.Log("Added EventSystem");
                added++;
            }
            if (GameSessionMenu.AddToScene(scene))
                added++;

            Undo.CollapseUndoOperations(undoGroup);
            if (added > 0)
                EditorSceneManager.MarkSceneDirty(scene);
            MagicMenuUtil.Log($"{sceneName}: {added} added. Save the scene.");
        }
    }
}
