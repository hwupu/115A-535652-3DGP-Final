using Project3DGP.Core.UI;
using Project3DGP.Game.Session;
using UnityEngine;
using UnityEngine.UI;

namespace Project3DGP.Game.UI
{
    /// <summary>
    /// The XP bar under the time bar (#78, architecture §4.8): "Lv 2" and a bar from this level to the next, with
    /// "120 / 200 XP" on it. Reads GameSession, never writes: it redraws on GameSession.Changed and pulses the
    /// level label on LeveledUp.
    /// </summary>
    public class XpBarView : MonoBehaviour
    {
        [SerializeField] BarView bar;
        [SerializeField] Text levelLabel;
        [Tooltip("At a level-up the level label grows by this factor and turns this colour for a moment.")]
        [SerializeField] float pulseScale = 1.4f;
        [SerializeField] float pulseSeconds = 0.8f;
        [SerializeField] Color levelUpColor = new(1f, 0.85f, 0.2f);

        GameSession _session;
        Color _labelColor = Color.white;
        float _pulseTime = -1f;

        void Start()
        {
            if (_session == null)
                Bind(GameSession.Instance);
        }

        void OnDestroy() => Unbind();

        /// <summary>Connects the bar to the session (Start does it; tests call it directly).</summary>
        public void Bind(GameSession session)
        {
            Unbind();
            _session = session;
            if (levelLabel != null)
                _labelColor = levelLabel.color;
            if (session == null)
            {
                Debug.Log("[HUD] No GameSession in the scene (Magic > Game > Add Game Session): the XP bar stays empty.", this);
                return;
            }
            session.Changed += Show;
            session.LeveledUp += OnLeveledUp;
            Show();
            bar.SetValue(session.LevelProgress01, instant: true);
        }

        void Unbind()
        {
            if (_session == null)
                return;
            _session.Changed -= Show;
            _session.LeveledUp -= OnLeveledUp;
            _session = null;
        }

        void Show()
        {
            if (levelLabel != null)
                levelLabel.text = $"Lv {_session.PlayerLevel}";
            bar.SetValue(_session.LevelProgress01);
            if (_session.IsMaxLevel)
            {
                bar.SetLabel("MAX");
                return;
            }
            var from = _session.XpForLevel(_session.PlayerLevel);
            var to = _session.XpForLevel(_session.PlayerLevel + 1);
            bar.SetLabel($"{_session.Xp - from} / {to - from} XP");
        }

        void OnLeveledUp(int level) => _pulseTime = 0f;

        void Update()
        {
            if (_pulseTime < 0f || levelLabel == null)
                return;
            _pulseTime += Time.unscaledDeltaTime;
            var t = Mathf.Clamp01(_pulseTime / pulseSeconds);
            levelLabel.transform.localScale = Vector3.one * Mathf.Lerp(pulseScale, 1f, t);
            levelLabel.color = Color.Lerp(levelUpColor, _labelColor, t);
            if (t >= 1f)
                _pulseTime = -1f;
        }

        internal bool IsPulsing => _pulseTime >= 0f;

        internal void SetForTests(BarView barView, Text label)
        {
            bar = barView;
            levelLabel = label;
        }
    }
}
