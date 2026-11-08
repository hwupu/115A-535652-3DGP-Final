using UnityEngine;

namespace Project3DGP.MiniEx.Bahadir.EnvEffects
{
    /// <summary>
    /// Stick-leg animation made in code (no clips), plus facing.
    ///  - Walking: the legs swing opposite to each other, angle = amplitude * sin(2*pi*phase).
    ///    The phase advances by speed / strideLength per second, so faster movement
    ///    (sprint, ice, wind) automatically gives faster steps.
    ///  - Standing: the swing fades out (weight goes to 0).
    ///  - Airborne: both legs blend to jumpAngle forward.
    ///  - The Rigidbody never rotates, so we turn the Visual towards the horizontal velocity.
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(MomentumController))]
    public class StickLegs : MonoBehaviour
    {
        [Header("Parts (drag from the Hierarchy)")]
        [SerializeField] Transform visual;      // parent of the legs; turned to face the movement
        [SerializeField] Transform legLeft;     // the PIVOT objects (at the hip), not the cylinders
        [SerializeField] Transform legRight;

        [Header("Swing")]
        [SerializeField] float swingAngle = 22.5f;    // degrees forward and back
        [SerializeField] float strideLength = 3f;     // metres travelled per full step cycle
        [SerializeField] float jumpAngle = 45f;       // both legs forward while airborne
        [SerializeField] float blendSharpness = 15f;  // bigger = snappier pose changes

        [Header("Facing")]
        [SerializeField] float turnSpeed = 720f;      // degrees per second

        Rigidbody _body;
        MomentumController _controller;
        float _phase;          // 0..1 = one full step cycle
        float _swingWeight;    // 0 = standing, 1 = full swing
        float _airWeight;      // 0 = on the ground, 1 = airborne pose

        void Awake()
        {
            _body = GetComponent<Rigidbody>();
            _controller = GetComponent<MomentumController>();
        }

        void Update()
        {
            Vector3 velocity = _body.linearVelocity;
            Vector3 horizontal = new Vector3(velocity.x, 0f, velocity.z);
            float speed = horizontal.magnitude;
            float dt = Time.deltaTime;
            float blend = 1f - Mathf.Exp(-blendSharpness * dt);   // frame-rate independent smoothing

            // 1. Face the direction we are moving (not when almost standing).
            if (visual != null && speed > 0.5f)
            {
                Quaternion facing = Quaternion.LookRotation(horizontal);
                visual.rotation = Quaternion.RotateTowards(visual.rotation, facing, turnSpeed * dt);
            }

            // 2. Blend weights: swing only on the ground and while moving; air pose while airborne.
            bool airborne = !_controller.IsGrounded;
            _airWeight = Mathf.Lerp(_airWeight, airborne ? 1f : 0f, blend);
            float wantedSwing = (speed > 0.3f && !airborne) ? 1f : 0f;
            _swingWeight = Mathf.Lerp(_swingWeight, wantedSwing, blend);

            // 3. The phase grows with distance walked: cycles per second = speed / strideLength.
            _phase = Mathf.Repeat(_phase + speed / strideLength * dt, 1f);

            // 4. Angles: opposite swing on the ground, the same forward angle in the air.
            float swing = swingAngle * _swingWeight * Mathf.Sin(2f * Mathf.PI * _phase);
            float left = Mathf.Lerp(swing, jumpAngle, _airWeight);
            float right = Mathf.Lerp(-swing, jumpAngle, _airWeight);

            // A leg hangs along -Y, so a NEGATIVE turn around X swings the foot forward (+Z).
            legLeft.localRotation = Quaternion.Euler(-left, 0f, 0f);
            legRight.localRotation = Quaternion.Euler(-right, 0f, 0f);
        }
    }
}