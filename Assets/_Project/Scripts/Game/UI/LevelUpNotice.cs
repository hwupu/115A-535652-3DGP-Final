using System.Collections.Generic;
using Project3DGP.Core.UI;
using Project3DGP.Game.Session;
using UnityEngine;

namespace Project3DGP.Game.UI
{
    /// <summary>
    /// Tells the player about a level-up (#70) in the HUD dialog bar: "Level 3! New: Sledgehammer". Reads
    /// GameSession.LeveledUp; every level passed at once is listed. The text format is in the Inspector.
    /// </summary>
    public class LevelUpNotice : MonoBehaviour
    {
        [Tooltip("Empty = the first DialogBar in the scene at Start.")]
        [SerializeField] DialogBar dialog;
        [Tooltip("{0} = the new level.")]
        [SerializeField] string levelFormat = "Level {0}!";
        [Tooltip("{0} = the unlocked clubs and skills, comma separated.")]
        [SerializeField] string unlockFormat = " New: {0}";
        [SerializeField] float seconds = 4f;

        GameSession _session;
        int _lastLevel;

        void Start()
        {
            if (dialog == null)
                dialog = FindAnyObjectByType<DialogBar>();
            Bind(GameSession.Instance, dialog);
        }

        void OnDestroy()
        {
            Unbind();
        }

        void Unbind()
        {
            if (_session == null)
                return;
            _session.LeveledUp -= OnLeveledUp;
            _session.Changed -= OnChanged;
        }

        /// <summary>Connects to the session and the dialog bar (Start does it; tests call it directly).</summary>
        public void Bind(GameSession session, DialogBar bar)
        {
            Unbind();
            _session = session;
            dialog = bar;
            if (_session == null)
                return;
            _lastLevel = _session.PlayerLevel;
            _session.LeveledUp += OnLeveledUp;   // raised before Changed: _lastLevel is still the old level
            _session.Changed += OnChanged;
        }

        void OnChanged() => _lastLevel = _session.PlayerLevel;   // also a new game or a loaded save

        /// <summary>The line for reaching <paramref name="level"/> from <paramref name="previous"/> (tests read it too).</summary>
        public string Message(int previous, int level)
        {
            var names = new List<string>();
            for (var l = previous + 1; l <= level; l++)
                foreach (var unlock in _session.UnlocksAt(l))
                    if (unlock.DisplayName.Length > 0) names.Add(unlock.DisplayName);
            var text = string.Format(levelFormat, level);
            return names.Count > 0 ? text + string.Format(unlockFormat, string.Join(", ", names)) : text;
        }

        void OnLeveledUp(int level)
        {
            if (dialog != null)
                dialog.Show(Message(_lastLevel, level), seconds);
        }
    }
}
