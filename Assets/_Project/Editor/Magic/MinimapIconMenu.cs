using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Project3DGP.EditorTools
{
    /// <summary>
    /// Magic > Level > Add Minimap Icon to Selection (#78): a red hazard icon on each selected scene object that has
    /// none yet (cat, scarab, guardian, ...), so it shows on the HUD minimap. Objects directly under Gameplay/Hazards
    /// get one from Add HUD and Menus anyway. Change the colour on its material (Materials/Placeholder/Minimap*).
    /// One Undo step; the scene is marked dirty, not saved.
    /// </summary>
    public static class MinimapIconMenu
    {
        [MenuItem(MagicMenuUtil.Root + "Level/Add Minimap Icon to Selection", priority = MagicMenuUtil.LevelPriority + 6)]
        static void Add()
        {
            var layer = LevelMinimap.Layer;
            if (layer < 0)
                return;
            Undo.SetCurrentGroupName("Add Minimap Icons");
            var undoGroup = Undo.GetCurrentGroup();
            var added = 0;
            foreach (var go in Selection.gameObjects)
                if (go.scene.IsValid() && LevelMinimap.AddIcon(go.transform, layer, "Hazard", LevelMinimap.HazardColor, 1.8f))
                    added++;
            Undo.CollapseUndoOperations(undoGroup);
            if (added > 0)
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            MagicMenuUtil.Log($"Minimap icons: {added} added (objects that already had one were skipped). Save the scene.");
        }

        [MenuItem(MagicMenuUtil.Root + "Level/Add Minimap Icon to Selection", true)]
        static bool CanAdd() => Selection.gameObjects.Length > 0;
    }
}
