using System;
using UnityEngine;

namespace Project3DGP.Game.Flow
{
    /// <summary>
    /// The way out of a level (#38, architecture §4.7b): the garden gate in L1, a portal elsewhere. Closed until
    /// LevelManager (#27) calls Open on LevelGoals.AllMet; then the hamster walks to WalkTarget
    /// (GolferAvatar.WalkTo) and arriving there loads the next level. Opening shows / hides objects (the gate's
    /// bars go, the portal surface appears); FX and sound can listen to Opened.
    /// Retry reloads the level, so there is no Close.
    /// </summary>
    public class LevelExit : MonoBehaviour
    {
        [Tooltip("Where the hamster walks to once the exit is open (through the gate). Empty = this object.")]
        [SerializeField] Transform walkTarget;
        [Tooltip("Shown only while open (e.g. the portal surface).")]
        [SerializeField] GameObject[] showWhenOpen = Array.Empty<GameObject>();
        [Tooltip("Shown only while closed (e.g. the gate's bars).")]
        [SerializeField] GameObject[] hideWhenOpen = Array.Empty<GameObject>();

        public bool IsOpen { get; private set; }
        public Transform WalkTarget => walkTarget != null ? walkTarget : transform;

        public event Action Opened;

        void Awake() => ShowState(false);

        /// <summary>Opens the exit once; later calls do nothing.</summary>
        [ContextMenu("Open")]
        public void Open()
        {
            if (IsOpen)
                return;
            IsOpen = true;
            ShowState(true);
            Opened?.Invoke();
        }

        void ShowState(bool open)
        {
            foreach (var go in showWhenOpen)
                if (go != null) go.SetActive(open);
            foreach (var go in hideWhenOpen)
                if (go != null) go.SetActive(!open);
        }

        internal void SetForTests(Transform target, GameObject[] show, GameObject[] hide)
        {
            walkTarget = target;
            showWhenOpen = show;
            hideWhenOpen = hide;
            ShowState(IsOpen);
        }
    }
}
