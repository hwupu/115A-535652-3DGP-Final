using System;
using Project3DGP.Core.Gauges;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

namespace Project3DGP.MiniEx.Henry
{
    /// <summary>
    /// The hamster's swing. Walk up to a ball, press F to start the power meter, press F again to hit.
    /// Shot distance = club max distance x power (where the marker stopped). The club's accuracy adds a
    /// random sideways angle. Each shot costs time on the ShotTimer, and plays a sound and a particle
    /// burst at the ball. R puts all balls back.
    ///
    /// The flow to tell the TA (one shot):
    ///   1. Update() finds the nearest ball in front of the hamster (FindBallInReach).
    ///   2. F (1st press) starts the power meter at the club's speed (PingPongGauge.Begin).
    ///   3. F (2nd press) calls Swing(): power = where the marker stopped (0..1).
    ///   4. distance = club.maxDistance x power; direction = facing + random error from accuracy.
    ///   5. Effects (particles + sound), the ball starts moving, the ShotTimer loses 10 s,
    ///      and the Shot event tells the HUD to show the distance.
    /// </summary>
    public class GolfSwing : MonoBehaviour
    {
        [SerializeField] ClubSelector clubs;
        [SerializeField] ShotTimer timer;
        [Tooltip("How close a ball must be to hit it (m).")]
        [SerializeField] float reach = 1.2f;
        [Tooltip("Largest sideways error for a club with 0% accuracy (degrees).")]
        [SerializeField] float maxErrorDegrees = 20f;
        [SerializeField] PingPongGauge powerGauge = new PingPongGauge();
        [SerializeField] AudioSource audioSource;
        [SerializeField] AudioClip hitSound;
        [Tooltip("World-space particle system, moved to the ball on each hit.")]
        [SerializeField] ParticleSystem hitParticles;
        [SerializeField] int hitParticleCount = 30;

        GolfBall[] _balls;   // all balls in the scene, found once at Start

        // Event: the HUD subscribes to show "Last shot: 12.3 m". GolfSwing doesn't know about the HUD.
        /// <summary>Raised after each shot with the distance (m).</summary>
        public event Action<float> Shot;

        public PingPongGauge PowerGauge => powerGauge;
        public GolfBall BallInReach { get; private set; }
        public float LastShotDistance { get; private set; }

        // Find every GolfBall once (searching the scene every frame would be slow).
        void Start() => _balls = FindObjectsByType<GolfBall>(FindObjectsSortMode.None);

        void Update()
        {
            // Check every frame which ball (if any) we could hit; the HUD uses it for the prompt text.
            BallInReach = FindBallInReach();

            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                // The same key does two things, depending on whether the meter is running (a tiny state machine).
                if (keyboard.fKey.wasPressedThisFrame)
                {
                    if (powerGauge.Running)
                        Swing();                       // second press: hit
                    else if (BallInReach != null && !timer.IsUp)
                    {
                        powerGauge.CyclesPerSecond = clubs.Current.meterSpeed;
                        powerGauge.Begin();            // first press: start the marker
                    }
                }

                // R: put every ball back where it started (ShotTimer refills the time on R as well).
                if (keyboard.rKey.wasPressedThisFrame)
                    foreach (var ball in _balls)
                        ball.ResetToHome();
            }

            // Walked away from the ball: cancel the swing.
            if (powerGauge.Running && BallInReach == null)
                powerGauge.Stop();

            // Advance the marker by this frame's time (does nothing while the meter is stopped).
            powerGauge.Tick(Time.deltaTime);
        }

        void Swing()
        {
            var power = powerGauge.Stop();   // 0..1: where the marker was when F was pressed
            var club = clubs.Current;        // the club selected on the cards (ClubSelector)

            // Accuracy: rotate the facing direction by a random angle; the less accurate, the wider the range.
            // Example: accuracy 0.45 (Sledgehammer) -> up to (1 - 0.45) x 20 = 11 degrees left or right;
            //          accuracy 1.0 (Putter)        -> 0 degrees, always straight.
            var maxError = (1f - club.accuracy) * maxErrorDegrees;
            var direction = Quaternion.Euler(0f, Random.Range(-maxError, maxError), 0f) * transform.forward;
            // Example: Sledgehammer 28 m x 50% power = 14 m.
            var distance = club.maxDistance * power;

            // Burst at the ball's position before it starts moving. The system simulates in world space,
            // so moving it next time doesn't drag the old particles along.
            if (hitParticles != null)
            {
                hitParticles.transform.position = BallInReach.transform.position;
                hitParticles.Emit(hitParticleCount);
            }

            BallInReach.Hit(direction, distance);   // the ball moves itself from now on (GolfBall.Update)
            timer.ChargeShot();                      // -10 s: the time bar reacts through the Meter's Changed event
            if (audioSource != null && hitSound != null)
                audioSource.PlayOneShot(hitSound);

            LastShotDistance = distance;
            Shot?.Invoke(distance);
        }

        /// <summary>The nearest resting ball within reach and in front of the hamster, or null.</summary>
        GolfBall FindBallInReach()
        {
            GolfBall best = null;
            var bestDistance = reach;   // start with the reach, so only balls closer than that can win
            foreach (var ball in _balls)
            {
                if (ball.IsMoving)
                    continue;
                var offset = ball.transform.position - transform.position;
                offset.y = 0f;                     // compare on the ground only (ignore height)
                var distance = offset.magnitude;
                // Dot product > 0.3: the ball is roughly in front (within about 70 degrees of facing).
                if (distance < bestDistance && Vector3.Dot(transform.forward, offset.normalized) > 0.3f)
                {
                    best = ball;
                    bestDistance = distance;
                }
            }
            return best;
        }
    }
}
