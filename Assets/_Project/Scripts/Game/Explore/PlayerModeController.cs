using System;
using System.Collections;
using Project3DGP.Core.Movement;
using Project3DGP.Game.Cameras;
using Project3DGP.Game.Controls;
using Project3DGP.Game.Flow;
using Project3DGP.Game.Golf;
using Project3DGP.Game.Player;
using UnityEngine;

namespace Project3DGP.Game.Explore
{
    /// <summary>What the player is doing in explore mode.</summary>
    public enum PlayerMode { Walking, Shooting, BallInFlight, Finished }

    /// <summary>
    /// Explore mode (#19): the player walks to the ball instead of teleporting to it. A small state machine:
    ///   Walking      WASD walks, Shift sprints, Space jumps (third / first person view); V cycles Third person -> First person -> Free fly -> Ball view.
    ///                Close enough to the resting ball -> Shooting.
    ///   Shooting     Henry's shot sequence as before: the avatar stands in its stance at the ball (GolferAvatar.TeleportTo),
    ///                aim view, aim cone and power meter visible. Q -> back to Walking. Left click -> BallInFlight.
    ///   BallInFlight the player stands still, the camera follows the ball. Ball stops -> Walking (third person).
    ///   Finished     all goals met: the exit opened and the avatar walks there by itself (GolferAvatar.WalkTo).
    /// Shooting is allowed only in Shooting mode (ShotController.InputEnabled). After Q you must first walk a bit
    /// away from the ball (shootRadius + rearmMargin), else you would be pulled straight back into the shot.
    /// The same happens at the level start, so you begin by walking.
    /// </summary>
    public class PlayerModeController : MonoBehaviour
    {
        [SerializeField] GolfInput input;
        [SerializeField] ShotController shot;
        [SerializeField] GolfBall ball;
        [SerializeField] GolferAvatar avatar;
        [SerializeField] CharacterWalker walker;
        [SerializeField] CameraRig cameraRig;
        [Tooltip("Optional: the level's exit. When it opens, the player stops (Finished).")]
        [SerializeField] LevelExit exit;
        [Tooltip("Objects shown only while shooting (aim cone, power meter).")]
        [SerializeField] GameObject[] shootingOnly = Array.Empty<GameObject>();
        [Tooltip("Walk this close to the resting ball (m, on the ground) to start the shot.")]
        [SerializeField] float shootRadius = 1.2f;
        [Tooltip("After Q (or at the start), walk this much further than shootRadius before the ball can catch you again (m).")]
        [SerializeField] float rearmMargin = 0.5f;

        // The views V cycles through while walking. Only the first two let the player walk.
        static readonly CameraView[] ExploreViews =
            { CameraView.ThirdPerson, CameraView.FirstPerson, CameraView.FreeFly, CameraView.BallView };

        int _exploreView;      // index into ExploreViews
        bool _armed;           // may getting close to the ball start the shot?
        ILevelFlow _flow;

        public PlayerMode Mode { get; private set; } = PlayerMode.Walking;
        /// <summary>True while the player is actually moving (for the walk animation later).</summary>
        public bool IsWalking => Mode == PlayerMode.Walking && walker.IsWalking;

        public event Action<PlayerMode> ModeChanged;

        void OnEnable()
        {
            input.Acquire();
            input.ExitShot += OnExitShot;
            input.ToggleView += OnToggleView;
            input.Jump += OnJump;
            shot.ShotTaken += OnShotTaken;
            ball.CameToRest += OnBallCameToRest;
            if (exit != null)
                exit.Opened += OnExitOpened;
            cameraRig.FollowShotsAutomatically = false;   // this script decides the views
            cameraRig.Player = avatar.transform;
        }

        void OnDisable()
        {
            input.ExitShot -= OnExitShot;
            input.ToggleView -= OnToggleView;
            input.Jump -= OnJump;
            input.Release();
            shot.ShotTaken -= OnShotTaken;
            ball.CameToRest -= OnBallCameToRest;
            if (exit != null)
                exit.Opened -= OnExitOpened;
            if (_flow != null)
                _flow.PausedChanged -= OnPausedChanged;
        }

        // A coroutine: wait one frame, so the other Start methods (e.g. the avatar placed at the tee) have run.
        IEnumerator Start()
        {
            walker.enabled = false;
            shot.InputEnabled = false;
            SetShootingObjectsVisible(false);
            _flow = LevelFlowFinder.Find(null);
            if (_flow != null)
                _flow.PausedChanged += OnPausedChanged;
            yield return null;
            _armed = DistanceToBall() > shootRadius;
            EnterWalking();
        }

