using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Project3DGP.Core.UI;
using Project3DGP.Game.Danger;
using Project3DGP.Game.Flow;
using Project3DGP.Game.Session;
using Project3DGP.Game.UI;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Project3DGP.Game.Tests
{
    /// <summary>
    /// HUD, level dialogs and settings (#20) against the stand-in level (UiTestLevelFlow) and save slot
    /// (UiTestSaveStore), the same interfaces LevelManager, DangerManager, ScoreKeeper and Save &amp; Load implement.
    /// </summary>
    public class GameUiTests
    {
        readonly List<GameObject> _objects = new();
        UiTestLevelFlow _flow;

        [SetUp]
        public void SetUp()
        {
            var go = New("Flow", active: false);
            _flow = go.AddComponent<UiTestLevelFlow>();
            _flow.SetForTests(total: 100f, cost: 10f);
            go.SetActive(true);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var go in _objects)
                if (go != null) Object.Destroy(go);
            _objects.Clear();
            Time.timeScale = 1f;
        }

        GameObject New(string name, bool active = true)
        {
            var go = new GameObject(name);
            go.SetActive(active);
            _objects.Add(go);
            return go;
        }

        HudView Hud(out Image icon, out Text strokes, out GameObject group)
        {
            group = New("Timer");
            var bar = New("Bar").AddComponent<BarView>();
            icon = New("Icon").AddComponent<Image>();
            strokes = New("Strokes").AddComponent<Text>();
            var hud = New("Hud").AddComponent<HudView>();
            hud.SetForTests(group, bar, icon, strokes);
            return hud;
        }

        LevelMenus Menus(out GameObject pause, out GameObject confirm, out GameObject timeUp, out SettingsPanel settings,
            ISaveStore store, float delay = 0.1f)
        {
            pause = New("Pause");
            confirm = New("Confirm");
            timeUp = New("TimeUp");
            settings = New("Settings").AddComponent<SettingsPanel>();
            var menus = New("Menus").AddComponent<LevelMenus>();
            menus.SetForTests(pause, confirm, timeUp, settings, store, delay);
            menus.Bind(_flow);
            return menus;
        }

        [UnityTest]
        public IEnumerator Hud_ShowsStrokesAndStage()
        {
            var hud = Hud(out var icon, out var strokes, out var group);
            yield return null;   // the flow starts
            hud.Bind(_flow);
            Assert.That(group.activeSelf, Is.True, "timer shown with a danger timer");
            Assert.That(strokes.text, Is.EqualTo("Strokes: 0"));
            var normalColor = icon.color;

            _flow.TakeShot();
            Assert.That(strokes.text, Is.EqualTo("Strokes: 1"));

            _flow.SetRemaining(40f);   // progress 0.6 -> Warning
            Assert.That(_flow.State, Is.EqualTo(DangerState.Warning));
            Assert.That(icon.color, Is.Not.EqualTo(normalColor), "the stage icon changes with the stage");
        }

        [UnityTest]
        public IEnumerator Hud_WithoutLevel_HidesTimerAndStrokes()
        {
            Object.DestroyImmediate(_flow.gameObject);   // else Start finds it in the scene
            var hud = Hud(out _, out var strokes, out var group);
            hud.Bind(null);
            yield return null;
            Assert.That(group.activeSelf, Is.False);
            Assert.That(strokes.gameObject.activeSelf, Is.False);
            hud.OpenMenu();   // no level: nothing happens, no exception
        }

        [UnityTest]
        public IEnumerator MenuButton_PausesTheLevel_AndShowsThePauseDialog()
        {
            var hud = Hud(out _, out _, out _);
            var menus = Menus(out var pause, out _, out _, out _, null);
            yield return null;
            hud.Bind(_flow);

            hud.OpenMenu();
            Assert.That(_flow.IsPaused, Is.True);
            Assert.That(Time.timeScale, Is.EqualTo(0f));
            Assert.That(pause.activeSelf, Is.True);

            menus.Resume();
            Assert.That(_flow.IsPaused, Is.False);
            Assert.That(pause.activeSelf, Is.False);
        }

        [UnityTest]
        public IEnumerator Settings_OpensFromPause_AndBackReturnsToIt()
        {
            var menus = Menus(out var pause, out _, out _, out var settings, null);
            yield return null;
            _flow.SetPaused(true);

            menus.OpenSettings();
            Assert.That(settings.IsOpen, Is.True);
            Assert.That(pause.activeSelf, Is.False);

            menus.CloseSettings();
            Assert.That(settings.IsOpen, Is.False);
            Assert.That(pause.activeSelf, Is.True);
        }

        [UnityTest]
        public IEnumerator Save_WritesAtOnce_ThenAsksBeforeOverwriting()
        {
            var store = New("Store").AddComponent<UiTestSaveStore>();
            var menus = Menus(out var pause, out var confirm, out _, out _, store);
            yield return null;
            _flow.SetPaused(true);

            menus.Save();
            Assert.That(store.SaveCount, Is.EqualTo(1), "no save yet: saved without asking");
            Assert.That(confirm.activeSelf, Is.False);

            menus.Save();
            Assert.That(store.SaveCount, Is.EqualTo(1), "a save exists: asks first");
            Assert.That(confirm.activeSelf, Is.True);
            Assert.That(pause.activeSelf, Is.False);

            menus.CancelOverwrite();
            Assert.That(store.SaveCount, Is.EqualTo(1));
            Assert.That(pause.activeSelf, Is.True);

            menus.Save();
            menus.ConfirmOverwrite();
            Assert.That(store.SaveCount, Is.EqualTo(2));
            Assert.That(store.TryLoad(out var save), Is.True);
            Assert.That(save.LevelScene, Is.Not.Empty);
        }

        [UnityTest]
        public IEnumerator TimeUp_ShowsTheDialogAfterTheFailScene_RetryRestarts()
        {
            var menus = Menus(out _, out _, out var timeUp, out _, null, delay: 0.2f);
            yield return null;

            _flow.SetRemaining(0f);
            Assert.That(_flow.Phase, Is.EqualTo(LevelPhase.Failed));
            Assert.That(timeUp.activeSelf, Is.False, "the fail scene plays first");
            yield return new WaitForSecondsRealtime(0.3f);
            Assert.That(timeUp.activeSelf, Is.True);

            menus.Retry();
            Assert.That(_flow.RetryCount, Is.EqualTo(1));
            Assert.That(_flow.Phase, Is.EqualTo(LevelPhase.Playing));
            Assert.That(timeUp.activeSelf, Is.False);
        }

        [UnityTest]
        public IEnumerator SettingsPanel_SlidersWriteTheSettings()
        {
            var go = New("Settings", active: false);
            var panel = go.AddComponent<SettingsPanel>();
            var master = New("Master").AddComponent<Slider>();
            var music = New("Music").AddComponent<Slider>();
            var sfx = New("Sfx").AddComponent<Slider>();
            panel.SetForTests(master, music, sfx);
            var settings = new Settings { MasterVolume = 0.8f, MusicVolume = 0.5f, SfxVolume = 0.3f };

            panel.Open(settings);   // first activation: Awake connects the sliders
            yield return null;
            Assert.That(music.value, Is.EqualTo(0.5f).Within(1e-4f), "sliders show the settings");

            master.value = 0.4f;
            sfx.value = 0.9f;
            Assert.That(settings.MasterVolume, Is.EqualTo(0.4f).Within(1e-4f));
            Assert.That(settings.SfxVolume, Is.EqualTo(0.9f).Within(1e-4f));
            Assert.That(AudioListener.volume, Is.EqualTo(0.4f).Within(1e-4f), "master applies at once");
            AudioListener.volume = 1f;
        }
    }
}
