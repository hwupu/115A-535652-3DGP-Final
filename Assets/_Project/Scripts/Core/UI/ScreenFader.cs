using System.Collections;
using UnityEngine;

namespace Project3DGP.Core.UI
{
    /// <summary>
    /// Full-screen fade: a black Image on top of everything, with a CanvasGroup whose alpha goes
    /// 0 (invisible) -> 1 (black) and back. Use it from a coroutine: <c>yield return fader.FadeTo(1f, 0.25f);</c>
    /// Reusable for portals, level changes (#38) and the ending.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class ScreenFader : MonoBehaviour
    {
        CanvasGroup _group;

        void Awake()
        {
            _group = GetComponent<CanvasGroup>();
            _group.alpha = 0f;
            _group.blocksRaycasts = false;   // never block clicks on the UI underneath
        }

        /// <summary>Changes the alpha to <paramref name="target"/> over <paramref name="duration"/> seconds.</summary>
        public IEnumerator FadeTo(float target, float duration)
        {
            // unscaledDeltaTime: the fade still works if the game is paused (Time.timeScale = 0).
            var start = _group.alpha;
            for (var time = 0f; time < duration; time += Time.unscaledDeltaTime)
            {
                // Lerp from the starting alpha to the target as time goes 0 -> duration.
                _group.alpha = Mathf.Lerp(start, target, time / duration);
                yield return null;   // wait one frame
            }
            _group.alpha = target;
        }
    }
}
