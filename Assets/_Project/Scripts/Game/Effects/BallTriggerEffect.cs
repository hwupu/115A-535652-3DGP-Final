using Project3DGP.Game.Golf;
using UnityEngine;

namespace Project3DGP.Game.Effects
{
    /// <summary>
    /// Plays an effect where the ball enters this trigger (#39): water splash on the pond, confetti on the cup.
    /// The effect is a particle system already in the scene (a replaceable prefab instance), reused each time.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class BallTriggerEffect : MonoBehaviour
    {
        [SerializeField] ParticleSystem effect;
        [Tooltip("On = play at the ball; off = play at the effect's own place.")]
        [SerializeField] bool atBall = true;

        void OnTriggerEnter(Collider other)
        {
            var ball = other.GetComponentInParent<GolfBall>();
            if (ball != null)
                Play(ball.transform.position);
        }

        /// <summary>Plays the effect (the trigger calls it; tests call it directly).</summary>
        public void Play(Vector3 ballPosition) =>
            EffectUtil.PlayAt(effect, atBall ? ballPosition : effect != null ? effect.transform.position : ballPosition);

        internal void SetForTests(ParticleSystem particles) => effect = particles;
    }
}
