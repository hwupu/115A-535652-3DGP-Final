using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Project3DGP.Core.Interaction;
using Project3DGP.Core.Movement;
using Project3DGP.Core.Spawning;
using Project3DGP.MiniEx.Pongpat.ObjectInteraction;
using UnityEngine;
using UnityEngine.TestTools;

namespace Project3DGP.MiniEx.Pongpat.Tests
{
    /// <summary>
    /// Tests create temporary physics objects only. Exercise scene assets remain Editor-authored.
    /// Collision counts come from actual PhysX contacts, never simulated event calls.
    /// </summary>
    public class PongpatObjectInteractionTests
    {
        static readonly Vector3 Origin = new Vector3(1000f, 0f, 1000f);
        readonly List<GameObject> _objects = new List<GameObject>();
        float _originalTimeScale;
        float _originalFixedDeltaTime;
        Random.State _originalRandomState;

        [SetUp]
        public void SetUp()
        {
            _originalTimeScale = Time.timeScale;
            _originalFixedDeltaTime = Time.fixedDeltaTime;
            _originalRandomState = Random.state;
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
            Random.InitState(6644);
        }

        [TearDown]
        public void TearDown()
        {
            for (var i = _objects.Count - 1; i >= 0; i--)
                if (_objects[i] != null)
                    Object.DestroyImmediate(_objects[i]);
            _objects.Clear();
            Physics.SyncTransforms();
            Time.timeScale = _originalTimeScale;
            Time.fixedDeltaTime = _originalFixedDeltaTime;
            Random.state = _originalRandomState;
        }

        GameObject Box(string name, Vector3 localPosition, Vector3 size)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _objects.Add(go);
            go.name = name;
            go.transform.position = Origin + localPosition;
            go.transform.localScale = size;
            return go;
        }

        void Ground()
        {
            Box("Test floor", new Vector3(0f, -0.5f, 0f), new Vector3(30f, 1f, 30f));
        }

        ImpactRecycleBody Obstacle(Vector3 position)
        {
            var go = Box("Test obstacle", position, Vector3.one * 0.9f);
            var body = go.AddComponent<Rigidbody>();
            body.mass = 1f;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            return go.AddComponent<ImpactRecycleBody>();
        }

        static IEnumerator FixedSeconds(float seconds)
        {
            var steps = Mathf.CeilToInt(seconds / Time.fixedDeltaTime);
            for (var step = 0; step < steps; step++)
                yield return new WaitForFixedUpdate();
        }

        static void ConfigureDemo(ObjectInteractionDemo demo, string fieldName, object value)
        {
            // Configure the same serialized fields as an Editor-authored scene, before Awake runs.
            var field = typeof(ObjectInteractionDemo).GetField(fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, "Missing serialized demo field: " + fieldName);
            field.SetValue(demo, value);
        }

        [UnityTest]
        public IEnumerator PhysicalCharacterPushesPack_AtLeastTenDistinctObstaclesCollide()
        {
            Ground();
            var pack = new List<ImpactRecycleBody>();
            var packBodies = new HashSet<Rigidbody>();
            var contactParticipants = new HashSet<ImpactRecycleBody>();
            var startingPositions = new List<Vector3>();
            for (var row = 0; row < 5; row++)
            {
                for (var column = 0; column < 4; column++)
                {
                    var obstacle = Obstacle(new Vector3(-1.47f + column * 0.98f, 0.5f,
                        2.5f + row * 0.98f));
                    obstacle.RecyclingEnabled = false;
                    pack.Add(obstacle);
                    packBodies.Add(obstacle.Body);
                    startingPositions.Add(obstacle.transform.position);
                    obstacle.BodyCollided += (sender, other) =>
                    {
                        if (packBodies.Contains(other))
                            contactParticipants.Add(sender);
                    };
                }
            }

            var character = Box("Test character", new Vector3(0f, 1f, 0f),
                new Vector3(1.2f, 1.8f, 1.2f));
            var characterBody = character.AddComponent<Rigidbody>();
            characterBody.mass = 30f;
            characterBody.constraints = RigidbodyConstraints.FreezeRotation;
            characterBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            var motor = character.AddComponent<PlanarBodyMotor>();
            Physics.SyncTransforms();

            // Allow floor contacts to settle before the character starts moving.
            yield return FixedSeconds(0.3f);
            Assert.That(contactParticipants.Count, Is.Zero, "The initial pack must not overlap.");
            var start = characterBody.position;
            motor.MoveInput = Vector2.up;
            motor.Sprint = true;
            yield return FixedSeconds(2f);
            motor.MoveInput = Vector2.zero;

            Assert.That(characterBody.position.z - start.z, Is.GreaterThan(2f),
                "The character must physically move into the pack.");
            Assert.That(contactParticipants.Count, Is.GreaterThanOrEqualTo(10),
                "Count distinct obstacles in actual obstacle-to-obstacle collisions, not floor/player contacts.");
            var moved = 0;
            for (var i = 0; i < pack.Count; i++)
                if (Vector3.Distance(pack[i].Body.position, startingPositions[i]) > 0.25f)
                    moved++;
            Assert.That(moved, Is.GreaterThanOrEqualTo(2), "Character contact must transfer momentum to props.");
        }

