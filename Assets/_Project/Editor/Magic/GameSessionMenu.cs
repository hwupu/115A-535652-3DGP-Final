using System.IO;
using Project3DGP.Game.Golf;
using Project3DGP.Game.Session;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Project3DGP.EditorTools
{
    /// <summary>
    /// Magic > Game > Add Game Session (#89): makes Prefabs/Shared/GameSession.prefab if missing (all 3 clubs,
    /// starting with the Standard club only) and puts one in the open scene (under Systems if the scene has it).
    /// Run it in every scene you can press Play in (MainMenu and each level), so the session always exists.
    /// Only what is missing; one Undo step; the scene is not saved.
    /// </summary>
    public static class GameSessionMenu
    {
        public const string PrefabPath = GolfBallPrefabMenu.Folder + "/GameSession.prefab";

        [MenuItem(MagicMenuUtil.Root + "Game/Add Game Session", priority = MagicMenuUtil.GamePriority + 14)]
        static void Add()
        {
            var prefab = CreatePrefab();
            if (prefab == null)
                return;

            var scene = EditorSceneManager.GetActiveScene();
            if (Object.FindAnyObjectByType<GameSession>(FindObjectsInactive.Include) != null)
            {
                MagicMenuUtil.Log($"Skipped: {scene.name} already has a GameSession.");
                return;
            }
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            var systems = LevelSkeleton.Find(scene, "Systems");
            if (systems != null)
                go.transform.SetParent(systems.transform, false);
            Undo.RegisterCreatedObjectUndo(go, "Add GameSession");
            EditorSceneManager.MarkSceneDirty(scene);
            MagicMenuUtil.Log($"Added GameSession to {scene.name}" + (systems != null ? " (Systems)" : "") + ". Save the scene to keep it.");
        }

        internal static GameObject CreatePrefab()
        {
            if (File.Exists(PrefabPath))
            {
                MagicMenuUtil.Log("Skipped (exists) " + PrefabPath);
                return AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            }

            ClubProfilesMenu.CreateAll();
            var root = new GameObject("GameSession");
            try
            {
                var session = root.AddComponent<GameSession>();
                var so = new SerializedObject(session);
                var all = so.FindProperty("allClubs");
                all.arraySize = ClubProfilesMenu.AllPaths.Length;
                for (var i = 0; i < ClubProfilesMenu.AllPaths.Length; i++)
                    all.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<ClubProfile>(ClubProfilesMenu.AllPaths[i]);
                var starting = so.FindProperty("startingClubs");
                starting.arraySize = 1;
                starting.GetArrayElementAtIndex(0).objectReferenceValue = AssetDatabase.LoadAssetAtPath<ClubProfile>(ClubProfilesMenu.StandardPath);
                so.ApplyModifiedPropertiesWithoutUndo();
                MagicMenuUtil.EnsureFolder(GolfBallPrefabMenu.Folder);
                var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                MagicMenuUtil.Log("Created " + PrefabPath + " (3 clubs, starts with Standard)");
                return prefab;
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }
    }
}
