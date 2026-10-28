using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Project3DGP.Game.Controls;
using Project3DGP.Game.Golf;
using Project3DGP.Game.Scoring;
using Project3DGP.Game.Session;
using Project3DGP.Game.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;

namespace Project3DGP.Game.Tests
{
    /// <summary>
    /// Leveling (#70): unlocks at level-ups, XP for finished levels and collectibles, the ShotController following the
    /// unlocked clubs, and the level-up message.
    /// </summary>
    public class LevelingTests
    {
        readonly List<Object> _objects = new();
        ClubProfile _standard, _hammer;
        SkillProfile _skill;

        [SetUp]
        public void SetUp()
        {
            _standard = Asset<ClubProfile>("Standard");
            _standard.DisplayName = "Standard";
            _hammer = Asset<ClubProfile>("Sledgehammer");
            _hammer.DisplayName = "Sledgehammer";
            _skill = Asset<SkillProfile>("SkillA");
            _skill.DisplayName = "Skill A";
        }

        [TearDown]
        public void TearDown()
        {
            // Immediately and newest first: the next test's GameSession must not find this one as Instance.
            for (var i = _objects.Count - 1; i >= 0; i--)
                if (_objects[i] != null) Object.DestroyImmediate(_objects[i]);
            _objects.Clear();
        }

        T Track<T>(T o) where T : Object
        {
            _objects.Add(o);
            return o;
        }

        T Asset<T>(string name) where T : ScriptableObject
        {
            var asset = Track(ScriptableObject.CreateInstance<T>());
            asset.name = name;
            return asset;
        }

        // Levels 2 / 3 at 100 / 250 XP; Lv2 unlocks the hammer, Lv3 the skill.
        GameSession Session()
        {
            var go = Track(new GameObject("GameSession"));
            go.SetActive(false);
            var session = go.AddComponent<GameSession>();
            session.ConfigureForTests(new List<ClubProfile> { _standard, _hammer }, new List<ClubProfile> { _standard },
                new[] { 100, 250 },
                new List<LevelUnlock> { new() { Level = 2, Club = _hammer }, new() { Level = 3, Skill = _skill } });
            go.SetActive(true);
            return session;
        }

        [Test]
        public void LevelUp_UnlocksTheClubsAndSkillsOfEveryLevelPassed()
        {
            var session = Session();
            Assert.That(session.UnlockedClubs, Is.EqualTo(new[] { _standard }));

            session.AddXp(300);   // straight to level 3

            Assert.That(session.UnlockedClubs, Is.EqualTo(new[] { _standard, _hammer }));
            Assert.That(session.HasSkill(_skill), Is.True);
        }

        [Test]
        public void Load_AppliesTheUnlocksOfTheSavedLevel()
        {
            var session = Session();
            session.Load(new GameSessionData { PlayerLevel = 2, Xp = 120, UnlockedClubs = { "Standard" } });
            Assert.That(session.UnlockedClubs, Is.EqualTo(new[] { _standard, _hammer }), "a save from before the unlock existed");
            Assert.That(session.HasSkill(_skill), Is.False);
        }

        [Test]
        public void RecordResult_GivesXpForFinishing_AndForTimeLeft()
        {
            var session = Session();
            session.RecordResult(new LevelResult("L1_Garden", 3, 42.4f, 500));
            Assert.That(session.Xp, Is.EqualTo(100 + 42), "100 for finishing + 1 per second left");
            Assert.That(session.PlayerLevel, Is.EqualTo(2));
        }

        [Test]
        public void Collectible_GivesItsXpOnce_AndHides()
        {
            var session = Session();
            var seedObject = Track(new GameObject("Seed"));
            seedObject.AddComponent<SphereCollider>().isTrigger = true;
            var seed = seedObject.AddComponent<XpCollectible>();

            seed.Collect();
            seed.Collect();

            Assert.That(session.Xp, Is.EqualTo(seed.Xp));
            Assert.That(seedObject.activeSelf, Is.False);
        }

        [UnityTest]
        public IEnumerator ShotController_UsesTheUnlockedClubs_AndGetsNewOnes()
        {
            var session = Session();
            var actions = Track(InputActionAsset.FromJson(File.ReadAllText("Assets/_Project/Settings/Input/GolfControls.inputactions")));
            var input = Track(ScriptableObject.CreateInstance<GolfInput>());
            input.SetActions(actions);
            var ballObject = Track(GameObject.CreatePrimitive(PrimitiveType.Sphere));
            ballObject.AddComponent<Rigidbody>().isKinematic = true;
            var ball = ballObject.AddComponent<GolfBall>();
            var shotObject = Track(new GameObject("Shot"));
            shotObject.SetActive(false);
            var shot = shotObject.AddComponent<ShotController>();
            shot.Configure(input, ball, new[] { _standard, _hammer });   // the Inspector list: all clubs
            shotObject.SetActive(true);
            yield return null;   // Start reads the session

            Assert.That(shot.AvailableClubs, Is.EqualTo(new[] { _standard }), "only the unlocked club");
            session.AddXp(100);
            Assert.That(shot.AvailableClubs, Is.EqualTo(new[] { _standard, _hammer }), "the level-up added the hammer");
            Assert.That(shot.CurrentClub, Is.SameAs(_standard), "the current club stays");
            shotObject.SetActive(false);   // release the input before the assets go
        }

        [Test]
        public void LevelUpNotice_ListsWhatEveryPassedLevelUnlocked()
        {
            var session = Session();
            var notice = Track(new GameObject("Notice")).AddComponent<LevelUpNotice>();
            notice.Bind(session, null);
            Assert.That(notice.Message(1, 3), Is.EqualTo("Level 3! New: Sledgehammer, Skill A"));
            Assert.That(notice.Message(3, 4), Is.EqualTo("Level 4!"), "nothing unlocked");
        }
    }
}
