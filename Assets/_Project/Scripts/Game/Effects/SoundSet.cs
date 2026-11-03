using System.Collections.Generic;
using Project3DGP.Game.Golf;
using UnityEngine;

namespace Project3DGP.Game.Effects
{
    /// <summary>
    /// The sounds of one level (#72): the shared gameplay events every level has (played by GameplaySounds), plus
    /// named extras for the level's own events (hazards...). One asset per level in Settings/Audio, so each level can
    /// sound different (grass, sand, snow) with the same code. Created by Magic > Game > Add Sounds.
    /// </summary>
    [CreateAssetMenu(menuName = "Project3DGP/Sound Set", fileName = "Sounds")]
    public class SoundSet : ScriptableObject
    {
        [Header("Shot, by the club's swing style")]
        public SoundCue ShotClub = new();
        public SoundCue ShotHammer = new();
        public SoundCue ShotTap = new();

        [Header("Ball")]
        [Tooltip("Landing / bouncing on the ground (contact facing up).")]
        public SoundCue BounceGround = new();
        [Tooltip("Hitting a wall, tree, rock, fence...")]
        public SoundCue BounceHard = new();
        public SoundCue OutOfBounds = new();

        [Header("Avatar")]
        public SoundCue Footstep = new();
        [Tooltip("Metres walked per footstep sound.")]
        public float StepDistance = 0.7f;

        [Header("Progress")]
        [Tooltip("A level goal is met (ball in the cup, branch target hit).")]
        public SoundCue GoalReached = new();
        public SoundCue GateOpen = new();
        public SoundCue XpCollect = new();
        public SoundCue LevelUp = new();
        public SoundCue LevelFailed = new();

        [Header("Level extras (played by name: GameplaySounds.PlayExtra)")]
        public List<NamedSoundCue> Extras = new();

        public SoundCue Shot(ClubProfile club) => club == null ? ShotClub : club.Swing switch
        {
            SwingStyle.Hammer => ShotHammer,
            SwingStyle.Tap => ShotTap,
            _ => ShotClub,
        };

        /// <summary>The extra with this name, or null.</summary>
        public SoundCue Extra(string name) => Extras.Find(e => e.Name == name)?.Cue;
    }
}
