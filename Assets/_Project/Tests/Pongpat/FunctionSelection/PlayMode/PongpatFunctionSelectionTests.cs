using System.Collections;
using System.Reflection;
using NUnit.Framework;
using Project3DGP.Core.UI;
using Project3DGP.MiniEx.Pongpat.FunctionSelection;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Project3DGP.MiniEx.Pongpat.Tests
{
    public partial class PongpatFunctionSelectionTests
    {
        GameObject _root;
        FunctionSelectionDemo _demo;
        CharacterController _player;
        Transform _spawn;
        Transform[] _monsters;
        GameObject[] _avatars;
        GameObject[] _collectibles;
        Slider _volume;
        Slider _brightness;
        AudioSource _audio;
        Light _light;
        Text _status;

        IEnumerator CreateDemo()
        {
            _root = new GameObject("Function selection test fixture");
            _root.SetActive(false);
            var pages = new GameObject[4];
            for (var i = 0; i < pages.Length; i++)
                pages[i] = Child("Page " + i, _root.transform);

            var router = _root.AddComponent<PageRouter>();
            SetField(router, "pages", pages);
            var ground = Child("Test ground", pages[1].transform).AddComponent<BoxCollider>();
            ground.transform.position = new Vector3(0f, 99.5f, 0f);
            ground.size = new Vector3(30f, 1f, 30f);
            _spawn = Child("Spawn", pages[1].transform).transform;
            _spawn.position = new Vector3(0f, 100f, 0f);
            _player = Child("Player", pages[1].transform).AddComponent<CharacterController>();
            _player.height = 1.8f;
            _player.radius = 0.4f;
            _player.center = new Vector3(0f, 0.9f, 0f);
            _player.transform.position = _spawn.position;
            _avatars = new[] { Child("Amber", _player.transform), Child("Frost", _player.transform) };
            _monsters = new Transform[2];
            for (var i = 0; i < _monsters.Length; i++)
            {
                _monsters[i] = Child("Monster " + i, pages[1].transform).transform;
                _monsters[i].position = new Vector3(-5f + i * 10f, 101f, -5f);
            }

            _collectibles = new GameObject[3];
            for (var i = 0; i < _collectibles.Length; i++)
            {
                _collectibles[i] = Child("Seed " + i, pages[1].transform);
                _collectibles[i].transform.position = new Vector3(-4f + i * 4f, 100.5f, 4f);
            }

            _light = Child("Demo light", _root.transform).AddComponent<Light>();
            _audio = Child("Demo audio", _root.transform).AddComponent<AudioSource>();
            // This fixture checks routing and values without producing sound in the Test Runner.
            _audio.enabled = false;
            _volume = Ui<Slider>("Volume", pages[2].transform);
            _volume.value = 0.5f;
            _brightness = Ui<Slider>("Brightness", pages[2].transform);
            _brightness.value = 0.5f;
            _status = Ui<Text>("Level status", pages[1].transform);

            _demo = _root.AddComponent<FunctionSelectionDemo>();
            SetField(_demo, "router", router);
            SetField(_demo, "player", _player);
            SetField(_demo, "spawnPoint", _spawn);
            SetField(_demo, "avatarVisuals", _avatars);
            SetField(_demo, "monsters", _monsters);
            SetField(_demo, "collectibles", _collectibles);
            SetField(_demo, "demoLight", _light);
            SetField(_demo, "demoAudio", _audio);
            SetField(_demo, "volumeSlider", _volume);
            SetField(_demo, "brightnessSlider", _brightness);
            SetField(_demo, "levelStatus", _status);
            // Tests position the player directly; no synthetic keyboard or held real keys are needed.
            SetField(_demo, "moveSpeed", 0f);
            SetField(_demo, "patrolDistance", 0f);
            _root.SetActive(true);
            yield return null;
        }

        [TearDown]
        public void TearDown()
        {
            if (_root != null)
                Object.DestroyImmediate(_root);
        }

        static GameObject Child(string name, Transform parent)
        {
            var child = new GameObject(name);
            child.transform.SetParent(parent, false);
            return child;
        }

        static T Ui<T>(string name, Transform parent) where T : Component
        {
            var child = new GameObject(name, typeof(RectTransform), typeof(T));
            child.transform.SetParent(parent, false);
            return child.GetComponent<T>();
        }

        static void SetField(object target, string name, object value)
        {
            // Mimic Editor-authored references before the scene objects' first activation.
            var field = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, "Missing serialized field: " + name);
            field.SetValue(target, value);
        }

        void PositionPlayer(Vector3 position)
        {
            _player.enabled = false;
            _player.transform.position = new Vector3(position.x, _spawn.position.y, position.z);
            _player.enabled = true;
            Physics.SyncTransforms();
        }

        [UnityTest]
        public IEnumerator CharacterChoice_PersistsThroughMenuAndLevelNavigation()
        {
            yield return CreateDemo();
            _demo.OpenCharacters();
            _demo.SelectFrost();
            _demo.OpenLevel();
            yield return null;
            _demo.ReturnToMenu();
            _demo.OpenLevel();
            yield return null;

            Assert.That(_demo.SelectedCharacter, Is.EqualTo(1));
            Assert.That(_avatars[0].activeSelf, Is.False);
            Assert.That(_avatars[1].activeInHierarchy, Is.True);
        }

        [UnityTest]
        public IEnumerator SettingsSliders_ChangeDemoVolumeAndLight_WithoutChangingGlobalVolume()
        {
            yield return CreateDemo();
            var originalGlobalVolume = AudioListener.volume;
            var otherLight = Child("Unrelated light", _root.transform).AddComponent<Light>();
            otherLight.intensity = 2.3f;
            var initialIntensity = _light.intensity;
            _demo.OpenSettings();
            _volume.value = 0.23f;
            _brightness.value = 1f;
            yield return null;

            Assert.That(_audio.volume, Is.EqualTo(0.23f).Within(0.001f));
            Assert.That(_light.intensity, Is.GreaterThan(initialIntensity));
            Assert.That(otherLight.intensity, Is.EqualTo(2.3f));
            Assert.That(AudioListener.volume, Is.EqualTo(originalGlobalVolume));
            _demo.ReturnToMenu();
            _demo.OpenSettings();
            Assert.That(_volume.value, Is.EqualTo(0.23f).Within(0.001f));
            Assert.That(_brightness.value, Is.EqualTo(1f));
        }

        [UnityTest]
        public IEnumerator CollectedSeeds_SurviveMenu_AndRestartRestoresCompletedLevel()
        {
            yield return CreateDemo();
            _demo.OpenLevel();
            PositionPlayer(_collectibles[0].transform.position);
            yield return null;
            Assert.That(_demo.CollectedCount, Is.EqualTo(1));
            _demo.ReturnToMenu();
            yield return null;
            _demo.OpenLevel();
            yield return null;
            Assert.That(_demo.CollectedCount, Is.EqualTo(1));
            Assert.That(_collectibles[0].activeSelf, Is.False);

            for (var i = 1; i < _collectibles.Length; i++)
            {
                PositionPlayer(_collectibles[i].transform.position);
                yield return null;
            }
            Assert.That(_demo.LevelFinished, Is.True);
            _demo.RestartLevel();
            Assert.That(_demo.LevelFinished, Is.False);
            Assert.That(_demo.CollectedCount, Is.Zero);
            foreach (var collectible in _collectibles)
                Assert.That(collectible.activeSelf, Is.True);
            Assert.That(Vector3.Distance(_player.transform.position, _spawn.position), Is.LessThan(0.01f));
        }

        [UnityTest]
        public IEnumerator MonsterContact_ReturnsPlayerToSpawn_AndKeepsCollectedSeed()
        {
            yield return CreateDemo();
            _demo.OpenLevel();
            PositionPlayer(_collectibles[0].transform.position);
            yield return null;
            PositionPlayer(_monsters[0].position);
            yield return null;

            Assert.That(Vector3.Distance(_player.transform.position, _spawn.position), Is.LessThan(0.05f));
            Assert.That(_demo.CollectedCount, Is.EqualTo(1));
            Assert.That(_collectibles[0].activeSelf, Is.False);
            Assert.That(_status.text, Does.Contain("monster caught you"));
        }
    }
}
