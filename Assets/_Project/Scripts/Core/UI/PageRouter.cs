using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Project3DGP.Core.UI
{
    /// <summary>
    /// Shows one Editor-authored page at a time. Keep this component on an always-active
    /// object outside the pages, so a hidden page can still be opened again.
    /// </summary>
    public class PageRouter : MonoBehaviour
    {
        [Tooltip("Separate page roots; do not include this object's parent or the router itself.")]
        [SerializeField] GameObject[] pages = Array.Empty<GameObject>();
        [SerializeField] int menuIndex;
        [SerializeField] int initialIndex;
        [Tooltip("Optional first button/slider for each page, using the same array indices as Pages.")]
        [SerializeField] Selectable[] firstSelections = Array.Empty<Selectable>();

        public int CurrentIndex { get; private set; } = -1;
        public event Action<int> PageChanged;

        void Awake() => Show(initialIndex);

        /// <summary>Invalid destinations leave the current page and keyboard focus unchanged.</summary>
        public void Show(int index)
        {
            if (pages == null || index < 0 || index >= pages.Length || pages[index] == null)
                return;

            var destination = pages[index];
            for (var i = 0; i < pages.Length; i++)
            {
                if (pages[i] != null)
                    pages[i].SetActive(pages[i] == destination);
            }

            // Hidden buttons must not retain keyboard focus. Mouse-only setups need no EventSystem here.
            var eventSystem = EventSystem.current;
            if (eventSystem != null)
            {
                eventSystem.SetSelectedGameObject(null);
                if (firstSelections != null && index < firstSelections.Length)
                {
                    var first = firstSelections[index];
                    if (first != null && first.IsActive() && first.IsInteractable())
                        eventSystem.SetSelectedGameObject(first.gameObject);
                }
            }

            if (CurrentIndex == index)
                return;

            CurrentIndex = index;
            PageChanged?.Invoke(index);
        }

        public void ReturnToMenu() => Show(menuIndex);
    }
}
