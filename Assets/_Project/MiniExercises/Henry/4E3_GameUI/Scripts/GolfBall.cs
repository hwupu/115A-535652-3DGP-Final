using UnityEngine;

namespace Project3DGP.MiniEx.Henry
{
    /// <summary>
    /// A golf ball with deliberately simple "physics": no Rigidbody, it just slides forward and slows
    /// down to a stop, with a small hop. Distance is decided by the shot (club x power), not by forces.
    /// Why no Rigidbody? The PO asked for simple, predictable shots: the exact distance is known at the
    /// moment of the hit, and the ball is animated from start to end with an easing curve.
    /// </summary>
    public class GolfBall : MonoBehaviour
    {
        [Tooltip("Ball speed right after the hit (m/s). Higher = shorter flight time.")]
        [SerializeField] float launchSpeed = 14f;
        [Tooltip("Hop height per 10 m of distance (m).")]
        [SerializeField] float hopPer10m = 0.6f;
        [SerializeField] float radius = 0.15f;

        Vector3 _home;      // starting position in the scene, for R (reset)
        Vector3 _start;     // where the current shot started
        Vector3 _end;       // where it will stop
        float _duration;    // how long the shot takes (s)
        float _elapsed;     // time since the hit (s)
        float _hop;         // hop height for this shot (m)

        public bool IsMoving { get; private set; }

        void Awake() => _home = transform.position;

        /// <summary>Sends the ball <paramref name="distance"/> metres along <paramref name="direction"/>.</summary>
        public void Hit(Vector3 direction, float distance)
        {
            direction.y = 0f;          // stay on the ground plane
            direction.Normalize();     // length 1, so direction * distance = exactly `distance` metres

            // Stop in front of walls (and other balls): cast a sphere along the path, cut the distance at the hit.
            var origin = transform.position + Vector3.up * 0.02f;
            if (Physics.SphereCast(origin, radius * 0.9f, direction, out var hit, distance, Physics.AllLayers,
                    QueryTriggerInteraction.Ignore))
                distance = Mathf.Max(0f, hit.distance - 0.05f);

            _start = transform.position;
            _end = _start + direction * distance;   // the landing point is known right away
            // Ease-out (below) starts at speed 2 x distance / duration, so duration = 2 x distance / launchSpeed.
            _duration = Mathf.Max(0.2f, 2f * distance / launchSpeed);
            _hop = hopPer10m * distance / 10f;   // longer shots hop higher
            _elapsed = 0f;
            IsMoving = true;
        }

        public void ResetToHome()
        {
            IsMoving = false;
            transform.position = _home;
        }

        void Update()
        {
            if (!IsMoving)
                return;

            _elapsed += Time.deltaTime;
            // t = progress of the shot from 0 (just hit) to 1 (stopped).
            var t = Mathf.Clamp01(_elapsed / _duration);

            // Ease-out: 1 - (1 - t)^2. Fast at first, slowing to 0 at the end, like rolling with friction.
            var travelled = 1f - (1f - t) * (1f - t);
            // Lerp(a, b, x) = a + (b - a) * x: the point x of the way from start to end.
            var position = Vector3.Lerp(_start, _end, travelled);
            // Hop: a parabola 4t(1 - t) that is 0 at both ends and 1 in the middle.
            position.y += _hop * 4f * t * (1f - t);
            transform.position = position;

            if (t >= 1f)
                IsMoving = false;
        }
    }
}
