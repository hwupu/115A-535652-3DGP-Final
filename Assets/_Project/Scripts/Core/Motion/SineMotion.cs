using UnityEngine;

namespace Project3DGP.Core.Motion
{
    /// <summary>
    /// Periodic motion with a sine (or cosine) wave: angle = amplitude x sin(2 pi f t), turning the object
    /// about one of its local axes. In 4.A.1 this is C's aiming wiggle: the club head swings a little back and
    /// forth behind the ball, like a golfer lining up a shot.
    ///
    /// Handout version (4.A.1): "animate C periodically (sin/cos) in the local frame of B, along B's y-axis".
    /// That is a SLIDE along the parent's local y-axis. In our model B's y-axis points along the arm, so the
    /// slide made the club pump up and down, which doesn't mean anything in golf. We changed it to a TURN
    /// (same sine wave, same periodic motion, same local frame) so it reads as aiming and fits the game theme.
    /// The slide version is kept below, commented out: uncomment it (and comment out the turn) to show it.
    /// Disable the component to stop it: the object goes back to its rest pose.
    /// </summary>
    public class SineMotion : MonoBehaviour
    {
        [Tooltip("Local axis to turn about, e.g. (0, 0, 1) = the object's own Z axis.")]
        [SerializeField] Vector3 axis = Vector3.forward;
        [Tooltip("Degrees each way from the rest rotation.")]
        [SerializeField] float amplitude = 8f;
        [Tooltip("Full cycles per second.")]
        [SerializeField] float frequency = 1f;
        [Tooltip("Use cos instead of sin (same motion, starts at the end instead of the middle).")]
        [SerializeField] bool useCosine;

        // Handout version: slide along the parent's (B's) local y-axis.
        // [Tooltip("Axis in the PARENT's local space: (0, 1, 0) = B's local y-axis.")]
        // [SerializeField] Vector3 slideAxis = Vector3.up;
        // [Tooltip("Metres from the rest position to each end.")]
        // [SerializeField] float slideAmplitude = 0.02f;

        Vector3 _restPosition;
        Quaternion _restRotation;
        float _time;   // seconds since the motion (re)started

        /// <summary>Current wave value x amplitude (degrees).</summary>
        public float CurrentOffset { get; private set; }

        void Awake()
        {
            _restPosition = transform.localPosition;
            _restRotation = transform.localRotation;
        }

        // Restart the cycle each time it is switched on, so sin starts at 0 = at the rest pose (no jump).
        void OnEnable() => _time = 0f;

        void OnDisable()
        {
            CurrentOffset = 0f;
            transform.localPosition = _restPosition;
            transform.localRotation = _restRotation;
        }

        void Update()
        {
            _time += Time.deltaTime;
            // Phase in radians: 2 pi x frequency x time. sin repeats every 2 pi, i.e. every 1/frequency seconds.
            var phase = 2f * Mathf.PI * frequency * _time;
            var wave = useCosine ? Mathf.Cos(phase) : Mathf.Sin(phase);   // -1 .. 1

            // Handout version: SLIDE along B's local y-axis. localPosition is measured in the parent's (B's) frame,
            // so (0, 1, 0) is B's own y-axis wherever B is turned. Uncomment these two lines and comment out the
            // two TURN lines below to show it.
            // CurrentOffset = slideAmplitude * wave;
            // transform.localPosition = _restPosition + slideAxis.normalized * CurrentOffset;

            // Our version: TURN about the local axis (aiming wiggle).
            CurrentOffset = amplitude * wave;
            transform.localRotation = _restRotation * Quaternion.AngleAxis(CurrentOffset, axis);
        }
    }
}
