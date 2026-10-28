using System.Collections;
using NUnit.Framework;
using Project3DGP.Game.Golf;
using Project3DGP.Game.Scoring;
using UnityEngine;
using UnityEngine.TestTools;

namespace Project3DGP.Game.Tests
{
    /// <summary>
    /// BallHitTarget (#22) and the LevelGoal rules (§4.6): the ball rolls into a wall-like target; it is met once,
    /// shows it, ignores the ball while not accepting, and resets for a retry. Test-only objects.
    /// </summary>
    public class BallHitTargetTests
    {
        GameObject _ground, _targetObject, _shown, _hidden;
        GolfBall _ball;
        BallHitTarget _target;
        int _reached;

        [SetUp]
        public void SetUp()
        {
            Time.timeScale = 4f;
            _ground = GameObject.CreatePrimitive(PrimitiveType.Plane);

            var ballObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ballObject.transform.localScale = Vector3.one * 0.2f;
            ballObject.transform.position = new Vector3(0f, 0.1f, 0f);
            ballObject.AddComponent<Rigidbody>().mass = 0.1f;
            _ball = ballObject.AddComponent<GolfBall>();

            // A 2 m wide, solid target 3 m ahead; inactive while wiring so Awake sees the fields.
            _targetObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _targetObject.SetActive(false);
            _targetObject.transform.position = new Vector3(0f, 0.5f, 3f);
            _targetObject.transform.localScale = new Vector3(2f, 1f, 0.2f);
            _shown = new GameObject("Shown");
            _hidden = new GameObject("Hidden");
            _target = _targetObject.AddComponent<BallHitTarget>();
            _target.SetForTests(new[] { _shown }, new[] { _hidden });
            _targetObject.SetActive(true);

            _reached = 0;
            _target.Reached += _ => _reached++;
        }

        [TearDown]
        public void TearDown()
        {
            Time.timeScale = 1f;
            foreach (var go in new[] { _ground, _ball.gameObject, _targetObject, _shown, _hidden })
                Object.Destroy(go);
        }

        IEnumerator RollIntoTarget()
        {
            yield return new WaitForFixedUpdate();
            _ball.Hit(new Vector3(0f, 0f, 6f));
            for (var t = 0f; t < 1.5f; t += Time.fixedDeltaTime)
                yield return new WaitForFixedUpdate();
        }

        [UnityTest]
        public IEnumerator BallHits_GoalIsMetOnce_AndShowsIt()
        {
            Assert.That(_shown.activeSelf, Is.False, "starts not met");
            Assert.That(_hidden.activeSelf, Is.True);

            yield return RollIntoTarget();

            Assert.That(_target.IsMet, Is.True);
            Assert.That(_reached, Is.EqualTo(1), "Reached fires once even if the ball touches it again");
            Assert.That(_shown.activeSelf, Is.True);
            Assert.That(_hidden.activeSelf, Is.False);
        }

        [UnityTest]
        public IEnumerator NotAccepting_IgnoresTheBall()
        {
            _target.Accepting = false;   // like the cup before the other goals are met
            yield return RollIntoTarget();
            Assert.That(_target.IsMet, Is.False);
            Assert.That(_reached, Is.Zero);
        }

        [UnityTest]
        public IEnumerator ResetGoal_MakesItOpenAgain_ForRetry()
        {
            yield return RollIntoTarget();
            _target.ResetGoal();
            Assert.That(_target.IsMet, Is.False);
            Assert.That(_shown.activeSelf, Is.False);
            Assert.That(_hidden.activeSelf, Is.True);
        }
    }
}
