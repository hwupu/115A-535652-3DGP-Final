using System.IO;
using UnityEditor;
using UnityEngine;

namespace Project3DGP.EditorTools
{
    /// <summary>
    /// Entry point for agent-assisted Editor tooling (Tools > 3DGP).
    /// Add scene-setup helpers here or in sibling files; keep them idempotent and Undo-able.
    /// Scenes are authored in the Editor, never built at runtime (see docs/decisions/0005).
    /// </summary>
    public static class ProjectSetupMenu
    {
        static readonly string[] Folders =
        {
            "Assets/_Project/Scripts", "Assets/_Project/Editor", "Assets/_Project/Scenes",
            "Assets/_Project/Prefabs", "Assets/_Project/Materials", "Assets/_Project/Art",
            "Assets/_Project/Audio", "Assets/_Project/UI", "Assets/ThirdParty",
        };

        [MenuItem("Tools/3DGP/Create Project Folders")]
        static void CreateFolders()
        {
            foreach (var f in Folders)
                Directory.CreateDirectory(f);
            AssetDatabase.Refresh();
            Debug.Log("[3DGP] Project folders ensured.");
        }
    }
}
