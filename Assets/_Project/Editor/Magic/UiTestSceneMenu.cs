using Project3DGP.Game.Session;
using Project3DGP.Game.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Project3DGP.EditorTools
{
    /// <summary>
    /// Magic > Game > Open UI Test Area (#20): the HUD and the level dialogs on a stand-in level (UiTestLevelFlow:
    /// the timer runs down, S = shot, F = time up, Esc / Menu = pause), plus a GameSession with an in-memory save
    /// slot (UiTestSaveStore), so pause, save + overwrite, settings, retry and time-up can be tried before
    /// LevelManager, DangerManager, ScoreKeeper and Save &amp; Load exist. New, UNSAVED scene; close it without saving.
    /// </summary>
    public static class UiTestSceneMenu
    {
        [MenuItem(MagicMenuUtil.Root + "Game/Open UI Test Area", priority = MagicMenuUtil.GamePriority + 15)]
        static void Open()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                MagicMenuUtil.Log("Cancelled: open scenes were not saved.");
                return;
            }
            if (GameSessionMenu.CreatePrefab() == null)
                return;

            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(GameSessionMenu.PrefabPath);   // after NewScene
            var session = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            session.AddComponent<UiTestSaveStore>();

            var flow = new GameObject("UiTestLevelFlow").AddComponent<UiTestLevelFlow>();
            GameUiBuilder.EnsureEventSystem(null);
            GameUiBuilder.BuildHud(GameUiBuilder.NewCanvas("HUD", 0), flow);
            GameUiBuilder.BuildMenus(flow);

            MagicMenuUtil.Log("Opened an unsaved UI test area. Press Play: S = shot, F = time up, Esc or Menu = pause. Close it without saving.");
        }
    }
}
