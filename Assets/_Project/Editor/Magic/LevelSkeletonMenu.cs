using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Project3DGP.EditorTools
{
    /// <summary>
    /// Magic > Level > Create Level Scene Skeleton (#8): adds the missing slots of LevelSkeleton
    /// to the active scene and moves the default camera and light from the root into their group
    /// (world position kept). Nothing else is moved, renamed or changed. One Undo step.
    /// The scene is marked dirty, not saved: check it, then save it yourself.
    /// </summary>
    public static class LevelSkeletonMenu
    {
        [MenuItem(MagicMenuUtil.Root + "Level/Create Level Scene Skeleton", priority = MagicMenuUtil.LevelPriority)]
        static void CreateSkeleton()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (string.IsNullOrEmpty(scene.path))
            {
                MagicMenuUtil.Log("Save the scene first, then run Create Level Scene Skeleton again.");
                return;
            }

            Undo.SetCurrentGroupName("Create Level Scene Skeleton");
            var undoGroup = Undo.GetCurrentGroup();
            var created = 0;

            foreach (var slot in LevelSkeleton.Slots)
            {
                if (LevelSkeleton.Find(scene, slot.Path) != null)
                    continue;

                var go = CreateObject(slot);
                var parentPath = LevelSkeleton.ParentPath(slot.Path);
                if (parentPath != null)
                    go.transform.SetParent(LevelSkeleton.Find(scene, parentPath).transform, false);
                go.transform.localPosition = slot.Position;
                go.transform.localScale = slot.Scale;

                Undo.RegisterCreatedObjectUndo(go, "Create " + slot.Path);
                MagicMenuUtil.Log("Created " + slot.Path);
                created++;
            }

            var moved = 0;
            foreach (var (name, parentPath) in LevelSkeleton.Moves)
            {
                var go = LevelSkeleton.Find(scene, name);
                if (go == null || go.transform.parent != null)
                    continue;
                Undo.SetTransformParent(go.transform, LevelSkeleton.Find(scene, parentPath).transform, true,
                    "Move " + name);
                MagicMenuUtil.Log($"Moved {name} into {parentPath}");
                moved++;
            }

            Undo.CollapseUndoOperations(undoGroup);
            if (created + moved > 0)
                EditorSceneManager.MarkSceneDirty(scene);
            MagicMenuUtil.Log($"Level skeleton in {scene.name}: {created} created, " +
                              $"{LevelSkeleton.Slots.Length - created} already existed, {moved} moved. Save the scene to keep it.");
        }

        static GameObject CreateObject(LevelSkeleton.Slot slot)
        {
            GameObject go;
            switch (slot.Shape)
            {
                case LevelSkeleton.Shape.Plane:
                    go = GameObject.CreatePrimitive(PrimitiveType.Plane);
                    break;
                case LevelSkeleton.Shape.Cylinder:
                    go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    break;
                default:
                    go = new GameObject();
                    break;
            }

            go.name = LevelSkeleton.Name(slot.Path);
            if (slot.Trigger && go.TryGetComponent(out Collider collider))
            {
                // The primitive's capsule doesn't fit a flat disc; use a box that reaches 0.1 m above it.
                Object.DestroyImmediate(collider);
                var box = go.AddComponent<BoxCollider>();
                box.size = new Vector3(1f, 20f, 1f);
                box.isTrigger = true;
            }
            return go;
        }
    }
}
