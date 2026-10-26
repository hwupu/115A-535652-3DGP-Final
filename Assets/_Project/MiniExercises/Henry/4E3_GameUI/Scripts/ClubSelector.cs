using Project3DGP.Core.UI;
using UnityEngine;

namespace Project3DGP.MiniEx.Henry
{
    /// <summary>
    /// Connects the club data to the generic SelectionPanel: writes each club's name and stats onto
    /// its card, and tells the rest of the demo which club is selected right now.
    /// SelectionPanel (Core) knows nothing about golf; this demo class adds the golf meaning to it.
    /// </summary>
    public class ClubSelector : MonoBehaviour
    {
        [SerializeField] SelectionPanel panel;
        [SerializeField] ClubDefinition[] clubs;

        // Clamp keeps the index valid (SelectedIndex is -1 before the panel's Start has run).
        /// <summary>The selected club (the first one until the panel has made its first selection).</summary>
        public ClubDefinition Current => clubs[Mathf.Clamp(panel.SelectedIndex, 0, clubs.Length - 1)];

        // Awake runs before any Start, so the cards have their text before they are first shown.
        void Awake()
        {
            // Card i shows club i.
            for (var i = 0; i < clubs.Length && i < panel.Cards.Count; i++)
            {
                var club = clubs[i];
                panel.Cards[i].SetContent(
                    $"{i + 1}. {club.displayName}",
                    $"Distance {club.maxDistance:0} m\nAccuracy {club.accuracy * 100f:0}%",   // :0 = no decimals
                    club.color);
            }
        }
    }
}
