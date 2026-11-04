using Project3DGP.Core.Cameras;
using Project3DGP.Core.Movement;
using Project3DGP.MiniEx.Pongpat.ObjectInteraction;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project3DGP.EditorTools
{
    /// <summary>Explicit, Undo-supported cleanup of another demo accidentally added to Pongpat's scene.</summary>
    public static class PongpatObjectInteractionCleanupMenu
    {
        const string ScenePath = "Assets/_Project/MiniExercises/Pongpat/4C4_ObjectInteraction/Scenes/4C4_ObjectInteraction.unity";

        // This separate removal command is deliberately never called by the create-only setup tool.
        [MenuItem(MagicMenuUtil.Root + "Mini-Exercises/Pongpat 4.C.4: Remove Henry Demo Objects",
            priority = MagicMenuUtil.MiniExercisesPriority + 33)]
        public static void RemoveHenryDemoObjects()
        {
            var scene = SceneManager.GetActiveScene();
            if (EditorApplication.isPlayingOrWillChangePlaymode || scene.path != ScenePath)
            {
                MagicMenuUtil.Log("Open Pongpat's 4.C.4 scene and stop Play mode before removing old demo objects.");
                return;
            }

            var roots = scene.GetRootGameObjects();
            var hasOwnDemo = false;
            foreach (var root in roots)
                hasOwnDemo |= root.GetComponent<ObjectInteractionDemo>() != null;
            if (!hasOwnDemo)
            {
                MagicMenuUtil.Log("No Pongpat ObjectInteractionDemo found; nothing removed.");
                return;
            }

            Undo.IncrementCurrentGroup();
            var group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Remove Henry objects from Pongpat 4.C.4");
            var removed = 0;
            foreach (var root in roots)
            {
                if ((root.name == "Arena" || root.name == "HUD") && HasHenryDemoComponent(root) ||
                    root.name == "Hamster" && root.GetComponent<ThirdPersonWalker>() != null)
                {
                    MagicMenuUtil.Log("Removing old demo root " + root.name + ".");
                    Undo.DestroyObjectImmediate(root);
                    removed++;
                }
                else if (root.name == "Main Camera")
                {
                    var oldFollow = root.GetComponent<OrbitFollowCamera>();
                    if (oldFollow == null)
                        continue;
                    Undo.DestroyObjectImmediate(oldFollow);
                    MagicMenuUtil.Log("Removed old hamster camera follow component.");
                    removed++;
                }
            }
            Undo.CollapseUndoOperations(group);
            if (removed > 0)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
            MagicMenuUtil.Log("Pongpat 4.C.4 cleanup complete: " + removed + " old objects/components removed.");
        }

        static bool HasHenryDemoComponent(GameObject root)
        {
            // Identify the foreign component without introducing an assembly dependency on Henry.
            foreach (var component in root.GetComponents<MonoBehaviour>())
            {
                if (component == null)
                    continue;
                var componentNamespace = component.GetType().Namespace;
                if (componentNamespace != null && componentNamespace.StartsWith("Project3DGP.MiniEx.Henry"))
                    return true;
            }
            return false;
        }
    }
}
