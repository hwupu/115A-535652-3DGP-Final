using System.Collections;
using NUnit.Framework;
using Project3DGP.Game.Flow;
using UnityEngine;
using UnityEngine.TestTools;

namespace Project3DGP.Game.Tests
{
    /// <summary>LevelExit (#38): closed until Open, which swaps the closed/open looks and fires Opened once.</summary>
    public class LevelExitTests
    {
        GameObject _root, _portal, _bars, _target;
        LevelExit _exit;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("Exit");
            _portal = new GameObject("Portal");
            _bars = new GameObject("Bars");
            _target = new GameObject("WalkTarget");
            _target.transform.position = new Vector3(1f, 0f, 2f);
            _exit = _root.AddComponent<LevelExit>();
            _exit.SetForTests(_target.transform, new[] { _portal }, new[] { _bars });
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var go in new[] { _root, _portal, _bars, _target })
                Object.Destroy(go);
        }

        [UnityTest]
        public IEnumerator StartsClosed_ShowingTheClosedLook()
        {
            yield return null;
            Assert.That(_exit.IsOpen, Is.False);
            Assert.That(_portal.activeSelf, Is.False, "portal hidden while closed");
            Assert.That(_bars.activeSelf, Is.True, "bars shown while closed");
            Assert.That(_exit.WalkTarget, Is.SameAs(_target.transform));
        }

        [UnityTest]
        public IEnumerator Open_SwapsTheLook_AndFiresOpenedOnce()
        {
            yield return null;
            var opened = 0;
            _exit.Opened += () => opened++;
            _exit.Open();
            _exit.Open();

            Assert.That(_exit.IsOpen, Is.True);
            Assert.That(opened, Is.EqualTo(1), "Opened fires once");
            Assert.That(_portal.activeSelf, Is.True);
            Assert.That(_bars.activeSelf, Is.False);
        }
    }
}
