using Project3DGP.Game.Session;
using UnityEngine;

namespace Project3DGP.Game.Effects
{
    /// <summary>
    /// Background music for one scene (docs/audio/sound-list.md): plays <see cref="music"/> from the start (fading
    /// in, looping), plus an optional <see cref="jingle"/> once at the same moment (the happy ending fanfare). 2D,
    /// keeps playing while the game is paused, and follows the Music volume setting live (the settings slider).
    /// Magic > Game > Add Music puts one in MainMenu, L1_Garden and Ending; L2/L3 owners add their own and drag in
    /// their track. Other code can switch tracks with <see cref="Play"/> or fade out with <see cref="Stop"/>.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class MusicPlayer : MonoBehaviour
    {
        [SerializeField] AudioClip music;
        [Tooltip("Track level before the Music setting. Tracks are matched to -18 LUFS; PulseBox (no edits allowed) uses about 0.45.")]
        [SerializeField, Range(0f, 1f)] float volume = 1f;
        [SerializeField] bool loop = true;
        [SerializeField, Min(0f)] float fadeInSeconds = 1f;
        [Tooltip("Optional: plays once together with the music (e.g. a fanfare).")]
        [SerializeField] AudioClip jingle;
        [SerializeField, Range(0f, 1f)] float jingleVolume = 1f;

        AudioSource _audio;
        float _fade = 1f;         // 0..1, the fade-in / fade-out envelope
        float _fadeSpeed;         // per second; > 0 fades in, < 0 fades out
        float _trackVolume;

        public AudioClip Current => _audio != null ? _audio.clip : null;

        void Awake()
        {
            _audio = GetComponent<AudioSource>();
            _audio.playOnAwake = false;
            _audio.spatialBlend = 0f;
            _audio.ignoreListenerPause = true;
        }

        void Start()
        {
            if (music != null)
                Play(music, volume, loop, fadeInSeconds);
            if (jingle != null)
                _audio.PlayOneShot(jingle, jingleVolume * MusicSetting());
        }

        /// <summary>Starts a track from the beginning (cuts the current one), fading in over <paramref name="fadeIn"/> s.</summary>
        public void Play(AudioClip clip, float trackVolume = 1f, bool looping = true, float fadeIn = 0f)
        {
            if (clip == null)
                return;
            _trackVolume = trackVolume;
            _audio.clip = clip;
            _audio.loop = looping;
            _fade = fadeIn > 0f ? 0f : 1f;
            _fadeSpeed = fadeIn > 0f ? 1f / fadeIn : 0f;
            ApplyVolume();
            _audio.Play();
        }

        /// <summary>Fades the music out over <paramref name="fadeOut"/> s (0 = at once), then stops it.</summary>
        public void Stop(float fadeOut = 1f)
        {
            if (fadeOut <= 0f)
            {
                _audio.Stop();
                return;
            }
            _fadeSpeed = -1f / fadeOut;
        }

        void Update()
        {
            if (_fadeSpeed != 0f)
            {
                _fade = Mathf.Clamp01(_fade + _fadeSpeed * Time.unscaledDeltaTime);   // unscaled: fades while paused too
                if (_fade >= 1f && _fadeSpeed > 0f)
                    _fadeSpeed = 0f;
                else if (_fade <= 0f && _fadeSpeed < 0f)
                {
                    _fadeSpeed = 0f;
                    _audio.Stop();
                }
            }
            ApplyVolume();
        }

        void ApplyVolume() => _audio.volume = _trackVolume * _fade * MusicSetting();

        static float MusicSetting() =>
            GameSession.Instance != null ? Mathf.Clamp01(GameSession.Instance.Data.Settings.MusicVolume) : 1f;
    }
}
