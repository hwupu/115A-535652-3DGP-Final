using System;

namespace Project3DGP.Game.Scoring
{
    /// <summary>
    /// Stroke count of the current level attempt (architecture §4.6). Interface by Henry (#20) for the HUD;
    /// ScoreKeeper (#18, Pongpat) implements it.
    /// </summary>
    public interface IStrokeCounter
    {
        int Strokes { get; }
        /// <summary>New stroke count (also after a retry resets it to 0).</summary>
        event Action<int> StrokesChanged;
    }
}
