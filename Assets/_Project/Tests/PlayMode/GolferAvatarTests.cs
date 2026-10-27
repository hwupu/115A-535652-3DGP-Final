using System.Collections;
using NUnit.Framework;
using Project3DGP.Game.Golf;
using Project3DGP.Game.Player;
using UnityEngine;
using UnityEngine.TestTools;

namespace Project3DGP.Game.Tests
{
    /// <summary>GolferAvatar (#88): stands beside the ball on the ground, faces the aim, holds the right tool.</summary>
    public class GolferAvatarTests
    {
        GameObject _ground, _root, _toolA, _toolB;
        GolferAvatar _avatar;
        Transform _socket;
        ClubProfile _clubA, _clubB;

        [SetUp]
        public void SetUp()
        {
            _ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            // A minimal avatar like the prefab: root > Visual > ... > ToolSocket (found by name in Awake).
            _root = new GameObject("Avatar");
            var visual = new GameObject("Visual").transform;
            visual.SetParent(_root.transform, false);
            _socket = new GameObject("ToolSocket").transform;
            _socket.SetParent(visual, false);
            _socket.localPosition = new Vector3(0.2f, 0.8f, 0.5f);
            _avatar = _root.AddComponent<GolferAvatar>();

            _toolA = new GameObject("ToolA");
            _toolB = new GameObject("ToolB");
            _clubA = ScriptableObject.CreateInstance<ClubProfile>();
            _clubA.ToolPrefab = _toolA;
            _clubB = ScriptableObject.CreateInstance<ClubProfile>();
            _clubB.ToolPrefab = _toolB;
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var go in new[] { _ground, _root, _toolA, _toolB })
                Object.Destroy(go);
            Object.Destroy(_clubA);
            Object.Destroy(_clubB);
        }

        [UnityTest]
        public IEnumerator TeleportTo_StandsBesideTheBall_OnTheGround_FacingTheAim()
        {
            yield return new WaitForFixedUpdate();   // the ground collider is in the physics scene
            var ball = new Vector3(2f, 0.1f, 3f);
            _avatar.TeleportTo(ball, 90f);

            var position = _root.transform.position;
            Assert.That(position.y, Is.EqualTo(0f).Within(0.01f), "feet on the ground plane");
            var flat = new Vector3(position.x - ball.x, 0f, position.z - ball.z);
            Assert.That(flat.magnitude, Is.InRange(0.5f, 1.5f), "close to the ball, not on it");
            Assert.That(_root.transform.eulerAngles.y, Is.EqualTo(90f).Within(0.01f), "faces the aim");
            // In the aim's frame the ball is in front of the avatar (positive forward component).
            Assert.That(Vector3.Dot(ball - position, _root.transform.forward), Is.GreaterThan(0f));
        }

        [UnityTest]
        public IEnumerator FaceYaw_CirclesTheBall_KeepingTheStance()
        {
            yield return new WaitForFixedUpdate();
            var ball = new Vector3(0f, 0.1f, 0f);
            _avatar.TeleportTo(ball, 0f);
            var before = _root.transform.InverseTransformPoint(ball);   // ball as seen from the avatar

            _avatar.FaceYaw(135f);

            var after = _root.transform.InverseTransformPoint(ball);
            Assert.That(Vector3.Distance(new Vector3(before.x, 0f, before.z), new Vector3(after.x, 0f, after.z)),
                Is.LessThan(0.01f), "the ball stays at the same spot relative to the hamster");
            Assert.That(_root.transform.eulerAngles.y, Is.EqualTo(135f).Within(0.01f));
        }

        [UnityTest]
        public IEnumerator SetTool_PutsTheToolInTheSocket_AndReplacesTheOldOne()
        {
            yield return null;
            _avatar.SetTool(_clubA);
            Assert.That(_socket.childCount, Is.EqualTo(1));
            Assert.That(_socket.GetChild(0).name, Is.EqualTo("ToolA"));
            Assert.That(_socket.GetChild(0).localPosition, Is.EqualTo(Vector3.zero), "grip in the hand");

            _avatar.SetTool(_clubB);
            yield return null;   // Destroy happens at the end of the frame
            Assert.That(_socket.childCount, Is.EqualTo(1));
            Assert.That(_socket.GetChild(0).name, Is.EqualTo("ToolB"));
            Assert.That(_avatar.CurrentTool, Is.SameAs(_clubB));

            _avatar.SetTool(null);
            yield return null;
            Assert.That(_socket.childCount, Is.Zero, "empty hand");
        }

        [UnityTest]
        public IEnumerator PlayMethods_WithoutAnimator_DoNotThrow()
        {
            yield return null;
            _avatar.PlaySwing(_clubA);
            _avatar.PlayCelebrate();
            _avatar.PlayFail();
            yield return new WaitForSeconds(1.2f);
            Assert.That(_root.transform.Find("Visual").localPosition.y, Is.GreaterThanOrEqualTo(0f), "code fallback ran without an Animator");
        }
    }
}
