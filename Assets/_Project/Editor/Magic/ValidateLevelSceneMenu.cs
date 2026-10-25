using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Project3DGP.EditorTools
{
    /// <summary>
    /// Magic > Level > Validate Level Scene (#8): lists what the active scene is missing
    /// compared to LevelSkeleton, plus a camera, a light and Build Settings. Changes nothing.
    /// </summary>
    public static class ValidateLevelSceneMenu
    {
        [MenuItem(MagicMenuUtil.Root + "Level/Validate Level Scene", priority = MagicMenuUtil.LevelPriority + 1)]
        static void Validate()
        {
            var scene = EditorSceneManager.GetActiveScene();
            var problems = new List<string>();

            foreach (var slot in LevelSkeleton.Slots)
                if (LevelSkeleton.Find(scene, slot.Path) == null)
                    problems.Add($"Missing {slot.Path}: {slot.Purpose}");

            var cup = LevelSkeleton.Find(scene, "Gameplay/Goal/Cup");
            if (cup != null && !(cup.TryGetComponent(out Collider collider) && collider.isTrigger))
                problems.Add("Gameplay/Goal/Cup needs a trigger collider.");

            if (Object.FindObjectsByType<Camera>(FindObjectsSortMode.None).Length == 0)
                problems.Add("No camera in the scene.");
            if (Object.FindObjectsByType<Light>(FindObjectsSortMode.None).Length == 0)
                problems.Add("No light in the scene.");

            if (System.Array.FindIndex(EditorBuildSettings.scenes, s => s.path == scene.path) < 0)
                problems.Add("Scene is not in Build Settings (Magic > Scenes > Sync Build Settings).");

            if (problems.Count == 0)
            {
                MagicMenuUtil.Log($"Validate {scene.name}: OK, all {LevelSkeleton.Slots.Length} slots present.");
                return;
            }

            Debug.LogWarning($"[Magic] Validate {scene.name}: {problems.Count} problem(s)\n- " +
                             string.Join("\n- ", problems));
        }
    }
}
