using System;
using System.Collections.Generic;
using Project3DGP.Game.Controls;
using Project3DGP.Game.Golf;
using UnityEngine;
using UnityEngine.Rendering;

namespace Project3DGP.Game.Cameras
{
    /// <summary>What the camera is doing (architecture §4.4). Aim and Follow are the golf views; the others are
    /// the explore-mode views the player cycles with V while walking.</summary>
    public enum CameraView { Aim, Follow, ThirdPerson, FirstPerson, FreeFly, BallView }

    /// <summary>
    /// The camera man (#19). It moves the scene's Main Camera; it never moves the ball or the player.
    ///   Aim         orbits the ball while shooting (right mouse drag); AimYaw = where the shot goes
    ///   Follow      follows the ball in flight, a bit behind and above it
    ///   ThirdPerson behind the walking player, orbit with right drag
    ///   FirstPerson at the player's eyes (the player's model is hidden, only its shadow stays)
    ///   FreeFly     flies with WASD + Space / Left Ctrl, look with right drag; stops at walls (sphere cast)
    ///   BallView    orbits the resting ball from further away, to look at the next shot
    /// Every view computes a "wanted" camera pose; the real camera moves a fraction of the way there each frame
    /// (exponential smoothing), so switching views blends smoothly.
    /// Without PlayerModeController (FollowShotsAutomatically on) it follows shots by itself: Follow on ShotTaken,
    /// back to Aim on CameToRest.
    /// </summary>
    public class CameraRig : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The camera this rig moves (Cameras/Main Camera).")]
        [SerializeField] Camera targetCamera;
        [SerializeField] GolfInput input;
        [SerializeField] GolfBall ball;
        [SerializeField] ShotController shot;
        [Tooltip("The walking player (explore mode). Optional.")]
        [SerializeField] Transform player;
        [SerializeField] CameraView startView = CameraView.Aim;

        [Header("Orbit (Aim, Follow, Third person, Ball view)")]
        [Tooltip("Degrees per pixel of mouse movement while the right button is held.")]
        [SerializeField] float mouseSensitivity = 0.2f;
        [SerializeField] float startPitch = 20f;
        [SerializeField] float minOrbitPitch = 5f;
        [SerializeField] float maxOrbitPitch = 75f;
        [Tooltip("Height above the ball / player's feet the camera looks at (m).")]
        [SerializeField] float ballFocusHeight = 0.3f;
        [SerializeField] float playerFocusHeight = 0.8f;
        [SerializeField] float aimDistance = 5f;
        [SerializeField] float followDistance = 4f;
        [SerializeField] float thirdPersonDistance = 4f;
        [SerializeField] float ballViewDistance = 8f;

        [Header("First person and free fly")]
        [Tooltip("Eye height above the player's feet (m).")]
        [SerializeField] float eyeHeight = 0.25f;
        [SerializeField] float flySpeed = 8f;
        [Tooltip("Size of the free-fly camera's 'body' for collisions (m).")]
        [SerializeField] float flyRadius = 0.3f;
        [Tooltip("What the free-fly camera can't pass through.")]
        [SerializeField] LayerMask flyBlockers = ~0;

        [Header("Smoothing")]
        [Tooltip("How quickly the camera catches up (higher = snappier).")]
        [SerializeField] float sharpness = 10f;
        [Tooltip("Lower sharpness while following the ball, so the flight feels smooth.")]
        [SerializeField] float followSharpness = 6f;
        [Tooltip("First person and free fly need almost no lag.")]
        [SerializeField] float directSharpness = 30f;

        float _yaw, _pitch;           // the orbit / look angles in degrees, shared by all views
        Vector3 _flyPosition;         // the free-fly camera's own position
        bool _snap = true;            // first frame: jump to the wanted pose instead of blending
        float _shakeStrength, _shakeTime, _shakeDuration;
        Vector3 _lastShake;           // shake offset added last frame
        readonly List<(Renderer renderer, ShadowCastingMode mode)> _hidden = new();

        public CameraView View { get; private set; }
        /// <summary>Horizontal forward of the camera in degrees (0 = +Z, 90 = +X); the aim cone and shot follow it.</summary>
        public float AimYaw => _yaw;
        /// <summary>On: Follow on ShotTaken, Aim on CameToRest. PlayerModeController turns it off and sets the views itself.</summary>
        public bool FollowShotsAutomatically { get; set; } = true;
        public Transform Player { get => player; set => player = value; }

        public event Action<CameraView> ViewChanged;

        void Awake()
        {
            if (targetCamera == null)
                targetCamera = Camera.main;
            // Start looking the way the scene's camera looks.
            var forward = targetCamera.transform.forward;
            _yaw = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
            _pitch = startPitch;
            View = startView;
        }

        void OnEnable()
        {
            input.Acquire();
            if (shot != null)
                shot.ShotTaken += OnShotTaken;
            if (ball != null)
                ball.CameToRest += OnBallCameToRest;
        }

        void OnDisable()
        {
            input.Release();
            if (shot != null)
                shot.ShotTaken -= OnShotTaken;
            if (ball != null)
                ball.CameToRest -= OnBallCameToRest;
        }

        /// <summary>Switches the view; the camera blends there over the next frames.</summary>
        public void SetView(CameraView view)
        {
            if (view == View)
                return;
            if (View == CameraView.FirstPerson)
                ShowPlayer();
            if (view == CameraView.FirstPerson)
                HidePlayer();
            if (view == CameraView.FreeFly)
                _flyPosition = targetCamera.transform.position;   // start flying from where the camera is
            View = view;
            ViewChanged?.Invoke(view);
        }

