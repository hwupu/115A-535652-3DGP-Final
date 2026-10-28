using Project3DGP.Game.Flow;
using Project3DGP.Game.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Project3DGP.EditorTools
{
    /// <summary>
    /// Magic > Level > Add Level Exit (#38): makes the Course/Gate prefab a LevelExit (once, every placed gate
    /// follows), then, if the open level has no LevelExit yet, puts a Gate in the Gameplay/Exit slot (made if missing) (move it to
    /// where the level ends). L1 already has a gate from the greybox, so nothing is added there.
    /// Also connects the exit to TEMP(#27) AvatarWiring_TEMP (G opens the exit, the hamster walks through).
    /// One Undo step; the scene is marked dirty, not saved.
    /// </summary>
    public static class LevelExitMenu
    {
        [MenuItem(MagicMenuUtil.Root + "Level/Add Level Exit", priority = MagicMenuUtil.LevelPriority + 4)]
        static void Add()
        {
            var scene = EditorSceneManager.GetActiveScene();
            var gameplay = LevelSkeleton.Find(scene, "Gameplay");
            if (gameplay == null)
            {
                MagicMenuUtil.Log("Run Magic > Level > Create Level Scene Skeleton first.");
                return;
            }

            GateLevelExit.Ensure();

            Undo.SetCurrentGroupName("Add Level Exit");
            var undoGroup = Undo.GetCurrentGroup();
            var changed = false;

            var exit = Object.FindAnyObjectByType<LevelExit>(FindObjectsInactive.Include);
            if (exit == null)
            {
                // The Exit slot was added to the skeleton later (#38): make it if this scene is older.
                var slot = LevelSkeleton.Find(scene, "Gameplay/Exit");
                if (slot == null)
                {
                    slot = new GameObject("Exit");
                    slot.transform.SetParent(gameplay.transform, false);
                    Undo.RegisterCreatedObjectUndo(slot, "Add Exit slot");
                }
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(GateLevelExit.PrefabPath);
                var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
                go.transform.SetParent(slot.transform, false);
                Undo.RegisterCreatedObjectUndo(go, "Add Gate");
                exit = go.GetComponent<LevelExit>();
                MagicMenuUtil.Log("Added a Gate to Gameplay/Exit. Move it to where the level ends (the hamster walks through it to +Z).");
                changed = true;
            }
            else
            {
                MagicMenuUtil.Log($"Level exit already in the scene: {exit.name}");
            }

            var wiring = Object.FindAnyObjectByType<GolferAvatarTestWiring>(FindObjectsInactive.Include);
            if (wiring != null)
            {
                var so = new SerializedObject(wiring);
                var property = so.FindProperty("exit");
                if (property.objectReferenceValue == null)
                {
                    Undo.RecordObject(wiring, "Connect exit");
                    property.objectReferenceValue = exit;
                    so.ApplyModifiedProperties();
                    MagicMenuUtil.Log($"Connected the exit to TEMP(#27) {wiring.name}: in Play mode, G opens it.");
                    changed = true;
                }
            }

            Undo.CollapseUndoOperations(undoGroup);
            if (changed)
                EditorSceneManager.MarkSceneDirty(scene);
        }
    }
}
