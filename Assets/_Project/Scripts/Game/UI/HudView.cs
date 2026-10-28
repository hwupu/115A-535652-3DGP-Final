using Project3DGP.Core.UI;
using Project3DGP.Game.Danger;
using Project3DGP.Game.Flow;
using UnityEngine;
using UnityEngine.UI;

namespace Project3DGP.Game.UI
{
    /// <summary>
    /// The level HUD (#20, architecture §4.8), read-only: upper left the time bar (time left) and the stage icon
    /// (one sprite per DangerState, Henry's cat faces), the stroke count, and the menu button (upper right) that
    /// pauses the level. Reads an ILevelFlow (LevelManager, #27); if the scene has none yet, the timer and
    /// strokes stay hidden. The power meter is its own view (PowerMeterView).
    /// </summary>
    public class HudView : MonoBehaviour
    {
        [Tooltip("The level's ILevelFlow (LevelManager, #27). Empty = the first one in the scene at Start.")]
        [SerializeField] MonoBehaviour flow;
        [Tooltip("Time bar + stage icon; hidden while there is no ILevelFlow.")]
        [SerializeField] GameObject timerGroup;
        [SerializeField] BarView timeBar;
        [SerializeField] Image stageIcon;
        [Tooltip("One per DangerState: Normal, Warning, Critical, Failed. An empty slot shows the colour below instead.")]
        [SerializeField] Sprite[] stageSprites = new Sprite[4];
        [SerializeField] Color[] stageColors =
        {
            new(0.45f, 0.8f, 0.45f), new(1f, 0.8f, 0.2f), new(1f, 0.45f, 0.15f), new(0.85f, 0.15f, 0.15f),
        };
        [SerializeField] Text strokesLabel;
        [Tooltip("The bar grows by this factor for a moment when a shot costs time (ProgressJumped).")]
        [SerializeField] float jumpScale = 1.15f;
        [SerializeField] float jumpSeconds = 0.3f;

        ILevelFlow _flow;
        IDangerTimer _danger;
        float _jumpTime = -1f;

        public ILevelFlow Flow => _flow;

        void Start()
        {
            if (_flow == null)
                Bind(LevelFlowFinder.Find(flow));
        }

        void OnDestroy() => Unbind();

        /// <summary>Connects the HUD to a level (Start does it from the scene; tests call it directly).</summary>
        public void Bind(ILevelFlow levelFlow)
        {
            Unbind();
            _flow = levelFlow;
            _danger = levelFlow?.Danger;
            if (timerGroup != null)
                timerGroup.SetActive(_danger != null);
            if (strokesLabel != null)
                strokesLabel.gameObject.SetActive(levelFlow?.Strokes != null);
            if (levelFlow == null)
            {
                Debug.Log("[HUD] No ILevelFlow in the scene yet (LevelManager, #27): time bar and strokes are hidden.", this);
                return;
            }

            if (_danger != null)
            {
                _danger.StateChanged += ShowStage;
                _danger.ProgressJumped += OnProgressJumped;
                ShowStage(_danger.State);
                timeBar.SetValue(1f - _danger.Progress01, instant: true);
            }
            if (levelFlow.Strokes != null)
            {
                levelFlow.Strokes.StrokesChanged += ShowStrokes;
                ShowStrokes(levelFlow.Strokes.Strokes);
            }
        }

        void Unbind()
        {
            if (_danger != null)
            {
                _danger.StateChanged -= ShowStage;
                _danger.ProgressJumped -= OnProgressJumped;
            }
            if (_flow?.Strokes != null)
                _flow.Strokes.StrokesChanged -= ShowStrokes;
            _flow = null;
            _danger = null;
        }

        /// <summary>Menu button (upper right): opens the pause dialog while playing.</summary>
        public void OpenMenu()
        {
            if (_flow != null && _flow.Phase == LevelPhase.Playing && !_flow.IsPaused)
                _flow.SetPaused(true);
        }

        void Update()
        {
            if (_danger == null)
                return;
            timeBar.SetValue(1f - _danger.Progress01);   // the bar shows the time left

            if (_jumpTime < 0f)
                return;
            _jumpTime += Time.unscaledDeltaTime;
            var t = Mathf.Clamp01(_jumpTime / jumpSeconds);
            timeBar.transform.localScale = Vector3.one * Mathf.Lerp(jumpScale, 1f, t);
            if (t >= 1f)
                _jumpTime = -1f;
        }

        void ShowStage(DangerState state)
        {
            if (stageIcon == null)
                return;
            var index = (int)state;
            var sprite = index < stageSprites.Length ? stageSprites[index] : null;
            stageIcon.sprite = sprite;
            // Henry's art keeps its own colours; the placeholder is a coloured square.
            stageIcon.color = sprite != null ? Color.white : index < stageColors.Length ? stageColors[index] : Color.white;
        }

        void ShowStrokes(int strokes)
        {
            if (strokesLabel != null)
                strokesLabel.text = $"Strokes: {strokes}";
        }

        void OnProgressJumped(float progress01) => _jumpTime = 0f;

        internal void SetForTests(GameObject group, BarView bar, Image icon, Text strokes)
        {
            timerGroup = group;
            timeBar = bar;
            stageIcon = icon;
            strokesLabel = strokes;
        }
    }
}
