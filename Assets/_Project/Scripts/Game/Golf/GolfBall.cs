using System;
using UnityEngine;

namespace Project3DGP.Game.Golf
{
    /// <summary>
    /// The golf ball (#14, architecture §4.1). A Rigidbody sphere moved by Unity physics.
    /// API stub: the public members are final (others compile against them); rest detection,
    /// valid-ground tracking and out-of-bounds handling follow in the next PR.
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
    public class GolfBall : MonoBehaviour
    {
        Rigidbody _body;
        Vector3 _lastValidPosition;

        public Vector3 Position => transform.position;
        public Vector3 Velocity => _body != null ? _body.linearVelocity : Vector3.zero;
        public bool IsAtRest { get; private set; } = true;
        /// <summary>Last position on valid ground; out of bounds puts the ball back here.</summary>
        public Vector3 LastValidPosition => _lastValidPosition;

        /// <summary>Raised by Hit (the shot left the tee).</summary>
        public event Action Launched;
        /// <summary>Every collision while moving (sound, particles, noise gauge).</summary>
        public event Action<Collision> Collided;
        /// <summary>The ball stopped (or was reset): next shot allowed, avatar teleports.</summary>
        public event Action CameToRest;
        /// <summary>Entered an InvalidZone or fell below the kill height; raised before the reset.</summary>
#pragma warning disable CS0067 // raised by the out-of-bounds logic in the next PR
        public event Action WentOutOfBounds;
#pragma warning restore CS0067

        void Awake()
        {
            _body = GetComponent<Rigidbody>();
            _lastValidPosition = transform.position;
        }

        /// <summary>Shoots the ball with this velocity (m/s). Called by ShotController only.</summary>
        public void Hit(Vector3 velocity)
        {
            IsAtRest = false;
            _body.linearVelocity = velocity;
            Launched?.Invoke();
        }

        /// <summary>Extra force from the level: wind (Desert), ice, traps.</summary>
        public void AddExternalForce(Vector3 force, ForceMode mode) => _body.AddForce(force, mode);

        /// <summary>Puts the ball back on the last valid ground, stopped.</summary>
        public void ResetToLastValid()
        {
            _body.linearVelocity = Vector3.zero;
            _body.angularVelocity = Vector3.zero;
            _body.position = _lastValidPosition;
            transform.position = _lastValidPosition;
            IsAtRest = true;
            CameToRest?.Invoke();
        }

        void OnCollisionEnter(Collision collision) => Collided?.Invoke(collision);
    }
}
