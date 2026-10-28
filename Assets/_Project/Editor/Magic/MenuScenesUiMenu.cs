using Project3DGP.Game.Flow;
using Project3DGP.Game.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Project3DGP.EditorTools
{
    /// <summary>
    /// Magic > Scenes > Add Main Menu UI / Add Ending UI (#30, architecture §4.8): fills the open MainMenu or Ending
    /// scene with its placeholder screen (GameUiBuilder), the story intro in the main menu (#51, placeholder text in
    /// the StoryPages Inspector), an EventSystem and the GameSession if missing.
    /// Only what is missing; one Undo step; the scene is marked dirty, not saved.
    /// </summary>
    public static class MenuScenesUiMenu
    {
        [MenuItem(MagicMenuUtil.Root + "Scenes/Add Main Menu UI", priority = MagicMenuUtil.ScenesPriority + 3)]
        static void AddMainMenu() => Fill(GameScenes.MainMenu, () =>
            Object.FindAnyObjectByType<MainMenuView>(FindObjectsInactive.Include) == null
                ? GameUiBuilder.BuildMainMenu().gameObject : null,
            () => AddStory<MainMenuView>("intro", "Intro", GameUiBuilder.IntroPages, new Color(0.2f, 0.3f, 0.25f)));

        [MenuItem(MagicMenuUtil.Root + "Scenes/Add Ending UI", priority = MagicMenuUtil.ScenesPriority + 4)]
        static void AddEnding() => Fill(GameScenes.Ending, () =>
            Object.FindAnyObjectByType<EndingView>(FindObjectsInactive.Include) == null
                ? GameUiBuilder.BuildEnding().gameObject : null);

        // The story panel on the view's canvas, wired into the view's field, if that field is still empty.
        static GameObject AddStory<TView>(string field, string name, string[] pages, Color background) where TView : MonoBehaviour
        {
            var view = Object.FindAnyObjectByType<TView>(FindObjectsInactive.Include);
            var so = new SerializedObject(view);
            var property = so.FindProperty(field);
            if (property.objectReferenceValue != null)
                return null;
            var story = GameUiBuilder.BuildStoryPages(view.GetComponentInParent<Canvas>().gameObject, name, pages, background);
            property.objectReferenceValue = story;
            so.ApplyModifiedProperties();   // recorded for Undo
            return story.gameObject;
        }

        static void Fill(string sceneName, System.Func<GameObject> build, System.Func<GameObject> addStory = null)
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
            var story = addStory?.Invoke();
            if (story != null)
            {
                Undo.RegisterCreatedObjectUndo(story, "Add " + story.name);
                MagicMenuUtil.Log($"Added the story {story.name.ToLower()} (placeholder pages: edit them on {story.name} > StoryPages)");
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
