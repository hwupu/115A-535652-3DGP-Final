using System;
using UnityEngine;

namespace Project3DGP.Game.Effects
{
    /// <summary>
    /// One sound event (#72): a few variants picked at random, a volume and a small random pitch change, so repeats
    /// don't sound identical. Drag other clips in the Inspector to replace a sound; no code changes needed.
    /// </summary>
    [Serializable]
    public class SoundCue
    {
        public AudioClip[] Clips = Array.Empty<AudioClip>();
        [Range(0f, 1f)] public float Volume = 1f;
        [Range(0f, 0.5f)] public float PitchJitter = 0.06f;

        public bool IsEmpty => Clips == null || Clips.Length == 0;

        /// <summary>A random variant, or null if there are none.</summary>
        public AudioClip Pick() => IsEmpty ? null : Clips[UnityEngine.Random.Range(0, Clips.Length)];

        /// <summary>A pitch around 1 within the jitter.</summary>
        public float PickPitch() => 1f + UnityEngine.Random.Range(-PitchJitter, PitchJitter);
    }

    /// <summary>A sound cue with a name (level extras: hazards and other level-only events).</summary>
    [Serializable]
    public class NamedSoundCue
    {
        public string Name;
        public SoundCue Cue = new();
    }
}
