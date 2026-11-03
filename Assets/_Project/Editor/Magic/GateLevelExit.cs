using System.IO;
using Project3DGP.Game.Flow;
using UnityEditor;
using UnityEngine;

namespace Project3DGP.EditorTools
{
    /// <summary>
    /// Makes the Course/Gate placeholder a working LevelExit (#38): wooden bars across the opening (shown while
    /// closed), the purple portal surface (shown once open) and a WalkTarget 1.5 m behind the gate (+Z), where
    /// the hamster walks to. New gates get it from PlaceholderPrefabsMenu; Ensure updates an existing Gate prefab
    /// once (every placed gate follows the prefab).
    /// </summary>
    internal static class GateLevelExit
    {
        public static string PrefabPath => PlaceholderPrefabsMenu.Folder + "/Course/Gate.prefab";
        public static readonly Vector3 WalkTargetPosition = new(0f, 0f, 1.5f);

        public static void Add(GameObject root)
        {
            var visual = root.transform.Find("Visual");
            var bars = new GameObject("Bars").transform;
            bars.SetParent(visual, false);
            foreach (var x in new[] { -0.6f, 0f, 0.6f })
                PlaceholderPrefabsMenu.Part(bars, PrimitiveType.Cube, "Bar", new Vector3(x, 1.5f, 0f), new Vector3(0.12f, 3f, 0.12f), PlaceholderMaterials.Wood);
            PlaceholderPrefabsMenu.Part(bars, PrimitiveType.Cube, "Crossbar", new Vector3(0f, 1.5f, 0f), new Vector3(2f, 0.12f, 0.12f), PlaceholderMaterials.Wood);

            var walkTarget = new GameObject("WalkTarget").transform;
            walkTarget.SetParent(root.transform, false);
            walkTarget.localPosition = WalkTargetPosition;

            var exit = root.AddComponent<LevelExit>();
            var so = new SerializedObject(exit);
            so.FindProperty("walkTarget").objectReferenceValue = walkTarget;
            var portal = visual.Find("PortalSurface");
            var show = so.FindProperty("showWhenOpen");
            show.arraySize = portal != null ? 1 : 0;
            if (portal != null)
                show.GetArrayElementAtIndex(0).objectReferenceValue = portal.gameObject;
            var hide = so.FindProperty("hideWhenOpen");
            hide.arraySize = 1;
            hide.GetArrayElementAtIndex(0).objectReferenceValue = bars.gameObject;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void Ensure()
        {
            if (!File.Exists(PrefabPath))
                PlaceholderPrefabsMenu.CreateAll();
            var root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                if (root.GetComponent<LevelExit>() != null || root.transform.Find("Visual") == null)
                    return;
                Add(root);
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                MagicMenuUtil.Log("Made " + PrefabPath + " a LevelExit (bars, portal surface, WalkTarget)");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }
    }
}
