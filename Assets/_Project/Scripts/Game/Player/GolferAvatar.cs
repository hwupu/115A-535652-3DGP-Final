using System.Collections;
using Project3DGP.Game.Golf;
using UnityEngine;

namespace Project3DGP.Game.Player
{
    /// <summary>
    /// The hamster golfer (#88, architecture §4.3). Game code only calls the methods below; who calls them is
    /// LevelManager (#27): CameToRest -> TeleportTo, ShotTaken -> PlaySwing, ClubChanged -> SetTool, and FaceYaw
    /// every frame while aiming. The look lives in the "Visual" child (placeholder hamster now, Blender model
    /// later), so swapping the model changes no script.
    ///
    /// Animation: if the model has an Animator (#36), the Play* methods set its triggers ("Swing", "Celebrate",
    /// "Fail"). Without one, a tiny code fallback moves the "ShoulderR" pivot / the Visual, so a shot still reads.
    /// </summary>
    public class GolferAvatar : MonoBehaviour
    {
        [Tooltip("Where the held tool goes. Empty = found by name \"ToolSocket\" under the Visual.")]
        [SerializeField] Transform toolSocket;
        [Tooltip("Where to stand, relative to the ball, in the aim's frame: x = right, z = forward (metres). " +
                 "Default: a bit left of and behind the ball, so the right hand's tool reaches it.")]
        [SerializeField] Vector3 standOffset = new(-0.45f, 0f, -0.75f);
        [Tooltip("Layers the feet can stand on (ground snap after a teleport).")]
        [SerializeField] LayerMask groundLayers = ~0;

        GameObject _tool;            // the instantiated ToolPrefab in the hand
        Animator _animator;          // from the Blender model, once it exists (#36)
        Transform _visual, _shoulder;
        Coroutine _fallback;         // the running code animation, if any
        Vector3 _ballPosition;       // from the last TeleportTo: FaceYaw walks around this point
        bool _hasBall;

        public ClubProfile CurrentTool { get; private set; }

        void Awake()
        {
            _visual = transform.Find("Visual");
            _animator = GetComponentInChildren<Animator>();
            if (toolSocket == null)
                toolSocket = FindDeep(transform, "ToolSocket");
            _shoulder = FindDeep(transform, "ShoulderR");
        }

        /// <summary>Stands beside the ball (next shooting position) and faces the aim.</summary>
        public void TeleportTo(Vector3 ballPosition, float aimYaw)
        {
            _ballPosition = ballPosition;
            _hasBall = true;
            FaceYaw(aimYaw);
        }

        /// <summary>
        /// Faces the aim (called every frame while aiming). The hamster circles the ball with the camera,
        /// keeping the same stance, so the ball stays at its right hand.
        /// </summary>
        public void FaceYaw(float aimYaw)
        {
            var facing = Quaternion.Euler(0f, aimYaw, 0f);
            if (!_hasBall)
            {
                transform.rotation = facing;
                return;
            }
            // standOffset is in the aim's frame: turn it by the yaw, then add it to the ball's position.
            var position = _ballPosition + facing * standOffset;
            transform.SetPositionAndRotation(SnapToGround(position), facing);
        }

        /// <summary>Puts this club's ToolPrefab in the hand (replacing the old one). Null = empty hand.</summary>
        public void SetTool(ClubProfile club)
        {
            if (club == CurrentTool && (club == null || _tool != null))
                return;
            CurrentTool = club;
            if (_tool != null)
                Destroy(_tool);
            _tool = null;
            if (club == null || club.ToolPrefab == null || toolSocket == null)
                return;
            // Instantiate as a child of the socket: the prefab's grip (its origin) lands exactly in the hand.
            _tool = Instantiate(club.ToolPrefab, toolSocket, false);
            _tool.name = club.ToolPrefab.name;
        }

        public void PlaySwing(ClubProfile club)
        {
            if (!Trigger("Swing"))
                Play(SwingFallback());
        }

        public void PlayCelebrate()
        {
            if (!Trigger("Celebrate"))
                Play(HopFallback());
        }

        public void PlayFail()
        {
            if (!Trigger("Fail"))
                Play(LeanBackFallback());
        }

        // ---- Animator path (#36) ----

        bool Trigger(string name)
        {
            if (_animator == null || _animator.runtimeAnimatorController == null)
                return false;
            _animator.SetTrigger(name);
            return true;
        }

        // ---- Code fallback until the Blender clips exist ----

        void Play(IEnumerator routine)
        {
            if (_fallback != null)
                StopCoroutine(_fallback);
            ResetPose();
            _fallback = StartCoroutine(routine);
        }

        void ResetPose()
        {
            if (_shoulder != null)
                _shoulder.localRotation = Quaternion.identity;
            if (_visual != null)
                _visual.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }

        // Shoulder back (slow), through (fast), then back to rest: same idea as mini-exercise 4.A.1, about the
        // shoulder's local Z axis.
        IEnumerator SwingFallback()
        {
            if (_shoulder == null)
                yield break;
            yield return TurnShoulder(0f, 100f, 0.25f);
            yield return TurnShoulder(100f, -90f, 0.12f);
            yield return new WaitForSeconds(0.3f);
            yield return TurnShoulder(-90f, 0f, 0.3f);
        }

        IEnumerator TurnShoulder(float from, float to, float seconds)
        {
            for (var t = 0f; t < seconds; t += Time.deltaTime)
            {
                _shoulder.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(from, to, t / seconds));
                yield return null;
            }
            _shoulder.localRotation = Quaternion.Euler(0f, 0f, to);
        }

        // Two little hops: height = sin over each hop (0 -> up -> 0).
        IEnumerator HopFallback()
        {
            if (_visual == null)
                yield break;
            for (var t = 0f; t < 1f; t += Time.deltaTime)
            {
                _visual.localPosition = Vector3.up * (0.3f * Mathf.Abs(Mathf.Sin(t * 2f * Mathf.PI)));
                yield return null;
            }
            _visual.localPosition = Vector3.zero;
        }

        // Scared: lean back and stay leaning (Retry rebuilds the scene).
        IEnumerator LeanBackFallback()
        {
            if (_visual == null)
                yield break;
            for (var t = 0f; t < 0.4f; t += Time.deltaTime)
            {
                _visual.localRotation = Quaternion.Euler(Mathf.Lerp(0f, -20f, t / 0.4f), 0f, 0f);
                yield return null;
            }
        }

        // Feet on the ground: cast down from above the stand point; keep the height if nothing is below.
        Vector3 SnapToGround(Vector3 position)
        {
            var from = position + Vector3.up * 2f;
            return Physics.Raycast(from, Vector3.down, out var hit, 10f, groundLayers, QueryTriggerInteraction.Ignore)
                ? hit.point
                : position;
        }

        static Transform FindDeep(Transform parent, string name)
        {
            foreach (var child in parent.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == name)
                    return child;
            }
            return null;
        }
    }
}
