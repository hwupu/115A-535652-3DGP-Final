using System;
using UnityEngine;

namespace Project3DGP.Core.Gauges
{
    /// <summary>
    /// A value between 0 and Max (health, stamina, time left...) that raises events when it changes.
    /// The UI doesn't poll it: it subscribes to <see cref="Changed"/> and redraws only when something
    /// happened (damage, healing, a shot costing time...). That is the "bars update with events" part.
    /// </summary>
    // [Serializable] + plain class (not a MonoBehaviour): it can be a field of any component and
    // still be edited in the Inspector. One class works for health, stamina, XP, time...
    [Serializable]
    public class Meter
    {
        [SerializeField] float max = 100f;
        [SerializeField] float current = 100f;

        /// <summary>Raised after every change of <see cref="Current"/>.</summary>
        public event Action<Meter> Changed;

        /// <summary>Raised once when the value reaches 0.</summary>
        public event Action<Meter> Emptied;

        public Meter() { }   // needed by Unity's serializer

        public Meter(float max)
        {
            this.max = max;
            current = max;
        }

        public float Max => max;
        public float Current => current;
        /// <summary>Current / Max, from 0 to 1: what a bar shows as its fill.</summary>
        public float Normalized => max > 0f ? current / max : 0f;
        public bool IsEmpty => current <= 0f;

        public void Add(float amount) => Set(current + amount);
        public void Subtract(float amount) => Set(current - amount);
        public void Fill() => Set(max);

        public void Set(float value)
        {
            // Every change goes through Set(), so the events can never be forgotten.
            var previous = current;
            current = Mathf.Clamp(value, 0f, max);   // never below 0 or above Max
            if (Mathf.Approximately(previous, current))
                return;   // no real change (e.g. already full): no event

            Changed?.Invoke(this);
            // Emptied only fires on the step from above 0 to 0, not again while it stays at 0.
            if (current <= 0f && previous > 0f)
                Emptied?.Invoke(this);
        }
    }
}
