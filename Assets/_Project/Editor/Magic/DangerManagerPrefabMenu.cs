using System.IO;
using Project3DGP.Game.Danger;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project3DGP.EditorTools
{
    /// <summary>Create the shared timer prefab. Its config is assigned separately in each level scene.</summary>
    public static class DangerManagerPrefabMenu
    {
        public const string PrefabPath = "Assets/_Project/Prefabs/Shared/DangerManager.prefab";

        [MenuItem(MagicMenuUtil.Root + "Game/Create Danger Manager Prefab", priority = MagicMenuUtil.GamePriority + 24)]
        public static void Create()
        {
            if (File.Exists(PrefabPath))
            {
                MagicMenuUtil.Log("Skipped (exists) " + PrefabPath);
                return;
            }

            MagicMenuUtil.EnsureFolder(Path.GetDirectoryName(PrefabPath));
            var preview = EditorSceneManager.NewPreviewScene();
            try
            {
                var root = new GameObject("DangerManager");
                SceneManager.MoveGameObjectToScene(root, preview);
                root.AddComponent<DangerManager>();
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                MagicMenuUtil.Log("Created " + PrefabPath + ". Assign the level's DangerConfig on its scene instance; LevelManager (#27) drives the timer.");
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(preview);
            }
        }
    }
}
