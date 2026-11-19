namespace Project3DGP.Game.Flow
{
    /// <summary>
    /// Optional dev hook for GodKeys (#143), next to ILevelFlow: a level flow that implements it can be failed on
    /// demand. Implemented by the TEMP(#27) avatar wiring and the UI test flow; LevelManager (#27) can implement it
    /// too (stop the danger timer at 0 and fail as if the time ran out).
    /// </summary>
    public interface ILevelCheats
    {
        /// <summary>Time is up now: the level's fail scene plays, then the time-up dialog (LevelFailed).</summary>
        void ForceFail();
    }
}
