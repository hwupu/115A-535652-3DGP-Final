using UnityEngine;

namespace Project3DGP.Game.Session
{
    /// <summary>
    /// A shot skill unlocked by leveling (#70). Placeholder for now: the team decides the real skills later; code asks
    /// GameSession.HasSkill(skill) to switch an effect on. Unlocks are listed on the GameSession prefab (Unlocks).
    /// </summary>
    [CreateAssetMenu(menuName = "Project3DGP/Skill Profile", fileName = "Skill")]
    public class SkillProfile : ScriptableObject
    {
        public string DisplayName = "Skill";
        [TextArea] public string Description;
    }
}
