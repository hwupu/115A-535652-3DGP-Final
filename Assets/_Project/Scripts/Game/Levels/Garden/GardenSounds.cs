using Project3DGP.Game.Effects;
using UnityEngine;

namespace Project3DGP.Game.Levels.Garden
{
    /// <summary>
    /// The Garden cat's sounds (#72): a light cue when it turns suspicious, a heavy one when it wakes up. Played at the
    /// cat through GameplaySounds, from the L1 SoundSet extras "CatSuspicious" and "CatWake" (replace the clips there).
    /// Magic > Game > Add Sounds puts it on Systems/Sounds in L1.
    /// </summary>
    public class GardenSounds : MonoBehaviour
    {
        public const string CatSuspicious = "CatSuspicious";
        public const string CatWake = "CatWake";

        [SerializeField] GardenCat cat;

        void OnEnable()
        {
            if (cat == null)
                return;
            cat.StateChanged += OnCatStateChanged;
            cat.WokeUp += OnCatWokeUp;
        }

        void OnDisable()
        {
            if (cat == null)
                return;
            cat.StateChanged -= OnCatStateChanged;
            cat.WokeUp -= OnCatWokeUp;
        }

        void OnCatStateChanged(CatState state)
        {
            if (state == CatState.Suspicious)
                GameplaySounds.PlayExtra(CatSuspicious, cat.transform.position);
        }

        void OnCatWokeUp() => GameplaySounds.PlayExtra(CatWake, cat.transform.position);
    }
}
