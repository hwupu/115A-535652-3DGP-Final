using Project3DGP.Core.UI;
using Project3DGP.Game.Danger;
using Project3DGP.Game.Flow;
using Project3DGP.Game.Golf;
using UnityEngine;

namespace Project3DGP.Game.Levels.Garden
{
    /// <summary>
    /// L1 tutorial prompts (#28): short hints in the HUD's dialog bar, each shown once per level attempt when the
    /// game first gets to that point. Non-blocking; a click on the bar skips a hint.
    ///   start              look around, the power marker, (switching tools if more than one)
    ///   first shot         every shot costs time
    ///   ball stops first   the goals, cup last
    ///   first Warning      the cat stirs          first Critical   the cat is almost awake
    /// The danger hints need the level's IDangerTimer (DangerManager, #7) through ILevelFlow.
    /// </summary>
    public class GardenTutorial : MonoBehaviour
    {
        [SerializeField] DialogBar dialog;
        [SerializeField] ShotController shot;
        [SerializeField] GolfBall ball;
        [Tooltip("The level's ILevelFlow (LevelManager, #27). Empty = the first one in the scene at Start.")]
        [SerializeField] MonoBehaviour flow;

        [Header("Texts")]
        [SerializeField, TextArea] string lookText = "Hold the right mouse button and move the mouse to look around. The line from the ball shows your aim.";
        [SerializeField, TextArea] string powerText = "The marker on the left goes up and down. Click when it's at the power you want.";
        [SerializeField, TextArea] string clubsText = "Press 1-3 or scroll the mouse wheel to switch tools.";
        [SerializeField, TextArea] string shotCostText = "Every shot takes time. Don't let the cat wake up!";
        [SerializeField, TextArea] string goalsText = "Reach every goal. The cup counts only when it is the last one.";
        [SerializeField, TextArea] string warningText = "The cat is stirring... hurry!";
        [SerializeField, TextArea] string criticalText = "The cat is almost awake! Finish quickly!";

        IDangerTimer _danger;
        bool _shotShown, _goalsShown, _warningShown, _criticalShown;

        void Start()
        {
            if (_danger == null)
                Bind(LevelFlowFinder.Find(flow));
            Show(lookText);
            Show(powerText);
            if (shot != null && shot.AvailableClubs.Count > 1)
                Show(clubsText);
        }

        void OnEnable()
        {
            if (shot != null) shot.ShotTaken += OnShotTaken;
            if (ball != null) ball.CameToRest += OnCameToRest;
        }

        void OnDisable()
        {
            if (shot != null) shot.ShotTaken -= OnShotTaken;
            if (ball != null) ball.CameToRest -= OnCameToRest;
        }

        void OnDestroy() => Bind(null);

        /// <summary>Connects the danger hints to the level (Start does it from the scene; tests call it directly).</summary>
        public void Bind(ILevelFlow levelFlow)
        {
            if (_danger != null)
                _danger.StateChanged -= OnDangerChanged;
            _danger = levelFlow?.Danger;
            if (_danger != null)
                _danger.StateChanged += OnDangerChanged;
        }

        void OnShotTaken(ShotInfo info)
        {
            if (_shotShown)
                return;
            _shotShown = true;
            if (dialog != null) dialog.Clear();   // the player got it: drop the remaining start hints
            Show(shotCostText);
        }

        void OnCameToRest()
        {
            if (!_shotShown || _goalsShown)
                return;
            _goalsShown = true;
            Show(goalsText);
        }

        void OnDangerChanged(DangerState state)
        {
            if (state == DangerState.Warning && !_warningShown)
            {
                _warningShown = true;
                Show(warningText);
            }
            else if (state == DangerState.Critical && !_criticalShown)
            {
                _criticalShown = true;
                if (dialog != null) dialog.Clear();   // urgent: replaces any hint still waiting
                Show(criticalText);
            }
        }

        void Show(string text)
        {
            if (dialog != null && !string.IsNullOrEmpty(text))
                dialog.Show(text);
        }

        internal void SetForTests(DialogBar bar) => dialog = bar;
    }
}
