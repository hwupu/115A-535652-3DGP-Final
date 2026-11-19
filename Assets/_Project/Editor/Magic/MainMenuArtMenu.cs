using Project3DGP.Game.Flow;
using Project3DGP.Game.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Project3DGP.EditorTools
{
    /// <summary>
    /// Magic > Scenes > Apply Main Menu Art: in the open MainMenu scene, puts the painted garden (UI/MainMenu/
    /// MainMenu_Background.jpg) on the menu's Background and adds the lettered title picture (UI/MainMenu/Title.png)
    /// over the Title text, which is switched off (GameUiBuilder.ApplyMainMenuArt; Add Main Menu UI does the same).
    /// Buttons, settings and the intro stay. One Undo step; the scene is marked dirty, not saved.
    /// </summary>
    public static class MainMenuArtMenu
    {
        [MenuItem(MagicMenuUtil.Root + "Scenes/Apply Main Menu Art", priority = MagicMenuUtil.ScenesPriority + 6)]
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
            if (canvas == null)
            {
                MagicMenuUtil.Log("No main menu canvas: run Magic > Scenes > Add Main Menu UI (it already uses the art).");
                return;
            }

            Undo.SetCurrentGroupName("Apply Main Menu Art");
            var undoGroup = Undo.GetCurrentGroup();
            var changed = GameUiBuilder.ApplyMainMenuArt(canvas.transform);
            Undo.CollapseUndoOperations(undoGroup);
            if (changed == 0)
            {
                MagicMenuUtil.Log("Skipped: the main menu already has its background and title picture");
                return;
            }
            EditorSceneManager.MarkSceneDirty(scene);
        }
    }
}
