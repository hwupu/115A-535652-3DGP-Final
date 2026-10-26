using System;
using UnityEngine;

namespace Project3DGP.Core.Portals
{
    /// <summary>
    /// A portal: a trigger zone that sends a <see cref="PortalTraveler"/> to <see cref="Destination"/>.
    /// It can start locked (grey, no particles) and be unlocked later by game code, e.g. a switch,
    /// or in the game "level completed" (#38). The travel itself (fade, move, burst) is done by the traveler.
    ///
    /// The flow to tell the TA (one teleport):
    ///   1. The hamster walks into the portal's trigger box -> Unity calls OnTriggerEnter here.
    ///   2. If the portal is open and the visitor has a PortalTraveler, we call traveler.Travel(this).
    ///   3. PortalTraveler fades out, moves the hamster to Destination, bursts particles, fades in.
    /// Split of jobs: the Portal decides WHO may travel and WHERE to; the traveler decides HOW.
    /// </summary>
    public class Portal : MonoBehaviour
    {
        // Destination is just a Transform (the "Arrival" empty object in each area): its position AND
        // rotation are copied to the hamster, so it also faces the right way after arriving.
        [Tooltip("Where the traveler arrives. Put it a little away from the destination's own portal.")]
        [SerializeField] Transform destination;
        [SerializeField] bool locked;

        // Scene (level) switching, for the game's garden gate (#38). Not used in this demo; to try it,
        // uncomment these lines (and DestinationScene below) and the block in PortalTraveler.TravelRoutine(), and add the scene to Build Settings.
        // [Tooltip("If set, the portal loads this scene instead of teleporting inside the current one.")]
        // [SerializeField] string destinationScene;

        [Header("Look")]
        [Tooltip("Looping particles at the portal (the swirl). Stopped while locked.")]
        [SerializeField] ParticleSystem loopParticles;
        [Tooltip("Renderer tinted with the open/locked colour (e.g. the glowing pad).")]
        [SerializeField] Renderer padRenderer;
        [SerializeField] Color openColor = new Color(0.6f, 0.3f, 0.95f);
        [SerializeField] Color lockedColor = new Color(0.35f, 0.35f, 0.35f);

        // Shader property ID of the URP Lit colour, looked up once instead of by string every time.
        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        /// <summary>Raised when the portal is unlocked.</summary>
        public event Action<Portal> Unlocked;

        public Transform Destination => destination;
        // public string DestinationScene => destinationScene;
        public bool IsLocked => locked;

        // Show the starting state (open or locked) when the scene starts.
        void Start() => ApplyLook();

        /// <summary>Opens a locked portal. Called by PortalUnlockSwitch here; by "cup completed" in the game.</summary>
        public void Unlock()
        {
            if (!locked)
                return;
            locked = false;
            ApplyLook();
            Unlocked?.Invoke(this);
        }

        // Unity calls this when another collider enters our trigger collider. (Triggers detect overlaps but
        // don't block movement.) The kinematic Rigidbody on the portal makes sure this event fires.
        void OnTriggerEnter(Collider other)
        {
            if (locked || destination == null)
                return;
            // Only objects with a PortalTraveler can travel (the hamster); anything else is ignored.
            if (other.TryGetComponent(out PortalTraveler traveler))
                traveler.Travel(this);
        }

        /// <summary>Open = portal colour + particles running; locked = grey + particles stopped.</summary>
        void ApplyLook()
        {
            var color = locked ? lockedColor : openColor;   // ?: = if locked use grey, else the portal's colour

            if (padRenderer != null)
            {
                // MaterialPropertyBlock: change this renderer's colour without creating a new material.
                var block = new MaterialPropertyBlock();
                padRenderer.GetPropertyBlock(block);
                block.SetColor(BaseColorId, color);
                padRenderer.SetPropertyBlock(block);
            }

            if (loopParticles != null)
            {
                var main = loopParticles.main;
                main.startColor = color;   // new particles get the portal's colour
                if (locked)
                    loopParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                else
                    loopParticles.Play();
            }
        }
    }
}
