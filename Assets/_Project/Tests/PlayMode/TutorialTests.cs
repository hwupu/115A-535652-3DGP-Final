using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Project3DGP.Core.UI;
using Project3DGP.Game.Levels.Garden;
using Project3DGP.Game.UI;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Project3DGP.Game.Tests
{
    /// <summary>DialogBar (Core) queues, skips and times out lines; GardenTutorial (#28) shows its hints once, by state.</summary>
    public class TutorialTests
    {
        readonly List<GameObject> _objects = new();

        [TearDown]
        public void TearDown()
        {
            foreach (var go in _objects)
                if (go != null) Object.Destroy(go);
            _objects.Clear();
        }

        GameObject New(string name, bool active = true)
        {
            var go = new GameObject(name);
            go.SetActive(active);
            _objects.Add(go);
            return go;
        }

        DialogBar Bar(float seconds, out GameObject panel)
        {
            panel = New("Panel");
            var text = New("Text").AddComponent<Text>();
            var bar = New("DialogBar").AddComponent<DialogBar>();
            bar.Configure(panel, text, seconds);
            return bar;
        }

        [UnityTest]
        public IEnumerator DialogBar_ShowsLinesInOrder_SkipAndTimeOut()
        {
            var bar = Bar(0.2f, out var panel);
            Assert.That(panel.activeSelf, Is.False, "hidden while empty");

            bar.Show("first");
            bar.Show("second");
            Assert.That(bar.CurrentText, Is.EqualTo("first"));
            Assert.That(panel.activeSelf, Is.True);

            bar.Skip();   // a click on the bar
            Assert.That(bar.CurrentText, Is.EqualTo("second"));

            yield return new WaitForSeconds(0.35f);
            Assert.That(bar.IsShowing, Is.False, "timed out, nothing queued");
            Assert.That(panel.activeSelf, Is.False);
        }

        [UnityTest]
        public IEnumerator GardenTutorial_StartHints_ThenDangerHintsOnce()
        {
            var flowObject = New("Flow", active: false);
            var flow = flowObject.AddComponent<UiTestLevelFlow>();
            flow.SetForTests(total: 100f, cost: 10f);
            flowObject.SetActive(true);

            var bar = Bar(30f, out _);
            var tutorialObject = New("Tutorial", active: false);
            var tutorial = tutorialObject.AddComponent<GardenTutorial>();
            tutorial.SetForTests(bar);
            tutorialObject.SetActive(true);
            yield return null;   // Start: the first hint, and the flow is found in the scene

            StringAssert.Contains("right mouse", bar.CurrentText);

            flow.SetRemaining(40f);   // Warning
            flow.SetRemaining(10f);   // Critical: replaces everything still waiting
            StringAssert.Contains("almost awake", bar.CurrentText);
            bar.Skip();
            Assert.That(bar.IsShowing, Is.False, "the queued start and warning hints were dropped");

            flow.Retry();             // back to Normal, then Warning again: not repeated in this attempt
            flow.SetRemaining(40f);
            Assert.That(bar.IsShowing, Is.False);
        }
    }
}
