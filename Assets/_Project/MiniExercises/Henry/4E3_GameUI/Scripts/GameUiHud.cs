using Project3DGP.Core.Gauges;
using Project3DGP.Core.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Project3DGP.MiniEx.Henry
{
    /// <summary>
    /// Connects the game state to the bars and the info text.
    /// Time bar: event-driven (redraws only when the ShotTimer's Meter raises Changed).
    /// Power bar: follows the running marker every frame (it moves continuously).
    /// Info text: a prompt that tells the player what to do next, plus the last shot's distance.
    /// The HUD only reads game state; it never changes it.
    /// </summary>
    public class GameUiHud : MonoBehaviour
    {
        [SerializeField] ShotTimer timer;
        [SerializeField] GolfSwing swing;
        [SerializeField] BarView timeBar;
        [SerializeField] BarView powerBar;
        [SerializeField] Text infoText;

        [Header("Time bar colours")]
        [SerializeField] Color normalColor = new Color(0.3f, 0.8f, 0.35f);
        [SerializeField] Color warningColor = new Color(1f, 0.8f, 0.2f);
        [SerializeField] Color criticalColor = new Color(0.95f, 0.25f, 0.2f);
        [SerializeField, Range(0f, 1f)] float warningAt = 0.3f;
        [SerializeField, Range(0f, 1f)] float criticalAt = 0.1f;

        int _shownSeconds = -1;   // the second currently on the label (-1 = nothing yet)
        string _lastShot = "";    // set by the Shot event

        void OnEnable()
        {
            timer.Remaining.Changed += OnTimeChanged;   // subscribe to the event
            swing.Shot += OnShot;
        }

        void OnDisable()
        {
            timer.Remaining.Changed -= OnTimeChanged;   // always unsubscribe what you subscribed
            swing.Shot -= OnShot;
        }

        // Draw the starting state once, before any event has happened.
        void Start()
        {
            timeBar.SetValue(timer.Remaining.Normalized, instant: true);
            OnTimeChanged(timer.Remaining);
        }

        // Event handler: runs whenever the time changes (countdown each frame, -10 s after a shot, R refill).
        void OnTimeChanged(Meter time)
        {
            timeBar.SetValue(time.Normalized);

            // Colour by how much is left: green -> yellow -> red.
            // (Nested ?: = if / else if / else.)
            var fill = timeBar.FillGraphic;
            if (fill != null)
                fill.color = time.Normalized <= criticalAt ? criticalColor
                    : time.Normalized <= warningAt ? warningColor
                    : normalColor;

            // Rebuild the label only when the whole second changes (not every frame).
            var seconds = Mathf.CeilToInt(time.Current);
            if (seconds != _shownSeconds)
            {
                _shownSeconds = seconds;
                timeBar.SetLabel($"Time {seconds} s");
            }
        }

        // Event handler for GolfSwing.Shot. :0.0 = one decimal.
        void OnShot(float distance) => _lastShot = $"Last shot: {distance:0.0} m  (-{timer.ShotCost:0} s)";

        void Update()
        {
            // The marker moves every frame, so the power bar is polled every frame (instant: no smoothing,
            // the bar must show exactly where the marker is).
            var gauge = swing.PowerGauge;
            powerBar.SetValue(gauge.Value01, instant: true);
            powerBar.SetLabel($"{gauge.Value01 * 100f:0}%");

            // Prompt: the first matching situation wins.
            string prompt;
            if (timer.IsUp)
                prompt = "Time's up! Press R to restart.";
            else if (gauge.Running)
                prompt = "Press F to hit!";
            else if (swing.BallInReach != null)
                prompt = "Press F to start the power meter";
            else
                prompt = "Walk to a ball";

            infoText.text = $"{prompt}\n{_lastShot}";
        }
    }
}
