using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Project3DGP.Game.Golf;
using Project3DGP.Game.Scoring;
using Project3DGP.Game.Session;
using UnityEngine;
using UnityEngine.TestTools;

namespace Project3DGP.Game.Tests
{
    /// <summary>GameSession (#89): one instance across scenes, starts with the starting clubs, unlocks keep slot order.</summary>
    public class GameSessionTests
    {
        ClubProfile _standard, _sledgehammer, _mysterious;
        readonly List<GameObject> _objects = new();

        [SetUp]
        public void SetUp()
        {
            _standard = Club("Standard");
            _sledgehammer = Club("Sledgehammer");
            _mysterious = Club("Mysterious");
        }

        [TearDown]
        public void TearDown()
        {
            // Immediately: the next test's session must not find this one still as Instance.
            foreach (var go in _objects)
                if (go != null) Object.DestroyImmediate(go);
            _objects.Clear();
            Object.Destroy(_standard);
            Object.Destroy(_sledgehammer);
            Object.Destroy(_mysterious);
        }

        static ClubProfile Club(string name)
        {
            var club = ScriptableObject.CreateInstance<ClubProfile>();
            club.name = name;
            return club;
        }

        // Like the prefab in a scene: inactive while configured, so Awake sees the club lists.
        GameSession Spawn()
        {
            var go = new GameObject("GameSession");
            go.SetActive(false);
            var session = go.AddComponent<GameSession>();
            session.ConfigureForTests(new List<ClubProfile> { _standard, _sledgehammer, _mysterious }, new List<ClubProfile> { _standard });
            _objects.Add(go);
            go.SetActive(true);
            return session;
        }

        [UnityTest]
        public IEnumerator FirstSession_IsTheInstance_ACopyRemovesItself()
        {
            var first = Spawn();
            var second = Spawn();   // e.g. the next scene's GameSession
            yield return null;      // Destroy happens at the end of the frame

            Assert.That(GameSession.Instance, Is.SameAs(first));
            Assert.That(second == null, Is.True, "the copy is destroyed");
        }

        [UnityTest]
        public IEnumerator StartsWithTheStandardClubOnly_UnlocksKeepSlotOrder()
        {
            var session = Spawn();
            yield return null;
            Assert.That(session.UnlockedClubs, Is.EqualTo(new[] { _standard }));

            session.Unlock(_mysterious);
            session.Unlock(_sledgehammer);
            Assert.That(session.UnlockedClubs, Is.EqualTo(new[] { _standard, _sledgehammer, _mysterious }), "order of allClubs, not of unlocking");
            Assert.That(session.Unlock(_sledgehammer), Is.False, "already unlocked");
        }

        [UnityTest]
        public IEnumerator Load_ReplacesTheData_AndRaisesChanged()
        {
            var session = Spawn();
            yield return null;
            session.RecordResult(new LevelResult("L1_Garden", 4, 20f, 90));
            var changed = 0;
            session.Changed += () => changed++;

            var saved = new GameSessionData { PlayerLevel = 4 };
            saved.Unlock("Standard");
            saved.Unlock("Mysterious");
            session.Load(saved);

            Assert.That(changed, Is.EqualTo(1));
            Assert.That(session.PlayerLevel, Is.EqualTo(4));
            Assert.That(session.UnlockedClubs, Is.EqualTo(new[] { _standard, _mysterious }));
            Assert.That(session.GetBestResult("L1_Garden"), Is.Null, "the loaded save had no results");
        }
    }
}
