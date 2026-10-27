using Project3DGP.Game.Golf;
using UnityEngine;

namespace Project3DGP.Game.Player
{
    /// <summary>
    /// Development helper (#88): does the avatar wiring that LevelManager (#27, Pongpat) does in the game, so the
    /// avatar can be tried in the Shot Test Area. Not part of the game; don't put it in a level scene.
    ///   ball stops -> TeleportTo     shot -> PlaySwing     club change -> SetTool     aiming -> FaceYaw
    /// Test keys (Input System): C = PlayCelebrate, X = PlayFail.
    /// </summary>
    public class GolferAvatarTestWiring : MonoBehaviour
    {
        [SerializeField] ShotController shot;
        [SerializeField] GolfBall ball;
        [SerializeField] GolferAvatar avatar;

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
        }

        void OnDisable()
        {
            ball.CameToRest -= OnCameToRest;
            shot.ShotTaken -= OnShotTaken;
            shot.ClubChanged -= avatar.SetTool;
        }

        void Update()
        {
            if (shot.Phase == ShotPhase.Aiming && ball.IsAtRest)
                avatar.FaceYaw(shot.AimYaw);   // TEMP(#19): CameraRig.AimYaw

            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard == null)
                return;
            if (keyboard.cKey.wasPressedThisFrame)
                avatar.PlayCelebrate();
            if (keyboard.xKey.wasPressedThisFrame)
                avatar.PlayFail();
        }

        void OnCameToRest() => avatar.TeleportTo(ball.Position, shot.AimYaw);
        void OnShotTaken(ShotInfo info) => avatar.PlaySwing(info.Club);
    }
}
