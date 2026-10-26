using UnityEngine;
using UnityEngine.InputSystem;

namespace Project3DGP.Core.Motion
{
    /// <summary>
    /// A joint of a hierarchical model that turns about ONE of its local axes.
    /// Another script (an animator) sets <see cref="Angle"/>; this component applies it as
    /// localRotation = rest rotation x (Angle x Direction about Axis).
    /// Because it sets the LOCAL rotation, every child turns with it: that is what makes a hierarchy.
    /// Direction (+1 / -1) mirrors the motion; the reverse key flips it.
    /// </summary>
    public class AxisJoint : MonoBehaviour
    {
        [Tooltip("Local axis to rotate about, e.g. (0, 0, 1) = the joint's own Z axis.")]
        [SerializeField] Vector3 axis = Vector3.forward;
        [Tooltip("+1 or -1. The reverse key flips it.")]
        [SerializeField] int direction = 1;
        [SerializeField] Key reverseKey = Key.None;

        Quaternion _restRotation;   // the joint's rotation as built in the editor (the pose at Angle = 0)

        /// <summary>Angle in degrees, set by an animator. 0 = rest pose.</summary>
        public float Angle { get; set; }
        public int Direction => direction;

        void Awake() => _restRotation = transform.localRotation;

        public void Reverse() => direction = -direction;

        void Update()
        {
            if (reverseKey != Key.None && Keyboard.current != null && Keyboard.current[reverseKey].wasPressedThisFrame)
                Reverse();
        }

        // LateUpdate: after the animator has set this frame's Angle in its Update.
        // Quaternion.AngleAxis(angle, axis) = "turn by angle degrees about axis". Multiplying it after the rest
        // rotation turns about the joint's OWN (local) axis. Direction -1 turns the same angle the other way.
        void LateUpdate() =>
            transform.localRotation = _restRotation * Quaternion.AngleAxis(Angle * direction, axis);
    }
}
