using System;
using Project3DGP.Game.Danger;
using Project3DGP.Game.Flow;
using Project3DGP.Game.Golf;
using Project3DGP.Game.Scoring;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    /// TEMP(#27): also a minimal ILevelFlow, so the HUD menu button, Esc and the pause dialog work in the levels:
    /// pause (timeScale 0, shot input off), Retry (reloads the scene), Main menu (loads MainMenu). No timer or strokes.
    /// </summary>
    public class GolferAvatarTestWiring : MonoBehaviour, ILevelFlow
    {
        [SerializeField] ShotController shot;
        [SerializeField] GolfBall ball;
        [SerializeField] GolferAvatar avatar;
        [Tooltip("Optional: the level's exit (#38).")]
        [SerializeField] LevelExit exit;

        // ---------- TEMP(#27) ILevelFlow: LevelManager does all of this ----------
        public LevelPhase Phase => exit != null && exit.IsOpen ? LevelPhase.Completed : LevelPhase.Playing;
        public IDangerTimer Danger => null;     // DangerManager (#7)
        public IStrokeCounter Strokes => null;  // ScoreKeeper (#18)
        public bool IsPaused { get; private set; }
        public event Action<bool> PausedChanged;
        public event Action LevelStarted { add { } remove { } }
        public event Action<LevelResult> LevelCompleted { add { } remove { } }
        public event Action LevelFailed { add { } remove { } }

        public void SetPaused(bool paused)
        {
            if (paused == IsPaused)
                return;
            IsPaused = paused;
            Time.timeScale = paused ? 0f : 1f;
            shot.InputEnabled = !paused && (exit == null || !exit.IsOpen);
            PausedChanged?.Invoke(paused);
        }

        public void Retry()
        {
            SetPaused(false);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void GoToMainMenu()
        {
            SetPaused(false);
            SceneManager.LoadScene(GameScenes.MainMenu);
        }

        void OnDestroy()
        {
            if (IsPaused)
                Time.timeScale = 1f;
        }

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