        /// <summary>Turns the camera to this yaw (e.g. towards the ball when the player starts a shot).</summary>
        public void SetAimYaw(float yaw) => _yaw = yaw;

        /// <summary>Shakes the camera; the shake fades out over duration seconds.</summary>
        public void Shake(float strength, float duration)
        {
            _shakeStrength = strength;
            _shakeDuration = Mathf.Max(0.01f, duration);
            _shakeTime = _shakeDuration;
        }

        void OnShotTaken(ShotInfo _)
        {
            if (FollowShotsAutomatically)
                SetView(CameraView.Follow);
        }

        void OnBallCameToRest()
        {
            if (FollowShotsAutomatically)
                SetView(CameraView.Aim);
        }

        // LateUpdate: after the ball and the player moved this frame, so the camera doesn't lag one frame behind.
        void LateUpdate()
        {
            var freeLook = View == CameraView.FirstPerson || View == CameraView.FreeFly;

            // 1. Right mouse drag turns the camera: mouse x -> yaw, mouse y -> pitch (looking up / down).
            var delta = input.OrbitDelta;
            _yaw += delta.x * mouseSensitivity;
            _pitch -= delta.y * mouseSensitivity;
            _pitch = freeLook ? Mathf.Clamp(_pitch, -80f, 80f) : Mathf.Clamp(_pitch, minOrbitPitch, maxOrbitPitch);

            // 2. Where the camera wants to be for the current view.
            var rotation = Quaternion.Euler(_pitch, _yaw, 0f);
            var wanted = View switch
            {
                CameraView.Aim => Orbit(BallFocus(), aimDistance, rotation),
                CameraView.Follow => Orbit(BallFocus(), followDistance, rotation),
                CameraView.BallView => Orbit(BallFocus(), ballViewDistance, rotation),
                CameraView.ThirdPerson => Orbit(PlayerFeet() + Vector3.up * playerFocusHeight, thirdPersonDistance, rotation),
                CameraView.FirstPerson => PlayerFeet() + Vector3.up * eyeHeight,
                CameraView.FreeFly => Fly(rotation),
                _ => targetCamera.transform.position,
            };

            // 3. Move part of the way there. 1 - e^(-sharpness * dt) is the same blend at any frame rate.
            var cameraTransform = targetCamera.transform;
            var k = View == CameraView.Follow ? followSharpness : freeLook ? directSharpness : sharpness;
            var t = _snap ? 1f : 1f - Mathf.Exp(-k * Time.deltaTime);
            _snap = false;
            var position = Vector3.Lerp(cameraTransform.position - ShakeOffset(0f), wanted, t);
            cameraTransform.SetPositionAndRotation(position, Quaternion.Slerp(cameraTransform.rotation, rotation, t));

            // 4. Shake on top, fading out (applied after the blend so it doesn't build up).
            cameraTransform.position += ShakeOffset(Time.deltaTime);
        }

        // A point on a sphere around the focus: step back from the focus along the camera's forward.
        static Vector3 Orbit(Vector3 focus, float distance, Quaternion rotation) =>
            focus - rotation * Vector3.forward * distance;

        Vector3 BallFocus() => (ball != null ? ball.Position : Vector3.zero) + Vector3.up * ballFocusHeight;

        Vector3 PlayerFeet() => player != null ? player.position : BallFocus();

        // Free fly: move in the camera's own directions; a sphere cast stops the move just before any collider.
        Vector3 Fly(Quaternion rotation)
        {
            var move = input.Move;
            var direction = rotation * new Vector3(move.x, 0f, move.y) + Vector3.up * input.FlyVertical;
            var distance = Mathf.Min(direction.magnitude, 1f) * flySpeed * Time.deltaTime;
            if (distance > 0f)
            {
                direction.Normalize();
                if (Physics.SphereCast(_flyPosition, flyRadius, direction, out var hit, distance, flyBlockers,
                        QueryTriggerInteraction.Ignore))
                    distance = Mathf.Max(0f, hit.distance - 0.01f);   // stop just before the wall
                _flyPosition += direction * distance;
            }
            return _flyPosition;
        }

        // The offset of the current shake. Passing dt = 0 only reads it (to remove last frame's offset).
        Vector3 ShakeOffset(float dt)
        {
            if (dt <= 0f)
                return _lastShake;
            _shakeTime = Mathf.Max(0f, _shakeTime - dt);
            var fade = _shakeTime / Mathf.Max(0.01f, _shakeDuration);   // 1 at the start, 0 at the end
            _lastShake = fade > 0f ? UnityEngine.Random.insideUnitSphere * (_shakeStrength * fade) : Vector3.zero;
            return _lastShake;
        }

        // First person: the camera is inside the player's model, so draw only its shadow.
        void HidePlayer()
        {
            if (player == null)
                return;
            foreach (var renderer in player.GetComponentsInChildren<Renderer>())
            {
                _hidden.Add((renderer, renderer.shadowCastingMode));
                renderer.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
            }
        }

        void ShowPlayer()
        {
            foreach (var (renderer, mode) in _hidden)
                if (renderer != null)
                    renderer.shadowCastingMode = mode;
            _hidden.Clear();
        }
    }
}
