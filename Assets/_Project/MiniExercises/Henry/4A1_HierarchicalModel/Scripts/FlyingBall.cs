using UnityEngine;

namespace Project3DGP.MiniEx.Henry
{
    /// <summary>
    /// The golf ball. No Rigidbody: its flight is computed here so every line can be explained.
    /// Each frame: velocity += acceleration x dt, position += velocity x dt (Euler integration).
    /// Gravity pulls it down, a sideways acceleration curves it (hook/slice), it bounces and rolls,
    /// and after flightSeconds it goes back to the tee for the next round.
    /// </summary>
    public class FlyingBall : MonoBehaviour
    {
        [SerializeField] float gravity = -9.81f;
        [Tooltip("Vertical speed kept after a bounce (0.45 = 45%).")]
        [SerializeField] float bounciness = 0.45f;
        [Tooltip("How fast the ball slows down while rolling (per second).")]
        [SerializeField] float rollingFriction = 1.5f;
        [Tooltip("Seconds from the hit until the ball is put back on the tee.")]
        [SerializeField] float flightSeconds = 5f;

        Vector3 _tee;              // starting position = rest height above the ground
        Vector3 _velocity;         // m/s, changed by gravity, curve, bounces and friction
        Vector3 _sideAcceleration; // m/s^2 sideways while in the air = the curve (hook/slice)
        float _time;               // seconds since the hit

        public bool InFlight { get; private set; }
        public float TimeLeft => InFlight ? flightSeconds - _time : 0f;

        void Awake() => _tee = transform.position;

        /// <summary>Called by SwingAnimator at impact.</summary>
        public void Launch(Vector3 velocity, Vector3 sideAcceleration)
        {
            _velocity = velocity;
            _sideAcceleration = sideAcceleration;
            _time = 0f;
            InFlight = true;
        }

        void Update()
        {
            if (!InFlight)
                return;

            var dt = Time.deltaTime;
            _time += dt;
            // Rolling = at ground height and not moving up or down any more.
            var onGround = transform.position.y <= _tee.y + 0.001f && Mathf.Approximately(_velocity.y, 0f);

            if (onGround)
            {
                // Rolling: lose a bit of speed every second.
                _velocity *= Mathf.Max(0f, 1f - rollingFriction * dt);
            }
            else
            {
                // In the air: gravity (down) + curve (sideways).
                _velocity += (Vector3.up * gravity + _sideAcceleration) * dt;
            }

            // Euler step: new position = old position + velocity x time.
            var position = transform.position + _velocity * dt;

            // Hit the ground while falling: bounce with less speed, or start rolling if the bounce is tiny.
            if (position.y < _tee.y && _velocity.y < 0f)
            {
                position.y = _tee.y;
                _velocity.y = -_velocity.y * bounciness;   // flip up, keep 45% of the speed
                if (_velocity.y < 0.5f)
                    _velocity.y = 0f;
                _velocity.x *= 0.8f;   // each bounce also loses some forward speed
                _velocity.z *= 0.8f;
            }
            transform.position = position;

            // New round: after 5 s the ball goes back on the tee, wherever it is.
            if (_time >= flightSeconds)
                ResetToTee();
        }

        public void ResetToTee()
        {
            InFlight = false;
            _velocity = Vector3.zero;
            transform.position = _tee;
        }
    }
}
