using System.Collections.Generic;
using NUnit.Framework;
using Project3DGP.Game.Levels.Garden;

namespace Project3DGP.Game.Tests
{
    /// <summary>CatAlertMachine (#21): the Level 1 cat follows the danger stages only (no noise).</summary>
    public class CatAlertMachineTests
    {
        CatAlertMachine _machine;
        List<CatState> _entered;

        [SetUp]
        public void SetUp()
        {
            _machine = new CatAlertMachine { SuspiciousAfterSeconds = 4f };
            _entered = new List<CatState>();
            _machine.StateChanged += (_, next) => _entered.Add(next);
        }

        [Test]
        public void StartsAsleep_AndStaysAsleepWhileCalm()
        {
            _machine.Tick(60f);
            Assert.That(_machine.State, Is.EqualTo(CatState.Asleep));
            Assert.That(_entered, Is.Empty);
        }

        [Test]
        public void Warning_Twitches_ThenBecomesSuspiciousAfterAWhile()
        {
            _machine.SetAlert(CatAlert.Warning);
            Assert.That(_machine.State, Is.EqualTo(CatState.Twitching));
            _machine.Tick(3.9f);
            Assert.That(_machine.State, Is.EqualTo(CatState.Twitching), "not yet");
            _machine.Tick(0.2f);
            Assert.That(_machine.State, Is.EqualTo(CatState.Suspicious));
        }

        [Test]
        public void FullEscalation_GoesThroughEveryState_InOrder()
        {
            _machine.SetAlert(CatAlert.Warning);
            _machine.Tick(5f);
            _machine.SetAlert(CatAlert.Critical);
            _machine.SetAlert(CatAlert.TimeUp);
            Assert.That(_entered, Is.EqualTo(new[] { CatState.Twitching, CatState.Suspicious, CatState.Searching, CatState.Awake }));
        }

        [Test]
        public void TimeUp_WakesTheCat_FromAnyState_Once()
        {
            _machine.SetAlert(CatAlert.TimeUp);
            _machine.SetAlert(CatAlert.TimeUp);
            _machine.Tick(10f);
            Assert.That(_machine.State, Is.EqualTo(CatState.Awake));
            Assert.That(_entered, Is.EqualTo(new[] { CatState.Awake }), "StateChanged fires once");
        }

        [Test]
        public void BackFromCritical_ToWarning_StaysSuspicious_NotTwitching()
        {
            _machine.SetAlert(CatAlert.Critical);
            _machine.SetAlert(CatAlert.Warning);   // if time is ever given back
            Assert.That(_machine.State, Is.EqualTo(CatState.Suspicious));
        }

        [Test]
        public void Calm_PutsTheCatBackToSleep_ForRetry()
        {
            _machine.SetAlert(CatAlert.TimeUp);
            _machine.SetAlert(CatAlert.Calm);
            Assert.That(_machine.State, Is.EqualTo(CatState.Asleep));
            _machine.SetAlert(CatAlert.Warning);
            Assert.That(_machine.State, Is.EqualTo(CatState.Twitching), "a fresh attempt starts the escalation again");
        }
    }
}
