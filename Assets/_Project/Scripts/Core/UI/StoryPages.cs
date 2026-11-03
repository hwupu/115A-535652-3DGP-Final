using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Project3DGP.Core.UI
{
    /// <summary>
    /// A full-screen story panel: one page of text at a time. Next (button, Space or Enter) turns the page, Skip
    /// (button or Esc) ends it; after the last page the panel hides and calls back (e.g. load the first level).
    /// The pages are a list in the Inspector, so the text changes without code. The component sits on an always
    /// active object; only the panel child is shown and hidden.
    /// </summary>
    public class StoryPages : MonoBehaviour
    {
        [SerializeField] GameObject panel;
        [SerializeField] Text pageLabel;
        [Tooltip("Optional \"2 / 4\" label.")]
        [SerializeField] Text counterLabel;
        [SerializeField, TextArea(3, 8)] List<string> pages = new();
        [Tooltip("Space / Enter = next page, Esc = skip.")]
        [SerializeField] bool keys = true;

        Action _onDone;
        int _startFrame = -1;

        public bool IsPlaying => panel != null && panel.activeSelf;
        public int PageIndex { get; private set; }
        public int PageCount => pages.Count;

        void Awake()
        {
            if (panel != null && _onDone == null)
                panel.SetActive(false);
        }

        /// <summary>Shows the first page; <paramref name="onDone"/> runs after the last page or Skip.</summary>
        public void Play(Action onDone)
        {
            _onDone = onDone;
            _startFrame = Time.frameCount;   // the key that started it (Enter on a button) must not also turn the page
            PageIndex = 0;
            if (pages.Count == 0 || panel == null)
            {
                Finish();
                return;
            }
            panel.SetActive(true);
            ShowPage();
        }

        public void Next()
        {
            if (!IsPlaying)
                return;
            PageIndex++;
            if (PageIndex >= pages.Count)
                Finish();
            else
                ShowPage();
        }

        public void Skip()
        {
            if (IsPlaying)
                Finish();
        }

        void Update()
        {
            var keyboard = Keyboard.current;
            if (!keys || keyboard == null || !IsPlaying || Time.frameCount == _startFrame)
                return;
            if (keyboard.escapeKey.wasPressedThisFrame)
                Skip();
            else if (keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame)
                Next();
        }

        void ShowPage()
        {
            if (pageLabel != null)
                pageLabel.text = pages[PageIndex];
            if (counterLabel != null)
                counterLabel.text = $"{PageIndex + 1} / {pages.Count}";
        }

        void Finish()
        {
            if (panel != null)
                panel.SetActive(false);
            var done = _onDone;   // cleared first: the callback may start another story
            _onDone = null;
            done?.Invoke();
        }

        /// <summary>Wiring for code that builds the panel (Magic tools, tests).</summary>
        public void Configure(GameObject storyPanel, Text page, Text counter, IEnumerable<string> pageTexts)
        {
            panel = storyPanel;
            pageLabel = page;
            counterLabel = counter;
            pages = new List<string>(pageTexts);
        }
    }
}
