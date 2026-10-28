using Project3DGP.Game.Golf;
using UnityEngine;

namespace Project3DGP.Game.Effects
{
    /// <summary>
    /// Impact sparks (#39): on every shot the effect jumps to the ball and plays once. The effect is a particle
    /// system already in the scene (a prefab instance Henry can replace), reused for every shot: no spawning.
    /// </summary>
    public class ShotImpactEffect : MonoBehaviour
    {
        [SerializeField] ShotController shot;
        [SerializeField] GolfBall ball;
        [SerializeField] ParticleSystem effect;

        void OnEnable()
        {
            if (shot != null)
                shot.ShotTaken += OnShotTaken;
        }

        void OnDisable()
        {
            if (shot != null)
                shot.ShotTaken -= OnShotTaken;
        }

        void OnShotTaken(ShotInfo info) => EffectUtil.PlayAt(effect, ball.transform.position);
    }
}
