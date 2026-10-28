using System;
using Project3DGP.Game.Golf;

namespace Project3DGP.Game.Session
{
    /// <summary>One row of the leveling table (#70): reaching Level unlocks the club and/or the skill.</summary>
    [Serializable]
    public class LevelUnlock
    {
        public int Level = 2;
        public ClubProfile Club;
        public SkillProfile Skill;

        /// <summary>"Sledgehammer", "Skill A", or both joined with ", ".</summary>
        public string DisplayName =>
            Club != null && Skill != null ? $"{Club.DisplayName}, {Skill.DisplayName}"
            : Club != null ? Club.DisplayName
            : Skill != null ? Skill.DisplayName : "";
    }
}
