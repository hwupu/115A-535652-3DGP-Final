using Project3DGP.Game.Scoring;
using UnityEngine;

namespace Project3DGP.Game.Golf
{
    /// <summary>
    /// A goal the ball meets by touching it (#22, architecture §4.6): a target spot on a branch, a lever, a
    /// switch (#24). Works with a solid collider (the ball bounces off) or a trigger (the ball passes through).
    /// When met: the objects in Show When Met appear, those in Hide When Met disappear, and Flip Part (e.g. the
    /// lever handle) turns to Flip Euler. The ball keeps playing; nothing else changes.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class BallHitTarget : LevelGoal
    {
        [SerializeField] GameObject[] showWhenMet = { };
        [SerializeField] GameObject[] hideWhenMet = { };
        [Tooltip("Optional: turned to Flip Euler (local) when met, e.g. a lever handle.")]
        [SerializeField] Transform flipPart;
        [SerializeField] Vector3 flipEuler;

        Quaternion _flipRest;

        void Awake()
        {
            if (flipPart != null)
                _flipRest = flipPart.localRotation;
            Show(false);
        }

        void OnCollisionEnter(Collision collision) => TryMeet(collision.rigidbody);
        void OnTriggerEnter(Collider other) => TryMeet(other.attachedRigidbody);

        void TryMeet(Rigidbody body)
        {
            if (body != null && body.TryGetComponent<GolfBall>(out _))
                MarkMet();
        }

        internal void SetForTests(GameObject[] show, GameObject[] hide)
        {
            showWhenMet = show;
            hideWhenMet = hide;
        }

        protected override void OnMet() => Show(true);
        protected override void OnGoalReset() => Show(false);

        void Show(bool met)
        {
            foreach (var go in showWhenMet)
                if (go != null) go.SetActive(met);
            foreach (var go in hideWhenMet)
                if (go != null) go.SetActive(!met);
            if (flipPart != null)
                flipPart.localRotation = met ? Quaternion.Euler(flipEuler) : _flipRest;
        }
    }
}
