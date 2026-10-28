using System;
using UnityEngine;

namespace Project3DGP.Game.Scoring
{
    /// <summary>
    /// Something the ball must reach or hit (architecture §4.6): the cup, a branch target spot, a lever.
    /// A level is won when all its goals are met, the cup last (LevelGoals, #18). Subclasses decide WHEN the
    /// goal is met and call MarkMet(); the rest (once per attempt, Accepting, reset) is here.
    /// Base class written in #22 (Henry) so BallHitTarget can use it; Cup and LevelGoals build on it (#18, Pongpat).
    /// </summary>
    public abstract class LevelGoal : MonoBehaviour
    {
        public bool IsMet { get; private set; }

        /// <summary>False = ignores the ball (LevelGoals keeps the cup closed until the other goals are met).</summary>
        public bool Accepting { get; set; } = true;

        /// <summary>Fires once per attempt, with the goal that was met.</summary>
        public event Action<LevelGoal> Reached;

        /// <summary>Retry: not met again. Accepting is left to LevelGoals.</summary>
        public void ResetGoal()
        {
            if (!IsMet)
                return;
            IsMet = false;
            OnGoalReset();
        }

        /// <summary>Call when the ball meets this goal. Ignored if already met or not accepting.</summary>
        protected void MarkMet()
        {
            if (IsMet || !Accepting)
                return;
            IsMet = true;
            OnMet();
            Reached?.Invoke(this);
        }

        /// <summary>Show that the goal is met (light up, flip the lever, ...).</summary>
        protected virtual void OnMet() { }

        /// <summary>Undo what OnMet showed.</summary>
        protected virtual void OnGoalReset() { }
    }
}
