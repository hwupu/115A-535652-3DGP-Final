using System;
using System.Collections;
using Project3DGP.Core.UI;
using UnityEngine;
// using UnityEngine.SceneManagement;   // needed for the scene-switching block below

namespace Project3DGP.Core.Portals
{
    /// <summary>
    /// Put this on the character. When a <see cref="Portal"/> calls <see cref="Travel"/>:
    /// fade out -> move to the destination -> particle burst -> fade in. A cooldown stops the character
    /// from bouncing straight back through the portal it arrives next to.
    /// Timeline of one teleport (fadeTime = 0.25 s):
    ///   0.00 s  fade to black starts
    ///   0.25 s  screen is black -> move the hamster (the player doesn't see the jump) -> burst
    ///   0.50 s  scene visible again; portals usable again 1 s after arriving (cooldown)
    /// </summary>
    public class PortalTraveler : MonoBehaviour
    {
        [Tooltip("Seconds after arriving before another portal can be used.")]
        [SerializeField] float cooldown = 1f;
        [Tooltip("Optional. Without a fader the teleport is instant.")]
        [SerializeField] ScreenFader fader;
        [SerializeField] float fadeTime = 0.25f;
        [Tooltip("World-space particle system, moved to the character and burst on arrival.")]
        [SerializeField] ParticleSystem arrivalBurst;
        [SerializeField] int arrivalParticleCount = 40;

        /// <summary>Raised after arriving, with the portal that was used.</summary>
        public event Action<PortalTraveler, Portal> Teleported;

        public bool IsTraveling { get; private set; }
        public int TeleportCount { get; private set; }

        float _readyTime;                  // Time.time when portals may be used again
        CharacterController _controller;   // optional; must be switched off while moving it

        void Awake() => _controller = GetComponent<CharacterController>();

        public void Travel(Portal portal)
        {
            // Ignore portals while a teleport is running or during the cooldown after arriving.
            if (IsTraveling || Time.time < _readyTime)
                return;
            StartCoroutine(TravelRoutine(portal));
        }

        // A coroutine runs over several frames: each `yield return` waits, then continues where it left off.
        IEnumerator TravelRoutine(Portal portal)
        {
            IsTraveling = true;

            if (fader != null)
                yield return fader.FadeTo(1f, fadeTime);   // to black

            // Scene (level) switching, for the game's garden gate (#38). Uncomment (with the field in Portal
            // and the using at the top) to load another scene instead of teleporting:
            // if (!string.IsNullOrEmpty(portal.DestinationScene))
            // {
            //     SceneManager.LoadScene(portal.DestinationScene);
            //     yield break;
            // }

            // A CharacterController keeps its own idea of where it is: moving the transform while it is
            // enabled gets overwritten on the next Move(). So: off -> move -> sync physics -> on.
            if (_controller != null)
                _controller.enabled = false;
            transform.SetPositionAndRotation(portal.Destination.position, portal.Destination.rotation);
            Physics.SyncTransforms();   // tell the physics engine about the new position right away
            if (_controller != null)
                _controller.enabled = true;

            if (arrivalBurst != null)
            {
                // One shared particle system is moved to the hamster and emits a burst. It simulates in world
                // space, so the particles stay where they were born even when the system moves again.
                arrivalBurst.transform.position = transform.position + Vector3.up * 0.4f;   // around the body
                arrivalBurst.Emit(arrivalParticleCount);
            }

            TeleportCount++;
            _readyTime = Time.time + cooldown;
            Teleported?.Invoke(this, portal);   // event: the HUD updates the area name and counter

            if (fader != null)
                yield return fader.FadeTo(0f, fadeTime);   // back to the scene

            IsTraveling = false;
        }
    }
}
