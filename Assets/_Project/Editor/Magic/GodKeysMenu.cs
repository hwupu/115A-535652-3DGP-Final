using Project3DGP.Game.Flow;
using UnityEditor;
using UnityEngine;

namespace Project3DGP.EditorTools
{
    /// <summary>
    /// Magic > Game > Add God Keys (#143): puts the GodKeys dev cheats (F5 one-shot goal, F6 time up, F7 victory;
    /// Editor and development builds only) on the GameSession prefab, which every playable scene has. No scene is
    /// touched. Only if missing.
    /// </summary>
    public static class GodKeysMenu
    {
        [MenuItem(MagicMenuUtil.Root + "Game/Add God Keys", priority = MagicMenuUtil.GamePriority + 15)]
        static void Add()
        {
            if (GameSessionMenu.CreatePrefab() == null)
                return;
            var root = PrefabUtility.LoadPrefabContents(GameSessionMenu.PrefabPath);
            try
            {
                if (root.GetComponent<GodKeys>() != null)
                {
                    MagicMenuUtil.Log($"Skipped (exists) GodKeys on {GameSessionMenu.PrefabPath}");
                    return;
                }
                root.AddComponent<GodKeys>();
                PrefabUtility.SaveAsPrefabAsset(root, GameSessionMenu.PrefabPath);
                MagicMenuUtil.Log($"Added GodKeys to {GameSessionMenu.PrefabPath}: in Play mode F5 = one-shot goal, F6 = time up, F7 = victory (Ending).");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }
    }
}
