using System;
using Project3DGP.Core.Motion;
using UnityEngine;

namespace Project3DGP.Game.Golf
{
    /// <summary>
    /// The golf ball (#14, architecture §4.1). A Rigidbody sphere moved by Unity physics.
    ///
    /// Life of one shot:
    ///   at rest (kinematic, frozen) --Hit(velocity)--> moving (physics) --RestDetector says stopped--> at rest
    ///   moving --enters an InvalidZone or falls below killHeight--> WentOutOfBounds, back to the last valid ground
    ///
    /// Rolling: PhysX spins the sphere from contact friction, so it rolls by itself. PhysX has no rolling
    /// resistance, so linear/angular damping slow it down and the RestDetector ends the shot.
    /// At rest the ball is kinematic: wind, ice and traps can't move it until the next Hit (PO, 2026-10-27).
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
    public class GolfBall : MonoBehaviour
    {
        [Header("Stopping")]
        [SerializeField] RestDetector rest = new(0.15f, 0.5f, 20f);
        [Tooltip("Air + rolling drag (Rigidbody linear damping) while moving.")]
        [SerializeField] float linearDamping = 0.5f;
        [Tooltip("Slows the spin, which (through friction) slows the rolling (Rigidbody angular damping).")]
        [SerializeField] float angularDamping = 2f;

        [Header("Out of bounds")]
        [Tooltip("Falling below this world height counts as out of bounds.")]
        [SerializeField] float killHeight = -20f;
        [Tooltip("A contact counts as ground if its normal points this much up (0.7 = slopes up to ~45 degrees).")]
        [SerializeField] float groundNormalMinY = 0.7f;

        Rigidbody _body;
        SphereCollider _sphere;
        const int HistorySize = 10;          // 10 samples x 0.1 s = the last second on the ground
        const float SampleInterval = 0.1f;
        readonly Vector3[] _groundHistory = new Vector3[HistorySize];   // ring buffer, newest at _historyHead
        int _historyHead, _historyCount;
        float _lastSampleTime = float.NegativeInfinity;
        Vector3 _lastRestPosition;    // where the ball last stopped: always safe, the fallback
        int _invalidZonesInside;      // InvalidZone triggers we are in right now

        public Vector3 Position => transform.position;
        public Vector3 Velocity => IsAtRest ? Vector3.zero : _body.linearVelocity;
        public bool IsAtRest { get; private set; } = true;
        /// <summary>Last position on valid ground; out of bounds puts the ball back here.</summary>
        public Vector3 LastValidPosition => FindLastValidPosition();
        /// <summary>True if the last stop was forced by the timeout (debug/HUD).</summary>
        public bool LastStopTimedOut => rest.TimedOut;

        /// <summary>Raised by Hit (the shot left the tee).</summary>
        public event Action Launched;
        /// <summary>Every collision while moving (sound, particles).</summary>
        public event Action<Collision> Collided;
        /// <summary>The ball stopped (or was reset): next shot allowed, avatar teleports.</summary>
        public event Action CameToRest;
        /// <summary>Entered an InvalidZone or fell below the kill height; raised before the reset.</summary>
        public event Action WentOutOfBounds;

        void Awake()
        {
            _body = GetComponent<Rigidbody>();
            _sphere = GetComponent<SphereCollider>();
            _body.linearDamping = linearDamping;
            _body.angularDamping = angularDamping;
            _body.interpolation = RigidbodyInterpolation.Interpolate;   // smooth on screen between physics steps
            _lastRestPosition = transform.position;
            Freeze();
        }

        /// <summary>Shoots the ball with this velocity (m/s). Called by ShotController only.</summary>
        public void Hit(Vector3 velocity)
        {
            IsAtRest = false;
            _body.isKinematic = false;
            // Continuous collision: a fast ball can't tunnel through a thin wall between two physics steps.
            _body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            _body.angularVelocity = Vector3.zero;
            _body.linearVelocity = velocity;
            rest.Begin();
            Launched?.Invoke();
        }

