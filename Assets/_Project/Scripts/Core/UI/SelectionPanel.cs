using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project3DGP.Core.UI
{
    /// <summary>
    /// A list of <see cref="SelectableCard"/>s where exactly one is selected (gear, characters, ...).
    /// Select with the mouse (click a card) or the number keys 1..9. The selected card is highlighted
    /// by the card itself (frame + scale), and the panel plays a sound on top of it.
    /// </summary>
    public class SelectionPanel : MonoBehaviour
    {
        [SerializeField] List<SelectableCard> cards = new List<SelectableCard>();
        [SerializeField] int startIndex;
        [SerializeField] bool numberKeys = true;

        [Header("Highlight sound")]
        [SerializeField] AudioSource audioSource;
        [SerializeField] AudioClip selectSound;
        [Tooltip("Volume of the select sound (1 = the clip's full volume).")]
        [SerializeField, Range(0f, 1f)] float selectVolume = 0.5f;

        /// <summary>Raised with the new index whenever the selection changes.</summary>
        public event Action<int> Selected;

        public int SelectedIndex { get; private set; } = -1;
        public IReadOnlyList<SelectableCard> Cards => cards;

        // Subscribe to every card's click, then select the starting card silently (no sound at scene start).
        void Start()
        {
            for (var i = 0; i < cards.Count; i++)
            {
                var index = i;   // copy for the lambda: each card remembers its own index
                cards[i].Clicked += () => Select(index);
            }
            Select(startIndex, playEffects: false);
        }

        void Update()
        {
            var keyboard = Keyboard.current;
            if (!numberKeys || keyboard == null)
                return;

            // Key.Digit1 .. Key.Digit9 are consecutive in the Input System's Key enum.
            for (var i = 0; i < cards.Count && i < 9; i++)
                if (keyboard[Key.Digit1 + i].wasPressedThisFrame)
                    Select(i);
        }

        /// <summary>The one place where the selection changes (mouse and keys both end up here).</summary>
        public void Select(int index, bool playEffects = true)
        {
            if (index < 0 || index >= cards.Count || index == SelectedIndex)
                return;

            SelectedIndex = index;
            // Exactly one card is highlighted: the chosen one on, all others off.
            for (var i = 0; i < cards.Count; i++)
                cards[i].SetSelected(i == index);

            if (playEffects && audioSource != null && selectSound != null)
                audioSource.PlayOneShot(selectSound, selectVolume);   // 2nd argument scales the volume
            Selected?.Invoke(index);   // tell listeners (none needed in the demo; ClubSelector reads SelectedIndex)
        }
    }
}
