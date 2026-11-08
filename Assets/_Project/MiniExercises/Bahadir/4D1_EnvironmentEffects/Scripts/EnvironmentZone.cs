using UnityEngine;

namespace Project3DGP.MiniEx.Bahadir.EnvEffects
{
    /// <summary>
    /// Base class of every zone. Put a subclass on an object with a trigger collider.
    ///   ApplyEffect  runs every physics step while the player is inside (continuous effects).
    ///   OnPlayerEnter runs once when the player enters (one-off effects).
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public abstract class EnvironmentZone : MonoBehaviour
    {
        // Unity calls Reset when the component is added: make the collider a trigger automatically.
        void Reset() => GetComponent<Collider>().isTrigger = true;

        void OnTriggerEnter(Collider other)
        {
            if (TryGetPlayer(other, out var player)) OnPlayerEnter(player);
        }

        void OnTriggerStay(Collider other)
        {
            if (TryGetPlayer(other, out var player)) ApplyEffect(player);
        }

        protected virtual void OnPlayerEnter(MomentumController player) { }
        protected abstract void ApplyEffect(MomentumController player);

        // The collider belongs to the player's Rigidbody, which carries the controller.
        static bool TryGetPlayer(Collider other, out MomentumController player)
        {
            player = null;
            return other.attachedRigidbody != null && other.attachedRigidbody.TryGetComponent(out player);
        }
    }
}