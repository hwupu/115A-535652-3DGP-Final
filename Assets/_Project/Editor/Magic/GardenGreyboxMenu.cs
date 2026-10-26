using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Project3DGP.EditorTools
{
    /// <summary>
    /// Magic > Level > Garden: Place Temporary Layout (#9): drops the placeholder prefabs into L1_Garden
    /// along an L-shaped route (tee bottom-left, hole top-right; par 2) so the code can be tested
    /// before the real layout is designed. Each object is created only if its name is missing.
    /// Replaces the skeleton's primitive Cup with the Cup prefab, and moves BallSpawn only if it is
    /// still at the skeleton default. Move anything freely afterwards. One Undo step.
    /// </summary>
    public static class GardenGreyboxMenu
    {
        const string SceneName = "L1_Garden";

        // Name, prefab (under Prefabs/), parent slot, position, Y rotation, scale.
        static readonly (string Name, string Prefab, string Parent, Vector3 Position, float Yaw, Vector3 Scale)[] Layout =
        {
            ("Cup", "Course/Cup", "Gameplay/Goal", new Vector3(18f, 0f, 18f), 0f, Vector3.one),
            ("Gate", "Course/Gate", "Gameplay", new Vector3(18f, 0f, 22.5f), 0f, Vector3.one),
            ("Pond", "Course/Pond", "Gameplay/InvalidZones", new Vector3(8f, 0.02f, 12f), 0f, new Vector3(0.6f, 1f, 0.5f)),
            ("Cat", "Characters/Cat", "Gameplay/Hazards", new Vector3(-2f, 0f, 6f), 200f, Vector3.one),

            ("Tree_01", "Nature/Tree", "Environment/Props", new Vector3(-23f, 0f, -8f), 0f, Vector3.one),
            ("Tree_02", "Nature/Tree", "Environment/Props", new Vector3(-23f, 0f, 6f), 40f, Vector3.one),
            ("Tree_03", "Nature/Tree", "Environment/Props", new Vector3(-22f, 0f, 21f), 80f, Vector3.one),
            ("Tree_04", "Nature/Tree", "Environment/Props", new Vector3(-8f, 0f, 24f), 120f, Vector3.one),
            ("Tree_05", "Nature/Tree", "Environment/Props", new Vector3(6f, 0f, 24f), 160f, Vector3.one),
            ("Tree_06", "Nature/Tree", "Environment/Props", new Vector3(5f, 0f, -10f), 200f, Vector3.one),
            ("Tree_07", "Nature/Tree", "Environment/Props", new Vector3(15f, 0f, -4f), 240f, Vector3.one),
            ("Tree_08", "Nature/Tree", "Environment/Props", new Vector3(22f, 0f, 2f), 280f, Vector3.one),
            ("Bush_01", "Nature/Bush", "Environment/Props", new Vector3(-14f, 0f, 8f), 0f, Vector3.one),
            ("Bush_02", "Nature/Bush", "Environment/Props", new Vector3(-21f, 0f, -18f), 0f, Vector3.one),
            ("Bush_03", "Nature/Bush", "Environment/Props", new Vector3(12f, 0f, 9f), 0f, Vector3.one),
            ("Bush_04", "Nature/Bush", "Environment/Props", new Vector3(23f, 0f, 12f), 0f, Vector3.one),
            ("Rock_01", "Nature/Rock", "Environment/Props", new Vector3(-15f, 0f, -2f), 30f, Vector3.one),
            ("Rock_02", "Nature/Rock", "Environment/Props", new Vector3(2f, 0f, 14f), 110f, Vector3.one),
            ("Rock_03", "Nature/Rock", "Environment/Props", new Vector3(20f, 0f, 8f), 250f, Vector3.one),
        };

        static readonly Vector3 TeePosition = new Vector3(-18f, 0.1f, -15f);

        [MenuItem(MagicMenuUtil.Root + "Level/Garden: Place Temporary Layout", priority = MagicMenuUtil.LevelPriority + 20)]
        static void PlaceLayout()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.name != SceneName)
            {
                MagicMenuUtil.Log($"Open {SceneName} first (this layout is for the Garden only).");
                return;
            }
            if (LevelSkeleton.Find(scene, "Gameplay") == null)
            {
                MagicMenuUtil.Log("Run Magic > Level > Create Level Scene Skeleton first.");
                return;
            }

            Undo.SetCurrentGroupName("Place Garden Layout");
            var undoGroup = Undo.GetCurrentGroup();
            var created = 0;

            // The skeleton's primitive Cup is a stand-in; the Cup prefab (hole + flag) replaces it.
            var oldCup = LevelSkeleton.Find(scene, "Gameplay/Goal/Cup");
            if (oldCup != null && !PrefabUtility.IsPartOfPrefabInstance(oldCup))
            {
                Undo.DestroyObjectImmediate(oldCup);
                MagicMenuUtil.Log("Removed the primitive Gameplay/Goal/Cup (replaced by the Cup prefab).");
            }

            foreach (var item in Layout)
            {
                var parent = LevelSkeleton.Find(scene, item.Parent);
                if (parent == null || parent.transform.Find(item.Name) != null)
                    continue;

                var prefabPath = $"{PlaceholderPrefabsMenu.Folder}/{item.Prefab}.prefab";
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefab == null)
                {
                    MagicMenuUtil.Log($"Missing {prefabPath}: run Magic > Prefabs > Create Placeholder Prefabs first.");
                    continue;
                }

                var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
                go.name = item.Name;
                go.transform.SetParent(parent.transform, false);
                go.transform.localPosition = item.Position;
                go.transform.localRotation = Quaternion.Euler(0f, item.Yaw, 0f);
                go.transform.localScale = item.Scale;
                Undo.RegisterCreatedObjectUndo(go, "Place " + item.Name);
                created++;
            }

            created += PlaceFences(scene);

            var spawn = LevelSkeleton.Find(scene, "Gameplay/BallSpawn");
            if (spawn != null && spawn.transform.localPosition == new Vector3(0f, 0.1f, 0f))
            {
                Undo.RecordObject(spawn.transform, "Move BallSpawn");
                spawn.transform.localPosition = TeePosition;
                MagicMenuUtil.Log("Moved Gameplay/BallSpawn to the tee " + TeePosition);
            }

            Undo.CollapseUndoOperations(undoGroup);
            EditorSceneManager.MarkSceneDirty(scene);
            MagicMenuUtil.Log($"Garden layout: {created} objects placed. Move them freely, then save the scene.");
        }

        // Fence behind the green: 6 segments along the top edge, 6 along the right edge.
        static int PlaceFences(UnityEngine.SceneManagement.Scene scene)
        {
            var parent = LevelSkeleton.Find(scene, "Environment/Boundary");
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PlaceholderPrefabsMenu.Folder}/Course/Fence.prefab");
            if (parent == null || prefab == null)
                return 0;

            var created = 0;
            for (var i = 0; i < 12; i++)
            {
                var name = $"Fence_{i + 1:00}";
                if (parent.transform.Find(name) != null)
                    continue;

                var alongTop = i < 6;
                var offset = 13f + (i % 6) * 2f;
                var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
                go.name = name;
                go.transform.SetParent(parent.transform, false);
                go.transform.localPosition = alongTop ? new Vector3(offset, 0f, 24.5f) : new Vector3(24.5f, 0f, offset);
                go.transform.localRotation = Quaternion.Euler(0f, alongTop ? 0f : 90f, 0f);
                Undo.RegisterCreatedObjectUndo(go, "Place " + name);
                created++;
            }
            return created;
        }
    }
}
