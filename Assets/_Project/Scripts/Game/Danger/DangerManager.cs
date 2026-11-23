using System;
using Project3DGP.Game.Golf;
using UnityEngine;

namespace Project3DGP.Game.Danger
{
    /// <summary>
    /// Level time budget (#7). LevelManager owns Begin/Pause/Resume and wires ShotTaken to ApplyShot,
    /// CameToRest to OnBallAtRest. This component never subscribes to the ball or shot controller itself.
    /// </summary>
    [DisallowMultipleComponent]
    public class DangerManager : MonoBehaviour, IDangerTimer
    {
        [SerializeField, Tooltip("This level's hand-tuned time budget. Sampled on Begin / ResetDanger.")]
        DangerConfig config;

        float _startSeconds;
        float _shotCostSeconds;
        float _warningBelowSeconds;
        float _criticalBelowSeconds;
        bool _begun;
        bool _paused = true;
        bool _inFlight;

        public DangerState State { get; private set; }
        public float RemainingSeconds { get; private set; }
        public float Progress01 => _startSeconds > 0f ? Mathf.Clamp01(1f - RemainingSeconds / _startSeconds) : 0f;
        public bool IsRunning => _begun && !_paused && !_inFlight && State != DangerState.Failed;

        public event Action<DangerState> StateChanged;
        public event Action<float> ProgressJumped;
        /// <summary>Raised on entering the named state; a jump can skip Warning entirely.</summary>
        public event Action OnWarning;
        public event Action OnCritical;
        public event Action OnFailed;

        public void Begin()
        {
            ResetDanger();
            if (_startSeconds <= 0f)
                return; // Missing or invalid config was logged by ResetDanger.
            _begun = true;
            _paused = false;
        }

        /// <summary>External pause (menu or level complete), independent of the ball's flight pause.</summary>
        public void Pause() => _paused = true;

        public void Resume()
        {
            if (!_begun || State == DangerState.Failed)
                return;
            _paused = false;
            if (!_inFlight)
                RefreshState(allowFailure: true);
        }

        public void ApplyShot(ShotInfo shot)
        {
            if (!IsRunning)
                return;
            // Suspend first: even a cost that exhausts the budget must let this shot finish.
            _inFlight = true;
            RemainingSeconds = Mathf.Max(0f, RemainingSeconds - _shotCostSeconds);
            RefreshState(allowFailure: false);
            ProgressJumped?.Invoke(Progress01);
        }

        public void OnBallAtRest()
        {
            if (!_begun || !_inFlight || State == DangerState.Failed)
                return;
            _inFlight = false;
            // All goals met calls Pause before rest: the winning last shot must remain successful.
            if (!_paused)
                RefreshState(allowFailure: true);
        }

        /// <summary>Restore the configured budget, stopped. Begin starts the next attempt.</summary>
        public void ResetDanger()
        {
            _begun = false;
            _paused = true;
            _inFlight = false;
            _startSeconds = 0f;
            RemainingSeconds = 0f;

            if (config == null)
            {
                SetState(DangerState.Normal);
                Debug.LogError("[Danger] Assign this level's DangerConfig before starting the timer.", this);
                return;
            }
            if (!config.TryValidate(out var error))
            {
                SetState(DangerState.Normal);
                Debug.LogError("[Danger] Invalid DangerConfig: " + error, this);
                return;
            }

            _startSeconds = config.StartSeconds;
            _shotCostSeconds = config.ShotCostSeconds;
            _warningBelowSeconds = config.WarningBelowSeconds;
            _criticalBelowSeconds = config.CriticalBelowSeconds;
            RemainingSeconds = _startSeconds;
            RefreshState(allowFailure: false);
        }

        void Update() => Tick(Time.deltaTime); // Scaled game time: timeScale 0 also freezes countdown.

        /// <summary>Deterministic countdown step, shared by Update and edit-mode tests.</summary>
        internal void Tick(float deltaSeconds)
        {
            if (!IsRunning || !DangerConfig.Finite(deltaSeconds) || deltaSeconds <= 0f)
                return;
            RemainingSeconds = Mathf.Max(0f, RemainingSeconds - deltaSeconds);
            RefreshState(allowFailure: true);
        }

        void RefreshState(bool allowFailure)
        {
            var next = RemainingSeconds <= 0f && allowFailure ? DangerState.Failed
                : RemainingSeconds <= _criticalBelowSeconds ? DangerState.Critical
                : RemainingSeconds <= _warningBelowSeconds ? DangerState.Warning
                : DangerState.Normal;
            SetState(next);
        }

        void SetState(DangerState state)
        {
            if (State == state)
                return;
            State = state;
            StateChanged?.Invoke(state);
            if (State != state)
                return; // A listener may already have reset/restarted the attempt.
            switch (state)
            {
                case DangerState.Warning: OnWarning?.Invoke(); break;
                case DangerState.Critical: OnCritical?.Invoke(); break;
                case DangerState.Failed: OnFailed?.Invoke(); break;
            }
        }
    }
}
