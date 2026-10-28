using Project3DGP.Game.Session;
using UnityEngine;
using UnityEngine.UI;

namespace Project3DGP.Game.UI
{
    /// <summary>
    /// Settings panel (#20, architecture §4.8), opened from the pause dialog or the main menu: master, music and
    /// SFX volume (0..1) in GameSession.Data.Settings. Master is applied at once (AudioListener.volume); the music
    /// player and SFX read MusicVolume / SfxVolume. Its Back button is wired by whoever opens it.
    /// </summary>
    public class SettingsPanel : MonoBehaviour
    {
        [SerializeField] Slider master;
        [SerializeField] Slider music;
        [SerializeField] Slider sfx;

        Settings _settings;

        public bool IsOpen => gameObject.activeSelf;

        void Awake()
        {
            if (master != null) master.onValueChanged.AddListener(v => Change(s => s.MasterVolume = v));
            if (music != null) music.onValueChanged.AddListener(v => Change(s => s.MusicVolume = v));
            if (sfx != null) sfx.onValueChanged.AddListener(v => Change(s => s.SfxVolume = v));
        }

        /// <summary>Shows the panel for these settings (default: the game session's).</summary>
        public void Open(Settings settings = null)
        {
            _settings = settings ?? (GameSession.Instance != null ? GameSession.Instance.Data.Settings : new Settings());
            if (master != null) master.SetValueWithoutNotify(_settings.MasterVolume);
            if (music != null) music.SetValueWithoutNotify(_settings.MusicVolume);
            if (sfx != null) sfx.SetValueWithoutNotify(_settings.SfxVolume);
            gameObject.SetActive(true);
        }

        public void Close() => gameObject.SetActive(false);

        void Change(System.Action<Settings> set)
        {
            if (_settings == null)
                return;
            set(_settings);
            GameSession.ApplySettings(_settings);
        }

        internal void SetForTests(Slider masterSlider, Slider musicSlider, Slider sfxSlider)
        {
            master = masterSlider;
            music = musicSlider;
            sfx = sfxSlider;
        }
    }
}
