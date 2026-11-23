using UnityEngine;

namespace Project3DGP.Game.Danger
{
    /// <summary>One level's time budget (architecture §4.5). Level owners set all values in seconds.</summary>
    [CreateAssetMenu(menuName = "Project3DGP/Danger Config", fileName = "DangerConfig")]
    public class DangerConfig : ScriptableObject
    {
        [Min(0f), Tooltip("Initial time budget. Must be greater than zero; set by the level owner.")]
        public float StartSeconds;
        [Min(0f), Tooltip("Time deducted for every shot, regardless of club.")]
        public float ShotCostSeconds;
        [Min(0f), Tooltip("Enter Warning at or below this much remaining time.")]
        public float WarningBelowSeconds;
        [Min(0f), Tooltip("Enter Critical at or below this much remaining time. Must not exceed Warning.")]
        public float CriticalBelowSeconds;

        internal bool TryValidate(out string error)
        {
            error = null;
            if (!Finite(StartSeconds) || StartSeconds <= 0f)
                error = "StartSeconds must be finite and greater than zero.";
            else if (!Finite(ShotCostSeconds) || ShotCostSeconds < 0f)
                error = "ShotCostSeconds must be finite and non-negative.";
            else if (!Finite(WarningBelowSeconds) || !Finite(CriticalBelowSeconds) ||
                     CriticalBelowSeconds < 0f || WarningBelowSeconds < CriticalBelowSeconds ||
                     WarningBelowSeconds > StartSeconds)
                error = "Thresholds must satisfy 0 <= CriticalBelowSeconds <= WarningBelowSeconds <= StartSeconds.";
            return error == null;
        }

        internal static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
