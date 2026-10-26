using UnityEngine;

namespace Project3DGP.Core.Interaction
{
    /// <summary>
    /// Lets a CharacterController push the Rigidbodies it walks into.
    /// A CharacterController moves kinematically, so the physics engine never pushes anything for it.
    /// Instead Unity reports every contact during Move() through OnControllerColliderHit, and this
    /// component turns that contact into a force on the other body.
    ///
    /// Why not just give the hamster a Rigidbody? Then physics would push the obstacles for us, but the
    /// obstacles would also push the hamster around, and walking would feel floaty. A CharacterController
    /// gives precise movement, and this script decides exactly how hard the hamster pushes.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class RigidbodyPusher : MonoBehaviour
    {
        [Tooltip("Push force (N) at reference speed. Higher = obstacles fly further.")]
        [SerializeField] float pushForce = 12f;

        [Tooltip("Walking speed (m/s) that gives the full push force; slower walking pushes softer.")]
        [SerializeField] float referenceSpeed = 3f;

        [Tooltip("Bodies heavier than this (kg) are too heavy to push.")]
        [SerializeField] float maxMass = 20f;

        public float PushForce { get => pushForce; set => pushForce = value; }

        CharacterController _controller;

        void Awake() => _controller = GetComponent<CharacterController>();

        // Unity calls this during CharacterController.Move() for every collider the controller touches.
        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            // Only push things that physics moves: a Rigidbody that is not kinematic and not too heavy.
            // (The ground and walls have no Rigidbody, so they are skipped here.)
            var body = hit.rigidbody;
            if (body == null || body.isKinematic || body.mass > maxMass)
                return;

            // Mostly downward move = we are standing on it, not walking into it.
            if (hit.moveDirection.y < -0.3f)
                return;

            // Push horizontally, in the direction we were moving (y = 0 so we don't push it into the ground).
            var direction = new Vector3(hit.moveDirection.x, 0f, hit.moveDirection.z).normalized;

            // Faster walking = harder push. speedFactor goes from 0 (standing) to 1 (at referenceSpeed or faster).
            var velocity = _controller.velocity;
            var speed = new Vector2(velocity.x, velocity.z).magnitude;
            var speedFactor = Mathf.Clamp01(speed / referenceSpeed);

            // Impulse = force x time. This callback runs every frame while touching, so multiplying by
            // deltaTime gives the same total push at any frame rate. Applying it at the contact point
            // (not the centre of mass) also adds torque, which is what tips a tall object over.
            var impulse = direction * (pushForce * speedFactor * Time.deltaTime);
            body.AddForceAtPosition(impulse, hit.point, ForceMode.Impulse);

            // Tell the object it was pushed (VanishAfterHit uses this to start its timer).
            // A CharacterController push does NOT trigger OnCollisionEnter on the other body, so we notify it ourselves.
            if (body.TryGetComponent(out IPushReceiver receiver))
                receiver.OnPushed(this, impulse);
        }
    }
}
