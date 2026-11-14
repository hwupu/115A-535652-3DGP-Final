using UnityEngine;

namespace Project3DGP.MiniEx.Bahadir.EnvEffects
{
    /// <summary>
    /// Ice: the player gets faster but has very little grip, so velocity changes slowly
    /// (slow start, long slide after releasing the keys).
    /// </summary>
    public class IceZone : EnvironmentZone
    {
        [Tooltip("Multiplies the acceleration. 1 = normal ground, small = slippery.")]
        [SerializeField, Range(0.01f, 1f)] float gripFactor = 0.1f;
        [Tooltip("Multiplies the walking/sprint speed on ice.")]
        [SerializeField] float speedFactor = 1.5f;

        protected override void ApplyEffect(MomentumController player)
        {
            player.ApplyGripFactor(gripFactor);
            player.ApplySpeedFactor(speedFactor);
        }
    }
}