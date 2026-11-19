using Project3DGP.Game.Flow;
using Project3DGP.Game.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Project3DGP.EditorTools
{
    /// <summary>
    /// Magic > Scenes > Apply Game Title: in the open MainMenu scene, sets the "Title" text of the main menu canvas to
    /// the game's name (GameUiBuilder.GameTitle, which Add Main Menu UI also uses). Only the text changes; font,
    /// size and theme stay. One Undo step; the scene is marked dirty, not saved.
    /// </summary>
    public static class GameTitleMenu
    {
        [MenuItem(MagicMenuUtil.Root + "Scenes/Apply Game Title", priority = MagicMenuUtil.ScenesPriority + 5)]
        static void Run()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.name != GameScenes.MainMenu)
            {
                MagicMenuUtil.Log($"Open Scenes/{GameScenes.MainMenu} first (the open scene is {scene.name}).");
                return;
            }
            var view = Object.FindAnyObjectByType<MainMenuView>(FindObjectsInactive.Include);
            var canvas = view != null ? view.GetComponentInParent<Canvas>(true) : null;
            var title = canvas != null ? canvas.transform.Find("Title") : null;
            if (title == null || !title.TryGetComponent(out Text text))
            {
                MagicMenuUtil.Log("No main menu Title text: run Magic > Scenes > Add Main Menu UI (it uses the game title).");
                return;
            }
            if (text.text == GameUiBuilder.GameTitle)
            {
                MagicMenuUtil.Log($"Skipped: the title is already \"{GameUiBuilder.GameTitle}\"");
                return;
            }
            Undo.RecordObject(text, "Apply Game Title");
            var old = text.text;
            text.text = GameUiBuilder.GameTitle;
            EditorSceneManager.MarkSceneDirty(scene);
            MagicMenuUtil.Log($"Main menu title: \"{old}\" -> \"{GameUiBuilder.GameTitle}\"");
        }
    }
}
