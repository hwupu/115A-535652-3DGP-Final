using UnityEngine;

namespace Project3DGP.MiniEx.Bahadir.EnvEffects
{
    /// <summary>
    /// Hot plate: whenever the player stands on the ground inside the zone, it is launched upwards.
    /// After landing it is launched again, so the player hops continuously. Steering in the air still works.
    /// Launch speed v gives a jump height h = v^2 / (2 g): 7.5 m/s is about 2.9 m.
    /// </summary>
    public class HotPlateZone : EnvironmentZone
    {
        [Tooltip("Upward speed given on every launch (m/s).")]
        [SerializeField] float launchSpeed = 7.5f;

        protected override void ApplyEffect(MomentumController player)
        {
            if (player.IsGrounded)
                player.Launch(launchSpeed);
        }
    }
}