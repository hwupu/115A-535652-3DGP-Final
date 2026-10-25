using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

namespace Project3DGP.EditorTools
{
    /// <summary>
    /// Magic > Scenes: creates the game's scene files and keeps Build Settings in the game order (#85).
    /// Scene contents (managers, tee, cup, ...) come from the level skeleton tool (#8), not from here.
    /// </summary>
    public static class GameScenesMenu
    {
        public const string ScenesFolder = "Assets/_Project/Scenes";
        const string SampleScenePath = "Assets/Scenes/SampleScene.unity";

        /// <summary>Game scenes in build order; MainMenu must stay first.</summary>
        public static readonly string[] GameScenes =
        {
            ScenesFolder + "/MainMenu.unity",
            ScenesFolder + "/L1_Garden.unity",
            ScenesFolder + "/L2_Desert.unity",
            ScenesFolder + "/L3_Lake.unity",
            ScenesFolder + "/Ending.unity",
        };

        [MenuItem(MagicMenuUtil.Root + "Scenes/Create Game Scenes", priority = MagicMenuUtil.ScenesPriority)]
        static void CreateGameScenes()
        {
            var created = MagicMenuUtil.CreateMissingScenes(GameScenes, withGround: false);
            MagicMenuUtil.Log($"Game scenes: {created.Count} created, {GameScenes.Length - created.Count} already existed.");
            SyncBuildSettings();
        }

        [MenuItem(MagicMenuUtil.Root + "Scenes/Sync Build Settings", priority = MagicMenuUtil.ScenesPriority + 1)]
        static void SyncBuildSettings()
        {
            var current = EditorBuildSettings.scenes;
            var result = new List<EditorBuildSettingsScene>();

            // Game scenes first, in order. Keep the enabled flag of entries that already exist.
            foreach (var path in GameScenes.Where(File.Exists))
            {
                var existing = current.FirstOrDefault(s => s.path == path);
                result.Add(existing ?? new EditorBuildSettingsScene(path, true));
            }

            // Then any other scenes someone added, except the template SampleScene.
            foreach (var scene in current)
            {
                if (GameScenes.Contains(scene.path))
                    continue;
                if (scene.path == SampleScenePath)
                {
                    MagicMenuUtil.Log("Removed SampleScene from Build Settings (the file itself is untouched).");
                    continue;
                }
                result.Add(scene);
            }

            if (result.Select(s => s.path).SequenceEqual(current.Select(s => s.path)))
            {
                MagicMenuUtil.Log("Build Settings already up to date.");
                return;
            }

            EditorBuildSettings.scenes = result.ToArray();
            MagicMenuUtil.Log("Build Settings: " + string.Join(", ", result.Select(s => Path.GetFileNameWithoutExtension(s.path))));
        }
    }
}
