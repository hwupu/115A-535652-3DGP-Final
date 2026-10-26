using Project3DGP.Core.Motion;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project3DGP.MiniEx.Henry
{
    /// <summary>
    /// Drives the hierarchy for one golf swing when F is pressed:
    ///   Address       : C wiggles like aiming (sin), the rest is still
    ///   Backswing     : SLOW, A turns up to backswingAngle, B turns the club face open
    ///   Downswing     : FAST (accelerating), A goes through 0 = impact -> the ball is launched
    ///   Follow-through: hold the finish pose
    ///   Return        : back to address, wiggle again
    /// The ball flies for 5 s, then goes back to the tee; F works again when it is back.
    ///
    /// How it drives the hierarchy (the story to tell the TA):
    ///   - This script only sets two numbers per frame: shoulder.Angle (A) and wrist.Angle (B).
    ///   - Each AxisJoint turns its own object about its own local axis by that angle.
    ///   - The parent-child links do the rest: B and C are children of A, so they follow A's turn;
    ///     C (the club) is a child of B, so it also follows B's turn. Nobody computes the club's world position.
    ///   - The phases are a small state machine: one Phase value + a timer (_time) that restarts in each phase.
    /// </summary>
    public class SwingAnimator : MonoBehaviour
    {
        // The states of the swing, in order. Current says which one we are in.
        public enum Phase { Address, Backswing, Downswing, FollowThrough, Return }

        [Header("Hierarchy")]
        [SerializeField] AxisJoint shoulder;    // A
        [SerializeField] AxisJoint wrist;       // B
        [SerializeField] SineMotion waggle;     // C
        [SerializeField] ToolSwitcher tools;
        [SerializeField] FlyingBall ball;

        [Header("Swing (degrees, seconds)")]
        [SerializeField] float backswingAngle = 120f;
        [SerializeField] float finishAngle = -110f;
        [SerializeField] float wristTurn = 60f;
        [SerializeField] float backswingTime = 1.2f;
        [SerializeField] float downswingTime = 0.2f;
        [SerializeField] float holdTime = 0.6f;
        [SerializeField] float returnTime = 0.8f;

        [Header("Ball")]
        [SerializeField] float launchSpeed = 8f;    // m/s along the ground
        [SerializeField] float launchHeight = 5f;   // m/s upward
        [SerializeField] float curve = 3f;          // m/s^2 sideways (hook/slice)
        [Tooltip("Launch speed multiplier per tool, same order as ToolSwitcher (club, sledgehammer).")]
        [SerializeField] float[] toolPower = { 1f, 1.6f };   // the sledgehammer hits 1.6x harder

        float _time;     // seconds in the current phase
        bool _ballHit;   // already launched in this downswing?

        public Phase Current { get; private set; } = Phase.Address;

        void Update()
        {
            // Address: wait for F (only while the ball is on the tee, so one ball = one swing).
            if (Current == Phase.Address)
            {
                if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame && !ball.InFlight)
                    Enter(Phase.Backswing);
                return;
            }

            // In every other phase: advance the phase timer, then pose the joints for this moment.
            // Each phase uses k = _time / phaseDuration, which goes 0 -> 1 during the phase.
            _time += Time.deltaTime;
            switch (Current)
            {
                case Phase.Backswing:
                {
                    // SmoothStep = slow start, slow end: a calm, controlled backswing.
                    var k = Mathf.SmoothStep(0f, 1f, _time / backswingTime);
                    shoulder.Angle = backswingAngle * k;
                    wrist.Angle = wristTurn * k;
                    if (_time >= backswingTime)
                        Enter(Phase.Downswing);
                    break;
                }
                case Phase.Downswing:
                {
                    // k^2 = starts slow, ends fast: the club accelerates into the ball.
                    var k = Mathf.Clamp01(_time / downswingTime);
                    var eased = k * k;
                    // Lerp(a, b, x) = a + (b - a) x: from the top of the backswing (120) to the finish (-110).
                    shoulder.Angle = Mathf.Lerp(backswingAngle, finishAngle, eased);
                    wrist.Angle = Mathf.Lerp(wristTurn, -wristTurn * 0.5f, eased);   // face open -> square -> closed (release)

                    // Impact = the moment A passes its rest angle (club straight down at the ball).
                    if (!_ballHit && shoulder.Angle <= 0f)
                    {
                        _ballHit = true;
                        LaunchBall();
                    }
                    if (k >= 1f)
                        Enter(Phase.FollowThrough);
                    break;
                }
                case Phase.FollowThrough:
                    // Hold the finish pose for a moment (nothing moves).
                    if (_time >= holdTime)
                        Enter(Phase.Return);
                    break;
                case Phase.Return:
                {
                    // Calmly back to the address pose (angles to 0).
                    var k = Mathf.SmoothStep(0f, 1f, _time / returnTime);
                    shoulder.Angle = Mathf.Lerp(finishAngle, 0f, k);
                    wrist.Angle = Mathf.Lerp(-wristTurn * 0.5f, 0f, k);
                    if (_time >= returnTime)
                        Enter(Phase.Address);
                    break;
                }
            }
        }

        /// <summary>Switches to a new phase and restarts the phase timer.</summary>
        void Enter(Phase phase)
        {
            Current = phase;
            _time = 0f;
            if (phase == Phase.Backswing)
                _ballHit = false;
            // C only wiggles while standing at address; during the swing the club is held firmly.
            waggle.enabled = phase == Phase.Address;
        }

        void LaunchBall()
        {
            // The backswing goes to the hamster's right (+X), so the ball flies to its left (-X).
            // Reversing A (key 1) mirrors the swing, so the ball flies the other way.
            var target = -transform.right * shoulder.Direction;
            // Which tool is in the hand (T)? Look up its power; 1 if the list is too short.
            var power = toolPower.Length > tools.CurrentIndex ? toolPower[tools.CurrentIndex] : 1f;
            // Launch velocity = forward part (along the ground) + upward part. Example club: 8 m/s across, 5 m/s up.
            var velocity = target * (launchSpeed * power) + Vector3.up * launchHeight;
            // The wrist turn (B) decides the curve: reversing B (key 2) curves the ball the other way.
            var side = transform.forward * (curve * wrist.Direction);
            ball.Launch(velocity, side);
        }
    }
}
