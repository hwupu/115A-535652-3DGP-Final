using Project3DGP.Game.Golf;
using Project3DGP.Game.Levels.Garden;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project3DGP.EditorTools
{
    /// <summary>
    /// Magic > Level > Garden: Set Up Cat (#21): in L1_Garden, adds GardenCat to Gameplay/Hazards/Cat (looking at
    /// the ball) and the TEMP(#7) Systems/CatTestKeys_TEMP (F1-F4 = danger stages) until DangerManager exists.
    /// Adds eyes to the Cat prefab once. Only what is missing; one Undo step; the scene is not saved.
    /// Run Magic > Level > Add Ball, Shot and Avatar first, so the cat has a ball to watch.
    /// </summary>
    public static class GardenCatSetupMenu
    {
        const string SceneName = "L1_Garden";
        const string CatPath = "Gameplay/Hazards/Cat";
        public const string TestKeysName = "CatTestKeys_TEMP";

        [MenuItem(MagicMenuUtil.Root + "Level/Garden: Set Up Cat", priority = MagicMenuUtil.LevelPriority + 21)]
        static void SetUp()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.name != SceneName)
            {
                MagicMenuUtil.Log($"Open {SceneName} first (the cat is Level 1's).");
                return;
            }
            var catObject = LevelSkeleton.Find(scene, CatPath);
            if (catObject == null)
            {
                MagicMenuUtil.Log($"Missing {CatPath}: run Magic > Level > Garden: Place Temporary Layout first.");
                return;
            }

            CatPrefabEyes.Ensure();

            Undo.SetCurrentGroupName("Set Up Garden Cat");
            var undoGroup = Undo.GetCurrentGroup();
            var added = 0;
            var ball = Object.FindAnyObjectByType<GolfBall>(FindObjectsInactive.Include);

            if (!catObject.TryGetComponent(out GardenCat cat))
            {
                cat = Undo.AddComponent<GardenCat>(catObject);
                var so = new SerializedObject(cat);
                so.FindProperty("lookTarget").objectReferenceValue = ball != null ? ball.transform : null;
                so.ApplyModifiedPropertiesWithoutUndo();
                MagicMenuUtil.Log($"Added GardenCat to {CatPath}" + (ball == null ? " (no ball in the scene: set Look Target by hand)" : ""));
                added++;
            }

            if (Object.FindAnyObjectByType<GardenCatTestKeys>(FindObjectsInactive.Include) == null)
            {
                var go = new GameObject(TestKeysName);
                SceneManager.MoveGameObjectToScene(go, scene);
                go.transform.SetParent(LevelSkeleton.Find(scene, "Systems").transform, false);
                var keys = go.AddComponent<GardenCatTestKeys>();
                var so = new SerializedObject(keys);
                so.FindProperty("cat").objectReferenceValue = cat;
                so.FindProperty("shot").objectReferenceValue = Object.FindAnyObjectByType<ShotController>(FindObjectsInactive.Include);
                so.ApplyModifiedPropertiesWithoutUndo();
                Undo.RegisterCreatedObjectUndo(go, "Add " + TestKeysName);
                MagicMenuUtil.Log($"Added TEMP(#7) Systems/{TestKeysName} (F1-F4). Delete it when DangerManager drives the cat.");
                added++;
            }

            Undo.CollapseUndoOperations(undoGroup);
            if (added > 0)
                EditorSceneManager.MarkSceneDirty(scene);
            MagicMenuUtil.Log($"Garden cat: {added} added. Press Play and try F1-F4, then save the scene.");
        }
    }
}
