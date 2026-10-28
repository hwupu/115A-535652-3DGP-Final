using System;
using System.Collections.Generic;
using Project3DGP.Game.Scoring;

namespace Project3DGP.Game.Session
{
    /// <summary>
    /// Everything that carries over between scenes (#89): unlocked clubs, player level and XP, the best result per
    /// level, this run's results, settings. Plain [Serializable] class with public fields, so save/load (#44) can write it with
    /// JsonUtility.ToJson and read it back with FromJson. Clubs are stored by asset name (e.g. "Standard").
    /// </summary>
    [Serializable]
    public class GameSessionData
    {
        public List<string> UnlockedClubs = new();
        public int PlayerLevel = 1;
        /// <summary>Total XP of this game; the level thresholds are on GameSession (xpToReachLevel).</summary>
        public int Xp;
        public List<BestResult> BestResults = new();
        /// <summary>Every level finished in this run, in order (the Ending totals). A new game starts empty.</summary>
        public List<BestResult> RunResults = new();
        public Settings Settings = new();

        public bool IsUnlocked(string clubName) => UnlockedClubs.Contains(clubName);

        /// <summary>Returns true if the club was locked before.</summary>
        public bool Unlock(string clubName)
        {
            if (string.IsNullOrEmpty(clubName) || IsUnlocked(clubName))
                return false;
            UnlockedClubs.Add(clubName);
            return true;
        }

        /// <summary>
        /// Adds the result to this run, and keeps it as the level's best if it is the first or a better one.
        /// Returns true if it is a new best.
        /// </summary>
        public bool RecordResult(LevelResult result)
        {
            RunResults.Add(BestResult.From(result));
            var index = BestResults.FindIndex(r => r.LevelId == result.LevelId);
            var candidate = BestResult.From(result);
            if (index < 0)
            {
                BestResults.Add(candidate);
                return true;
            }
            if (!candidate.IsBetterThan(BestResults[index]))
                return false;
            BestResults[index] = candidate;
            return true;
        }

        public LevelResult? GetBestResult(string levelId)
        {
            var index = BestResults.FindIndex(r => r.LevelId == levelId);
            return index < 0 ? null : BestResults[index].ToResult();
        }
    }

    /// <summary>A LevelResult in a form JsonUtility can write (LevelResult has readonly fields).</summary>
    [Serializable]
    public struct BestResult
    {
        public string LevelId;
        public int Strokes;
        public float RemainingSeconds;
        public int Score;

        public static BestResult From(LevelResult r) => new()
        {
            LevelId = r.LevelId, Strokes = r.Strokes, RemainingSeconds = r.RemainingSeconds, Score = r.Score,
        };

        public LevelResult ToResult() => new(LevelId, Strokes, RemainingSeconds, Score);

        /// <summary>Higher score wins; on a tie, fewer strokes; then more time left.</summary>
        public bool IsBetterThan(BestResult other)
        {
            if (Score != other.Score)
                return Score > other.Score;
            if (Strokes != other.Strokes)
                return Strokes < other.Strokes;
            return RemainingSeconds > other.RemainingSeconds;
        }
    }

    /// <summary>Player settings (#77 decides the menu; volumes 0..1).</summary>
    [Serializable]
    public class Settings
    {
        public float MasterVolume = 1f;
        public float MusicVolume = 1f;
        public float SfxVolume = 1f;
    }
}
