using Project3DGP.Core.UI;
using Project3DGP.Game.Golf;
using Project3DGP.Game.Levels.Garden;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project3DGP.EditorTools
{
    /// <summary>
    /// Magic > Level > Garden: Add Tutorial (#28): in L1_Garden, adds Systems/GardenTutorial (hints in the HUD's
    /// dialog bar), wired to the shot controller, the ball and the dialog bar. Needs Add Ball, Shot and Avatar and
    /// Add HUD and Menus first (the dialog bar comes with the HUD). Only what is missing; one Undo step; not saved.
    /// The hint texts are in the Inspector (GardenTutorial > Texts).
    /// </summary>
    public static class GardenTutorialMenu
    {
        const string SceneName = "L1_Garden";

        [MenuItem(MagicMenuUtil.Root + "Level/Garden: Add Tutorial", priority = MagicMenuUtil.LevelPriority + 22)]
        static void Add()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.name != SceneName)
            {
                MagicMenuUtil.Log($"Open {SceneName} first (the tutorial is Level 1's).");
                return;
            }
            if (Object.FindAnyObjectByType<GardenTutorial>(FindObjectsInactive.Include) != null)
            {
                MagicMenuUtil.Log("Skipped: L1_Garden already has a GardenTutorial.");
                return;
            }
            var dialog = Object.FindAnyObjectByType<DialogBar>(FindObjectsInactive.Include);
            var shot = Object.FindAnyObjectByType<ShotController>(FindObjectsInactive.Include);
            var ball = Object.FindAnyObjectByType<GolfBall>(FindObjectsInactive.Include);
            if (dialog == null || shot == null || ball == null)
            {
                MagicMenuUtil.Log("Run Magic > Level > Add Ball, Shot and Avatar and Add HUD and Menus first (no "
                    + (dialog == null ? "dialog bar" : shot == null ? "ShotController" : "GolfBall") + " in the scene).");
                return;
            }

            var go = new GameObject("GardenTutorial");
            SceneManager.MoveGameObjectToScene(go, scene);
            go.transform.SetParent(LevelSkeleton.Find(scene, "Systems").transform, false);
            var tutorial = go.AddComponent<GardenTutorial>();
            var so = new SerializedObject(tutorial);
            so.FindProperty("dialog").objectReferenceValue = dialog;
            so.FindProperty("shot").objectReferenceValue = shot;
            so.FindProperty("ball").objectReferenceValue = ball;
            so.ApplyModifiedPropertiesWithoutUndo();
            Undo.RegisterCreatedObjectUndo(go, "Add GardenTutorial");
            EditorSceneManager.MarkSceneDirty(scene);
            MagicMenuUtil.Log("Added Systems/GardenTutorial. Press Play to see the hints, then save the scene.");
        }
    }
}
