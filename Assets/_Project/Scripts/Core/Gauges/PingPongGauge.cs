using System;
using UnityEngine;

namespace Project3DGP.Core.Gauges
{
    /// <summary>
    /// A timing gauge like a golf game's power meter: after <see cref="Begin"/> the marker runs
    /// 0 -> 1 -> 0 -> 1 ... and <see cref="Stop"/> freezes it and returns where it stopped.
    /// The player's timing is the skill: stopping at the top gives 100% power.
    /// Example: 0.8 cycles per second -> 0 to 1 takes 0.625 s, and back down another 0.625 s.
    /// </summary>
    [Serializable]
    public class PingPongGauge
    {
        [Tooltip("Full up-and-down cycles per second. Higher = harder to time.")]
        [SerializeField] float cyclesPerSecond = 0.8f;

        float _phase;   // how many cycles have passed since Begin()

        public float Value01 { get; private set; }
        public bool Running { get; private set; }
        public float CyclesPerSecond { get => cyclesPerSecond; set => cyclesPerSecond = value; }

        public void Begin()
        {
            _phase = 0f;
            Value01 = 0f;
            Running = true;
        }

        /// <summary>Freezes the marker and returns its value (0..1).</summary>
        public float Stop()
        {
            Running = false;
            return Value01;
        }

        /// <summary>Call once per frame.</summary>
        public void Tick(float deltaTime)
        {
            if (!Running)
                return;

            _phase += deltaTime * cyclesPerSecond;
            // Mathf.PingPong(t, 1) goes up 0 -> 1 while t goes 0 -> 1, then back down while t goes 1 -> 2.
            // One cycle (up and down) = 2 units of t, so t = phase * 2.
            Value01 = Mathf.PingPong(_phase * 2f, 1f);
        }
    }
}
