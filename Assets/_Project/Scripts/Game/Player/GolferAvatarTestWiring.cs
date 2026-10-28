using Project3DGP.Game.Flow;
using Project3DGP.Game.Golf;
using UnityEngine;

namespace Project3DGP.Game.Player
{
    /// <summary>
    /// Development helper (#88): does the avatar wiring that LevelManager (#27, Pongpat) does in the game, so the
    /// avatar can be tried in the Shot Test Area. Not part of the game. TEMP(#27): it also stands in for LevelManager
    /// in level scenes as Systems/AvatarWiring_TEMP (Magic > Level > Add Ball, Shot and Avatar); delete that object
    /// once LevelManager does this wiring.
    ///   ball stops -> TeleportTo     shot -> PlaySwing     club change -> SetTool     aiming -> FaceYaw
    ///   exit opened -> WalkTo(exit.WalkTarget) (LevelManager then loads the next level; here it only logs)
    /// Test keys (Input System): C = PlayCelebrate, X = PlayFail, G = open the exit (as if all goals were met).
    /// </summary>
    public class GolferAvatarTestWiring : MonoBehaviour
    {
        [SerializeField] ShotController shot;
        [SerializeField] GolfBall ball;
        [SerializeField] GolferAvatar avatar;
        [Tooltip("Optional: the level's exit (#38).")]
        [SerializeField] LevelExit exit;

        void Start()
        {
            avatar.SetTool(shot.CurrentClub);
            avatar.TeleportTo(ball.Position, shot.AimYaw);
        }

        void OnEnable()
        {
            ball.CameToRest += OnCameToRest;
            shot.ShotTaken += OnShotTaken;
            shot.ClubChanged += avatar.SetTool;
            if (exit != null)
                exit.Opened += OnExitOpened;
        }

        void OnDisable()
        {
            ball.CameToRest -= OnCameToRest;
            shot.ShotTaken -= OnShotTaken;
            shot.ClubChanged -= avatar.SetTool;
            if (exit != null)
                exit.Opened -= OnExitOpened;
        }

        void Update()
        {
            if (shot.Phase == ShotPhase.Aiming && ball.IsAtRest && !avatar.IsWalking && (exit == null || !exit.IsOpen))
                avatar.FaceYaw(shot.AimYaw);   // TEMP(#19): CameraRig.AimYaw

            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard == null)
                return;
            if (keyboard.cKey.wasPressedThisFrame)
                avatar.PlayCelebrate();
            if (keyboard.xKey.wasPressedThisFrame)
                avatar.PlayFail();
            if (keyboard.gKey.wasPressedThisFrame && exit != null)
                exit.Open();
        }

        void OnExitOpened()
        {
            shot.InputEnabled = false;
            avatar.WalkTo(exit.WalkTarget.position, () => Debug.Log("[TEMP(#27)] Hamster reached the exit: LevelManager loads the next level here."));
        }

        void OnCameToRest()
        {
            if (exit == null || !exit.IsOpen)
                avatar.TeleportTo(ball.Position, shot.AimYaw);
        }
        void OnShotTaken(ShotInfo info) => avatar.PlaySwing(info.Club);
    }
}
