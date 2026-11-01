using UnityEngine;

namespace Project3DGP.Core.Movement
{
    /// <summary>
    /// Walks a character with Unity's CharacterController (collides with walls, climbs small steps and slopes).
    /// It reads no keys itself: the owner sets MoveInput and ReferenceYaw every frame. So the same walker works
    /// with keyboard input (game, mini-exercises) or with an AI that follows a path (A*, later).
    ///   MoveInput    x = right / left, y = forward / back, each -1..1
    ///   ReferenceYaw which way "forward" is, in degrees (usually the camera's yaw: W walks away from the camera)
    /// Disabling this component also disables the CharacterController, so the character has no collider while
    /// someone else places it (e.g. the shooting stance next to the ball).
    /// IsWalking and Speed are there for the walk / idle animation later.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class CharacterWalker : MonoBehaviour
    {
        [Tooltip("Walking speed (m/s).")]
        [SerializeField] float walkSpeed = 3f;
        [Tooltip("Speed while Sprint is on (m/s).")]
        [SerializeField] float sprintSpeed = 6f;
        [Tooltip("How high a jump goes (m).")]
        [SerializeField] float jumpHeight = 1f;
        [Tooltip("How fast the character turns to face its walking direction (degrees per second).")]
        [SerializeField] float turnSpeed = 540f;
        [Tooltip("Gravity (m/s^2). Stronger than real (-9.81) so the character sticks to slopes.")]
        [SerializeField] float gravity = -20f;
        [Tooltip("Below this speed (m/s) the character counts as standing (for animations).")]
        [SerializeField] float walkingThreshold = 0.1f;

        CharacterController _controller;
        float _verticalSpeed;   // falling speed (m/s); the CharacterController has no physics of its own

        public Vector2 MoveInput { get; set; }
        public float ReferenceYaw { get; set; }
        /// <summary>On = move at sprintSpeed instead of walkSpeed.</summary>
        public bool Sprint { get; set; }
        /// <summary>True while standing on something (false in the air, e.g. during a jump).</summary>
        public bool IsGrounded => _controller != null && _controller.enabled && _controller.isGrounded;
        public bool IsSprinting => Sprint && IsWalking;
        /// <summary>Current horizontal speed (m/s).</summary>
        public float Speed { get; private set; }
        public bool IsWalking => Speed > walkingThreshold;
        public float WalkSpeed { get => walkSpeed; set => walkSpeed = value; }

        void Awake() => _controller = GetComponent<CharacterController>();

        void OnEnable()
        {
            // Turning the controller on makes it start from wherever the transform is now.
            _controller.enabled = true;
            _verticalSpeed = 0f;
        }

        void OnDisable()
        {
            if (_controller != null)
                _controller.enabled = false;
            MoveInput = Vector2.zero;
            Sprint = false;
            Speed = 0f;
        }

        /// <summary>
        /// Jumps if standing on the ground; returns false in the air (no double jumps).
        /// From v^2 = 2 g h: the start speed that reaches jumpHeight under our gravity is sqrt(2 * |g| * h).
        /// </summary>
        public bool Jump()
        {
            if (!enabled || !_controller.isGrounded)
                return false;
            _verticalSpeed = Mathf.Sqrt(2f * -gravity * jumpHeight);
            return true;
        }

        void Update()
        {
            // 1. Input -> horizontal velocity. ClampMagnitude: diagonal (W + D) is not faster than straight.
            //    Quaternion.Euler(0, yaw, 0) turns the input so that "forward" points along ReferenceYaw.
            var input = Vector2.ClampMagnitude(MoveInput, 1f);
            var direction = Quaternion.Euler(0f, ReferenceYaw, 0f) * new Vector3(input.x, 0f, input.y);
            var horizontal = direction * (Sprint ? sprintSpeed : walkSpeed);

            // 2. Face the walking direction, turning at most turnSpeed degrees per second (smooth, no snapping).
            if (horizontal.sqrMagnitude > 0.0001f)
            {
                var facing = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, facing, turnSpeed * Time.deltaTime);
            }

            // 3. Gravity: on the ground keep a small downward push (stays glued to slopes), else fall faster.
            if (_controller.isGrounded && _verticalSpeed < 0f)
                _verticalSpeed = -2f;
            else
                _verticalSpeed += gravity * Time.deltaTime;

            // 4. Move. The controller stops at walls and slides along them.
            _controller.Move((horizontal + Vector3.up * _verticalSpeed) * Time.deltaTime);

            var velocity = _controller.velocity;
            Speed = new Vector2(velocity.x, velocity.z).magnitude;
        }
    }
}
