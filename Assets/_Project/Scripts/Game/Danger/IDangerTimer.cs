using System;

namespace Project3DGP.Game.Danger
{
    /// <summary>Danger stage of the level timer (architecture §4.5). Failed = time is up = game over.</summary>
    public enum DangerState { Normal, Warning, Critical, Failed }

    /// <summary>
    /// What the HUD and level escalation read from the danger timer (architecture §4.5). Interface by Henry
    /// (#20), so the UI can be built and tested before DangerManager (#7, Pongpat) exists; DangerManager
    /// implements it. Read-only: only LevelManager drives the timer (Begin, Pause, ApplyShot, ...).
    /// </summary>
    public interface IDangerTimer
    {
        DangerState State { get; }
        float RemainingSeconds { get; }
        /// <summary>0 = full time left, 1 = time out (HUD bar).</summary>
        float Progress01 { get; }
        bool IsRunning { get; }

        /// <summary>The stage changed: HUD stage icon, level escalation (cat, sandstorm, ice).</summary>
        event Action<DangerState> StateChanged;
        /// <summary>The bar jumped after a shot's time cost (new Progress01): HUD jump effect.</summary>
        event Action<float> ProgressJumped;
    }
}
