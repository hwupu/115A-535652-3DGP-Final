using System.IO;
using Project3DGP.Game.Danger;
using UnityEditor;
using UnityEngine;

namespace Project3DGP.EditorTools
{
    /// <summary>Create missing per-level configs; balancing values are entered by each level owner.</summary>
    public static class DangerConfigsMenu
    {
        public const string Folder = "Assets/_Project/Settings/Danger";

        [MenuItem(MagicMenuUtil.Root + "Game/Create Danger Configs", priority = MagicMenuUtil.GamePriority + 23)]
        public static void CreateAll()
        {
            MagicMenuUtil.EnsureFolder(Folder);
            var created = 0;
            foreach (var sceneName in new[] { "L1_Garden", "L2_Desert", "L3_Lake" })
            {
                var path = Folder + "/" + sceneName + ".asset";
                if (File.Exists(path))
                {
                    MagicMenuUtil.Log("Skipped (exists) " + path);
                    continue;
                }

                var config = ScriptableObject.CreateInstance<DangerConfig>();
                AssetDatabase.CreateAsset(config, path);
                created++;
                MagicMenuUtil.Log("Created " + path + ". Set the four time values by hand before Begin; Start Seconds must be > 0.");
            }
            AssetDatabase.SaveAssets();

            // Make the result visible even when the Console is closed or every asset already exists.
            var folder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(Folder);
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = folder;
            EditorGUIUtility.PingObject(folder);
            MagicMenuUtil.Log($"Danger configs ready: {created} created, {3 - created} already existed. " +
                              "Opened " + Folder + "; select your level's config to edit its four time values.");
        }
    }
}
