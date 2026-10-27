using UnityEngine;

namespace Project3DGP.Game.Golf
{
    /// <summary>
    /// One club's numbers (architecture §4.2). Tunable in the Inspector, so balancing needs no code (#46).
    /// #15 ships the Standard club; #16 adds the Sledgehammer and the Mysterious tool and the switching.
    /// </summary>
    [CreateAssetMenu(menuName = "Project3DGP/Club Profile", fileName = "Club")]
    public class ClubProfile : ScriptableObject
    {
        public string DisplayName = "Club";
        [Tooltip("Range: the power meter maps 0..1 to MinSpeed..MaxSpeed (m/s).")]
        public float MinSpeed = 4f;
        public float MaxSpeed = 16f;
        [Tooltip("Accuracy: full width of the aim cone (degrees). The shot goes in a random direction inside it.")]
        public float AimConeDegrees = 10f;
        [Tooltip("Control: seconds for the power marker to go up and down once. Shorter = harder to time.")]
        public float MarkerCycleSeconds = 1.6f;
        [Tooltip("Fixed launch angle above the ground (degrees). No pitch control in the MVP.")]
        public float LaunchPitch = 25f;
        [Tooltip("Level 1 noise gauge: how loud a shot is (0..1).")]
        public float Noise = 0.3f;
        [Tooltip("What the avatar holds (#88).")]
        public GameObject ToolPrefab;
    }
}
