using Project3DGP.Game.Controls;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Project3DGP.EditorTools
{
    /// <summary>
    /// Magic > Game > Open Input Test Scene (#13): opens a new, UNSAVED scene with a GolfInputDebugOverlay,
    /// to check the bindings in Play mode. Nothing is saved or added to the project; close it without saving.
    /// </summary>
    public static class GolfInputTestSceneMenu
    {
        [MenuItem(MagicMenuUtil.Root + "Game/Open Input Test Scene", priority = MagicMenuUtil.GamePriority + 1)]
        static void Open()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                MagicMenuUtil.Log("Cancelled: open scenes were not saved.");
                return;
            }

            if (GolfInputAssetMenu.Create() == null)
                return;

            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            // Load AFTER NewScene: opening a scene unloads unused assets and would empty an earlier reference.
            var input = AssetDatabase.LoadAssetAtPath<GolfInput>(GolfInputAssetMenu.InputPath);
            var overlay = new GameObject("GolfInputDebug").AddComponent<GolfInputDebugOverlay>();
            var so = new SerializedObject(overlay);
            so.FindProperty("input").objectReferenceValue = input;
            so.ApplyModifiedPropertiesWithoutUndo();
            Selection.activeGameObject = overlay.gameObject;
            MagicMenuUtil.Log("Opened an unsaved input test scene. Press Play; close it without saving.");
        }
    }
}
