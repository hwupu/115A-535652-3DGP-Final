using System;
using Project3DGP.Game.Danger;
using Project3DGP.Game.Scoring;

namespace Project3DGP.Game.Flow
{
    public enum LevelPhase { Intro, Playing, Completed, Failed }

    /// <summary>
    /// What the HUD, pause dialog and time-up dialog use from the level (architecture §4.7, §4.8). Interface by
    /// Henry (#20), so the UI can be built and tested before LevelManager (#27, Pongpat) exists; LevelManager
    /// implements it. Quitting the game is not here: the UI calls Application.Quit itself.
    /// </summary>
    public interface ILevelFlow
    {
        LevelPhase Phase { get; }
        IDangerTimer Danger { get; }
        IStrokeCounter Strokes { get; }

        bool IsPaused { get; }
        /// <summary>Pause dialog: Time.timeScale = 0, danger timer paused, shot input off (and back).</summary>
        void SetPaused(bool paused);
        /// <summary>Same level from the tee: timer, strokes and goals reset (also unpauses).</summary>
        void Retry();
        /// <summary>Unpauses and loads the MainMenu scene.</summary>
        void GoToMainMenu();

        event Action LevelStarted;
        /// <summary>All goals met; the hamster walks to the exit and the next level loads (no result screen).</summary>
        event Action<LevelResult> LevelCompleted;
        /// <summary>Time is up. The level's own fail scene (e.g. the cat wakes up) plays, then the UI shows the time-up dialog.</summary>
        event Action LevelFailed;
        event Action<bool> PausedChanged;
    }
}
