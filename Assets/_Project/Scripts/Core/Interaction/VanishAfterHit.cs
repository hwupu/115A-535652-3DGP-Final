using System;
using UnityEngine;

namespace Project3DGP.Core.Interaction
{
    /// <summary>
    /// Marks an object as "hit" the first time a <see cref="RigidbodyPusher"/> pushes it or another
    /// Rigidbody collides with it (static colliders such as the ground don't count). If vanishing is
    /// enabled, the object deactivates itself <see cref="Delay"/> seconds after that first hit.
    /// The owner listens to <see cref="Vanished"/> to respawn or pool it, then calls <see cref="ResetHit"/>.
    ///
    /// Two ways to get hit:
    ///   - pushed by the character  -> IPushReceiver.OnPushed (called by RigidbodyPusher)
    ///   - hit by another obstacle  -> OnCollisionEnter (called by the physics engine)
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class VanishAfterHit : MonoBehaviour, IPushReceiver
    {
        [Tooltip("Seconds between the first hit and vanishing.")]
        [SerializeField] float delay = 2f;

        [Tooltip("Off = objects are still marked as hit but never vanish.")]
        [SerializeField] bool vanishEnabled = true;

        // C# events: other scripts subscribe with +=, and this object calls them when something happens.
        // The object doesn't need to know who is listening (here: DominoArena).

        /// <summary>First hit since the last reset.</summary>
        public event Action<VanishAfterHit> Hit;

        /// <summary>Every collision with another Rigidbody (fires on both bodies).</summary>
        public event Action<VanishAfterHit> BodyContact;

        /// <summary>Raised right after the object deactivated itself.</summary>
        public event Action<VanishAfterHit> Vanished;

        public Rigidbody Body { get; private set; }
        public bool IsHit { get; private set; }
        public float LastBodyContactTime { get; private set; } = float.NegativeInfinity;
        public float Delay { get => delay; set => delay = value; }
        public bool VanishEnabled { get => vanishEnabled; set => vanishEnabled = value; }

        float _timeLeft;   // countdown until vanishing, starts at `delay` on the first hit

        void Awake() => Body = GetComponent<Rigidbody>();

        /// <summary>Starts the countdown. Only the FIRST hit counts; later hits don't restart the timer.</summary>
        public void MarkHit()
        {
            if (IsHit)
                return;
            IsHit = true;
            _timeLeft = delay;
            Hit?.Invoke(this);   // ?. = only call if someone subscribed
        }

        /// <summary>Back to "never hit", used when the object is placed again.</summary>
        public void ResetHit()
        {
            IsHit = false;
            _timeLeft = delay;
            LastBodyContactTime = float.NegativeInfinity;
        }

        // Way 1: the character pushed us.
        void IPushReceiver.OnPushed(RigidbodyPusher pusher, Vector3 impulse) => MarkHit();

        // Way 2: physics reports a collision. Unity calls this on BOTH objects of a collision.
        void OnCollisionEnter(Collision collision)
        {
            // collision.rigidbody is the OTHER object's Rigidbody. The ground and walls have none,
            // so only obstacle-against-obstacle collisions get past this line.
            if (collision.rigidbody == null)
                return;
            LastBodyContactTime = Time.time;
            BodyContact?.Invoke(this);
            MarkHit();
        }

        // The countdown: subtract the frame time each frame; at 0, vanish.
        void Update()
        {
            if (!IsHit || !vanishEnabled)
                return;

            _timeLeft -= Time.deltaTime;
            if (_timeLeft > 0f)
                return;

            // Deactivate instead of Destroy: the object can be reused (pooling), no garbage, no new Instantiate.
            gameObject.SetActive(false);
            Vanished?.Invoke(this);
        }
    }
}
