namespace Project3DGP.Game.Scoring
{
    /// <summary>
    /// How a level attempt ended (architecture §4.6). Built by ScoreKeeper (#18), kept by GameSession (#89).
    /// Written in #89 (Henry) so GameSession can use it; Pongpat owns it from here (score formula: #43).
    /// </summary>
    public readonly struct LevelResult
    {
        public readonly string LevelId;          // "L1_Garden"
        public readonly int Strokes;
        public readonly float RemainingSeconds;
        public readonly int Score;

        public LevelResult(string levelId, int strokes, float remainingSeconds, int score)
        {
            LevelId = levelId;
            Strokes = strokes;
            RemainingSeconds = remainingSeconds;
            Score = score;
        }
    }
}
