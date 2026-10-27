using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project3DGP.Game.Controls
{
    /// <summary>
    /// The one place that knows about the Input System actions (#13, architecture §2 rule 6).
    /// It wraps the GolfControls actions asset and turns them into plain C# events and values,
    /// so ShotController and CameraRig never deal with bindings, devices or callbacks.
    ///
    /// Shared asset: every prefab that needs input references the same GolfInput.asset.
    /// A user calls Acquire() in OnEnable and Release() in OnDisable; the actions are on while anyone uses them.
    ///
    /// Two action maps:
    ///   Gameplay : orbit + look, shoot, club 1-3, club scroll, toggle view (locked during flight / pause)
    ///   System   : pause, retry (always on, so Esc still works while gameplay is locked)
    ///
    /// Default bindings (keyboard + mouse):
    ///   hold right mouse + move = orbit camera   left click = shoot
    ///   1 / 2 / 3 = club slot                     mouse wheel = previous / next club
    ///   V = toggle view                           Esc = pause    R = retry
    /// </summary>
    [CreateAssetMenu(menuName = "Project3DGP/Golf Input", fileName = "GolfInput")]
    public class GolfInput : ScriptableObject
    {
        [Tooltip("The GolfControls.inputactions asset.")]
        [SerializeField] InputActionAsset actions;
        [Tooltip("Seconds between two club changes from the wheel. A trackpad sends many small scroll events.")]
        [SerializeField] float scrollCooldown = 0.15f;
        [Tooltip("Ignore tiny scroll values (trackpad noise).")]
        [SerializeField] float scrollDeadZone = 0.01f;

        // Map and action names in GolfControls.inputactions.
        public const string GameplayMap = "Gameplay";
        public const string SystemMap = "System";

        InputActionMap _gameplay, _system;
        InputAction _orbit, _look, _shoot, _clubScroll, _toggleView, _pause, _retry;
        InputAction[] _clubSlots;
        int _users;              // how many components called Acquire() and not Release()
        bool _gameplayEnabled = true;
        float _lastScrollTime = float.NegativeInfinity;

        /// <summary>Left click: shoot at the current power marker (ShotController).</summary>
        public event Action Shoot;
        /// <summary>Keys 1-3: select club slot 0, 1 or 2 (ShotController).</summary>
        public event Action<int> ClubSlotSelected;
        /// <summary>Mouse wheel: -1 = previous club, +1 = next club (ShotController).</summary>
        public event Action<int> ClubCycled;
        /// <summary>V: switch the camera view (CameraRig, optional overview).</summary>
        public event Action ToggleView;
        /// <summary>Esc: open / close the menu (LevelManager).</summary>
        public event Action Pause;
        /// <summary>R: retry the level (LevelManager).</summary>
        public event Action Retry;

        /// <summary>True while the right mouse button is held (CameraRig orbits).</summary>
        public bool IsOrbiting => _gameplayEnabled && _orbit != null && _orbit.IsPressed();

        /// <summary>Mouse movement this frame in pixels, only while orbiting; zero otherwise.</summary>
        public Vector2 OrbitDelta => IsOrbiting ? _look.ReadValue<Vector2>() : Vector2.zero;

        public bool IsActive => _users > 0;

        /// <summary>
        /// Gameplay actions on/off. Off during the ball's flight and while paused (architecture §3);
        /// the System map (pause, retry) stays on.
        /// </summary>
        public bool GameplayEnabled
        {
            get => _gameplayEnabled;
            set
            {
                _gameplayEnabled = value;
                if (IsActive)
                    ApplyGameplayEnabled();
            }
        }

        /// <summary>Start using the input (call in OnEnable). The first user turns the actions on.</summary>
        public void Acquire()
        {
            if (_users++ > 0)
                return;
            Bind();
            _system.Enable();
            ApplyGameplayEnabled();
        }

        /// <summary>Stop using the input (call in OnDisable). The last user turns the actions off.</summary>
        public void Release()
        {
            if (_users == 0 || --_users > 0)
                return;
            Unbind();
        }

        // A ScriptableObject asset lives as long as the editor, so start every load clean.
        void OnEnable()
        {
            _users = 0;
            _gameplayEnabled = true;
        }

        void OnDisable() => Unbind();

        void ApplyGameplayEnabled()
        {
            if (_gameplayEnabled)
                _gameplay.Enable();
            else
                _gameplay.Disable();
        }

        // Find the actions by name and subscribe to their "performed" callbacks.
        void Bind()
        {
            if (actions == null)
                throw new InvalidOperationException($"{name}: assign the GolfControls actions asset.");

            _gameplay = actions.FindActionMap(GameplayMap, throwIfNotFound: true);
            _system = actions.FindActionMap(SystemMap, throwIfNotFound: true);
            _orbit = _gameplay.FindAction("Orbit", true);
            _look = _gameplay.FindAction("Look", true);
            _shoot = _gameplay.FindAction("Shoot", true);
            _clubScroll = _gameplay.FindAction("ClubScroll", true);
            _toggleView = _gameplay.FindAction("ToggleView", true);
            _clubSlots = new[]
            {
                _gameplay.FindAction("Club1", true),
                _gameplay.FindAction("Club2", true),
                _gameplay.FindAction("Club3", true),
            };
            _pause = _system.FindAction("Pause", true);
            _retry = _system.FindAction("Retry", true);

            _shoot.performed += OnShoot;
            _clubScroll.performed += OnClubScroll;
            _toggleView.performed += OnToggleView;
            foreach (var slot in _clubSlots)
                slot.performed += OnClubSlot;
            _pause.performed += OnPause;
            _retry.performed += OnRetry;
        }

        void Unbind()
        {
            if (_gameplay == null)
                return;

            _shoot.performed -= OnShoot;
            _clubScroll.performed -= OnClubScroll;
            _toggleView.performed -= OnToggleView;
            foreach (var slot in _clubSlots)
                slot.performed -= OnClubSlot;
            _pause.performed -= OnPause;
            _retry.performed -= OnRetry;
            _gameplay.Disable();
            _system.Disable();
            _gameplay = _system = null;
            _users = 0;
        }

        void OnShoot(InputAction.CallbackContext _) => Shoot?.Invoke();
        void OnToggleView(InputAction.CallbackContext _) => ToggleView?.Invoke();
        void OnPause(InputAction.CallbackContext _) => Pause?.Invoke();
        void OnRetry(InputAction.CallbackContext _) => Retry?.Invoke();

        // Club1 -> slot 0, Club2 -> slot 1, Club3 -> slot 2.
        void OnClubSlot(InputAction.CallbackContext context) =>
            ClubSlotSelected?.Invoke(Array.IndexOf(_clubSlots, context.action));

        // Wheel up (positive) = previous club, wheel down (negative) = next club.
        // One notch can be 120 on Windows or many small values on a Mac trackpad, so only the sign counts,
        // and a short cooldown (unscaled time, so it works while paused) turns a swipe into one step.
        void OnClubScroll(InputAction.CallbackContext context)
        {
            var value = context.ReadValue<float>();
            if (Mathf.Abs(value) < scrollDeadZone || Time.unscaledTime - _lastScrollTime < scrollCooldown)
                return;
            _lastScrollTime = Time.unscaledTime;
            ClubCycled?.Invoke(value > 0f ? -1 : 1);
        }

        /// <summary>For tests: use this actions asset instead of the serialized one.</summary>
        internal void SetActions(InputActionAsset asset) => actions = asset;
    }
}
