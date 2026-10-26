using System;
using UnityEngine;
using UnityEngine.UI;

namespace Project3DGP.Core.UI
{
    /// <summary>
    /// One card of a <see cref="SelectionPanel"/>: a clickable button with a frame that lights up and
    /// grows a little when selected. The panel decides which card is selected; the card only shows it.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class SelectableCard : MonoBehaviour
    {
        [SerializeField] Graphic frame;
        [SerializeField] Graphic icon;
        [SerializeField] Text title;
        [SerializeField] Text details;
        [SerializeField] Color normalFrameColor = new Color(1f, 1f, 1f, 0.25f);
        [SerializeField] Color selectedFrameColor = new Color(1f, 0.85f, 0.2f, 1f);
        [SerializeField] float selectedScale = 1.08f;
        [Tooltip("Scale change per second when growing or shrinking.")]
        [SerializeField] float scaleSpeed = 1.5f;

        /// <summary>Raised when the card is clicked with the mouse.</summary>
        public event Action Clicked;

        float _targetScale = 1f;

        // Forward the uGUI Button's click as our own event; the panel listens to it.
        void Awake() => GetComponent<Button>().onClick.AddListener(() => Clicked?.Invoke());

        public void SetContent(string titleText, string detailsText, Color iconColor)
        {
            if (title != null) title.text = titleText;
            if (details != null) details.text = detailsText;
            if (icon != null) icon.color = iconColor;
        }

        public void SetSelected(bool selected)
        {
            // Highlight = yellow frame + slightly bigger. The scale change is animated in Update.
            if (frame != null)
                frame.color = selected ? selectedFrameColor : normalFrameColor;
            _targetScale = selected ? selectedScale : 1f;
        }

        void Update()
        {
            // Grow/shrink towards the target scale at a constant speed (unscaled time: works while paused too).
            var scale = Mathf.MoveTowards(transform.localScale.x, _targetScale, scaleSpeed * Time.unscaledDeltaTime);
            transform.localScale = new Vector3(scale, scale, 1f);
        }
    }
}
