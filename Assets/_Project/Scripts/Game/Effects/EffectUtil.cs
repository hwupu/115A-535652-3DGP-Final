using UnityEngine;

namespace Project3DGP.Game.Effects
{
    static class EffectUtil
    {
        /// <summary>Moves a one-shot particle system to the position and restarts it (one reused instance = cheap).</summary>
        public static void PlayAt(ParticleSystem effect, Vector3 position)
        {
            if (effect == null)
                return;
            effect.transform.position = position;
            effect.Clear(true);
            effect.Play(true);
        }
    }
}
