using UnityEngine;

namespace Project3DGP.Game.Golf
{
    /// <summary>
    /// World-space aim indicator (#15, architecture §4.2): a "V" of two lines from the ball, opening along the
    /// aim yaw, as wide as the club's cone (accuracy). The shot lands somewhere inside it. Visible only while Aiming.
    /// Drawn with a LineRenderer: 3 points = left edge end -> ball -> right edge end.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class AimCone : MonoBehaviour
    {
        [SerializeField] ShotController shot;
        [SerializeField] GolfBall ball;
        [Tooltip("Length of the cone lines (m).")]
        [SerializeField] float length = 3f;
        [Tooltip("Lift above the ground so the lines don't flicker in the grass (m).")]
        [SerializeField] float lift = 0.02f;

        LineRenderer _line;

        void Awake()
        {
            _line = GetComponent<LineRenderer>();
            _line.positionCount = 3;
            _line.useWorldSpace = true;
        }

        // LateUpdate: after the camera (and so the aim yaw) moved this frame.
        void LateUpdate()
        {
            var visible = shot.Phase == ShotPhase.Aiming && shot.CurrentClub != null && ball.IsAtRest;
            _line.enabled = visible;
            if (!visible)
                return;

            var halfCone = shot.CurrentClub.AimConeDegrees * 0.5f;
            var yaw = shot.AimYaw;   // TEMP(#19): CameraRig.AimYaw
            var origin = ball.Position + Vector3.up * lift;
            // Quaternion.Euler(0, angle, 0) * forward = a flat arrow turned to that yaw.
            _line.SetPosition(0, origin + Quaternion.Euler(0f, yaw - halfCone, 0f) * Vector3.forward * length);
            _line.SetPosition(1, origin);
            _line.SetPosition(2, origin + Quaternion.Euler(0f, yaw + halfCone, 0f) * Vector3.forward * length);
        }
    }
}
