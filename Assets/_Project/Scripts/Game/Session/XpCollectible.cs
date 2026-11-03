using System;
using Project3DGP.Game.Golf;
using UnityEngine;

namespace Project3DGP.Game.Session
{
    /// <summary>
    /// A pickup the ball rolls through (#70, Form B item collection): gives XP to GameSession once, plays an optional
    /// effect and hides. Level owners place the XpSeed prefab (Magic > Game > Set Up Leveling); Retry reloads the
    /// level, so the seed is back for the next attempt. The visual spins slowly so it reads as a pickup.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class XpCollectible : MonoBehaviour
    {
        [SerializeField] int xp = 20;
        [Tooltip("Optional: the mesh to spin (a child, so the trigger stays still).")]
        [SerializeField] Transform visual;
        [SerializeField] float spinDegreesPerSecond = 90f;
        [Tooltip("Optional: an effect prefab played where the seed was (particles, #39).")]
        [SerializeField] GameObject pickupEffect;

        public int Xp => xp;
        public bool IsCollected { get; private set; }

        /// <summary>Raised once, when the ball picks it up.</summary>
        public event Action<XpCollectible> Collected;

        void Reset() => GetComponent<Collider>().isTrigger = true;

        void Update()
        {
            if (visual != null)
                visual.Rotate(0f, spinDegreesPerSecond * Time.deltaTime, 0f, Space.World);
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.GetComponentInParent<GolfBall>() != null)
                Collect();
        }

        /// <summary>Gives the XP and hides the seed (the ball calls it through the trigger; tests call it directly).</summary>
        public void Collect()
        {
            if (IsCollected)
                return;
            IsCollected = true;
            if (GameSession.Instance != null)
                GameSession.Instance.AddXp(xp);
            if (pickupEffect != null)
                Instantiate(pickupEffect, transform.position, Quaternion.identity);
            Collected?.Invoke(this);
            gameObject.SetActive(false);
        }
    }
}
