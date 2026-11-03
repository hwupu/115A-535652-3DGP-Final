using NUnit.Framework;
using Project3DGP.Game.Scoring;
using Project3DGP.Game.Session;
using UnityEngine;

namespace Project3DGP.Game.Tests
{
    /// <summary>GameSessionData (#89): unlocks, best result per level, and a JSON round trip for save/load (#44).</summary>
    public class GameSessionDataTests
    {
        [Test]
        public void NewData_StartsAtLevel1_WithNothing()
        {
            var data = new GameSessionData();
            Assert.That(data.PlayerLevel, Is.EqualTo(1));
            Assert.That(data.Xp, Is.Zero);
            Assert.That(data.UnlockedClubs, Is.Empty);
            Assert.That(data.GetBestResult("L1_Garden"), Is.Null);
        }

        [Test]
        public void Unlock_OnlyOnce()
        {
            var data = new GameSessionData();
            Assert.That(data.Unlock("Standard"), Is.True);
            Assert.That(data.Unlock("Standard"), Is.False);
            Assert.That(data.UnlockedClubs, Has.Count.EqualTo(1));
        }

        [Test]
        public void RecordResult_KeepsTheBestPerLevel()
        {
            var data = new GameSessionData();
            Assert.That(data.RecordResult(new LevelResult("L1_Garden", 5, 30f, 100)), Is.True, "first result");
            Assert.That(data.RecordResult(new LevelResult("L1_Garden", 3, 60f, 80)), Is.False, "lower score");
            Assert.That(data.RecordResult(new LevelResult("L1_Garden", 4, 10f, 100)), Is.True, "same score, fewer strokes");
            Assert.That(data.RecordResult(new LevelResult("L2_Desert", 9, 5f, 10)), Is.True, "another level");

            var best = data.GetBestResult("L1_Garden").Value;
            Assert.That(best.Score, Is.EqualTo(100));
            Assert.That(best.Strokes, Is.EqualTo(4));
            Assert.That(data.BestResults, Has.Count.EqualTo(2));
        }

        [Test]
        public void JsonRoundTrip_KeepsEverything()
        {
            var data = new GameSessionData { PlayerLevel = 3, Xp = 250 };
            data.Unlock("Standard");
            data.Unlock("Sledgehammer");
            data.RecordResult(new LevelResult("L1_Garden", 4, 42.5f, 120));
            data.Settings.MusicVolume = 0.3f;

            var copy = JsonUtility.FromJson<GameSessionData>(JsonUtility.ToJson(data));

            Assert.That(copy.PlayerLevel, Is.EqualTo(3));
            Assert.That(copy.Xp, Is.EqualTo(250));
            Assert.That(copy.UnlockedClubs, Is.EqualTo(new[] { "Standard", "Sledgehammer" }));
            Assert.That(copy.GetBestResult("L1_Garden").Value.RemainingSeconds, Is.EqualTo(42.5f));
            Assert.That(copy.Settings.MusicVolume, Is.EqualTo(0.3f));
        }

        [Test]
        public void RecordResult_AddsEveryResultToTheRun_EvenIfNotBest()
        {
            var data = new GameSessionData();
            data.RecordResult(new LevelResult("L1_Garden", 3, 40f, 900));
            data.RecordResult(new LevelResult("L1_Garden", 6, 10f, 300));   // worse: not a new best
            data.RecordResult(new LevelResult("L2_Desert", 4, 30f, 700));
            Assert.That(data.RunResults, Has.Count.EqualTo(3));
            Assert.That(data.RunResults[1].Strokes, Is.EqualTo(6));
            Assert.That(data.BestResults, Has.Count.EqualTo(2));
        }
    }
}