        /// <summary>Extra force from the level: wind (Desert), ice, traps. Ignored at rest (PO).</summary>
        public void AddExternalForce(Vector3 force, ForceMode mode)
        {
            if (!IsAtRest)
                _body.AddForce(force, mode);
        }

        /// <summary>Puts the ball back on the last valid ground, stopped, and raises CameToRest.</summary>
        public void ResetToLastValid()
        {
            var target = FindLastValidPosition();
            _body.position = target;
            transform.position = target;
            Stop();
        }

        void FixedUpdate()
        {
            if (IsAtRest)
                return;

            if (_body.position.y < killHeight)
            {
                GoOutOfBounds();
                return;
            }

            if (rest.Tick(_body.linearVelocity.magnitude, Time.fixedDeltaTime))
                Stop();
        }

        void Stop()
        {
            Freeze();
            IsAtRest = true;
            _invalidZonesInside = 0;
            _lastRestPosition = transform.position;
            _historyCount = 0;   // the stop position is the new fallback; start a fresh history
            CameToRest?.Invoke();
        }

        // Kinematic = physics no longer moves it (no rolling away, no wind) until the next Hit.
        void Freeze()
        {
            if (!_body.isKinematic)
            {
                _body.linearVelocity = Vector3.zero;
                _body.angularVelocity = Vector3.zero;
            }
            _body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;   // the mode kinematic bodies support
            _body.isKinematic = true;
        }

        void GoOutOfBounds()
        {
            WentOutOfBounds?.Invoke();
            ResetToLastValid();
        }

        void OnCollisionEnter(Collision collision)
        {
            if (!IsAtRest)
                Collided?.Invoke(collision);
        }

        // While rolling or bouncing on ground (not a wall, not inside an InvalidZone), remember the spot
        // every 0.1 s in a small ring buffer (the oldest sample is overwritten).
        void OnCollisionStay(Collision collision)
        {
            if (IsAtRest || _invalidZonesInside > 0 || Time.time - _lastSampleTime < SampleInterval)
                return;
            for (var i = 0; i < collision.contactCount; i++)
            {
                if (collision.GetContact(i).normal.y >= groundNormalMinY)
                {
                    _historyHead = (_historyHead + 1) % HistorySize;
                    _groundHistory[_historyHead] = _body.position;
                    _historyCount = Mathf.Min(_historyCount + 1, HistorySize);
                    _lastSampleTime = Time.time;
                    return;
                }
            }
        }

        // "Nearest valid position": the newest ground sample that is clear of every InvalidZone
        // (the newest one is often right at the pond's edge), else where the ball last stopped.
        Vector3 FindLastValidPosition()
        {
            for (var n = 0; n < _historyCount; n++)
            {
                var candidate = _groundHistory[(_historyHead - n + HistorySize) % HistorySize];
                if (!OverlapsInvalidZone(candidate))
                    return candidate;
            }
            return _lastRestPosition;
        }

        void OnTriggerEnter(Collider other)
        {
            if (IsAtRest || !other.TryGetComponent<InvalidZone>(out _))
                return;
            _invalidZonesInside++;
            GoOutOfBounds();
        }

        void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent<InvalidZone>(out _) && _invalidZonesInside > 0)
                _invalidZonesInside--;
        }

        bool OverlapsInvalidZone(Vector3 centre)
        {
            var radius = 2f * _sphere.radius * MaxScale(transform.lossyScale);   // x2: keep a little margin
            foreach (var hit in Physics.OverlapSphere(centre, radius, ~0, QueryTriggerInteraction.Collide))
            {
                if (hit.TryGetComponent<InvalidZone>(out _))
                    return true;
            }
            return false;
        }

        static float MaxScale(Vector3 s) => Mathf.Max(Mathf.Abs(s.x), Mathf.Abs(s.y), Mathf.Abs(s.z));
    }
}
