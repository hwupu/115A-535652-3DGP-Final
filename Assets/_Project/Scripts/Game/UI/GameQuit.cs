using UnityEngine;

namespace Project3DGP.Game.UI
{
    /// <summary>Quit / Close buttons (#20, #30): quits the game, or stops Play mode in the Editor.</summary>
    public static class GameQuit
    {
        public static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
