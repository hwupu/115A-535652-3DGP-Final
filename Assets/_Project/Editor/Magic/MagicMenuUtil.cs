using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Project3DGP.EditorTools
{
    /// <summary>
    /// Shared helpers for the Magic menu (ADR-0011).
    /// Every helper only creates what is missing; nothing existing is reset, overwritten or deleted.
    /// </summary>
    internal static class MagicMenuUtil
    {
        public const string Root = "Magic/";

        // Menu priorities: a gap of more than 10 adds a separator.
        public const int SetupPriority = 0;
        public const int ScenesPriority = 100;
        public const int MiniExercisesPriority = 200;

        public static void Log(string message) => Debug.Log("[Magic] " + message);

        /// <summary>Creates the folder (and parents) if missing. Returns true if anything was created.</summary>
        public static bool EnsureFolder(string path)
        {
            path = path.Replace('\\', '/').TrimEnd('/');
            if (AssetDatabase.IsValidFolder(path))
                return false;

            var parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            if (!string.IsNullOrEmpty(parent))
                EnsureFolder(parent);

            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
            Log("Created folder " + path);
            return true;
        }

        /// <summary>Writes a text file only if it does not exist yet. Returns true if it was created.</summary>
        public static bool EnsureTextFile(string path, string contents)
        {
            if (File.Exists(path))
            {
                Log("Skipped (exists) " + path);
                return false;
            }

            EnsureFolder(Path.GetDirectoryName(path));
            File.WriteAllText(path, contents);
            AssetDatabase.ImportAsset(path);
            Log("Created " + path);
            return true;
        }

        /// <summary>
        /// Creates each missing scene with a camera and a light (plus a ground plane if asked).
        /// Existing scene files are never opened. The scenes open before the call are restored afterwards.
        /// </summary>
        public static List<string> CreateMissingScenes(IEnumerable<string> scenePaths, bool withGround)
        {
            var created = new List<string>();
            var missing = new List<string>();
            foreach (var path in scenePaths)
            {
                if (File.Exists(path))
                    Log("Skipped (exists) " + path);
                else
                    missing.Add(path);
            }

            if (missing.Count == 0)
                return created;

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Log("Cancelled: open scenes were not saved.");
                return created;
            }

            var previousSetup = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                foreach (var path in missing)
                {
                    EnsureFolder(Path.GetDirectoryName(path));
                    var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
                    if (withGround)
                    {
                        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
                        ground.name = "Ground";
                        ground.transform.localScale = new Vector3(5f, 1f, 5f);
                    }

                    EditorSceneManager.SaveScene(scene, path);
                    created.Add(path);
                    Log("Created scene " + path);
                }
            }
            finally
            {
                if (previousSetup.Length > 0 && previousSetup.All(s => !string.IsNullOrEmpty(s.path)))
                    EditorSceneManager.RestoreSceneManagerSetup(previousSetup);
            }

            return created;
        }
    }
}
