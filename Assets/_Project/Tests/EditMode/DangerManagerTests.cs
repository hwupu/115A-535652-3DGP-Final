using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Project3DGP.Game.Danger;
using Project3DGP.Game.Golf;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Project3DGP.Game.Tests
{
    /// <summary>Danger timer (#7): aiming time, shot costs, flight/menu pauses and the final-shot rule.</summary>
    public class DangerManagerTests
    {
        GameObject _object;
        DangerManager _timer;
        DangerConfig _config;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<DangerConfig>();
            // Fixture values, not the levels' balance settings.
            _config.StartSeconds = 100f;
            _config.ShotCostSeconds = 10f;
            _config.WarningBelowSeconds = 30f;
            _config.CriticalBelowSeconds = 10f;
            _object = new GameObject("Danger timer test");
            _timer = _object.AddComponent<DangerManager>();
            AssignConfig(_config);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_object);
            Object.DestroyImmediate(_config);
        }

        void AssignConfig(DangerConfig config)
        {
            var serialized = new SerializedObject(_timer);
            serialized.FindProperty("config").objectReferenceValue = config;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        [Test]
        public void BeforeBegin_ControlsCannotStartOrChargeTheTimer()
        {
            _timer.Resume();
            _timer.ApplyShot(default);
            _timer.OnBallAtRest();
            _timer.Tick(200f);

            Assert.That(_timer.IsRunning, Is.False);
            Assert.That(_timer.State, Is.EqualTo(DangerState.Normal));
            Assert.That(_timer.Progress01, Is.Zero);
        }

        [Test]
        public void Begin_StartsWithTheConfiguredBudget()
        {
            _timer.Begin();

            Assert.That(_timer.IsRunning, Is.True);
            Assert.That(_timer.RemainingSeconds, Is.EqualTo(100f));
            Assert.That(_timer.Progress01, Is.Zero);
            Assert.That(_timer.State, Is.EqualTo(DangerState.Normal));
        }

        [Test]
        public void Aiming_CountsDownAndRaisesStageEventsAtRemainingTimeThresholds()
        {
            var states = new List<DangerState>();
            var warning = 0;
            var critical = 0;
            var failed = 0;
            var jumps = 0;
            _timer.StateChanged += states.Add;
            _timer.OnWarning += () => warning++;
            _timer.OnCritical += () => critical++;
            _timer.OnFailed += () => failed++;
            _timer.ProgressJumped += _ => jumps++;
            _timer.Begin();

            _timer.Tick(69f);
            Assert.That(_timer.RemainingSeconds, Is.EqualTo(31f));
            Assert.That(_timer.State, Is.EqualTo(DangerState.Normal));
            _timer.Tick(1f);
            Assert.That(_timer.State, Is.EqualTo(DangerState.Warning));
            Assert.That(_timer.Progress01, Is.EqualTo(0.7f).Within(0.0001f));
            _timer.Tick(19f);
            Assert.That(_timer.State, Is.EqualTo(DangerState.Warning));
            _timer.Tick(1f);
            Assert.That(_timer.State, Is.EqualTo(DangerState.Critical));
            _timer.Tick(10f);

            Assert.That(_timer.State, Is.EqualTo(DangerState.Failed));
            Assert.That(_timer.RemainingSeconds, Is.Zero);
            Assert.That(_timer.Progress01, Is.EqualTo(1f));
            Assert.That(_timer.IsRunning, Is.False);
            Assert.That(states, Is.EqualTo(new[] { DangerState.Warning, DangerState.Critical, DangerState.Failed }));
            Assert.That(warning, Is.EqualTo(1));
            Assert.That(critical, Is.EqualTo(1));
            Assert.That(failed, Is.EqualTo(1));
            Assert.That(jumps, Is.Zero, "Continuous countdown is not a discrete shot jump.");
        }

        [Test]
        public void LargeTimeStep_ClampsAtZeroAndFailsOnlyOnce()
        {
            var failures = 0;
            _timer.OnFailed += () => failures++;
            _timer.Begin();
            _timer.Tick(1000f);
            _timer.Tick(1000f);
            _timer.Resume();
            _timer.OnBallAtRest();
            _timer.ApplyShot(default);

            Assert.That(_timer.RemainingSeconds, Is.Zero);
            Assert.That(_timer.Progress01, Is.EqualTo(1f));
            Assert.That(_timer.IsRunning, Is.False);
            Assert.That(failures, Is.EqualTo(1));
        }

        [Test]
        public void Shot_ChargesLevelCostImmediatelyAndSuspendsCountdownUntilRest()
        {
            var jumps = new List<float>();
            _timer.ProgressJumped += progress =>
            {
                jumps.Add(progress);
                Assert.That(_timer.IsRunning, Is.False, "The shot must be suspended before observers see its cost.");
                Assert.That(_timer.RemainingSeconds, Is.EqualTo(85f));
            };
            _timer.Begin();
            _timer.Tick(5f);
            _timer.ApplyShot(new ShotInfo(null, 90f, 1f, Vector3.forward * 20f));
            _timer.Tick(500f);
            _timer.ApplyShot(default); // A duplicate notification during flight must not charge twice.

            Assert.That(_timer.RemainingSeconds, Is.EqualTo(85f));
            Assert.That(jumps, Has.Count.EqualTo(1));
            Assert.That(jumps[0], Is.EqualTo(0.15f).Within(0.0001f));
            _timer.OnBallAtRest();
            Assert.That(_timer.IsRunning, Is.True);
            _timer.Tick(2f);
            Assert.That(_timer.RemainingSeconds, Is.EqualTo(83f));
        }

        [Test]
        public void ZeroCostShot_PublishesOneJumpAndStillPausesUntilRest()
        {
            _config.ShotCostSeconds = 0f;
            var jumps = new List<float>();
            _timer.ProgressJumped += jumps.Add;
            _timer.Begin();
            _timer.Tick(7f);
            _timer.ApplyShot(default);
            _timer.Tick(500f);
            _timer.ApplyShot(default);

            Assert.That(_timer.RemainingSeconds, Is.EqualTo(93f));
            Assert.That(_timer.IsRunning, Is.False);
            Assert.That(jumps, Has.Count.EqualTo(1));
            Assert.That(jumps[0], Is.EqualTo(0.07f).Within(0.0001f));
            _timer.OnBallAtRest();
            Assert.That(_timer.IsRunning, Is.True);
            _timer.Tick(3f);
            Assert.That(_timer.RemainingSeconds, Is.EqualTo(90f));
            Assert.That(jumps, Has.Count.EqualTo(1), "Rest resumes countdown without another shot jump.");
        }

        [Test]
        public void DifferentClubs_ChargeTheSamePerLevelCostAcrossAttempts()
        {
            ClubProfile precise = null;
            ClubProfile powerful = null;
            try
            {
                precise = ScriptableObject.CreateInstance<ClubProfile>();
                precise.DisplayName = "Precise test club";
                precise.MinSpeed = 2f;
                precise.MaxSpeed = 8f;
                precise.AimConeDegrees = 2f;
                precise.MarkerCycleSeconds = 2f;
                precise.Swing = SwingStyle.Tap;
                powerful = ScriptableObject.CreateInstance<ClubProfile>();
                powerful.DisplayName = "Powerful test club";
                powerful.MinSpeed = 8f;
                powerful.MaxSpeed = 28f;
                powerful.AimConeDegrees = 32f;
                powerful.MarkerCycleSeconds = 0.8f;
                powerful.Swing = SwingStyle.Hammer;

                _timer.Begin();
                _timer.Tick(5f);
                var beforePrecise = _timer.RemainingSeconds;
                _timer.ApplyShot(new ShotInfo(precise, 0f, 0.2f, Vector3.forward * 3f));
                var preciseCost = beforePrecise - _timer.RemainingSeconds;
                Assert.That(_timer.RemainingSeconds, Is.EqualTo(85f));

                _timer.Begin(); // Same level config, a new attempt using another club.
                _timer.Tick(5f);
                var beforePowerful = _timer.RemainingSeconds;
                _timer.ApplyShot(new ShotInfo(powerful, 90f, 1f, Vector3.right * 28f));
                var powerfulCost = beforePowerful - _timer.RemainingSeconds;

                Assert.That(preciseCost, Is.EqualTo(_config.ShotCostSeconds));
                Assert.That(powerfulCost, Is.EqualTo(preciseCost));
                Assert.That(_timer.RemainingSeconds, Is.EqualTo(85f));
                Assert.That(_timer.Progress01, Is.EqualTo(0.15f).Within(0.0001f));
            }
            finally
            {
                Object.DestroyImmediate(precise);
                Object.DestroyImmediate(powerful);
            }
        }

        [Test]
        public void ShotJump_CanSkipWarningAndReportsOnlyTheEnteredStage()
        {
            _config.ShotCostSeconds = 85f;
            var states = new List<DangerState>();
            var warnings = 0;
            var criticals = 0;
            _timer.StateChanged += states.Add;
            _timer.OnWarning += () => warnings++;
            _timer.OnCritical += () => criticals++;
            _timer.Begin();
            _timer.Tick(5f);
            _timer.ApplyShot(default);

            Assert.That(_timer.RemainingSeconds, Is.EqualTo(10f));
            Assert.That(states, Is.EqualTo(new[] { DangerState.Critical }));
            Assert.That(warnings, Is.Zero);
            Assert.That(criticals, Is.EqualTo(1));
        }

        [TestCase(10f)]
        [TestCase(5f)]
        public void ExhaustingShot_GetsItsFlightThenFailsAtRest(float remainingBeforeShot)
        {
            var failures = 0;
            _timer.OnFailed += () => failures++;
            _timer.Begin();
            _timer.Tick(100f - remainingBeforeShot);
            _timer.ApplyShot(default);
            _timer.Tick(500f);
            _timer.Resume(); // Closing a menu during flight cannot end the last shot early.

            Assert.That(_timer.RemainingSeconds, Is.Zero);
            Assert.That(_timer.Progress01, Is.EqualTo(1f));
            Assert.That(_timer.State, Is.EqualTo(DangerState.Critical));
            Assert.That(_timer.IsRunning, Is.False);
            Assert.That(failures, Is.Zero);

            _timer.OnBallAtRest();
            _timer.OnBallAtRest();
            _timer.Resume();
            Assert.That(_timer.State, Is.EqualTo(DangerState.Failed));
            Assert.That(_timer.IsRunning, Is.False);
            Assert.That(failures, Is.EqualTo(1));
        }

        [Test]
        public void WinningLastShot_CompletionPausePreventsFailureWhenTheBallStops()
        {
            var failures = 0;
            _timer.OnFailed += () => failures++;
            _timer.Begin();
            _timer.Tick(90f);
            _timer.ApplyShot(default);
            _timer.Pause(); // LevelManager completes the goals before CameToRest.
            _timer.OnBallAtRest();
            _timer.Tick(100f);
            _timer.OnBallAtRest();

            Assert.That(_timer.RemainingSeconds, Is.Zero);
            Assert.That(_timer.State, Is.EqualTo(DangerState.Critical));
            Assert.That(_timer.IsRunning, Is.False);
            Assert.That(failures, Is.Zero);
        }

        [Test]
        public void PauseWhileAiming_BlocksTimeAndShotsUntilResume()
        {
            _timer.Begin();
            _timer.Tick(5f);
            _timer.Pause();
            _timer.Tick(200f);
            _timer.ApplyShot(default);
            _timer.OnBallAtRest();

            Assert.That(_timer.RemainingSeconds, Is.EqualTo(95f));
            Assert.That(_timer.IsRunning, Is.False);
            _timer.Resume();
            _timer.Tick(5f);
            Assert.That(_timer.RemainingSeconds, Is.EqualTo(90f));
        }

        [Test]
        public void ResumeDuringFlight_DoesNotRemoveTheFlightPause()
        {
            _timer.Begin();
            _timer.ApplyShot(default);
            _timer.Pause();
            _timer.Resume();
            _timer.Tick(50f);

            Assert.That(_timer.RemainingSeconds, Is.EqualTo(90f));
            Assert.That(_timer.IsRunning, Is.False);
            _timer.OnBallAtRest();
            Assert.That(_timer.IsRunning, Is.True);
            _timer.Tick(5f);
            Assert.That(_timer.RemainingSeconds, Is.EqualTo(85f));
        }

        [Test]
        public void RestWhileMenuPaused_WaitsForResumeBeforeCountingDown()
        {
            _timer.Begin();
            _timer.ApplyShot(default);
            _timer.Pause();
            _timer.OnBallAtRest();
            _timer.Tick(50f);

            Assert.That(_timer.RemainingSeconds, Is.EqualTo(90f));
            Assert.That(_timer.IsRunning, Is.False);
            _timer.Resume();
            _timer.Tick(5f);
            Assert.That(_timer.RemainingSeconds, Is.EqualTo(85f));
        }

        [Test]
        public void ExhaustedShotRestingDuringMenuPause_FailsWhenResumed()
        {
            var failures = 0;
            _timer.OnFailed += () => failures++;
            _timer.Begin();
            _timer.Tick(90f);
            _timer.ApplyShot(default);
            _timer.Pause();
            _timer.OnBallAtRest();
            Assert.That(failures, Is.Zero);
            _timer.Resume();

            Assert.That(_timer.State, Is.EqualTo(DangerState.Failed));
            Assert.That(failures, Is.EqualTo(1));
        }

        [Test]
        public void ResetDuringFlight_RestoresFullBudgetStoppedAndClearsPendingFailure()
        {
            _timer.Begin();
            _timer.Tick(90f);
            _timer.ApplyShot(default);
            _timer.ResetDanger();
            _timer.OnBallAtRest();
            _timer.Resume();
            _timer.Tick(200f);

            Assert.That(_timer.RemainingSeconds, Is.EqualTo(100f));
            Assert.That(_timer.Progress01, Is.Zero);
            Assert.That(_timer.State, Is.EqualTo(DangerState.Normal));
            Assert.That(_timer.IsRunning, Is.False);
            _timer.Begin();
            _timer.Tick(5f);
            Assert.That(_timer.RemainingSeconds, Is.EqualTo(95f));
        }

        [Test]
        public void RetryAfterFailure_RestoresNormalAndAllowsNewStageEvents()
        {
            var warnings = 0;
            var failures = 0;
            _timer.OnWarning += () => warnings++;
            _timer.OnFailed += () => failures++;
            _timer.Begin();
            _timer.Tick(70f);
            _timer.Tick(30f);
            _timer.Begin();

            Assert.That(_timer.State, Is.EqualTo(DangerState.Normal));
            Assert.That(_timer.RemainingSeconds, Is.EqualTo(100f));
            Assert.That(_timer.IsRunning, Is.True);
            _timer.Tick(70f);
            _timer.Tick(30f);
            Assert.That(warnings, Is.EqualTo(2));
            Assert.That(failures, Is.EqualTo(2));
        }

        [Test]
        public void RetryFromStateChanged_DoesNotPublishAnOldFailureForTheNewAttempt()
        {
            var failures = 0;
            _timer.OnFailed += () => failures++;
            _timer.StateChanged += state =>
            {
                if (state == DangerState.Failed)
                    _timer.Begin();
            };
            _timer.Begin();
            _timer.Tick(100f);

            Assert.That(_timer.State, Is.EqualTo(DangerState.Normal));
            Assert.That(_timer.RemainingSeconds, Is.EqualTo(100f));
            Assert.That(_timer.IsRunning, Is.True);
            Assert.That(failures, Is.Zero);
        }

        [Test]
        public void ConfigChanges_ApplyOnTheNextAttempt()
        {
            _timer.Begin();
            _config.StartSeconds = 200f;
            _config.ShotCostSeconds = 20f;
            _timer.ApplyShot(default);
            Assert.That(_timer.RemainingSeconds, Is.EqualTo(90f));
            _timer.Begin();
            _timer.ApplyShot(default);
            Assert.That(_timer.RemainingSeconds, Is.EqualTo(180f));
            Assert.That(_timer.Progress01, Is.EqualTo(0.1f).Within(0.0001f));
        }

        [Test]
        public void MissingConfig_LogsAnActionableErrorAndStaysStopped()
        {
            AssignConfig(null);
            LogAssert.Expect(LogType.Error, "[Danger] Assign this level's DangerConfig before starting the timer.");
            _timer.Begin();
            _timer.Resume();
            _timer.Tick(200f);

            Assert.That(_timer.IsRunning, Is.False);
            Assert.That(_timer.Progress01, Is.Zero);
            Assert.That(_timer.State, Is.EqualTo(DangerState.Normal));
        }

        [TestCase("StartSeconds", 0f)]
        [TestCase("StartSeconds", -1f)]
        [TestCase("StartSeconds", float.NaN)]
        [TestCase("StartSeconds", float.PositiveInfinity)]
        [TestCase("ShotCostSeconds", -1f)]
        [TestCase("ShotCostSeconds", float.NaN)]
        [TestCase("ShotCostSeconds", float.PositiveInfinity)]
        [TestCase("WarningBelowSeconds", -1f)]
        [TestCase("WarningBelowSeconds", 101f)]
        [TestCase("WarningBelowSeconds", float.NaN)]
        [TestCase("WarningBelowSeconds", float.PositiveInfinity)]
        [TestCase("CriticalBelowSeconds", -1f)]
        [TestCase("CriticalBelowSeconds", 31f)]
        [TestCase("CriticalBelowSeconds", float.NaN)]
        [TestCase("CriticalBelowSeconds", float.PositiveInfinity)]
        public void InvalidConfig_LogsAndCannotRun(string field, float value)
        {
            typeof(DangerConfig).GetField(field).SetValue(_config, value);
            LogAssert.Expect(LogType.Error, new Regex("^\\[Danger\\] Invalid DangerConfig:"));
            _timer.Begin();
            _timer.Resume();
            _timer.Tick(200f);
            _timer.ApplyShot(default);

            Assert.That(_timer.IsRunning, Is.False);
            Assert.That(_timer.Progress01, Is.Zero);
            Assert.That(_timer.RemainingSeconds, Is.Zero);
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        [TestCase(float.NegativeInfinity)]
        public void InvalidDelta_DoesNotAddOrExhaustTime(float delta)
        {
            _timer.Begin();
            _timer.Tick(delta);

            Assert.That(_timer.RemainingSeconds, Is.EqualTo(100f));
            Assert.That(_timer.Progress01, Is.Zero);
            Assert.That(_timer.State, Is.EqualTo(DangerState.Normal));
            Assert.That(_timer.IsRunning, Is.True);
        }
    }
}
