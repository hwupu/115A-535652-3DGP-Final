using System.Collections;
using NUnit.Framework;
using Project3DGP.Game.Golf;
using UnityEngine;
using UnityEngine.TestTools;

namespace Project3DGP.Game.Tests
{
    /// <summary>
    /// GolfBall with real physics (#14). Each test builds a tiny world (ground + ball) in code, runs it
    /// at 4x speed and checks the ball's state and events. Test-only objects; the game never builds scenes.
    /// </summary>
    public class GolfBallTests
    {
        GameObject _ground;
        GolfBall _ball;
        int _restCount, _outCount;

        [SetUp]
        public void SetUp()
        {
            Time.timeScale = 4f;
            _ground = GameObject.CreatePrimitive(PrimitiveType.Plane);   // 10 m x 10 m
            _ground.transform.localScale = new Vector3(3f, 1f, 3f);       // 30 m x 30 m

            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.transform.localScale = Vector3.one * 0.2f;
            go.transform.position = new Vector3(0f, 0.1f, 0f);
            go.GetComponent<SphereCollider>().sharedMaterial = new PhysicsMaterial { dynamicFriction = 0.4f, staticFriction = 0.5f };
            go.AddComponent<Rigidbody>().mass = 0.1f;
            _ball = go.AddComponent<GolfBall>();
            _restCount = _outCount = 0;
            _ball.CameToRest += () => _restCount++;
            _ball.WentOutOfBounds += () => _outCount++;
        }

        [TearDown]
        public void TearDown()
        {
            Time.timeScale = 1f;
            Object.Destroy(_ball.gameObject);
            Object.Destroy(_ground);
        }

        IEnumerator WaitForRest(float maxSeconds)
        {
            var start = Time.time;
            while (_restCount == 0 && Time.time - start < maxSeconds)
                yield return new WaitForFixedUpdate();
        }

        [UnityTest]
        public IEnumerator Hit_RollsThenComesToRest()
        {
            yield return new WaitForFixedUpdate();
            _ball.Hit(new Vector3(0f, 2f, 4f));
            Assert.That(_ball.IsAtRest, Is.False);

            yield return WaitForRest(25f);

            Assert.That(_restCount, Is.EqualTo(1), "CameToRest raised once");
            Assert.That(_ball.IsAtRest, Is.True);
            Assert.That(_ball.LastStopTimedOut, Is.False, "stopped by the rest threshold, not the timeout");
            Assert.That(_ball.Velocity, Is.EqualTo(Vector3.zero));
            Assert.That(_ball.Position.z, Is.GreaterThan(1f), "it actually travelled forward");
            Assert.That(_outCount, Is.Zero);
        }

        [UnityTest]
        public IEnumerator AtRest_ExternalForceIsIgnored()
        {
            yield return new WaitForFixedUpdate();
            var start = _ball.Position;
            for (var i = 0; i < 10; i++)
            {
                _ball.AddExternalForce(Vector3.right * 50f, ForceMode.Force);   // strong wind
                yield return new WaitForFixedUpdate();
            }
            Assert.That(Vector3.Distance(_ball.Position, start), Is.LessThan(0.001f));
        }

        [UnityTest]
        public IEnumerator FallsOffTheEdge_ResetsToLastValidGround()
        {
            yield return new WaitForFixedUpdate();
            _ball.Hit(new Vector3(30f, 0f, 0f));   // rolls off the 15 m edge and falls to the kill height

            yield return WaitForRest(25f);

            Assert.That(_outCount, Is.EqualTo(1), "WentOutOfBounds raised");
            Assert.That(_restCount, Is.EqualTo(1), "then CameToRest after the reset");
            Assert.That(_ball.IsAtRest, Is.True);
            Assert.That(_ball.Position.y, Is.GreaterThan(0f), "back above the ground");
            Assert.That(Mathf.Abs(_ball.Position.x), Is.LessThan(15f), "back on the green");
        }

        [UnityTest]
        public IEnumerator EntersInvalidZone_ResetsOutsideTheZone()
        {
            var zone = new GameObject("Pond");
            zone.transform.position = new Vector3(0f, 0f, 6f);
            var box = zone.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = new Vector3(10f, 2f, 4f);   // z from 4 to 8
            zone.AddComponent<InvalidZone>();

            yield return new WaitForFixedUpdate();
            _ball.Hit(new Vector3(0f, 0f, 10f));   // rolls straight into the pond

            yield return WaitForRest(25f);

            Assert.That(_outCount, Is.EqualTo(1));
            Assert.That(_ball.IsAtRest, Is.True);
            Assert.That(_ball.Position.z, Is.LessThan(4f), "reset before the pond");
            Assert.That(_ball.Position.z, Is.GreaterThan(0.5f), "nearest valid ground, not the start");
            Object.Destroy(zone);
        }
    }
}
