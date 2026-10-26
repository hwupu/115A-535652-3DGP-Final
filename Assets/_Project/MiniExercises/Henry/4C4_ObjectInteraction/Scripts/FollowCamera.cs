using UnityEngine;
using UnityEngine.InputSystem;

namespace Project3DGP.MiniEx.Henry
{
    /// <summary>
    /// Orbit camera that follows a target. Hold the right mouse button and drag to orbit, scroll to zoom.
    /// The position is a point on a sphere around the target: focus - (rotation * forward) * distance.
    /// </summary>
    public class FollowCamera : MonoBehaviour
    {
        [SerializeField] Transform target;
        [SerializeField] float distance = 10f;      // radius of the sphere around the target (m)
        [SerializeField] float minDistance = 3f;
        [SerializeField] float maxDistance = 25f;
        [SerializeField] float yaw;                 // turn left/right around the target (degrees)
        [SerializeField] float pitch = 45f;         // look down angle (degrees)
        [SerializeField] float mouseSensitivity = 0.2f;
        [SerializeField] float zoomSensitivity = 0.01f;

        // LateUpdate runs after every Update, so the hamster has already moved this frame -> no jitter.
        void LateUpdate()
        {
            if (target == null)
                return;

            var mouse = Mouse.current;
            if (mouse != null)
            {
                // Right-drag: mouse X changes yaw, mouse Y changes pitch (clamped so we never go under the ground or flip over).
                if (mouse.rightButton.isPressed)
                {
                    var delta = mouse.delta.ReadValue();
                    yaw += delta.x * mouseSensitivity;
                    pitch = Mathf.Clamp(pitch - delta.y * mouseSensitivity, 10f, 85f);
                }

                // Scroll wheel: zoom in/out by changing the sphere's radius.
                var scroll = mouse.scroll.ReadValue().y;
                distance = Mathf.Clamp(distance - scroll * zoomSensitivity, minDistance, maxDistance);
            }

            // Spherical position: rotate "forward" by (pitch, yaw), then step BACK from the focus point
            // by `distance`. The camera keeps the same rotation, so it always looks at the focus.
            var rotation = Quaternion.Euler(pitch, yaw, 0f);
            var focus = target.position + Vector3.up * 0.5f;   // look at the hamster's body, not its feet
            transform.SetPositionAndRotation(focus - rotation * Vector3.forward * distance, rotation);
        }
    }
}
