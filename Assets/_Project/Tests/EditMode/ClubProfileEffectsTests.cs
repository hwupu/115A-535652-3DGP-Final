using NUnit.Framework;
using Project3DGP.Game.Golf;
using UnityEditor;
using UnityEngine;

namespace Project3DGP.Game.Tests
{
    /// <summary>
    /// The shipped club assets (#16) have the trade-offs from architecture §4.2:
    /// Sledgehammer = far but inaccurate and loud, Mysterious = short but precise, Standard in between.
    /// Checks the real assets through ShotMath, so a balancing change that breaks the trade-off fails here.
    /// </summary>
    public class ClubProfileEffectsTests
    {
        const string Folder = "Assets/_Project/Settings/Clubs/";
        ClubProfile _standard, _sledgehammer, _mysterious;

        [SetUp]
        public void SetUp()
        {
            _standard = AssetDatabase.LoadAssetAtPath<ClubProfile>(Folder + "Standard.asset");
            _sledgehammer = AssetDatabase.LoadAssetAtPath<ClubProfile>(Folder + "Sledgehammer.asset");
            _mysterious = AssetDatabase.LoadAssetAtPath<ClubProfile>(Folder + "Mysterious.asset");
            Assert.That(_standard != null && _sledgehammer != null && _mysterious != null,
                "Club assets missing: run Magic > Game > Create Club Profiles.");
        }

        [Test]
        public void Range_SledgehammerFarthest_MysteriousShortest()
        {
            float FullPower(ClubProfile club) => ShotMath.Velocity(club, 0f, 1f, 0f).magnitude;
            Assert.That(FullPower(_sledgehammer), Is.GreaterThan(FullPower(_standard)));
            Assert.That(FullPower(_standard), Is.GreaterThan(FullPower(_mysterious)));
        }

        [Test]
        public void Accuracy_SledgehammerWidestCone_MysteriousNarrowest()
        {
            // Worst case = a shot at the edge of the cone (spread 1): how far off the aim does it go?
            float WorstMiss(ClubProfile club) => Mathf.Abs(ShotMath.ShotYaw(club, 0f, 1f));
            Assert.That(WorstMiss(_sledgehammer), Is.GreaterThan(WorstMiss(_standard)));
            Assert.That(WorstMiss(_standard), Is.GreaterThan(WorstMiss(_mysterious)));
        }

        [Test]
        public void Control_SledgehammerMarkerFastest_MysteriousSlowest()
        {
            // Shorter cycle = the marker moves faster = harder to stop at the right power.
            Assert.That(_sledgehammer.MarkerCycleSeconds, Is.LessThan(_standard.MarkerCycleSeconds));
            Assert.That(_standard.MarkerCycleSeconds, Is.LessThan(_mysterious.MarkerCycleSeconds));
        }

        [Test]
        public void Noise_SledgehammerIsLoudest()
        {
            Assert.That(_sledgehammer.Noise, Is.GreaterThan(_standard.Noise));
            Assert.That(_sledgehammer.Noise, Is.GreaterThan(_mysterious.Noise));
        }

        [Test]
        public void EveryClub_HasAValidRange()
        {
            foreach (var club in new[] { _standard, _sledgehammer, _mysterious })
            {
                Assert.That(club.MinSpeed, Is.GreaterThan(0f), club.name);
                Assert.That(club.MaxSpeed, Is.GreaterThan(club.MinSpeed), club.name);
                Assert.That(club.LaunchPitch, Is.InRange(0f, 80f), club.name);
            }
        }
    }
}
