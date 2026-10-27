using System;

namespace Project3DGP.Core.Motion
{
    /// <summary>
    /// Decides when something moving has stopped: its speed stayed below a threshold for some time,
    /// or a timeout passed (so a ball stuck rocking in a corner still ends the shot).
    /// Plain C# (no MonoBehaviour), so it can be unit-tested without physics.
    /// </summary>
    [Serializable]
    public class RestDetector
    {
        public float speedThreshold = 0.15f;   // m/s: slower than this counts as "still"
        public float restSeconds = 0.5f;       // must stay still this long (the top of a bounce is slow only for an instant)
        public float timeoutSeconds = 20f;     // give up after this long and call it stopped anyway

        float _stillTime;   // seconds in a row below the threshold
        float _totalTime;   // seconds since Begin()

        public bool TimedOut { get; private set; }

        public RestDetector() { }

        public RestDetector(float speedThreshold, float restSeconds, float timeoutSeconds)
        {
            this.speedThreshold = speedThreshold;
            this.restSeconds = restSeconds;
            this.timeoutSeconds = timeoutSeconds;
        }

        /// <summary>Start a new motion (e.g. the ball was hit).</summary>
        public void Begin()
        {
            _stillTime = 0f;
            _totalTime = 0f;
            TimedOut = false;
        }

        /// <summary>Feed the current speed once per step. Returns true when the object counts as stopped.</summary>
        public bool Tick(float speed, float deltaTime)
        {
            _totalTime += deltaTime;
            // Any fast moment restarts the "still" clock: we want restSeconds of continuous stillness.
            _stillTime = speed < speedThreshold ? _stillTime + deltaTime : 0f;

            if (_stillTime >= restSeconds)
                return true;
            if (_totalTime >= timeoutSeconds)
            {
                TimedOut = true;
                return true;
            }
            return false;
        }
    }
}
