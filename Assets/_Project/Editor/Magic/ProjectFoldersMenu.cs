using UnityEditor;

namespace Project3DGP.EditorTools
{
    /// <summary>
    /// Magic > Setup > Create Project Folders: ensures the standard folders under Assets/_Project.
    /// Scenes are authored in the Editor, never built at runtime (ADR-0005, ADR-0011).
    /// </summary>
    public static class ProjectFoldersMenu
    {
        static readonly string[] Folders =
        {
            "Assets/_Project/Scripts", "Assets/_Project/Editor", "Assets/_Project/Scenes",
            "Assets/_Project/Prefabs", "Assets/_Project/Materials", "Assets/_Project/Art",
            "Assets/_Project/Audio", "Assets/_Project/UI", "Assets/_Project/MiniExercises",
            "Assets/ThirdParty",
        };

        [MenuItem(MagicMenuUtil.Root + "Setup/Create Project Folders", priority = MagicMenuUtil.SetupPriority)]
        static void CreateFolders()
        {
            var created = 0;
            foreach (var folder in Folders)
                if (MagicMenuUtil.EnsureFolder(folder))
                    created++;
            MagicMenuUtil.Log($"Project folders: {created} created, {Folders.Length - created} already existed.");
        }
    }
}
