using System;
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
    /// Animation: if the model has an Animator (#36), the Play* methods set its triggers ("Swing" with the int
    /// "Tool" = ClubProfile.Swing, "Equip", "Celebrate", "Fail") and the bool "Walking"; every frame it also sets
    /// the float "Speed" (m/s, measured, so walking in explore mode and WalkTo both animate) and the bool "Aiming"
    /// (FaceYaw was called in the last moment = standing at the ball, aiming). Without an Animator, a tiny code
    /// fallback moves the "ShoulderR" pivot / the Visual, so a shot still reads. The swing clips call
    /// OnSwingImpact (SwingImpactRelay on the model), which raises SwingImpact on the hit frame.
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
        [Tooltip("WalkTo speed (m/s): all goals met, the hamster walks to the open exit by itself.")]
        [SerializeField] float walkSpeed = 1.5f;

        GameObject _tool;            // the instantiated ToolPrefab in the hand
        Animator _animator;          // from the Blender model, once it exists (#36)
        Transform _visual, _shoulder;
        Coroutine _fallback;         // the running code animation, if any
        Coroutine _walk;             // the running WalkTo, if any
        Vector3 _ballPosition;       // from the last TeleportTo: FaceYaw walks around this point
        bool _hasBall;
        Vector3 _lastPosition;       // for the measured "Speed"
        float _speed;
        float _lastFaceYawTime = float.NegativeInfinity;

        const float AimingHoldSeconds = 0.25f;   // "Aiming" stays on this long after the last FaceYaw
        const float TeleportSpeed = 8f;          // faster than this in one frame = a teleport, not walking

        public ClubProfile CurrentTool { get; private set; }
        public bool IsWalking => _walk != null;

        /// <summary>The swing clip's hit frame (animation event, #36). Nothing raises it without the model.</summary>
        public event Action SwingImpact;

        void Awake()
        {
            _visual = transform.Find("Visual");
            _animator = GetComponentInChildren<Animator>();
            if (toolSocket == null)
                toolSocket = FindDeep(transform, "ToolSocket");
            _shoulder = FindDeep(transform, "ShoulderR");
            _lastPosition = transform.position;
        }

        void Update()
        {
            if (!HasController)
                return;
            var delta = transform.position - _lastPosition;
            delta.y = 0f;
            _lastPosition = transform.position;
            var measured = Time.deltaTime > 0f ? delta.magnitude / Time.deltaTime : 0f;
            if (measured > TeleportSpeed)
                measured = 0f;
            _speed = Mathf.Lerp(_speed, measured, 1f - Mathf.Exp(-12f * Time.deltaTime));   // smooth the frame jitter
            _animator.SetFloat("Speed", _speed);
            _animator.SetBool("Aiming", Time.time - _lastFaceYawTime < AimingHoldSeconds);
        }

        /// <summary>Stands beside the ball (next shooting position) and faces the aim.</summary>
        public void TeleportTo(Vector3 ballPosition, float aimYaw)
        {
            StopWalking();
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
            _lastFaceYawTime = Time.time;
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
            var switched = CurrentTool != null && club != null && club != CurrentTool;
            CurrentTool = club;
            if (switched)
                Trigger("Equip");
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
            if (HasController)
                _animator.SetInteger("Tool", club != null ? (int)club.Swing : 0);
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

        /// <summary>
        /// Walks straight to the target on the ground (all goals met: to the open exit, #38), facing the way it
        /// goes, then calls arrived. Another WalkTo or a TeleportTo stops the walk without calling arrived.
        /// </summary>
        public void WalkTo(Vector3 target, Action arrived = null)
        {
            StopWalking();
            if (_fallback != null)
                StopCoroutine(_fallback);
            _fallback = null;
            ResetPose();
            _hasBall = false;   // no longer standing at the ball
            _walk = StartCoroutine(Walk(target, arrived));
        }

        IEnumerator Walk(Vector3 target, Action arrived)
        {
            SetWalking(true);
            var useFallback = !HasController;
            for (var t = 0f; ; t += Time.deltaTime)
            {
                var toTarget = target - transform.position;
                toTarget.y = 0f;
                var step = walkSpeed * Time.deltaTime;
                if (toTarget.magnitude <= step)
                    break;
                var direction = toTarget.normalized;
                transform.SetPositionAndRotation(SnapToGround(transform.position + direction * step),
                    Quaternion.LookRotation(direction));
                // Code fallback: a small bob, two steps per second.
                if (useFallback && _visual != null)
                    _visual.localPosition = Vector3.up * (0.05f * Mathf.Abs(Mathf.Sin(t * 2f * Mathf.PI)));
                yield return null;
            }
            transform.position = SnapToGround(new Vector3(target.x, transform.position.y, target.z));
            _walk = null;   // done (not StopCoroutine: this is the running coroutine)
            SetWalking(false);
            ResetPose();
            arrived?.Invoke();
        }

        void StopWalking()
        {
            if (_walk == null)
                return;
            StopCoroutine(_walk);
            _walk = null;
            SetWalking(false);
            ResetPose();
        }

        void SetWalking(bool walking)
        {
            if (HasController)
                _animator.SetBool("Walking", walking);
        }

        // ---- Animator path (#36) ----

        bool HasController => _animator != null && _animator.runtimeAnimatorController != null;

        /// <summary>Called by SwingImpactRelay (the animation event lives on the model, not on this object).</summary>
        internal void RaiseSwingImpact() => SwingImpact?.Invoke();

        bool Trigger(string name)
        {
            if (!HasController)
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
