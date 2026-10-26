using UnityEngine;
using UnityEngine.UI;

namespace Project3DGP.Core.UI
{
    /// <summary>
    /// Draws a 0..1 value as a bar. The fill is a child RectTransform whose right (or top) anchor is moved
    /// to the value: anchorMax.x = 0.5 means the fill covers half the bar. No sprite is needed.
    /// With smoothTime > 0 the bar slides to the new value instead of jumping (nice for damage or time loss).
    /// </summary>
    public class BarView : MonoBehaviour
    {
        [SerializeField] RectTransform fill;
        [Tooltip("Fill bottom-to-top instead of left-to-right.")]
        [SerializeField] bool vertical;
        [Tooltip("Seconds to slide to a new value. 0 = jump instantly (use 0 for a power meter).")]
        [SerializeField] float smoothTime = 0.25f;
        [SerializeField] Text label;

        float _target;     // the value we want to show (set by SetValue)
        float _shown;      // the value drawn right now (slides towards _target)
        float _velocity;   // used by SmoothDamp

        public Graphic FillGraphic => fill != null ? fill.GetComponent<Graphic>() : null;

        public void SetValue(float value01, bool instant = false)
        {
            _target = Mathf.Clamp01(value01);
            if (instant || smoothTime <= 0f)
            {
                _shown = _target;
                Apply();
            }
        }

        public void SetLabel(string text)
        {
            if (label != null)
                label.text = text;
        }

        void Update()
        {
            if (smoothTime <= 0f || Mathf.Approximately(_shown, _target))
                return;
            // SmoothDamp eases towards the target like a spring with no overshoot.
            _shown = Mathf.SmoothDamp(_shown, _target, ref _velocity, smoothTime, Mathf.Infinity, Time.unscaledDeltaTime);
            Apply();
        }

        void Apply()
        {
            if (fill == null)
                return;
            // anchorMin stays at (0, 0); moving anchorMax stretches the fill from the left (or bottom) edge.
            var anchorMax = fill.anchorMax;
            if (vertical)
                anchorMax.y = _shown;
            else
                anchorMax.x = _shown;
            fill.anchorMax = anchorMax;
        }
    }
}
