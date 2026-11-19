using System;
using UnityEngine;

namespace Project3DGP.Game.UI
{
    /// <summary>
    /// Characters acting inside a screen or dialog (#141): a small off-screen "stage" (characters, a light and a
    /// camera rendering into a RenderTexture) shown by the RawImage this sits on. While the RawImage is active the
    /// stage is too, and each actor's Animator restarts and gets its trigger (Ending: both hamsters "Victory";
    /// time-up dialog: the hamster "Defeat"). Built by Magic > Scenes > Add Ending Victory Stage and
    /// Magic > Level > Add Time-Up Defeat Stage; nothing is created at runtime.
    /// </summary>
    public class CharacterStageView : MonoBehaviour
    {
        [Serializable]
        public struct Actor
        {
            public Animator animator;
            [Tooltip("Animator trigger set each time the stage shows, e.g. Victory or Defeat.")]
            public string trigger;
        }

        [Tooltip("The stage root (camera, light, characters). Kept inactive while this view is hidden.")]
        [SerializeField] GameObject stage;
        [SerializeField] Actor[] actors = Array.Empty<Actor>();

        void OnEnable() => Show();

        void OnDisable() => Hide();

        /// <summary>Activates the stage and starts every actor's clip from the beginning.</summary>
        public void Show()
        {
            if (stage != null)
                stage.SetActive(true);
            foreach (var actor in actors)
            {
                if (actor.animator == null || string.IsNullOrEmpty(actor.trigger))
                    continue;
                actor.animator.Rebind();   // back to the default state, so a reopened dialog plays the clip again
                actor.animator.Update(0f);
                actor.animator.SetTrigger(actor.trigger);
            }
        }

        /// <summary>Deactivates the stage (its camera stops rendering).</summary>
        public void Hide()
        {
            if (stage != null)
                stage.SetActive(false);
        }
    }
}
