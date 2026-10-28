using System.Collections.Generic;
using Project3DGP.Core.UI;
using Project3DGP.Game.Golf;
using Project3DGP.Game.Session;
using UnityEngine;

namespace Project3DGP.Game.UI
{
    /// <summary>
    /// The club cards on the right of the HUD (#78, architecture §4.8): one card per club the ShotController can use
    /// (more appear when #70 unlocks them), with the key, name and numbers. The current club's card lights up and
    /// grows (SelectableCard, from mini-exercise 4.E.3) and a select sound plays. Clicking a card picks that club,
    /// like keys 1-3, and only while aiming.
    /// </summary>
    public class ClubPanelView : MonoBehaviour
    {
        [Tooltip("Empty = the first ShotController in the scene at Start. No ShotController = no cards.")]
        [SerializeField] ShotController shot;
        [Tooltip("Card i shows club slot i (key i + 1). Extra cards are hidden.")]
        [SerializeField] List<SelectableCard> cards = new();
        [Tooltip("Colour of each card's icon square, by slot (placeholder until there are club icons).")]
        [SerializeField] Color[] iconColors = { new(0.3f, 0.55f, 0.95f), new(0.9f, 0.3f, 0.25f), new(0.6f, 0.4f, 0.9f) };

        [Header("Highlight sound")]
        [SerializeField] AudioSource audioSource;
        [SerializeField] AudioClip selectSound;
        [Tooltip("Volume of the select sound, times the SFX volume in Settings.")]
        [SerializeField, Range(0f, 1f)] float selectVolume = 0.5f;

        int _shownCount = -1;
        int _selected = -1;

        void Start()
        {
            for (var i = 0; i < cards.Count; i++)
            {
                var index = i;   // copy for the lambda: each card remembers its own slot
                cards[i].Clicked += () => OnCardClicked(index);
            }
            if (shot == null)
                shot = FindAnyObjectByType<ShotController>();
            Bind(shot);
        }

        void OnDestroy()
        {
            if (shot != null)
                shot.ClubChanged -= OnClubChanged;
        }

        /// <summary>Connects the panel to a ShotController (Start does it; tests call it directly).</summary>
        public void Bind(ShotController shotController)
        {
            if (shot != null)
                shot.ClubChanged -= OnClubChanged;
            shot = shotController;
            if (shot != null)
                shot.ClubChanged += OnClubChanged;
            Refresh();
        }

        // Unlocks (#70) can add clubs during the level: rebuild the cards when the count changes.
        void Update()
        {
            if (shot != null && shot.AvailableClubs.Count != _shownCount)
                Refresh();
        }

        void Refresh()
        {
            var clubs = shot != null ? shot.AvailableClubs : null;
            _shownCount = clubs?.Count ?? 0;
            for (var i = 0; i < cards.Count; i++)
            {
                var visible = i < _shownCount && clubs[i] != null;
                cards[i].gameObject.SetActive(visible);
                if (visible)
                    cards[i].SetContent($"{i + 1}  {clubs[i].DisplayName}", Details(clubs[i]), IconColor(i));
            }
            _selected = -1;
            Highlight(clubs != null ? IndexOf(shot.CurrentClub) : -1, playSound: false);
        }

        static string Details(ClubProfile club) =>
            $"Speed {club.MinSpeed:0}-{club.MaxSpeed:0} m/s\nAim cone {club.AimConeDegrees:0}°";

        Color IconColor(int slot) => slot < iconColors.Length ? iconColors[slot] : Color.white;

        int IndexOf(ClubProfile club)
        {
            var clubs = shot.AvailableClubs;
            for (var i = 0; i < clubs.Count; i++)
                if (clubs[i] == club) return i;
            return -1;
        }

        void OnClubChanged(ClubProfile club) => Highlight(IndexOf(club), playSound: true);

        void OnCardClicked(int slot)
        {
            if (shot != null && shot.CanChangeClub && slot < shot.AvailableClubs.Count)
                shot.SelectClub(shot.AvailableClubs[slot]);   // ClubChanged then moves the highlight
        }

        // Exactly one card is highlighted: the current club's on, all others off.
        void Highlight(int index, bool playSound)
        {
            if (index == _selected)
                return;
            _selected = index;
            for (var i = 0; i < cards.Count; i++)
                cards[i].SetSelected(i == index);
            if (!playSound || audioSource == null || selectSound == null || index < 0)
                return;
            var sfx = GameSession.Instance != null ? GameSession.Instance.Data.Settings.SfxVolume : 1f;
            audioSource.PlayOneShot(selectSound, selectVolume * sfx);
        }

        internal int SelectedIndex => _selected;

        internal void SetForTests(List<SelectableCard> cardList) => cards = cardList;
    }
}
