using System.Collections.Generic;
using System.Text;
using Project3DGP.Game.Flow;
using Project3DGP.Game.Session;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Project3DGP.Game.UI
{
    /// <summary>
    /// The Ending screen (#30, architecture §4.8): this run's results per level (GameSessionData.RunResults) and the
    /// totals: strokes, time left, score. Buttons: Main menu, Close (= quit).
    /// </summary>
    public class EndingView : MonoBehaviour
    {
        [SerializeField] Text summaryLabel;

        void Start()
        {
            var session = GameSession.Instance;
            Show(session != null ? session.Data.RunResults : null);
        }

        public void Show(IReadOnlyList<BestResult> results)
        {
            if (summaryLabel != null)
                summaryLabel.text = Format(results);
        }

        public void GoToMainMenu() => SceneManager.LoadScene(GameScenes.MainMenu);

        public void Quit() => GameQuit.Quit();

        /// <summary>One line per finished level, then the totals.</summary>
        public static string Format(IReadOnlyList<BestResult> results)
        {
            if (results == null || results.Count == 0)
                return "No levels finished in this run.";
            var text = new StringBuilder();
            int strokes = 0, score = 0;
            var seconds = 0f;
            foreach (var r in results)
            {
                text.AppendLine(Line(r.LevelId, r.Strokes, r.RemainingSeconds, r.Score));
                strokes += r.Strokes;
                seconds += r.RemainingSeconds;
                score += r.Score;
            }
            text.AppendLine();
            text.Append(Line("Total", strokes, seconds, score));
            return text.ToString();
        }

        static string Line(string name, int strokes, float seconds, int score) =>
            $"{name}:  {strokes} strokes,  {Mathf.FloorToInt(seconds)} s left,  {score} points";

        internal void SetForTests(Text label) => summaryLabel = label;
    }
}
