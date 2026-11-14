using UnityEngine;
using UnityEngine.InputSystem;

namespace Project3DGP.MiniEx.Bahadir.EnvEffects
{
    /// <summary>
    /// Simple third-person camera: stays behind the target at a fixed distance and pitch.
    /// Hold the RIGHT mouse button and drag left/right to orbit around the target.
    /// The yaw is public because the controller moves relative to it (W = away from the camera).
    /// </summary>
    public class FollowCamera : MonoBehaviour
    {
        [SerializeField] Transform target;
        [SerializeField] float distance = 8f;
        [SerializeField] float lookHeight = 1.5f;   // we look at a point this high above the target
        [SerializeField] float pitch = 25f;         // degrees looking down
        [SerializeField] float yawSpeed = 0.2f;     // degrees per mouse pixel (negative = inverted)
        [SerializeField] float followSharpness = 12f;

        float _yaw;
        public float Yaw => _yaw;

        void LateUpdate()
        {
            if (target == null) return;

            var mouse = Mouse.current;
            if (mouse != null && mouse.rightButton.isPressed)
                _yaw += mouse.delta.ReadValue().x * yawSpeed;

            // Camera position = focus point minus (look direction * distance).
            Vector3 focus = target.position + Vector3.up * lookHeight;
            Quaternion rotation = Quaternion.Euler(pitch, _yaw, 0f);
            Vector3 wanted = focus - rotation * Vector3.forward * distance;

            // Frame-rate independent smoothing: 1 - e^(-k*dt).
            float t = 1f - Mathf.Exp(-followSharpness * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, wanted, t);
            transform.rotation = rotation;
        }
    }
}