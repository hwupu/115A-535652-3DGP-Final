using Project3DGP.Core.Gauges;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project3DGP.MiniEx.Henry
{
    /// <summary>
    /// The time meter: counts down in real time, and every shot costs extra seconds (an event).
    /// At 0 no more shots are allowed. R refills it.
    /// This class only changes the Meter; it never touches the UI. The HUD listens to the Meter's
    /// Changed event and redraws the bar (separating game logic from UI).
    /// </summary>
    public class ShotTimer : MonoBehaviour
    {
        // Meter is [Serializable], so its Max and Current show up in the Inspector under "Time".
        [SerializeField] Meter time = new Meter(120f);   // 120 s to start with
        [Tooltip("Seconds taken away by each shot.")]
        [SerializeField] float shotCost = 10f;
        [SerializeField] bool countDown = true;

        public Meter Remaining => time;
        public bool IsUp => time.IsEmpty;
        public float ShotCost => shotCost;

        void Update()
        {
            // Real-time countdown: subtract the time this frame took (about 0.016 s at 60 fps).
            if (countDown)
                time.Subtract(Time.deltaTime);

            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.rKey.wasPressedThisFrame)
                time.Fill();
        }

        /// <summary>Called by GolfSwing after each shot.</summary>
        public void ChargeShot() => time.Subtract(shotCost);
    }
}
