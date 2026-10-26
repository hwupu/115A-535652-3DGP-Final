using UnityEngine;

namespace Project3DGP.Core.Interaction
{
    /// <summary>
    /// Implement on a component next to a Rigidbody to hear when a <see cref="RigidbodyPusher"/> pushes it.
    /// (A CharacterController push doesn't raise OnCollisionEnter on the body, so this is the only signal.)
    /// An interface keeps the pusher generic: it doesn't need to know what kind of object it pushed,
    /// only that the object wants to be told.
    /// </summary>
    public interface IPushReceiver
    {
        void OnPushed(RigidbodyPusher pusher, Vector3 impulse);
    }
}
