using System;
using UnityEngine;

namespace Project3DGP.Core.Interaction
{
    /// <summary>
    /// Detects impacts with dynamic bodies and optionally deactivates after a delay from the
    /// first impact. A pool owner handles Recycled, places the body, calls ResetImpact, then
    /// activates it again. No scene, pool, input, or game-specific types are required here.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class ImpactRecycleBody : MonoBehaviour
    {
        [SerializeField, Min(0f)] float recycleDelay = 2f;
        [SerializeField, Min(0f)] float minimumImpulse = 0.05f;
        [SerializeField] bool recyclingEnabled;

        float _elapsed;
        bool _recycled;

        public Rigidbody Body { get; private set; }
        public bool IsHit { get; private set; }

        /// <summary>Disabling recycling pauses the countdown. ResetImpact starts a fresh demonstration.</summary>
        public bool RecyclingEnabled { get => recyclingEnabled; set => recyclingEnabled = value; }
        public float RecycleDelay { get => recycleDelay; set => recycleDelay = Mathf.Max(0f, value); }

        /// <summary>One notification per qualifying collision, including subsequent impacts.</summary>
        public event Action<ImpactRecycleBody, Rigidbody> BodyCollided;

        /// <summary>Raised after this object is inactive; its owner can queue a safe respawn.</summary>
        public event Action<ImpactRecycleBody> Recycled;

        void Awake() => Body = GetComponent<Rigidbody>();

        public void ResetImpact()
        {
            IsHit = false;
            _elapsed = 0f;
            _recycled = false;
        }

        void OnCollisionEnter(Collision collision)
        {
            var other = collision.rigidbody;
            // Static floor/walls and kinematic scenery do not count as obstacle impacts.
            if (other == null || other.isKinematic ||
                collision.impulse.sqrMagnitude < minimumImpulse * minimumImpulse)
                return;

            BodyCollided?.Invoke(this, other);
            if (IsHit)
                return;

            // Later contacts never restart this timer, even during a long collision chain.
            IsHit = true;
            _elapsed = 0f;
        }

        void FixedUpdate()
        {
            if (!IsHit || !recyclingEnabled || _recycled)
                return;

            _elapsed += Time.fixedDeltaTime;
            if (_elapsed < recycleDelay)
                return;

            _recycled = true;
            gameObject.SetActive(false);
            Recycled?.Invoke(this);
        }
    }
}
