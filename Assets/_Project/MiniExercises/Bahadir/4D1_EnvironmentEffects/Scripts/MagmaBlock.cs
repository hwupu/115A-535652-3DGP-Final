using UnityEngine;

namespace Project3DGP.MiniEx.Bahadir.EnvEffects
{
    /// <summary>
    /// Solid magma block: touching it shoves the player away with an instant velocity change.
    /// Direction = from the block to the player on the ground plane, rotated by a random angle, so
    /// touching the same block twice sends you in two different directions.
    /// </summary>
    public class MagmaBlock : MonoBehaviour
    {
        [Tooltip("Horizontal speed added by the shove (m/s).")]
        [SerializeField] float shoveSpeed = 12f;
        [Tooltip("Upward speed added by the shove (m/s).")]
        [SerializeField] float upKick = 4f;
        [Tooltip("Random rotation of the shove direction (+- degrees).")]
        [SerializeField] float directionJitter = 35f;

        void OnCollisionEnter(Collision collision)
        {
            if (collision.rigidbody == null || !collision.rigidbody.TryGetComponent(out MomentumController player))
                return;

            Vector3 away = player.transform.position - transform.position;
            away.y = 0f;
            if (away.sqrMagnitude < 0.0001f) away = transform.forward;

            away = Quaternion.AngleAxis(Random.Range(-directionJitter, directionJitter), Vector3.up) * away.normalized;
            player.AddVelocityChange(away * shoveSpeed + Vector3.up * upKick);
        }
    }
}