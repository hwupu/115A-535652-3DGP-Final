using UnityEngine;
using UnityEngine.InputSystem;

namespace Project3DGP.MiniEx.Henry
{
    /// <summary>
    /// Swaps the tool in the hamster's hand (key T): all tools are children of C_Club and only one is
    /// active at a time. Because they are children of C, the active tool moves with C, B and A automatically.
    /// </summary>
    public class ToolSwitcher : MonoBehaviour
    {
        [SerializeField] GameObject[] tools;
        [SerializeField] Key switchKey = Key.T;

        int _current;

        public int CurrentIndex => _current;
        public string CurrentName => tools.Length > 0 ? tools[_current].name : "-";

        void Start() => Show(0);

        void Update()
        {
            if (Keyboard.current != null && Keyboard.current[switchKey].wasPressedThisFrame && tools.Length > 0)
                Show((_current + 1) % tools.Length);   // % wraps around: 0, 1, 0, 1, ...
        }

        void Show(int index)
        {
            _current = index;
            for (var i = 0; i < tools.Length; i++)
                tools[i].SetActive(i == index);
        }
    }
}
