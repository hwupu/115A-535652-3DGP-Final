using NUnit.Framework;
using Project3DGP.Game.Golf;
using UnityEngine;

namespace Project3DGP.Game.Tests
{
    /// <summary>ShotMath (#15): power -> speed, spread -> direction inside the cone, fixed pitch.</summary>
    public class ShotMathTests
    {
        ClubProfile _club;

        [SetUp]
        public void SetUp()
        {
            _club = ScriptableObject.CreateInstance<ClubProfile>();
            _club.MinSpeed = 4f;
            _club.MaxSpeed = 16f;
            _club.AimConeDegrees = 10f;
            _club.LaunchPitch = 30f;
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_club);

        [TestCase(0f, 4f)]
        [TestCase(0.5f, 10f)]
        [TestCase(1f, 16f)]
        [TestCase(2f, 16f)]   // clamped
        public void Speed_MapsPowerToClubRange(float power, float expected)
        {
            Assert.That(ShotMath.Speed(_club, power), Is.EqualTo(expected).Within(1e-4f));
        }

        [TestCase(-1f, 85f)]   // left edge: aim 90 - half cone 5
        [TestCase(0f, 90f)]
        [TestCase(1f, 95f)]
        [TestCase(3f, 95f)]    // clamped to the cone
        public void ShotYaw_StaysInsideTheCone(float spread, float expected)
        {
            Assert.That(ShotMath.ShotYaw(_club, 90f, spread), Is.EqualTo(expected).Within(1e-4f));
        }

        [Test]
        public void Velocity_HasSpeedPitchAndYaw()
        {
            var v = ShotMath.Velocity(_club, aimYaw: 90f, power01: 1f, spread: 0f);   // yaw 90 = +X

            Assert.That(v.magnitude, Is.EqualTo(16f).Within(1e-3f));
            var flat = new Vector3(v.x, 0f, v.z);
            Assert.That(Vector3.Angle(flat, Vector3.right), Is.LessThan(0.01f), "points along +X");
            Assert.That(Vector3.Angle(v, flat), Is.EqualTo(30f).Within(0.01f), "launch pitch above the ground");
            Assert.That(v.y, Is.GreaterThan(0f), "goes up, not down");
        }
    }
}
