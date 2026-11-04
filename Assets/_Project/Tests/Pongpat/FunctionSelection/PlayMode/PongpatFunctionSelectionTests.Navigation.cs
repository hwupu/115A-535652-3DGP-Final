using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Project3DGP.Core.UI;
using UnityEngine;

namespace Project3DGP.MiniEx.Pongpat.Tests
{
    public partial class PongpatFunctionSelectionTests
    {
        readonly List<GameObject> _objects = new List<GameObject>();

        [TearDown]
        public void TearDownRouterObjects()
        {
            for (var i = _objects.Count - 1; i >= 0; i--)
                if (_objects[i] != null)
                    Object.DestroyImmediate(_objects[i]);
            _objects.Clear();
        }

        GameObject New(string name)
        {
            var go = new GameObject(name);
            _objects.Add(go);
            return go;
        }

        static void SetField(PageRouter router, string name, object value)
        {
            // Mimic serialized scene references before the router's first activation.
            var field = typeof(PageRouter).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, "Missing serialized router field: " + name);
            field.SetValue(router, value);
        }

        PageRouter CreateRouter(GameObject[] pages, int initial = 0, int menu = 0)
        {
            var go = New("Always-active router");
            go.SetActive(false);
            var router = go.AddComponent<PageRouter>();
            SetField(router, "pages", pages);
            SetField(router, "initialIndex", initial);
            SetField(router, "menuIndex", menu);
            return router;
        }

        [Test]
        public void FirstActivation_ShowsOnlyInitialPage_AndCanOpenAnInitiallyHiddenPage()
        {
            var menu = New("Menu");
            var level = New("Level");
            var settings = New("Settings");
            menu.SetActive(false);
            level.SetActive(false);
            var router = CreateRouter(new[] { menu, level, settings });

            router.gameObject.SetActive(true);

            Assert.That(router.CurrentIndex, Is.EqualTo(0));
            Assert.That(menu.activeSelf, Is.True);
            Assert.That(level.activeSelf, Is.False);
            Assert.That(settings.activeSelf, Is.False);

            router.Show(1);

            Assert.That(router.CurrentIndex, Is.EqualTo(1));
            Assert.That(menu.activeSelf, Is.False);
            Assert.That(level.activeSelf, Is.True);
            Assert.That(settings.activeSelf, Is.False);
            Assert.That(router.gameObject.activeInHierarchy, Is.True);
        }

        [Test]
        public void InvalidOrMissingDestination_PreservesCurrentPage_WithoutEvents()
        {
            var menu = New("Menu");
            var settings = New("Settings");
            var router = CreateRouter(new[] { menu, null, settings });
            router.gameObject.SetActive(true);
            router.Show(2);
            var events = 0;
            router.PageChanged += _ => events++;

            router.Show(-1);
            router.Show(3);
            router.Show(1);

            Assert.That(router.CurrentIndex, Is.EqualTo(2));
            Assert.That(menu.activeSelf, Is.False);
            Assert.That(settings.activeSelf, Is.True);
            Assert.That(events, Is.Zero);
        }

        [Test]
        public void ReturnToMenu_UsesConfiguredMenu_AndEventsDescribeCompletedChanges()
        {
            var menu = New("Menu");
            var level = New("Level");
            var settings = New("Settings");
            var router = CreateRouter(new[] { level, menu, settings }, initial: 0, menu: 1);
            router.gameObject.SetActive(true);
            var changes = new List<int>();
            router.PageChanged += index =>
            {
                Assert.That(router.CurrentIndex, Is.EqualTo(index));
                Assert.That(index == 1 ? menu.activeSelf : settings.activeSelf, Is.True);
                Assert.That(level.activeSelf, Is.False);
                changes.Add(index);
            };

            router.Show(2);
            router.ReturnToMenu();
            router.ReturnToMenu();

            CollectionAssert.AreEqual(new[] { 2, 1 }, changes);
            Assert.That(menu.activeSelf, Is.True);
            Assert.That(settings.activeSelf, Is.False);
        }

        [Test]
        public void ReopeningSamePage_RepairsVisibility_WithoutReportingAnotherChange()
        {
            var menu = New("Menu");
            var settings = New("Settings");
            var router = CreateRouter(new[] { menu, settings });
            router.gameObject.SetActive(true);
            var events = 0;
            router.PageChanged += _ => events++;
            menu.SetActive(false);
            settings.SetActive(true);

            router.Show(0);

            Assert.That(menu.activeSelf, Is.True);
            Assert.That(settings.activeSelf, Is.False);
            Assert.That(events, Is.Zero);
        }
    }
}
