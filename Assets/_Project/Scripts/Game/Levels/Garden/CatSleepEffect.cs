using UnityEngine;

namespace Project3DGP.Game.Levels.Garden
{
    /// <summary>
    /// The cat's "Zzz" (#39): a looping particle system above its head that runs while the cat sleeps (Asleep,
    /// Twitching) and stops when it opens its eyes. The particles are a replaceable prefab instance.
    /// </summary>
    public class CatSleepEffect : MonoBehaviour
    {
        [SerializeField] GardenCat cat;
        [SerializeField] ParticleSystem zzz;

        void OnEnable()
        {
            if (cat == null || zzz == null)
                return;
            cat.StateChanged += Show;
            Show(cat.State);
        }

        void OnDisable()
        {
            if (cat != null)
                cat.StateChanged -= Show;
        }

        void Show(CatState state)
        {
            var sleeping = state == CatState.Asleep || state == CatState.Twitching;
            if (sleeping && !zzz.isPlaying)
                zzz.Play(true);
            else if (!sleeping && zzz.isPlaying)
                zzz.Stop(true, ParticleSystemStopBehavior.StopEmitting);   // the last Zs float away
        }
    }
}
