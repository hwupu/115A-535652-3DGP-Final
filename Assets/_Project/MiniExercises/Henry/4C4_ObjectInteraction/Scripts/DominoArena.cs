using System.Collections.Generic;
using Project3DGP.Core.Interaction;
using Project3DGP.Core.Spawning;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project3DGP.MiniEx.Henry
{
    /// <summary>
    /// Spawns the obstacles of 4.C.4: lineCount domino lines with dominoesPerLine tiles each, fanned out
    /// like the spokes of a wheel around the arena centre (loop-spawned, allowed by ADR-0010).
    /// Push the first tile of a line and the whole line topples: >= 10 obstacles collide with each other.
    /// Extra: a hit tile vanishes after vanishDelay seconds (VanishAfterHit) and the same tile is placed
    /// back at its starting spot in the line once that spot is free, so the obstacle count never changes.
    /// (The earlier random respawn with NonOverlapPlacement is kept, commented out, in TryRespawn.)
    /// R rebuilds the lines, V toggles vanishing.
    ///
    /// How the pieces fit together (the story to tell the TA):
    ///   1. Start() creates 100 dominoes ONCE and keeps them in a list (object pool, no Destroy/Instantiate later).
    ///   2. BuildLines() puts them in 10 lines using polar coordinates (angle per line, radius per tile).
    ///   3. The hamster's RigidbodyPusher pushes the first tile -> physics topples the rest of the line.
    ///   4. Each tile's VanishAfterHit raises events: Hit (turn orange), BodyContact (count it), Vanished.
    ///   5. On Vanished, TryRespawn() puts the tile back home if the spot is free, otherwise it waits.
    /// </summary>
    public class DominoArena : MonoBehaviour
    {
        // ---- Inspector settings (all of these are TA modification points) ----

        [Header("Layout")]
        [SerializeField] VanishAfterHit dominoPrefab;
        [SerializeField, Min(1)] int lineCount = 10;          // 10 lines ...
        [SerializeField, Min(1)] int dominoesPerLine = 10;    // ... x 10 tiles = 100 obstacles
        [Tooltip("Distance from the arena centre to the first tile of each line (m).")]
        [SerializeField] float firstDominoRadius = 2.5f;
        [Tooltip("Gap between tile centres along a line (m). Must be smaller than the tile height to chain.")]
        [SerializeField] float spacing = 0.6f;                // tile is 1 m tall, so a falling tile reaches the next one

        [Header("Vanish and respawn (extra)")]
        [SerializeField] bool vanishEnabled = true;
        [SerializeField] float vanishDelay = 5f;              // seconds from first hit to vanishing
        [Tooltip("Extra free space kept around a respawned tile (m).")]
        [SerializeField] float respawnPadding = 0.1f;
        // Random respawn (not used now, see TryRespawn):
        // [Tooltip("Width and depth of the random respawn area, centred on the arena (m).")]
        // [SerializeField] Vector2 respawnArea = new Vector2(21f, 21f);
        // [SerializeField] int maxTries = 30;

        [Header("Look")]
        [SerializeField] Color idleColor = new Color(0.95f, 0.95f, 0.9f);
        [SerializeField] Color hitColor = new Color(1f, 0.55f, 0.15f);

        // ---- Runtime state ----

        // Shader property ID of the URP Lit colour, looked up once instead of by string every time.
        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        readonly List<VanishAfterHit> _dominoes = new List<VanishAfterHit>();         // the pool: all 100 tiles
        readonly HashSet<VanishAfterHit> _collided = new HashSet<VanishAfterHit>();   // tiles that touched another tile (a set, so each counts once)
        readonly List<VanishAfterHit> _waitingForSpot = new List<VanishAfterHit>();   // vanished tiles whose home spot is still blocked
        Vector3[] _homePositions;      // where tile i stands in its line (index = line * dominoesPerLine + k)
        Quaternion[] _homeRotations;   // which way tile i faces in its line
        MaterialPropertyBlock _colorBlock;
        Vector3 _halfExtents;          // half the tile size, used by the overlap check

        // Read by ArenaHud to show the numbers on screen.
        public IReadOnlyList<VanishAfterHit> Dominoes => _dominoes;
        /// <summary>Tiles that touched another tile since they were last placed.</summary>
        public int CollidedCount => _collided.Count;
        public int RespawnCount { get; private set; }
        public float VanishDelay => vanishDelay;

        // Setting this (V key) also updates every tile, so toggling works in the middle of the demo.
        public bool VanishEnabled
        {
            get => vanishEnabled;
            set
            {
                vanishEnabled = value;
                foreach (var domino in _dominoes)
                    domino.VanishEnabled = value;
            }
        }

        void Start()
        {
            _colorBlock = new MaterialPropertyBlock();

            // Tile size comes from the prefab's collider (0.5 x 1 x 0.15 m), so changing the prefab just works.
            var box = dominoPrefab.GetComponent<BoxCollider>();
            _halfExtents = Vector3.Scale(box.size, dominoPrefab.transform.localScale) * 0.5f;
            _homePositions = new Vector3[lineCount * dominoesPerLine];
            _homeRotations = new Quaternion[lineCount * dominoesPerLine];

            // The loop that creates the >= 100 obstacles. They are created once and reused forever
            // (object pooling): vanishing only deactivates a tile, respawning reactivates the same one.
            for (var i = 0; i < lineCount * dominoesPerLine; i++)
            {
                var domino = Instantiate(dominoPrefab, transform);
                domino.name = $"Domino_{i:000}";

                // Subscribe to the tile's events (observer pattern): the tile doesn't need to know about
                // the arena; it just announces what happened, and the arena reacts.
                domino.Hit += OnHit;
                domino.BodyContact += OnBodyContact;
                domino.Vanished += OnVanished;
                _dominoes.Add(domino);
            }

            BuildLines();
        }

        void Update()
        {
            // Demo keys (Input System): R = stand all lines up again, V = turn vanishing on/off.
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.rKey.wasPressedThisFrame)
                    BuildLines();
                if (keyboard.vKey.wasPressedThisFrame)
                    VanishEnabled = !VanishEnabled;
            }

            // Tiles whose home spot was blocked (e.g. a fallen neighbour lies on it) wait here; retry each frame.
            // Loop backwards so RemoveAt(i) doesn't skip the next element.
            for (var i = _waitingForSpot.Count - 1; i >= 0; i--)
                if (TryRespawn(_waitingForSpot[i]))
                    _waitingForSpot.RemoveAt(i);
        }

        /// <summary>Puts every tile back into its line and remembers that spot as its home.
        /// Line i points at angle i * 360 / lineCount.</summary>
        public void BuildLines()
        {
            _waitingForSpot.Clear();
            _collided.Clear();

            // Polar coordinates: each line has an angle (10 lines -> every 36 degrees),
            // each tile in the line has a radius (first radius + k * spacing).
            for (var line = 0; line < lineCount; line++)
            {
                // Rotating "forward" (0, 0, 1) by the line's angle gives the line's direction on the ground.
                // The same rotation makes the thin side of the tile face along the line, so it falls onto the next one.
                var rotation = Quaternion.Euler(0f, line * 360f / lineCount, 0f);
                var direction = rotation * Vector3.forward;
                for (var k = 0; k < dominoesPerLine; k++)
                {
                    // centre + direction * radius, lifted by half the height so the tile stands ON the ground.
                    var position = transform.position
                                   + direction * (firstDominoRadius + k * spacing)
                                   + Vector3.up * _halfExtents.y;

                    // 2D (line, k) -> 1D list index.
                    var index = line * dominoesPerLine + k;
                    _homePositions[index] = position;
                    _homeRotations[index] = rotation;
                    Place(_dominoes[index], position, rotation);
                }
            }

            // Tell the physics engine about all the moved tiles right away (see the note in TryRespawn).
            Physics.SyncTransforms();
        }

        /// <summary>Puts a vanished tile back. Returns false if its spot is blocked (then it retries later).</summary>
        bool TryRespawn(VanishAfterHit domino)
        {
            // The box we test is the tile's size plus a little padding on the sides, so tiles never spawn touching.
            var checkExtents = _halfExtents + new Vector3(respawnPadding, 0f, respawnPadding);

            // Respawn at the tile's home spot in its line, but only when nothing overlaps it.
            // Physics.CheckBox asks the physics engine: "would a box here touch any collider?"
            // (the hamster, a fallen tile, a wall...). Triggers are ignored.
            // Box centre slightly above the ground so the ground itself doesn't count as overlap.
            var index = _dominoes.IndexOf(domino);
            var position = _homePositions[index];
            var rotation = _homeRotations[index];
            if (Physics.CheckBox(position + Vector3.up * 0.05f, checkExtents, rotation,
                    Physics.AllLayers, QueryTriggerInteraction.Ignore))
                return false;

            // Random respawn (the spec's extra wording), kept for reference:
            // var rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            // var areaCenter = transform.position + Vector3.up * (_halfExtents.y + 0.05f);
            // if (!NonOverlapPlacement.TryFindSpot(areaCenter, respawnArea, checkExtents, rotation,
            //         Physics.AllLayers, maxTries, out var position))
            //     return false;

            Place(domino, position, rotation);
            // Physics queries only see moved objects after a sync; without it the next respawn in
            // this same frame could land on top of this tile.
            Physics.SyncTransforms();
            RespawnCount++;
            return true;
        }

        /// <summary>Moves a tile to a spot and makes it a fresh, standing, white tile again.</summary>
        void Place(VanishAfterHit domino, Vector3 position, Quaternion rotation)
        {
            domino.transform.SetPositionAndRotation(position, rotation);
            domino.ResetHit();                      // not hit any more, timer restarts
            domino.Delay = vanishDelay;
            domino.VanishEnabled = vanishEnabled;
            _collided.Remove(domino);
            SetColor(domino, idleColor);

            domino.gameObject.SetActive(true);
            // Clear any speed left over from falling, otherwise it would keep moving after respawn.
            domino.Body.linearVelocity = Vector3.zero;
            domino.Body.angularVelocity = Vector3.zero;
        }

        // ---- Event handlers (called by each tile's VanishAfterHit) ----

        // First hit (pushed by the hamster or hit by another tile): show it in orange.
        void OnHit(VanishAfterHit domino) => SetColor(domino, hitColor);

        // Touched another tile: remember it for the HUD's "collided with another obstacle" count.
        void OnBodyContact(VanishAfterHit domino) => _collided.Add(domino);

        // The tile has just deactivated itself: put it back, or queue it if its spot is blocked.
        void OnVanished(VanishAfterHit domino)
        {
            if (!TryRespawn(domino))
                _waitingForSpot.Add(domino);
        }

        /// <summary>
        /// Changes one tile's colour without creating a new material. A MaterialPropertyBlock overrides
        /// the colour per renderer, so all 100 tiles still share one material (cheap to render).
        /// </summary>
        void SetColor(VanishAfterHit domino, Color color)
        {
            var meshRenderer = domino.GetComponentInChildren<Renderer>(true); // true: also while the tile is inactive
            if (meshRenderer == null)
                return;
            _colorBlock.SetColor(BaseColorId, color);
            meshRenderer.SetPropertyBlock(_colorBlock);
        }
    }
}
