using System.Collections.Generic;
using Project3DGP.Core.UI;
using Project3DGP.Game.Effects;
using Project3DGP.Game.Session;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Project3DGP.Game.UI
{
    /// <summary>
    /// GUI sound effects (#72) for one canvas: every button under it clicks (Back / Cancel / Close / Skip / Resume with
    /// a softer sound), and a slider ticks while the player drags it. Club cards are left out (ClubPanelView plays its
    /// own select sound). 2D, plays while the game is paused, scaled by the SFX volume setting.
    /// Magic > Game > Add Sounds puts it on every canvas of the open scene.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class UiSounds : MonoBehaviour
    {
        static readonly HashSet<string> BackLabels = new() { "Back", "Cancel", "Close", "Skip", "Resume" };

        [SerializeField] SoundCue click = new();
        [SerializeField] SoundCue back = new();
        [SerializeField] SoundCue slider = new();
        [SerializeField] float sliderInterval = 0.08f;

        AudioSource _audio;
        float _lastSliderTime = float.NegativeInfinity;

        void Awake()
        {
            _audio = GetComponent<AudioSource>();
            _audio.ignoreListenerPause = true;
            foreach (var button in GetComponentsInChildren<Button>(true))
            {
                if (button.GetComponent<SelectableCard>() != null)
                    continue;
                var label = button.GetComponentInChildren<Text>(true);
                var cue = label != null && BackLabels.Contains(label.text) ? back : click;
                button.onClick.AddListener(() => Play(cue));
            }
            foreach (var s in GetComponentsInChildren<Slider>(true))
            {
                var target = s.gameObject;
                s.onValueChanged.AddListener(_ => OnSliderChanged(target));
            }
        }

        // Only while the player drags it (not when the settings panel loads the saved values).
        void OnSliderChanged(GameObject target)
        {
            if (EventSystem.current == null || EventSystem.current.currentSelectedGameObject != target)
                return;
            if (Time.unscaledTime - _lastSliderTime < sliderInterval)
                return;
            _lastSliderTime = Time.unscaledTime;
            Play(slider);
        }

        void Play(SoundCue cue)
        {
            var clip = cue?.Pick();
            if (clip == null)
                return;
            _audio.pitch = cue.PickPitch();
            var sfx = GameSession.Instance != null ? GameSession.Instance.Data.Settings.SfxVolume : 1f;
            _audio.PlayOneShot(clip, cue.Volume * sfx);
        }
    }
}
