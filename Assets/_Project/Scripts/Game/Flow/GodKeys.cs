using System.Collections;
using System.Linq;
using Project3DGP.Core.Cameras;
using Project3DGP.Game.Golf;
using Project3DGP.Game.Player;
using Project3DGP.Game.Scoring;
using Project3DGP.Game.Session;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Project3DGP.Game.Flow
{
    /// <summary>
    /// Dev cheats (#143) to reach the level ends quickly. Editor and development builds only (off in release builds).
    /// On the GameSession prefab (Magic > Game > Add God Keys), so every playable scene has them.
    ///   F5  one-shot goal (demo): the hamster lines up and hits a hole in one (a scripted perfect shot into the Cup),
    ///       then every LevelGoal is met (the cup last) and the exit opens; he walks to the portal, the camera follows.
    ///   F6  time up: the level fails (ILevelCheats.ForceFail): its fail scene (L1: the cat wakes up), then the
    ///       time-up dialog with the defeat animation.
    ///   F7  victory: this level is recorded as finished (GameSession.RecordResult, unless it already is) and the
    ///       Ending scene loads.
    /// </summary>
    public class GodKeys : MonoBehaviour
    {
        [SerializeField] Key goalKey = Key.F5;
        [SerializeField] Key timeUpKey = Key.F6;
        [SerializeField] Key victoryKey = Key.F7;

        void Awake()
        {
            if (!Debug.isDebugBuild)
                enabled = false;
        }

        void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
                return;
            if (keyboard[goalKey].wasPressedThisFrame)
                OneShotGoal();
            if (keyboard[timeUpKey].wasPressedThisFrame)
                TimeUp();
            if (keyboard[victoryKey].wasPressedThisFrame)
                Victory();
        }

        bool _demoRunning;

        /// <summary>
        /// F5: the demo hole-in-one. The hamster lines up facing the cup, swings, the ball flies into the cup
        /// (ShotController.TakeScriptedShot), then every goal is met and the exit opens: the hamster walks out and
        /// the camera follows him. Without a ball or a cup in the level, only the goals and the exit.
        /// </summary>
        public void OneShotGoal()
        {
            var flow = LevelFlowFinder.Find(null);
            if (flow == null || flow.Phase != LevelPhase.Playing)
            {
                Log(flow == null ? "one-shot goal: not in a level" : $"one-shot goal: the level is {flow.Phase}");
                return;
            }
            if (_demoRunning)
                return;
            if (flow.IsPaused)
                flow.SetPaused(false);
            var shot = FindAnyObjectByType<ShotController>();
            var ball = FindAnyObjectByType<GolfBall>();
            var cup = FindCup();
            if (shot == null || ball == null || cup == null || !ball.IsAtRest || shot.Phase != ShotPhase.Aiming)
            {
                Log($"one-shot goal: no demo shot ({(cup == null ? "no Cup in the level" : "no ball at rest")}); meeting the goals directly");
                MeetAllGoals();
                return;
            }
            StartCoroutine(HoleInOne(shot, ball, cup));
        }

        IEnumerator HoleInOne(ShotController shot, GolfBall ball, Transform cup)
        {
            _demoRunning = true;
            shot.InputEnabled = false;   // the player can't shoot; the avatar keeps the facing below
            var toCup = cup.position - ball.Position;
            var yaw = Mathf.Atan2(toCup.x, toCup.z) * Mathf.Rad2Deg;
            var avatar = FindAnyObjectByType<GolferAvatar>();
            if (avatar != null)
            {
                avatar.TeleportTo(ball.Position, yaw);   // stance beside the ball, facing the cup
                yield return new WaitForSeconds(0.6f);
                if (ball == null)
                {
                    _demoRunning = false;
                    yield break;
                }
            }
            // The ball sinks into the hole: its centre a radius below the cup's ground.
            var radius = ball.TryGetComponent(out SphereCollider sphere) ? sphere.bounds.extents.y : 0.05f;
            var landed = false;
            if (!shot.TakeScriptedShot(cup.position + Vector3.down * radius, SwingImpactDelay, () => landed = true))
            {
                _demoRunning = false;
                MeetAllGoals();
                yield break;
            }
            Log("one-shot goal: hole in one!");
            while (!landed)
            {
                if (ball == null)   // the scene was reloaded (Retry)
                {
                    _demoRunning = false;
                    yield break;
                }
                yield return null;
            }
            yield return new WaitForSeconds(0.4f);
            MeetAllGoals();
            var orbit = FindAnyObjectByType<OrbitFollowCamera>();
            if (orbit != null && avatar != null)
                orbit.Target = avatar.transform;   // watch the walk to the portal (CameraRig does this itself)
            _demoRunning = false;
        }

        // Swing_Club hits on frame 10 at 24 fps; the ball leaves then.
        const float SwingImpactDelay = 0.4f;

        // The cup: a LevelGoal named like it (Cup, #18) or, before that exists, the Cup prefab instance by name.
        static Transform FindCup()
        {
            var goal = FindObjectsByType<LevelGoal>(FindObjectsSortMode.None).FirstOrDefault(g => g.GetType().Name.Contains("Cup") || g.name.StartsWith("Cup"));
            if (goal != null)
                return goal.transform;
            return FindObjectsByType<Transform>(FindObjectsSortMode.None).FirstOrDefault(t => t.name == "Cup" || t.name.StartsWith("Cup ("));
        }

        public static void MeetAllGoals()
        {
            var flow = LevelFlowFinder.Find(null);
            if (flow == null || flow.Phase != LevelPhase.Playing)
            {
                Log(flow == null ? "one-shot goal: not in a level" : $"one-shot goal: the level is {flow.Phase}");
                return;
            }
            if (flow.IsPaused)
                flow.SetPaused(false);
            // Goals that are not accepting yet (the cup, until the others are met) go last.
            var goals = FindObjectsByType<LevelGoal>(FindObjectsSortMode.None).OrderBy(g => g.Accepting ? 0 : 1).ToList();
            foreach (var goal in goals)
                goal.ForceMet();
            var exit = FindAnyObjectByType<LevelExit>();
            if (exit != null && !exit.IsOpen)
                exit.Open();   // until LevelGoals (#18) opens it when all goals are met
            Log($"one-shot goal: {goals.Count} goal(s) met{(exit != null ? ", exit open" : ", no exit in this level")}");
        }

        public static void TimeUp()
        {
            var flow = LevelFlowFinder.Find(null);
            if (flow == null || flow.Phase != LevelPhase.Playing)
            {
                Log(flow == null ? "time up: not in a level" : $"time up: the level is {flow.Phase}");
                return;
            }
            if (flow is not ILevelCheats cheats)
            {
                Log($"time up: {flow.GetType().Name} does not implement ILevelCheats");
                return;
            }
            if (flow.IsPaused)
                flow.SetPaused(false);
            cheats.ForceFail();
            Log("time up");
        }

        public static void Victory()
        {
            var level = SceneManager.GetActiveScene().name;
            var session = GameSession.Instance;
            if (session != null && level != GameScenes.MainMenu && level != GameScenes.Ending
                && session.Data.RunResults.All(r => r.LevelId != level))
            {
                var flow = LevelFlowFinder.Find(null);
                var strokes = flow?.Strokes?.Strokes ?? 0;
                var remaining = flow?.Danger?.RemainingSeconds ?? 0f;
                session.RecordResult(new LevelResult(level, strokes, remaining, 0));
            }
            Time.timeScale = 1f;
            Log($"victory: {level} -> {GameScenes.Ending}");
            SceneManager.LoadScene(GameScenes.Ending);
        }

        static void Log(string message) => Debug.Log("[GodKeys] " + message);
    }
}
