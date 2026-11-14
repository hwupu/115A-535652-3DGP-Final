using UnityEngine;
using UnityEngine.InputSystem;

namespace Project3DGP.MiniEx.Bahadir.EnvEffects
{
    /// <summary>
    /// Rigidbody character with momentum.
    /// WASD does not set the velocity directly: it accelerates the velocity TOWARDS "input * speed".
    ///   horizontal = MoveTowards(horizontal, target, acceleration * dt)
    /// A big acceleration = tight, instant control. A tiny one = sliding (ice).
    /// The same rule works in the air, with its own (smaller) acceleration.
    /// Space jumps (v = sqrt(2 g h)), Left Shift sprints.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class MomentumController : MonoBehaviour
    {
        [Header("Speed (m/s)")]
        [SerializeField] float walkSpeed = 5f;
        [SerializeField] float sprintSpeed = 9f;

        [Header("Acceleration (m/s^2): how fast the velocity follows the keys")]
        [SerializeField] float groundAcceleration = 40f;
        [SerializeField] float airAcceleration = 15f;

        [Header("Jump")]
        [SerializeField] float jumpHeight = 1.5f;

        [Header("References")]
        [SerializeField] FollowCamera cameraRig;




        public bool IsGrounded => _grounded;

        // ---- Hooks for environment zones ----
        // A zone calls the Apply... methods every physics step while the player is inside it.
        // The values are used (and cleared) in the next FixedUpdate, so overlapping zones just combine.
        float _speedFactor = 1f;
        float _gripFactor = 1f;
        Vector3 _windVelocity;

        public void ApplySpeedFactor(float factor) => _speedFactor *= factor;
        public void ApplyGripFactor(float factor) => _gripFactor *= factor;
        public void ApplyWindVelocity(Vector3 velocity) => _windVelocity += velocity;

        /// <summary>One-off: set the upward speed (hot plate, jump pads).</summary>
        public void Launch(float upSpeed)
        {
            Vector3 velocity = _body.linearVelocity;
            velocity.y = upSpeed;
            _body.linearVelocity = velocity;
        }

        /// <summary>One-off: add an instant velocity change (magma shove).</summary>
        public void AddVelocityChange(Vector3 change) => _body.AddForce(change, ForceMode.VelocityChange);





        Rigidbody _body;
        Vector2 _input;          // x = right/left, y = forward/back
        bool _sprint;
        bool _jumpQueued;        // set in Update, consumed in FixedUpdate (Update runs more often)
        bool _touchingGround;    // filled by the collision callbacks
        bool _grounded;          // stable value for the whole physics step

        void Awake()
        {
            _body = GetComponent<Rigidbody>();
            // A sleeping body gets no OnCollisionStay, which would break the ground check.
            _body.sleepThreshold = 0f;
        }

        void Update()
        {
            var keyboard = Keyboard.current;
            _input = Vector2.zero;
            if (keyboard == null) return;

            if (keyboard.wKey.isPressed) _input.y += 1f;
            if (keyboard.sKey.isPressed) _input.y -= 1f;
            if (keyboard.dKey.isPressed) _input.x += 1f;
            if (keyboard.aKey.isPressed) _input.x -= 1f;
            _input = Vector2.ClampMagnitude(_input, 1f);   // diagonals are not faster

            _sprint = keyboard.leftShiftKey.isPressed;
            if (keyboard.spaceKey.wasPressedThisFrame) _jumpQueued = true;
        }

        void FixedUpdate()
        {
            _grounded = _touchingGround;
            _touchingGround = false;

            // Input is relative to the camera's yaw.
            float yaw = cameraRig != null ? cameraRig.Yaw : 0f;
            Vector3 wishDirection = Quaternion.Euler(0f, yaw, 0f) * new Vector3(_input.x, 0f, _input.y);

            float speed = (_sprint ? sprintSpeed : walkSpeed) * _speedFactor;
            Vector3 targetVelocity = wishDirection * speed + _windVelocity;

            Vector3 velocity = _body.linearVelocity;
            Vector3 horizontal = new Vector3(velocity.x, 0f, velocity.z);
            float acceleration = (_grounded ? groundAcceleration : airAcceleration) * _gripFactor;
            horizontal = Vector3.MoveTowards(horizontal, targetVelocity, acceleration * Time.fixedDeltaTime);

            // Vertical speed belongs to the physics engine (gravity), except at the moment of a jump.
            float vertical = velocity.y;
            if (_jumpQueued && _grounded)
                vertical = Mathf.Sqrt(2f * -Physics.gravity.y * jumpHeight);
            _jumpQueued = false;

            _body.linearVelocity = new Vector3(horizontal.x, vertical, horizontal.z);
            _speedFactor = 1f; _gripFactor = 1f; _windVelocity = Vector3.zero;   // zones refill them for the next step
        }

        void OnCollisionEnter(Collision collision) => CheckGround(collision);
        void OnCollisionStay(Collision collision) => CheckGround(collision);

        // A contact whose surface normal points mostly up is something we stand on.
        void CheckGround(Collision collision)
        {
            for (int i = 0; i < collision.contactCount; i++)
            {
                if (collision.GetContact(i).normal.y > 0.5f)
                {
                    _touchingGround = true;
                    return;
                }
            }
        }
    }
}