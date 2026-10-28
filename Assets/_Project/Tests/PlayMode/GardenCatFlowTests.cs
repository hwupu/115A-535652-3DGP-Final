using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Project3DGP.Game.Levels.Garden;
using Project3DGP.Game.UI;
using UnityEngine;
using UnityEngine.TestTools;

namespace Project3DGP.Game.Tests
{
    /// <summary>
    /// L1 cat (#31): it follows the level's danger timer through ILevelFlow (the stand-in level here, DangerManager
    /// later): Critical = Searching, time up = Awake + WokeUp.
    /// </summary>
    public class GardenCatFlowTests
    {
        readonly List<GameObject> _objects = new();

        [TearDown]
        public void TearDown()
        {
            foreach (var go in _objects)
                if (go != null) Object.Destroy(go);
            _objects.Clear();
        }

        GameObject New(string name)
        {
            var go = new GameObject(name);
            go.SetActive(false);
            _objects.Add(go);
            return go;
        }

        [UnityTest]
        public IEnumerator Cat_FollowsTheDangerTimer_AndWakesAtTimeUp()
        {
            var flowObject = New("Flow");
            var flow = flowObject.AddComponent<UiTestLevelFlow>();
            flow.SetForTests(total: 100f, cost: 10f);   // Warning at 50 s left, Critical at 20 s left
            var catObject = New("Cat");
            var cat = catObject.AddComponent<GardenCat>();   // no "Visual" child: states only, no poses
            cat.SetFlowForTests(flow);
            var woke = 0;
            cat.WokeUp += () => woke++;
            flowObject.SetActive(true);
            catObject.SetActive(true);
            yield return null;   // both Starts: the level begins, the cat subscribes

            Assert.That(cat.State, Is.EqualTo(CatState.Asleep));
            flow.SetRemaining(40f);
            Assert.That(cat.State, Is.EqualTo(CatState.Twitching));
            flow.SetRemaining(15f);
            Assert.That(cat.State, Is.EqualTo(CatState.Searching));
            flow.SetRemaining(0f);
            Assert.That(cat.State, Is.EqualTo(CatState.Awake));
            Assert.That(woke, Is.EqualTo(1));
        }
    }
}
