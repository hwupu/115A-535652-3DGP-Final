using System.Collections.Generic;
using NUnit.Framework;
using Project3DGP.Core.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Project3DGP.Game.Tests
{
    /// <summary>Story intro / outro (#51): pages turn with Next, the callback runs once at the end or on Skip.</summary>
    public class StoryPagesTests
    {
        readonly List<GameObject> _objects = new();
        StoryPages _story;
        GameObject _panel;
        Text _page;
        int _done;

        [SetUp]
        public void SetUp()
        {
            _panel = New("Panel");
            _page = New("Page").AddComponent<Text>();
            _story = New("Story").AddComponent<StoryPages>();
            _story.Configure(_panel, _page, null, new[] { "one", "two", "three" });
            _done = 0;
        }

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
            _objects.Add(go);
            return go;
        }

        [Test]
        public void Next_TurnsThePages_ThenHidesAndCallsBackOnce()
        {
            _story.Play(() => _done++);
            Assert.That(_panel.activeSelf, Is.True);
            Assert.That(_page.text, Is.EqualTo("one"));

            _story.Next();
            Assert.That(_page.text, Is.EqualTo("two"));
            _story.Next();
            _story.Next();   // past the last page
            _story.Next();   // already finished: ignored

            Assert.That(_panel.activeSelf, Is.False);
            Assert.That(_done, Is.EqualTo(1));
        }

        [Test]
        public void Skip_EndsAtOnce()
        {
            _story.Play(() => _done++);
            _story.Skip();
            Assert.That(_panel.activeSelf, Is.False);
            Assert.That(_done, Is.EqualTo(1));
        }

        [Test]
        public void NoPages_CallsBackStraightAway()
        {
            _story.Configure(_panel, _page, null, new string[0]);
            _story.Play(() => _done++);
            Assert.That(_done, Is.EqualTo(1));
            Assert.That(_panel.activeSelf, Is.False);
        }
    }
}
