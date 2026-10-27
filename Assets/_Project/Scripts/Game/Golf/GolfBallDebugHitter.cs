using System.Collections.Generic;
using Project3DGP.Game.Controls;
using UnityEngine;

namespace Project3DGP.Game.Golf
{
    /// <summary>
    /// Development helper (#14): hits the ball without ShotController, to test the ball in the test area.
    /// Left click = hit along the camera's horizontal forward; 1/2/3 = soft/medium/hard; R = reset.
    /// Shows the ball's state and its events. Not part of the game; don't leave it in a level scene.
    /// </summary>
    public class GolfBallDebugHitter : MonoBehaviour
    {
        [SerializeField] GolfInput input;
        [SerializeField] GolfBall ball;
        [SerializeField] Transform aimCamera;
        [Tooltip("Launch speeds (m/s) for keys 1, 2, 3.")]
        [SerializeField] float[] speeds = { 6f, 12f, 20f };
        [Tooltip("Degrees above the ground.")]
        [SerializeField] float loftDegrees = 25f;

        int _speedIndex = 1;
        readonly Queue<string> _log = new();

        void OnEnable()
        {
            if (input == null || ball == null || aimCamera == null)
            {
                Debug.LogError($"{name}: assign Input, Ball and Aim Camera in the Inspector.", this);
                enabled = false;   // also calls OnDisable, which checks the same fields
                return;
            }
            input.Acquire();
            input.Shoot += OnShoot;
            input.ClubSlotSelected += OnSlot;
            input.Retry += OnRetry;
            ball.Launched += OnLaunched;
            ball.CameToRest += OnCameToRest;
            ball.WentOutOfBounds += OnOutOfBounds;
            ball.Collided += OnCollided;
        }

        void OnDisable()
        {
            if (input == null || ball == null || aimCamera == null)
                return;
            input.Shoot -= OnShoot;
            input.ClubSlotSelected -= OnSlot;
            input.Retry -= OnRetry;
            input.Release();
            ball.Launched -= OnLaunched;
            ball.CameToRest -= OnCameToRest;
            ball.WentOutOfBounds -= OnOutOfBounds;
            ball.Collided -= OnCollided;
        }

        void OnShoot()
        {
            if (!ball.IsAtRest)
                return;
            // Horizontal forward of the camera, then tilted up by the loft angle.
            var forward = Vector3.ProjectOnPlane(aimCamera.forward, Vector3.up).normalized;
            var axis = Vector3.Cross(Vector3.up, forward);   // the camera's right: tilting about it lifts the shot
            var direction = Quaternion.AngleAxis(-loftDegrees, axis) * forward;
            ball.Hit(direction * speeds[_speedIndex]);
        }

        void OnSlot(int slot) => _speedIndex = Mathf.Clamp(slot, 0, speeds.Length - 1);
        void OnRetry() => ball.ResetToLastValid();

        void OnLaunched() => Log("Launched");
        void OnCameToRest() => Log(ball.LastStopTimedOut ? "CameToRest (timeout)" : "CameToRest");
        void OnOutOfBounds() => Log("WentOutOfBounds");
        void OnCollided(Collision c) => Log("Collided with " + c.collider.name);

        void Log(string message)
        {
            _log.Enqueue($"{Time.time:0.0}s {message}");
            while (_log.Count > 6)
                _log.Dequeue();
            Debug.Log("[GolfBall] " + message);
        }

        void OnGUI()
        {
            if (!enabled)
                return;
            GUILayout.BeginArea(new Rect(10, 10, 340, 300), GUI.skin.box);
            GUILayout.Label("GolfBall debug (#14)");
            GUILayout.Label("Left click hit | 1/2/3 power | R reset | right-drag orbit");
            GUILayout.Label($"Power: {_speedIndex + 1} ({speeds[_speedIndex]} m/s)");
            GUILayout.Label($"At rest: {ball.IsAtRest}   speed: {ball.Velocity.magnitude:0.00} m/s");
            GUILayout.Label($"Last valid: {ball.LastValidPosition}");
            foreach (var line in _log)
                GUILayout.Label(line);
            GUILayout.EndArea();
        }
    }
}
