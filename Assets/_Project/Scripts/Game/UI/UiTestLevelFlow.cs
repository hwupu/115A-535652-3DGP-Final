using System;
using Project3DGP.Game.Danger;
using Project3DGP.Game.Flow;
using Project3DGP.Game.Scoring;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project3DGP.Game.UI
{
    /// <summary>
    /// A stand-in level for the UI Test Area and the UI tests (#20): a fake LevelManager + DangerManager +
    /// ScoreKeeper behind the same interfaces, so the HUD and dialogs can be tried without the real ones (#7,
    /// #18, #27). The timer runs down from totalSeconds; S = a shot (+1 stroke, costs shotCost seconds),
    /// F = time up now. It never loads scenes (Retry resets, Main menu only logs). Not for level scenes.
    /// </summary>
    public class UiTestLevelFlow : MonoBehaviour, ILevelFlow, IDangerTimer, IStrokeCounter
    {
        [SerializeField] float totalSeconds = 60f;
        [SerializeField] float shotCost = 5f;
        [Tooltip("Progress (0..1) where Warning and Critical start.")]
        [SerializeField] float warningAt = 0.5f;
        [SerializeField] float criticalAt = 0.8f;
        [SerializeField] bool showHelp = true;

        public LevelPhase Phase { get; private set; } = LevelPhase.Intro;
        public IDangerTimer Danger => this;
        IStrokeCounter ILevelFlow.Strokes => this;
        public bool IsPaused { get; private set; }

        public DangerState State { get; private set; }
        public float RemainingSeconds { get; private set; }
        public float Progress01 => totalSeconds > 0f ? 1f - RemainingSeconds / totalSeconds : 1f;
        public bool IsRunning => Phase == LevelPhase.Playing && !IsPaused;
        public int Strokes { get; private set; }

        /// <summary>How often Retry / Main menu were asked for (tests).</summary>
        public int RetryCount { get; private set; }
        public int MainMenuCount { get; private set; }

        public event Action LevelStarted;
        public event Action<LevelResult> LevelCompleted;
        public event Action LevelFailed;
        public event Action<bool> PausedChanged;
        public event Action<DangerState> StateChanged;
        public event Action<float> ProgressJumped;
        public event Action<int> StrokesChanged;

        void Awake() => RemainingSeconds = totalSeconds;

        void Start() => Begin();

        void OnDestroy()
        {
            if (IsPaused)
                Time.timeScale = 1f;
        }

        void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && IsRunning)
            {
                if (keyboard.sKey.wasPressedThisFrame) TakeShot();
                if (keyboard.fKey.wasPressedThisFrame) SetRemaining(0f);
            }
            if (IsRunning)
                SetRemaining(RemainingSeconds - Time.deltaTime);
        }

        /// <summary>A shot: one more stroke, and the bar jumps by shotCost.</summary>
        public void TakeShot()
        {
            if (!IsRunning)
                return;
            Strokes++;
            StrokesChanged?.Invoke(Strokes);
            SetRemaining(RemainingSeconds - shotCost);
            if (Phase == LevelPhase.Playing)
                ProgressJumped?.Invoke(Progress01);
        }

        /// <summary>Sets the time left; updates the stage, and fails the level at 0.</summary>
        public void SetRemaining(float seconds)
        {
            if (Phase != LevelPhase.Playing)
                return;
            RemainingSeconds = Mathf.Max(0f, seconds);
            var state = RemainingSeconds <= 0f ? DangerState.Failed
                : Progress01 >= criticalAt ? DangerState.Critical
                : Progress01 >= warningAt ? DangerState.Warning
                : DangerState.Normal;
            if (state != State)
            {
                State = state;
                StateChanged?.Invoke(state);
            }
            if (state == DangerState.Failed)
            {
                Phase = LevelPhase.Failed;
                LevelFailed?.Invoke();
            }
        }

        public void SetPaused(bool paused)
        {
            if (paused == IsPaused || (paused && Phase != LevelPhase.Playing))
                return;
            IsPaused = paused;
            Time.timeScale = paused ? 0f : 1f;
            PausedChanged?.Invoke(paused);
        }

        public void Retry()
        {
            RetryCount++;
            SetPaused(false);
            Begin();
        }

        public void GoToMainMenu()
        {
            MainMenuCount++;
            SetPaused(false);
            Debug.Log("[UI test] Main menu: LevelManager would load the MainMenu scene here.");
        }

        void Begin()
        {
            Phase = LevelPhase.Playing;
            RemainingSeconds = totalSeconds;
            Strokes = 0;
            State = DangerState.Normal;
            StateChanged?.Invoke(State);
            StrokesChanged?.Invoke(Strokes);
            LevelStarted?.Invoke();
        }

        void OnGUI()
        {
            if (!showHelp)
                return;
            GUI.Label(new Rect(10f, Screen.height - 50f, 900f, 40f),
                $"UI test level: {Phase}, {State}, {RemainingSeconds:0.0} s left, {Strokes} strokes.   " +
                "S = shot (costs time)   F = time up   Esc / menu button = pause");
        }

        internal void SetForTests(float total, float cost)
        {
            totalSeconds = total;
            shotCost = cost;
            showHelp = false;
            RemainingSeconds = total;
        }
    }
}
