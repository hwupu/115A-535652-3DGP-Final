using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Project3DGP.Core.Cameras;
using Project3DGP.Core.UI;
using Project3DGP.Game.Controls;
using Project3DGP.Game.Golf;
using Project3DGP.Game.Session;
using Project3DGP.Game.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Project3DGP.Game.Tests
{
    /// <summary>
    /// HUD additions (#78): the XP curve on GameSession and the XP bar, the club panel against a real ShotController,
    /// and the flat minimap icon.
    /// </summary>
    public class HudAdditionsTests
    {
        readonly List<Object> _objects = new();

        [TearDown]
        public void TearDown()
        {
            // Immediately, newest first (the ShotController before its input): the next test's GameSession must not
            // find this one still as Instance.
            for (var i = _objects.Count - 1; i >= 0; i--)
                if (_objects[i] != null) Object.DestroyImmediate(_objects[i]);
            _objects.Clear();
        }

        T Track<T>(T o) where T : Object
        {
            _objects.Add(o);
            return o;
        }

        GameObject New(string name, bool active = true)
        {
            var go = Track(new GameObject(name));
            go.SetActive(active);
            return go;
        }

        // Levels 2, 3, 4 at 100, 250, 450 total XP: max level 4.
        GameSession Session()
        {
            var go = New("GameSession", active: false);
            var session = go.AddComponent<GameSession>();
            session.ConfigureForTests(new List<ClubProfile>(), new List<ClubProfile>(), new[] { 100, 250, 450 });
            go.SetActive(true);
            return session;
        }

        [Test]
        public void AddXp_LevelsUpAsOftenAsTheXpAllows_StopsAtTheMaxLevel()
        {
            var session = Session();
            var levelUps = new List<int>();
            session.LeveledUp += levelUps.Add;

            session.AddXp(50);
            Assert.That(session.PlayerLevel, Is.EqualTo(1));
            Assert.That(session.LevelProgress01, Is.EqualTo(0.5f).Within(0.001f));

            session.AddXp(220);   // 270: past level 2 (100) and level 3 (250) in one go
            Assert.That(session.PlayerLevel, Is.EqualTo(3));
            Assert.That(levelUps, Is.EqualTo(new[] { 3 }), "one event with the new level");
            Assert.That(session.LevelProgress01, Is.EqualTo(0.1f).Within(0.001f), "(270 - 250) / (450 - 250)");

            session.AddXp(1000);
            Assert.That(session.PlayerLevel, Is.EqualTo(4));
            Assert.That(session.IsMaxLevel, Is.True);
            Assert.That(session.LevelProgress01, Is.EqualTo(1f));
            Assert.That(session.Xp, Is.EqualTo(1270), "XP is the total; it doesn't reset at a level-up");
        }

        [Test]
        public void XpBar_ShowsTheLevel_AndPulsesOnLevelUp()
        {
            var session = Session();
            var bar = New("Bar").AddComponent<BarView>();
            var label = New("Level").AddComponent<Text>();
            var view = New("XpBar").AddComponent<XpBarView>();
            view.SetForTests(bar, label);
            view.Bind(session);
            Assert.That(label.text, Is.EqualTo("Lv 1"));

            session.AddXp(40);
            Assert.That(view.IsPulsing, Is.False, "no level-up yet");
            session.AddXp(60);
            Assert.That(label.text, Is.EqualTo("Lv 2"));
            Assert.That(view.IsPulsing, Is.True);
        }

        [UnityTest]
        public IEnumerator ClubPanel_ShowsUsableClubs_FollowsTheShotController_ClicksSelect()
        {
            var actions = Track(InputActionAsset.FromJson(File.ReadAllText("Assets/_Project/Settings/Input/GolfControls.inputactions")));
            var input = Track(ScriptableObject.CreateInstance<GolfInput>());
            input.SetActions(actions);
            var clubA = Track(ScriptableObject.CreateInstance<ClubProfile>());
            var clubB = Track(ScriptableObject.CreateInstance<ClubProfile>());
            var ballObject = Track(GameObject.CreatePrimitive(PrimitiveType.Sphere));
            ballObject.AddComponent<Rigidbody>().isKinematic = true;
            var ball = ballObject.AddComponent<GolfBall>();
            var shotObject = New("Shot", active: false);
            var shot = shotObject.AddComponent<ShotController>();
            shot.Configure(input, ball, new[] { clubA, clubB });
            shotObject.SetActive(true);

            var cards = new List<SelectableCard>();
            for (var i = 0; i < 3; i++)
                cards.Add(New($"Card_{i + 1}").AddComponent<SelectableCard>());
            var panel = New("ClubPanel", active: false).AddComponent<ClubPanelView>();
            panel.SetForTests(cards);
            panel.gameObject.SetActive(true);
            panel.Bind(shot);
            yield return null;

            Assert.That(cards[2].gameObject.activeSelf, Is.False, "two usable clubs: the third card is hidden");
            Assert.That(panel.SelectedIndex, Is.EqualTo(0), "the first club is current at start");

            shot.SelectClub(clubB);   // e.g. key 2
            Assert.That(panel.SelectedIndex, Is.EqualTo(1));

            cards[0].GetComponent<Button>().onClick.Invoke();   // a mouse click on card 1
            Assert.That(shot.CurrentClub, Is.SameAs(clubA));
            Assert.That(panel.SelectedIndex, Is.EqualTo(0));

            shot.InputEnabled = false;   // paused, or the ball is flying
            cards[1].GetComponent<Button>().onClick.Invoke();
            Assert.That(shot.CurrentClub, Is.SameAs(clubA), "no club change while the shot can't change clubs");
            shotObject.SetActive(false);   // release the input before the assets go
        }

        [Test]
        public void MinimapIcon_LiesFlatAboveTheParent_WithItsOwnSize()
        {
            var parent = New("Ball").transform;
            parent.SetPositionAndRotation(new Vector3(3f, 1f, 2f), Quaternion.Euler(40f, 70f, 10f));   // rolled
            parent.localScale = Vector3.one * 0.2f;
            var icon = New("Icon").AddComponent<MinimapIcon>();
            icon.transform.SetParent(parent, false);

            icon.Apply();

            Assert.That(Quaternion.Angle(icon.transform.rotation, Quaternion.Euler(90f, 0f, 0f)), Is.LessThan(0.01f));
            Assert.That(Vector3.Distance(icon.transform.position, new Vector3(3f, 6f, 2f)), Is.LessThan(0.001f), "5 m above");
            Assert.That(icon.transform.lossyScale.x, Is.EqualTo(1.5f).Within(0.001f), "default size, not shrunk by the ball's scale");
        }
    }
}