        [UnityTest]
        public IEnumerator StaticFloorDoesNotStartHitOrRecycleTimer()
        {
            Ground();
            var obstacle = Obstacle(new Vector3(0f, 1.2f, 0f));
            obstacle.RecyclingEnabled = true;
            obstacle.RecycleDelay = 2f;
            var recycled = 0;
            var bodyContacts = 0;
            obstacle.Recycled += _ => recycled++;
            obstacle.BodyCollided += (_, __) => bodyContacts++;
            Physics.SyncTransforms();

            yield return FixedSeconds(2.5f);

            Assert.That(obstacle.IsHit, Is.False);
            Assert.That(obstacle.gameObject.activeSelf, Is.True);
            Assert.That(recycled, Is.Zero);
            Assert.That(bodyContacts, Is.Zero);
        }

        [UnityTest]
        public IEnumerator DynamicImpactRecyclesAfterTwoSeconds_Once_AndResetClearsState()
        {
            Ground();
            var obstacle = Obstacle(new Vector3(0f, 0.5f, 0f));
            obstacle.RecyclingEnabled = true;
            obstacle.RecycleDelay = 2f;
            var hitter = Obstacle(new Vector3(0f, 0.5f, -2f));
            var recycled = 0;
            var recycledAt = -1f;
            obstacle.Recycled += _ =>
            {
                recycled++;
                recycledAt = Time.fixedTime;
            };
            Physics.SyncTransforms();
            yield return FixedSeconds(0.2f);
            hitter.Body.linearVelocity = Vector3.forward * 7f;

            // Fail within one simulated second if no actual impact takes place.
            var remainingSteps = Mathf.CeilToInt(1f / Time.fixedDeltaTime);
            while (!obstacle.IsHit && remainingSteps-- > 0)
                yield return new WaitForFixedUpdate();
            Assert.That(obstacle.IsHit, Is.True, "The two dynamic bodies should collide.");
            var hitObservedAt = Time.fixedTime;
            Assert.That(recycled, Is.Zero);

            yield return FixedSeconds(1.7f);
            Assert.That(obstacle.gameObject.activeSelf, Is.True, "It must not vanish before two seconds.");
            yield return FixedSeconds(0.5f);
            Assert.That(obstacle.gameObject.activeSelf, Is.False);
            Assert.That(recycled, Is.EqualTo(1));
            Assert.That(recycledAt - hitObservedAt, Is.InRange(1.9f, 2.1f),
                "Fixed-step observation permits a small timing tolerance around two seconds.");

            yield return FixedSeconds(0.3f);
            Assert.That(recycled, Is.EqualTo(1), "An inactive body must recycle only once.");
            obstacle.ResetImpact();
            Assert.That(obstacle.IsHit, Is.False);
            obstacle.Body.position = Origin + new Vector3(10f, 0.5f, 0f);
            obstacle.Body.linearVelocity = Vector3.zero;
            obstacle.Body.angularVelocity = Vector3.zero;
            obstacle.gameObject.SetActive(true);
            Physics.SyncTransforms();
            yield return FixedSeconds(2.2f);
            Assert.That(obstacle.gameObject.activeSelf, Is.True,
                "A reset and respawn must not retain the previous impact countdown.");
            Assert.That(recycled, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator DemoDisablePausesRecycling_ReenableRespawnsAndResetPreservesPool()
        {
            Ground();
            var target = Obstacle(new Vector3(0f, 0.5f, 0f));
            var bystander = Obstacle(new Vector3(5f, 0.5f, 0f));
            var hitter = Obstacle(new Vector3(0f, 0.5f, -2f));
            var pool = new[] { target, bystander };
            var originalTargetPosition = target.transform.position;
            var originalBystanderPosition = bystander.transform.position;
            var character = Box("Pool test character", new Vector3(-10f, 1f, -10f),
                new Vector3(1.2f, 1.8f, 1.2f));
            var characterBody = character.AddComponent<Rigidbody>();
            characterBody.mass = 30f;
            characterBody.constraints = RigidbodyConstraints.FreezeRotation;
            var motor = character.AddComponent<PlanarBodyMotor>();
            var originalCharacterPosition = characterBody.position;

            var root = new GameObject("Pool test demo");
            _objects.Add(root);
            root.SetActive(false);
            var demo = root.AddComponent<ObjectInteractionDemo>();
            ConfigureDemo(demo, "character", motor);
            ConfigureDemo(demo, "obstacles", pool);
            ConfigureDemo(demo, "respawnCenter", Origin + new Vector3(0f, 0.53f, 0f));
            ConfigureDemo(demo, "respawnSize", new Vector2(24f, 24f));
            ConfigureDemo(demo, "obstacleHalfExtents", Vector3.one * 0.45f);
            ConfigureDemo(demo, "clearance", 0.04f);
            root.SetActive(true);
            demo.SetRecycling(true);
            Physics.SyncTransforms();
            yield return FixedSeconds(0.2f);
            hitter.Body.linearVelocity = Vector3.forward * 7f;

            var impactSteps = Mathf.CeilToInt(1f / Time.fixedDeltaTime);
            while (!target.IsHit && impactSteps-- > 0)
                yield return new WaitForFixedUpdate();
            Assert.That(target.IsHit, Is.True, "A real dynamic-body contact starts the lifecycle.");
            demo.enabled = false;

            // A disabled owner has no Recycled listener. Its pool must pause rather than disappear.
            yield return FixedSeconds(2.4f);
            Assert.That(target.gameObject.activeSelf, Is.True);
            Assert.That(target.RecyclingEnabled, Is.False);
            Assert.That(target.IsHit, Is.True, "Pausing keeps the original impact state.");
            Assert.That(demo.RespawnCount, Is.Zero);

            demo.enabled = true;
            Assert.That(target.RecyclingEnabled, Is.True);
            var respawnSteps = Mathf.CeilToInt(3f / Time.fixedDeltaTime);
            while (demo.RespawnCount == 0 && respawnSteps-- > 0)
                yield return new WaitForFixedUpdate();
            // Let the owner's Update refresh counts after its successful placement.
            yield return null;
            Assert.That(demo.RespawnCount, Is.EqualTo(1));
            Assert.That(demo.ObstacleCount, Is.EqualTo(pool.Length));
            Assert.That(demo.ActiveCount + demo.PendingCount, Is.EqualTo(demo.ObstacleCount));
            Assert.That(demo.ActiveCount, Is.EqualTo(pool.Length));
            Assert.That(demo.PendingCount, Is.Zero);
            Assert.That(target.gameObject.activeSelf, Is.True);
            Assert.That(target.IsHit, Is.False);
            Assert.That(Mathf.Abs(target.Body.position.x - Origin.x), Is.LessThanOrEqualTo(11.51f));
            Assert.That(Mathf.Abs(target.Body.position.z - Origin.z), Is.LessThanOrEqualTo(11.51f),
                "The recycled body's footprint must remain inside its configured spawn area.");

            Physics.SyncTransforms();
            // Slightly shorten the vertical query to exclude the floor after the box has settled.
            var nearby = Physics.OverlapBox(target.Body.position, new Vector3(0.49f, 0.44f, 0.49f),
                Quaternion.identity, Physics.AllLayers, QueryTriggerInteraction.Ignore);
            foreach (var collider in nearby)
                Assert.That(collider.attachedRigidbody, Is.SameAs(target.Body),
                    "The respawn must clear the character and other obstacle colliders.");

            characterBody.position += Vector3.right * 2f;
            demo.ResetDemo();
            Assert.That(demo.ActiveCount, Is.EqualTo(pool.Length));
            Assert.That(demo.PendingCount, Is.Zero);
            Assert.That(demo.ActiveCount + demo.PendingCount, Is.EqualTo(demo.ObstacleCount));
            Assert.That(demo.RespawnCount, Is.Zero);
            Assert.That(demo.PeakCollisionCount, Is.Zero);
            Assert.That(demo.RecentCollisionCount, Is.Zero);
            Assert.That(target.IsHit, Is.False);
            Assert.That(bystander.IsHit, Is.False);
            Assert.That(Vector3.Distance(target.Body.position, originalTargetPosition), Is.LessThan(0.001f));
            Assert.That(Vector3.Distance(bystander.Body.position, originalBystanderPosition), Is.LessThan(0.001f));
            Assert.That(Vector3.Distance(characterBody.position, originalCharacterPosition), Is.LessThan(0.001f));
        }

        [Test]
        public void RandomPlacementRejectsBlockedAreaWithoutReturningOverlappingFallback()
        {
            Box("Solid blocker", new Vector3(0f, 1f, 0f), new Vector3(10f, 4f, 10f));
            Physics.SyncTransforms();

            var found = RandomBoxPlacement.TryFind(Origin + Vector3.up, new Vector2(4f, 4f),
                Vector3.one * 0.45f, 0.05f, 50, out _);

            Assert.That(found, Is.False, "A full area must stay pending rather than use an overlapping fallback.");
        }

        [Test]
        public void RandomPlacementFindsClearPointInsideInsetBounds_AndIgnoresTriggers()
        {
            var center = Origin + Vector3.up;
            var trigger = Box("Trigger filling area", Vector3.up, new Vector3(10f, 4f, 10f));
            trigger.GetComponent<BoxCollider>().isTrigger = true;
            Physics.SyncTransforms();
            var extents = Vector3.one * 0.45f;
            const float clearance = 0.05f;

            var found = RandomBoxPlacement.TryFind(center, new Vector2(4f, 6f), extents,
                clearance, 50, out var position);

            Assert.That(found, Is.True);
            Assert.That(Mathf.Abs(position.x - center.x), Is.LessThanOrEqualTo(1.5f));
            Assert.That(Mathf.Abs(position.z - center.z), Is.LessThanOrEqualTo(2.5f));
            Assert.That(position.y, Is.EqualTo(center.y));
            Assert.That(Physics.CheckBox(position, extents + Vector3.one * clearance,
                Quaternion.identity, Physics.AllLayers, QueryTriggerInteraction.Ignore), Is.False);
        }

        [Test]
        public void SequentialRandomPlacementsRemainClearAfterPhysicsSync()
        {
            var center = Origin + Vector3.up;
            var extents = Vector3.one * 0.45f;
            const float clearance = 0.05f;
            var positions = new List<Vector3>();
            for (var i = 0; i < 30; i++)
            {
                Assert.That(RandomBoxPlacement.TryFind(center, new Vector2(12f, 12f),
                    extents, clearance, 200, out var position), Is.True,
                    "The roomy test area should fit all thirty bodies.");
                Assert.That(Physics.CheckBox(position, extents + Vector3.one * clearance,
                    Quaternion.identity, Physics.AllLayers, QueryTriggerInteraction.Ignore), Is.False,
                    "Every proposed point must clear colliders from earlier placements.");
                foreach (var existing in positions)
                {
                    Assert.That(Mathf.Abs(position.x - existing.x) >= 0.95f ||
                        Mathf.Abs(position.z - existing.z) >= 0.95f, Is.True,
                        "The new box must not overlap or violate clearance from an earlier box.");
                }
                positions.Add(position);
                Box("Placed obstacle " + i, position - Origin, extents * 2f);
                // Required when successive placements happen before the next simulation step.
                Physics.SyncTransforms();
            }
        }
    }
}
