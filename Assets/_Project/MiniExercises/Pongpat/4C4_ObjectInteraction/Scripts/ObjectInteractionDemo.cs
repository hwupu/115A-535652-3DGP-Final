using System.Collections.Generic;
using Project3DGP.Core.Interaction;
using Project3DGP.Core.Movement;
using Project3DGP.Core.Spawning;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project3DGP.MiniEx.Pongpat.ObjectInteraction
{
    /// <summary>
    /// Input, pooling and evidence for Pongpat's standalone exercise. All bodies and scenery are
    /// authored by the Magic tool; this component only moves and reuses those existing bodies.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class ObjectInteractionDemo : MonoBehaviour
    {
        [SerializeField] PlanarBodyMotor character;
        [SerializeField] ImpactRecycleBody[] obstacles = new ImpactRecycleBody[0];
        [SerializeField] Vector3 respawnCenter = new Vector3(0f, 0.53f, 0f);
        [SerializeField] Vector2 respawnSize = new Vector2(27f, 27f);
        [SerializeField] Vector3 obstacleHalfExtents = Vector3.one * 0.45f;
        [SerializeField, Min(0f)] float clearance = 0.04f;
        [SerializeField, Min(1)] int placementAttempts = 48;
        [SerializeField, Min(0.02f)] float retryInterval = 0.15f;
        [SerializeField, Min(0.1f)] float collisionWindow = 2f;
        [SerializeField] bool recyclingEnabled;

        readonly Dictionary<Rigidbody, int> _indices = new Dictionary<Rigidbody, int>();
        Vector3[] _startPositions;
        Quaternion[] _startRotations;
        float[] _lastContacts;
        bool[] _pending;
        Rigidbody _characterBody;
        Vector3 _characterStart;
        Quaternion _characterRotation;
        float _nextRespawnAttempt;
        GUIStyle _hudStyle;

        public int ObstacleCount => obstacles.Length;
        public int ActiveCount { get; private set; }
        public int PendingCount { get; private set; }
        public int RecentCollisionCount { get; private set; }
        public int PeakCollisionCount { get; private set; }
        public int RespawnCount { get; private set; }
        public bool RecyclingEnabled => recyclingEnabled;

        void Awake()
        {
            _startPositions = new Vector3[obstacles.Length];
            _startRotations = new Quaternion[obstacles.Length];
            _lastContacts = new float[obstacles.Length];
            _pending = new bool[obstacles.Length];
            for (var i = 0; i < obstacles.Length; i++)
            {
                var obstacle = obstacles[i];
                if (obstacle == null)
                    continue;
                _indices[obstacle.GetComponent<Rigidbody>()] = i;
                _startPositions[i] = obstacle.transform.position;
                _startRotations[i] = obstacle.transform.rotation;
                _lastContacts[i] = float.NegativeInfinity;
            }

            if (character != null)
            {
                _characterBody = character.GetComponent<Rigidbody>();
                _characterStart = character.transform.position;
                _characterRotation = character.transform.rotation;
            }
        }

        void OnEnable()
        {
            foreach (var obstacle in obstacles)
            {
                if (obstacle == null)
                    continue;
                obstacle.BodyCollided += OnBodyCollided;
                obstacle.Recycled += OnRecycled;
                obstacle.RecyclingEnabled = recyclingEnabled;
            }
        }

        void OnDisable()
        {
            foreach (var obstacle in obstacles)
            {
                if (obstacle == null)
                    continue;
                obstacle.BodyCollided -= OnBodyCollided;
                obstacle.Recycled -= OnRecycled;
                // Pause the countdown while no pool owner is listening for the recycle event.
                obstacle.RecyclingEnabled = false;
            }
            if (character != null)
            {
                character.MoveInput = Vector2.zero;
                character.Sprint = false;
            }
        }

        void Update()
        {
            ReadControls();
            if (Time.time >= _nextRespawnAttempt)
            {
                TryRespawns();
                _nextRespawnAttempt = Time.time + retryInterval;
            }
            UpdateCounts();
        }

        void ReadControls()
        {
            var keyboard = Keyboard.current;
            var input = Vector2.zero;
            if (keyboard != null)
            {
                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) input.y++;
                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) input.y--;
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) input.x++;
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) input.x--;
                if (keyboard.rKey.wasPressedThisFrame) ResetDemo();
                if (keyboard.vKey.wasPressedThisFrame) SetRecycling(!recyclingEnabled);
            }
            if (character == null)
                return;
            character.MoveInput = input;
            character.Sprint = keyboard != null &&
                (keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed);
        }

        void OnBodyCollided(ImpactRecycleBody obstacle, Rigidbody other)
        {
            // A character hit starts the recycle timer, but only two pool members count toward
            // the >=10-obstacle requirement. Ground contacts and repeated callbacks cannot inflate it.
            if (!_indices.TryGetValue(other, out var otherIndex) ||
                !_indices.TryGetValue(obstacle.Body, out var index))
                return;
            _lastContacts[index] = Time.time;
            _lastContacts[otherIndex] = Time.time;
        }

        void OnRecycled(ImpactRecycleBody obstacle)
        {
            if (_indices.TryGetValue(obstacle.Body, out var index))
                _pending[index] = true;
        }

        void TryRespawns()
        {
            // Bodies that vanished in FixedUpdate must be absent from the physics queries.
            // Synchronize again after each placement so later candidates see earlier respawns.
            var synchronized = false;
            for (var i = 0; i < obstacles.Length; i++)
            {
                if (!_pending[i])
                    continue;
                if (!synchronized)
                {
                    Physics.SyncTransforms();
                    synchronized = true;
                }
                if (!RandomBoxPlacement.TryFind(respawnCenter, respawnSize,
                    obstacleHalfExtents, clearance, placementAttempts, out var position))
                    continue;

                var obstacle = obstacles[i];
                obstacle.ResetImpact();
                obstacle.transform.SetPositionAndRotation(position, Quaternion.identity);
                obstacle.gameObject.SetActive(true);
                // Body is cached by Awake before its first hit/recycle.
                obstacle.Body.position = position;
                obstacle.Body.rotation = Quaternion.identity;
                obstacle.Body.linearVelocity = Vector3.zero;
                obstacle.Body.angularVelocity = Vector3.zero;
                obstacle.Body.WakeUp();
                _pending[i] = false;
                RespawnCount++;
                Physics.SyncTransforms();
            }
        }

        public void SetRecycling(bool enabled)
        {
            recyclingEnabled = enabled;
            foreach (var obstacle in obstacles)
            {
                if (obstacle == null)
                    continue;
                obstacle.RecyclingEnabled = enabled;
                obstacle.ResetImpact();
            }
        }

        public void ResetDemo()
        {
            // Deactivate the entire pool first: no body can collide with a partially reset layout.
            foreach (var obstacle in obstacles)
                if (obstacle != null) obstacle.gameObject.SetActive(false);
            if (_characterBody != null)
            {
                _characterBody.position = _characterStart;
                _characterBody.rotation = _characterRotation;
                _characterBody.linearVelocity = Vector3.zero;
                _characterBody.angularVelocity = Vector3.zero;
                character.MoveInput = Vector2.zero;
            }
            for (var i = 0; i < obstacles.Length; i++)
            {
                var obstacle = obstacles[i];
                if (obstacle == null)
                    continue;
                obstacle.ResetImpact();
                obstacle.transform.SetPositionAndRotation(_startPositions[i], _startRotations[i]);
                obstacle.gameObject.SetActive(true);
                var body = obstacle.GetComponent<Rigidbody>();
                body.position = _startPositions[i];
                body.rotation = _startRotations[i];
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                body.WakeUp();
                _pending[i] = false;
                _lastContacts[i] = float.NegativeInfinity;
            }
            PeakCollisionCount = 0;
            RespawnCount = 0;
            _nextRespawnAttempt = Time.time;
            Physics.SyncTransforms();
            UpdateCounts();
        }

        void UpdateCounts()
        {
            ActiveCount = PendingCount = RecentCollisionCount = 0;
            for (var i = 0; i < obstacles.Length; i++)
            {
                if (obstacles[i] != null && obstacles[i].gameObject.activeInHierarchy) ActiveCount++;
                if (_pending[i]) PendingCount++;
                if (Time.time - _lastContacts[i] <= collisionWindow) RecentCollisionCount++;
            }
            PeakCollisionCount = Mathf.Max(PeakCollisionCount, RecentCollisionCount);
        }

        void OnGUI()
        {
            if (_hudStyle == null)
                _hudStyle = new GUIStyle(GUI.skin.label) { fontSize = 17, wordWrap = true };
            GUI.Box(new Rect(12f, 12f, 550f, 170f), GUIContent.none);
            GUI.Label(new Rect(24f, 22f, 526f, 150f),
                "Pongpat | 4.C.4 Object interaction\n" +
                "WASD / arrows: walk    Shift: sprint    R: reset    V: recycle\n" +
                $"Cubes: {ActiveCount} active + {PendingCount} waiting / {ObstacleCount} total\n" +
                $"Obstacles in mutual contacts (last {collisionWindow:0.#} s): {RecentCollisionCount}   Peak: {PeakCollisionCount}\n" +
                $"Recycling: {(recyclingEnabled ? "ON (2 s after hit)" : "OFF")}    Respawns: {RespawnCount}\n" +
                "Hold Shift + W into the dense group. Aim for a peak of 10+.", _hudStyle);
        }
    }
}
