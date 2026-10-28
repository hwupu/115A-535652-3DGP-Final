using System;

namespace Project3DGP.Game.Levels.Garden
{
    /// <summary>The danger stage the cat reacts to. Same order as DangerState (#7): Normal, Warning, Critical, Failed.</summary>
    public enum CatAlert { Calm, Warning, Critical, TimeUp }

    /// <summary>What the cat is doing. Awake = time is up, the level is lost.</summary>
    public enum CatState { Asleep, Twitching, Suspicious, Searching, Awake }

    /// <summary>
    /// Level 1 cat state machine (#21). The cat follows the danger timer only (PO 2026-10-28: no noise):
    ///
    ///   Asleep --Warning--> Twitching --after SuspiciousAfterSeconds--> Suspicious
    ///      ^                                                               |
    ///      |                       any --Critical--> Searching             |
    ///      +--Calm (retry)--       any --TimeUp-->   Awake (game over)  <--+
    ///
    /// Coming back down from Critical to Warning (if time is ever given back) goes to Suspicious: the cat
    /// has already opened its eyes. Plain C#, so it is tested without a scene.
    /// </summary>
    [Serializable]
    public class CatAlertMachine
    {
        public float SuspiciousAfterSeconds = 4f;

        float _timeInState;

        public CatState State { get; private set; } = CatState.Asleep;
        public CatAlert Alert { get; private set; } = CatAlert.Calm;

        /// <summary>Old state, new state.</summary>
        public event Action<CatState, CatState> StateChanged;

        public void SetAlert(CatAlert alert)
        {
            if (alert == Alert)
                return;
            var wasCritical = Alert == CatAlert.Critical;
            Alert = alert;
            switch (alert)
            {
                case CatAlert.Calm:
                    Enter(CatState.Asleep);
                    break;
                case CatAlert.Warning:
                    Enter(wasCritical ? CatState.Suspicious : CatState.Twitching);
                    break;
                case CatAlert.Critical:
                    Enter(CatState.Searching);
                    break;
                case CatAlert.TimeUp:
                    Enter(CatState.Awake);
                    break;
            }
        }

        /// <summary>Call every frame: only Twitching changes by itself (to Suspicious after a while).</summary>
        public void Tick(float deltaTime)
        {
            _timeInState += deltaTime;
            if (State == CatState.Twitching && _timeInState >= SuspiciousAfterSeconds)
                Enter(CatState.Suspicious);
        }

        void Enter(CatState next)
        {
            _timeInState = 0f;
            if (next == State)
                return;
            var previous = State;
            State = next;
            StateChanged?.Invoke(previous, next);
        }
    }
}
