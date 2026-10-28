using System;
using UnityEngine;

namespace Project3DGP.Game.Levels.Garden
{
    /// <summary>
    /// Sir Grumblepaws, the Level 1 cat (#21). The danger timer decides the state (CatAlertMachine); this
    /// component shows it with simple code poses on the placeholder's "Visual" parts, until the cat model and
    /// its animations exist (#37). Time up = Awake = WokeUp: the level is lost (LevelManager handles the
    /// failure itself through DangerState.Failed, #27).
    ///   Asleep      lying, eyes closed, slow breathing
    ///   Twitching   lying, ears flick, tail swishes
    ///   Suspicious  lying, eyes half open, looks left and right
    ///   Searching   sits up, eyes open, follows the ball with its head
    ///   Awake       stands up, eyes wide, tail up, stares at the ball
    /// Wiring: DangerManager.StateChanged -> SetAlert (TEMP(#7): F1-F4 on GardenCatTestKeys until it exists).
    /// </summary>
    public class GardenCat : MonoBehaviour
    {
        [SerializeField] CatAlertMachine machine = new();
        [Tooltip("What the cat watches once it is suspicious (the ball).")]
        [SerializeField] Transform lookTarget;
        [Tooltip("How fast poses blend (1/s).")]
        [SerializeField] float blendSpeed = 4f;

        Transform _visual, _body, _earLeft, _earRight, _tail, _eyeLeft, _eyeRight;
        Vector3 _bodyScale, _eyeScale;
        Quaternion _earLeftRest, _earRightRest, _tailRest;

        // Current (smoothed) pose values.
        float _height, _pitch, _yaw, _eyesOpen, _tailLift;
        float _earFlickTimer;

        public CatState State => machine.State;
        public CatAlert Alert => machine.Alert;

        public event Action<CatState> StateChanged;
        /// <summary>Fires once when time is up and the cat wakes.</summary>
        public event Action WokeUp;

        void Awake()
        {
            _visual = transform.Find("Visual");
            _body = Part("Body");
            _earLeft = Part("EarLeft");
            _earRight = Part("EarRight");
            _tail = Part("Tail");
            _eyeLeft = Part("EyeLeft");
            _eyeRight = Part("EyeRight");
            if (_body != null) _bodyScale = _body.localScale;
            if (_eyeLeft != null) _eyeScale = _eyeLeft.localScale;
            if (_earLeft != null) _earLeftRest = _earLeft.localRotation;
            if (_earRight != null) _earRightRest = _earRight.localRotation;
            if (_tail != null) _tailRest = _tail.localRotation;

            var target = TargetPose(machine.State);
            (_height, _pitch, _eyesOpen, _tailLift) = (target.Height, target.Pitch, target.EyesOpen, target.TailLift);
        }

        void OnEnable() => machine.StateChanged += OnMachineStateChanged;
        void OnDisable() => machine.StateChanged -= OnMachineStateChanged;

        public void SetAlert(CatAlert alert) => machine.SetAlert(alert);

        void OnMachineStateChanged(CatState previous, CatState next)
        {
            StateChanged?.Invoke(next);
            if (next == CatState.Awake)
                WokeUp?.Invoke();
        }

        void Update()
        {
            machine.Tick(Time.deltaTime);
            if (_visual != null)
                Animate(Time.deltaTime);
        }

        // ---- Placeholder poses ----

        readonly struct Pose
        {
            public readonly float Height, Pitch, EyesOpen, TailLift, TailSwish, Breathing;
            public Pose(float height, float pitch, float eyesOpen, float tailLift, float tailSwish, float breathing)
            {
                Height = height; Pitch = pitch; EyesOpen = eyesOpen;
                TailLift = tailLift; TailSwish = tailSwish; Breathing = breathing;
            }
        }

        // Height of the Visual (m, lying is lower), pitch (degrees, + = nose down), eye openness 0..1,
        // tail lift (degrees up), tail swish amplitude (degrees), breathing amplitude (body scale).
        static Pose TargetPose(CatState state) => state switch
        {
            CatState.Asleep => new Pose(-0.25f, 6f, 0.1f, 0f, 4f, 0.05f),
            CatState.Twitching => new Pose(-0.25f, 6f, 0.1f, 0f, 15f, 0.05f),
            CatState.Suspicious => new Pose(-0.2f, 2f, 0.5f, 10f, 10f, 0.03f),
            CatState.Searching => new Pose(0f, 0f, 1f, 20f, 30f, 0f),
            _ => new Pose(0.1f, -5f, 1.3f, 50f, 8f, 0f),   // Awake
        };

        void Animate(float dt)
        {
            var state = machine.State;
            var target = TargetPose(state);
            var blend = 1f - Mathf.Exp(-blendSpeed * dt);   // frame-rate independent smoothing
            _height = Mathf.Lerp(_height, target.Height, blend);
            _pitch = Mathf.Lerp(_pitch, target.Pitch, blend);
            _eyesOpen = Mathf.Lerp(_eyesOpen, target.EyesOpen, blend);
            _tailLift = Mathf.Lerp(_tailLift, target.TailLift, blend);
            _yaw = Mathf.LerpAngle(_yaw, LookYaw(state), blend);

            _visual.localPosition = new Vector3(0f, _height, 0f);
            _visual.localRotation = Quaternion.Euler(_pitch, _yaw, 0f);

            var time = Time.time;
            if (_body != null)
                _body.localScale = Vector3.Scale(_bodyScale, new Vector3(1f, 1f + target.Breathing * Mathf.Sin(time * 1.5f), 1f));
            if (_eyeLeft != null && _eyeRight != null)
                _eyeLeft.localScale = _eyeRight.localScale = Vector3.Scale(_eyeScale, new Vector3(1f, _eyesOpen, 1f));
            if (_tail != null)
                _tail.localRotation = _tailRest * Quaternion.Euler(-_tailLift, target.TailSwish * Mathf.Sin(time * 3f), 0f);
            AnimateEars(state, dt);
        }

        // Twitching: a quick ear flick about every 1.2 s.
        void AnimateEars(CatState state, float dt)
        {
            if (_earLeft == null || _earRight == null)
                return;
            var flick = 0f;
            if (state == CatState.Twitching)
            {
                _earFlickTimer = (_earFlickTimer + dt) % 1.2f;
                flick = _earFlickTimer < 0.15f ? 35f : 0f;
            }
            _earLeft.localRotation = _earLeftRest * Quaternion.Euler(0f, 0f, flick);
            _earRight.localRotation = _earRightRest * Quaternion.Euler(0f, 0f, -flick * 0.5f);
        }

        // Where the head points, relative to the cat's own forward (degrees).
        float LookYaw(CatState state)
        {
            switch (state)
            {
                case CatState.Suspicious:
                    return 30f * Mathf.Sin(Time.time * 0.8f);   // slow look left and right
                case CatState.Searching:
                case CatState.Awake:
                    if (lookTarget == null)
                        return 0f;
                    var local = transform.InverseTransformPoint(lookTarget.position);
                    // Atan2(x, z): angle seen from above, 0 = straight ahead. Clamp: the cat turns its head, not its body.
                    return Mathf.Clamp(Mathf.Atan2(local.x, local.z) * Mathf.Rad2Deg, -100f, 100f);
                default:
                    return 0f;
            }
        }

        Transform Part(string partName)
        {
            if (_visual == null)
                return null;
            foreach (var child in _visual.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == partName)
                    return child;
            }
            return null;
        }
    }
}
