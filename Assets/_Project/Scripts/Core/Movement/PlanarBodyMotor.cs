using UnityEngine;

namespace Project3DGP.Core.Movement
{
    /// <summary>
    /// Moves a dynamic Rigidbody along the world X/Z plane. Input is supplied by the caller,
    /// so keyboard controls and AI can use the same motor. Physics transfers momentum to props.
    /// Configure the body's rotation constraints in the Editor to keep the character upright.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class PlanarBodyMotor : MonoBehaviour
    {
        [SerializeField, Min(0f)] float walkSpeed = 4f;
        [SerializeField, Min(0f)] float sprintSpeed = 7f;
        [SerializeField, Min(0f)] float acceleration = 30f;

        Rigidbody _body;

        public Vector2 MoveInput { get; set; }
        public bool Sprint { get; set; }
        public float WalkSpeed { get => walkSpeed; set => walkSpeed = Mathf.Max(0f, value); }

        void Awake() => _body = GetComponent<Rigidbody>();

        void OnDisable()
        {
            MoveInput = Vector2.zero;
            Sprint = false;
        }

        void FixedUpdate()
        {
            // Limit diagonal input before converting it into a world-space horizontal velocity.
            var input = Vector2.ClampMagnitude(MoveInput, 1f);
            var target = new Vector3(input.x, 0f, input.y) * (Sprint ? sprintSpeed : walkSpeed);
            var velocity = _body.linearVelocity;
            var horizontal = new Vector3(velocity.x, 0f, velocity.z);
            horizontal = Vector3.MoveTowards(horizontal, target, acceleration * Time.fixedDeltaTime);

            // Keep gravity and vertical collision impulses; only the horizontal motion is controlled.
            _body.linearVelocity = new Vector3(horizontal.x, velocity.y, horizontal.z);
        }
    }
}
