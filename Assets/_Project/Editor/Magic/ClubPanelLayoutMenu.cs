using Project3DGP.Core.UI;
using Project3DGP.Game.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Project3DGP.EditorTools
{
    /// <summary>
    /// Magic > Level > Update Club Panel: for a level scene whose HUD was made before the layout change, moves the
    /// club cards (ClubPanel) from the right middle to the upper right, right under the minimap (the layout
    /// Add HUD and Menus now builds), and makes the card icons 60 x 60 with their aspect kept, for the tool pictures
    /// (ClubProfile.Icon). Only the panel's position/size and the icon rects change; the theme, cards, sound and
    /// links stay. One Undo step; the scene is marked dirty, not saved.
    /// </summary>
    public static class ClubPanelLayoutMenu
    {
        [MenuItem(MagicMenuUtil.Root + "Level/Update Club Panel", priority = MagicMenuUtil.LevelPriority + 6)]
        static void Run()
        {
            var panel = Object.FindAnyObjectByType<ClubPanelView>(FindObjectsInactive.Include);
            if (panel == null)
            {
                MagicMenuUtil.Log("No ClubPanel in the open scene: run Magic > Level > Add HUD and Menus (it already uses the new layout).");
                return;
            }

            Undo.SetCurrentGroupName("Update Club Panel");
            var undoGroup = Undo.GetCurrentGroup();
            var rect = (RectTransform)panel.transform;
            var cards = panel.GetComponentsInChildren<SelectableCard>(true);
            Undo.RecordObject(rect, "Move Club Panel");
            GameUiBuilder.PlaceClubPanel(rect, Mathf.Max(1, cards.Length));
            MagicMenuUtil.Log($"ClubPanel ({cards.Length} cards) moved under the minimap, upper right");

            var icons = 0;
            foreach (var card in cards)
            {
                var icon = card.transform.Find("Icon");
                if (icon == null || !icon.TryGetComponent(out Image image))
                    continue;
                var iconRect = (RectTransform)icon;
                Undo.RecordObject(iconRect, "Club Icon Size");
                Undo.RecordObject(image, "Club Icon Aspect");
                iconRect.anchoredPosition = new Vector2(10f, 0f);
                iconRect.sizeDelta = new Vector2(60f, 60f);
                image.preserveAspect = true;
                icons++;
            }
            MagicMenuUtil.Log($"{icons} card icons set to 60 x 60, aspect kept (pictures come from each ClubProfile.Icon)");

            Undo.CollapseUndoOperations(undoGroup);
            EditorSceneManager.MarkSceneDirty(panel.gameObject.scene);
        }
    }
}
