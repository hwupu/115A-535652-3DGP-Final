using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Project3DGP.Game.Controls;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project3DGP.Game.Tests
{
    /// <summary>
    /// GolfInput with simulated devices (#13). InputTestFixture gives each test a fresh, fake Input System,
    /// so pressing keys here never touches the real keyboard and mouse.
    /// </summary>
    public class GolfInputTests : InputTestFixture
    {
        const string ActionsPath = "Assets/_Project/Settings/Input/GolfControls.inputactions";

        Keyboard _keyboard;
        Mouse _mouse;
        InputActionAsset _actions;
        GolfInput _input;
        readonly List<string> _events = new();

        public override void Setup()
        {
            base.Setup();
            _keyboard = InputSystem.AddDevice<Keyboard>();
            _mouse = InputSystem.AddDevice<Mouse>();

            // Load a fresh copy of the real actions file, so the test checks the shipped bindings.
            _actions = InputActionAsset.FromJson(File.ReadAllText(ActionsPath));
            _input = ScriptableObject.CreateInstance<GolfInput>();
            _input.SetActions(_actions);
            _input.Shoot += () => _events.Add("Shoot");
            _input.ClubSlotSelected += slot => _events.Add("Slot" + slot);
            _input.ClubCycled += step => _events.Add("Cycle" + step);
            _input.ToggleView += () => _events.Add("View");
            _input.Pause += () => _events.Add("Pause");
            _input.Retry += () => _events.Add("Retry");
            _input.Acquire();
        }

        public override void TearDown()
        {
            Object.DestroyImmediate(_input);
            Object.DestroyImmediate(_actions);
            _events.Clear();
            base.TearDown();
        }

        [Test]
        public void LeftClick_RaisesShoot()
        {
            Press(_mouse.leftButton);
            Assert.That(_events, Is.EqualTo(new[] { "Shoot" }));
        }

        [Test]
        public void NumberKeys_SelectSlotsZeroToTwo()
        {
            Press(_keyboard.digit1Key);
            Press(_keyboard.digit2Key);
            Press(_keyboard.digit3Key);
            Assert.That(_events, Is.EqualTo(new[] { "Slot0", "Slot1", "Slot2" }));
        }

        [Test]
        public void WheelDown_SelectsNextClub()
        {
            Set(_mouse.scroll.y, -120f);
            Assert.That(_events, Is.EqualTo(new[] { "Cycle1" }));
        }

        [Test]
        public void WheelUp_SelectsPreviousClub()
        {
            Set(_mouse.scroll.y, 120f);
            Assert.That(_events, Is.EqualTo(new[] { "Cycle-1" }));
        }

        [Test]
        public void Wheel_SecondNotchWithinCooldown_IsIgnored()
        {
            Set(_mouse.scroll.y, -120f);
            Set(_mouse.scroll.y, 0f);
            Set(_mouse.scroll.y, -120f);
            Assert.That(_events, Is.EqualTo(new[] { "Cycle1" }));
        }

        [Test]
        public void ViewPauseRetryKeys_RaiseTheirEvents()
        {
            Press(_keyboard.vKey);
            Press(_keyboard.escapeKey);
            Press(_keyboard.rKey);
            Assert.That(_events, Is.EqualTo(new[] { "View", "Pause", "Retry" }));
        }

        [Test]
        public void MouseMove_OrbitsOnlyWhileRightButtonHeld()
        {
            Set(_mouse.delta, new Vector2(5f, 3f));
            Assert.That(_input.IsOrbiting, Is.False);
            Assert.That(_input.OrbitDelta, Is.EqualTo(Vector2.zero));

            Press(_mouse.rightButton);
            Set(_mouse.delta, new Vector2(5f, 3f));
            Assert.That(_input.IsOrbiting, Is.True);
            Assert.That(_input.OrbitDelta, Is.EqualTo(new Vector2(5f, 3f)));
        }

        [Test]
        public void GameplayLocked_IgnoresShootButPauseStillWorks()
        {
            _input.GameplayEnabled = false;
            Press(_mouse.leftButton);
            Press(_keyboard.digit1Key);
            Press(_keyboard.escapeKey);
            Assert.That(_events, Is.EqualTo(new[] { "Pause" }));
        }

        [Test]
        public void Release_KeepsActionsOnUntilLastUser()
        {
            _input.Acquire();   // second user
            _input.Release();
            Assert.That(_input.IsActive, Is.True);
            Press(_mouse.leftButton);

            _input.Release();   // last user
            Assert.That(_input.IsActive, Is.False);
            Release(_mouse.leftButton);
            Press(_mouse.leftButton);
            Assert.That(_events, Is.EqualTo(new[] { "Shoot" }));
        }
    }
}
