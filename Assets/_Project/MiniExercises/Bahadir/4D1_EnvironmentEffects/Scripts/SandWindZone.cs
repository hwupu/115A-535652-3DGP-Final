using UnityEngine;

namespace Project3DGP.MiniEx.Bahadir.EnvEffects
{
    /// <summary>
    /// Wind: a constant velocity bias on the player (it drifts with the wind unless it walks against it)
    /// and a constant acceleration on loose rigidbodies such as the rolling bush.
    /// </summary>
    public class SandWindZone : EnvironmentZone
    {
        [Tooltip("Extra velocity pushing the player (m/s, world space). X = 4 blows to the right.")]
        [SerializeField] Vector3 windVelocity = new Vector3(4f, 0f, 0f);
        [Tooltip("Acceleration given to loose rigidbodies, e.g. the bush (m/s^2).")]
        [SerializeField] float bodyAcceleration = 3f;

        protected override void ApplyEffect(MomentumController player) => player.ApplyWindVelocity(windVelocity);

        protected override void ApplyEffectToBody(Rigidbody body) =>
            body.AddForce(windVelocity.normalized * bodyAcceleration, ForceMode.Acceleration);

        // Shows the wind direction in the Scene view when the zone is selected.
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position + Vector3.up * 2f, windVelocity);
        }
    }
}