        void Update()
        {
            if (Mode != PlayerMode.Walking)
                return;

            // Walk only in the third / first person views; free fly and ball view move the camera instead.
            var view = cameraRig.View;
            var canWalk = view == CameraView.ThirdPerson || view == CameraView.FirstPerson;
            walker.MoveInput = canWalk ? input.Move : Vector2.zero;
            walker.Sprint = canWalk && input.IsSprinting;
            walker.ReferenceYaw = cameraRig.AimYaw;   // W walks away from the camera

            var distance = DistanceToBall();
            if (!_armed && distance > shootRadius + rearmMargin)
                _armed = true;
            if (_armed && distance <= shootRadius && ball.IsAtRest && shot.Phase == ShotPhase.Aiming && Time.timeScale > 0f)
                EnterShooting();
        }

        void EnterWalking()
        {
            SetShootingObjectsVisible(false);
            shot.InputEnabled = false;
            Physics.SyncTransforms();   // the avatar may have been placed by TeleportTo; let the controller start there
            walker.enabled = true;
            cameraRig.SetView(ExploreViews[_exploreView]);
            SetMode(PlayerMode.Walking);
        }

        void EnterShooting()
        {
            walker.enabled = false;   // also turns the CharacterController off: Henry's stance code places the avatar now
            // Face the ball: the yaw of the line from the player to the ball (0 = +Z, 90 = +X).
            var toBall = ball.Position - avatar.transform.position;
            var yaw = Mathf.Atan2(toBall.x, toBall.z) * Mathf.Rad2Deg;
            cameraRig.SetAimYaw(yaw);
            avatar.TeleportTo(ball.Position, yaw);   // a short snap into the shooting stance beside the ball
            SetShootingObjectsVisible(true);
            shot.InputEnabled = true;
            cameraRig.SetView(CameraView.Aim);
            SetMode(PlayerMode.Shooting);
        }

        void OnExitShot()
        {
            if (Mode != PlayerMode.Shooting)
                return;
            _armed = false;   // walk away first, else we'd jump straight back into the shot
            EnterWalking();
        }

        // Space: jump, but only while walking in a view where the player moves (in free fly Space flies up).
        void OnJump()
        {
            var view = cameraRig.View;
            if (Mode == PlayerMode.Walking && (view == CameraView.ThirdPerson || view == CameraView.FirstPerson))
                walker.Jump();
        }

        void OnToggleView()
        {
            if (Mode != PlayerMode.Walking)
                return;   // shooting is always the aim view
            _exploreView = (_exploreView + 1) % ExploreViews.Length;
            cameraRig.SetView(ExploreViews[_exploreView]);
        }

        void OnShotTaken(ShotInfo _)
        {
            if (Mode != PlayerMode.Shooting)
                return;
            SetShootingObjectsVisible(false);
            shot.InputEnabled = false;
            cameraRig.SetView(CameraView.Follow);
            SetMode(PlayerMode.BallInFlight);
        }

        void OnBallCameToRest()
        {
            if (Mode != PlayerMode.BallInFlight)
                return;
            _armed = true;      // a short shot that stops next to you starts the next shot right away
            _exploreView = 0;   // back to the player, third person
            EnterWalking();
        }

        void OnExitOpened()
        {
            walker.enabled = false;
            shot.InputEnabled = false;
            SetShootingObjectsVisible(false);
            _exploreView = 0;
            cameraRig.SetView(CameraView.ThirdPerson);   // watch the avatar walk to the exit
            SetMode(PlayerMode.Finished);
        }

        // Closing the pause menu turns shot input back on; only keep it on while shooting.
        void OnPausedChanged(bool paused)
        {
            if (!paused && Mode != PlayerMode.Shooting)
                shot.InputEnabled = false;
        }

        float DistanceToBall()
        {
            var offset = ball.Position - avatar.transform.position;
            offset.y = 0f;   // on the ground only: a ball on a small step is still "close"
            return offset.magnitude;
        }

        void SetShootingObjectsVisible(bool visible)
        {
            foreach (var go in shootingOnly)
                if (go != null)
                    go.SetActive(visible);
        }

        void SetMode(PlayerMode mode)
        {
            Mode = mode;
            ModeChanged?.Invoke(mode);
        }
    }
}
