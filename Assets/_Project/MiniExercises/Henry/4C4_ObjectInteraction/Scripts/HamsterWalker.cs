using UnityEngine;
using UnityEngine.InputSystem;

namespace Project3DGP.MiniEx.Henry
{
    /// <summary>
    /// Keyboard walking for the hamster: WASD/arrows move relative to the camera, Shift runs, Space jumps.
    /// A CharacterController has no Rigidbody, so gravity and jumping are integrated here by hand.
    /// Pushing obstacles is done by RigidbodyPusher (Core) on the same object.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class HamsterWalker : MonoBehaviour
    {
        [SerializeField] float walkSpeed = 3f;      // m/s
        [SerializeField] float runSpeed = 6f;       // m/s while holding Shift
        [SerializeField] float jumpHeight = 1.2f;   // m
        [SerializeField] float gravity = -20f;      // m/s^2, stronger than real (-9.81) so jumps feel snappy
        [Tooltip("How fast the hamster turns to face its walking direction (degrees per second).")]
        [SerializeField] float turnSpeed = 720f;
        [SerializeField] Transform cameraTransform;

        CharacterController _controller;
        float _verticalSpeed;   // our own "physics" for the Y axis: jump and fall speed

        void Awake()
        {
            _controller = GetComponent<CharacterController>();
            if (cameraTransform == null && Camera.main != null)
                cameraTransform = Camera.main.transform;
        }

        void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
                return;

            // 1. Read the keys into a 2D input vector: x = left/right, y = forward/back.
            var input = Vector2.zero;
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) input.y += 1f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) input.y -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) input.x += 1f;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) input.x -= 1f;
            // Without this, diagonal (W+D) would be length 1.41 and 41% faster than straight.
            input = Vector2.ClampMagnitude(input, 1f);

            // 2. Camera-relative: "forward" is where the camera looks, flattened onto the ground.
            //    Cross(up, forward) gives the camera's right on the ground.
            var forward = cameraTransform != null
                ? Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized
                : Vector3.forward;
            var right = Vector3.Cross(Vector3.up, forward);
            var speed = keyboard.leftShiftKey.isPressed ? runSpeed : walkSpeed;
            var move = (forward * input.y + right * input.x) * speed;   // horizontal velocity in m/s

            // 3. Turn smoothly towards the walking direction (at most turnSpeed degrees per second).
            if (move.sqrMagnitude > 0.001f)
            {
                var facing = Quaternion.LookRotation(move);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, facing, turnSpeed * Time.deltaTime);
            }

            // 4. Vertical speed: jump and gravity.
            if (_controller.isGrounded)
            {
                _verticalSpeed = -1f; // small downward speed keeps isGrounded true on the next frame
                if (keyboard.spaceKey.wasPressedThisFrame)
                    _verticalSpeed = Mathf.Sqrt(2f * jumpHeight * -gravity); // v = sqrt(2gh): speed needed to reach jumpHeight
            }

            // Euler integration: velocity += acceleration * dt.
            _verticalSpeed += gravity * Time.deltaTime;
            move.y = _verticalSpeed;

            // 5. Move = velocity * dt. The CharacterController slides along walls and calls
            //    OnControllerColliderHit for everything it touches (RigidbodyPusher listens to that).
            _controller.Move(move * Time.deltaTime);
        }
    }
}
