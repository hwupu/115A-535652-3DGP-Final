using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Project3DGP.Core.UI
{
    /// <summary>
    /// A non-blocking text bar at the bottom of the screen for hints and story lines. Lines are queued and shown
    /// one at a time; each stays for its seconds (game time, so it waits while the game is paused) or until the
    /// player clicks the bar. Only the bar itself catches clicks; the rest of the screen plays on.
    /// </summary>
    public class DialogBar : MonoBehaviour, IPointerClickHandler
    {
        [Tooltip("Shown while a line is up, hidden otherwise (usually this object's child panel).")]
        [SerializeField] GameObject panel;
        [SerializeField] Text label;
        [SerializeField] float defaultSeconds = 6f;

        readonly Queue<(string Text, float Seconds)> _queue = new();
        float _timeLeft;

        public bool IsShowing { get; private set; }
        public string CurrentText => IsShowing && label != null ? label.text : null;

        /// <summary>A line was hidden (time out or click), before the next one shows.</summary>
        public event Action<string> LineHidden;

        void Awake() => Hide();

        /// <summary>Queues a line; it shows at once if nothing else is up. seconds &lt;= 0 = the default.</summary>
        public void Show(string text, float seconds = 0f)
        {
            _queue.Enqueue((text, seconds > 0f ? seconds : defaultSeconds));
            if (!IsShowing)
                Next();
        }

        /// <summary>Hides the current line and shows the next queued one.</summary>
        public void Skip()
        {
            if (!IsShowing)
                return;
            var text = label != null ? label.text : "";
            Hide();
            LineHidden?.Invoke(text);
            Next();
        }

        public void Clear()
        {
            _queue.Clear();
            Hide();
        }

        public void OnPointerClick(PointerEventData eventData) => Skip();

        void Update()
        {
            if (!IsShowing)
                return;
            _timeLeft -= Time.deltaTime;
            if (_timeLeft <= 0f)
                Skip();
        }

        void Next()
        {
            if (_queue.Count == 0)
                return;
            var (text, seconds) = _queue.Dequeue();
            if (label != null)
                label.text = text;
            _timeLeft = seconds;
            IsShowing = true;
            if (panel != null)
                panel.SetActive(true);
        }

        void Hide()
        {
            IsShowing = false;
            if (panel != null)
                panel.SetActive(false);
        }

        /// <summary>For tests and tools that build the bar in code.</summary>
        public void Configure(GameObject panelObject, Text text, float seconds)
        {
            panel = panelObject;
            label = text;
            defaultSeconds = seconds;
            Hide();
        }
    }
}
