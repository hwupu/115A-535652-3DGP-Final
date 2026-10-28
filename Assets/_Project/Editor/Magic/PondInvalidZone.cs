using System.IO;
using Project3DGP.Game.Golf;
using UnityEditor;
using UnityEngine;

namespace Project3DGP.EditorTools
{
    /// <summary>
    /// Brings an existing Course/Pond prefab up to date (#95): adds the InvalidZone component (#14) and, only if
    /// the trigger still has the first placeholder size (1 m deep, top at the water), raises its top to 0.5 m
    /// above the water: a ball rolling over the pond (centre 0.1 m up) never reached the old trigger.
    /// Every placed pond updates with the prefab. Called by Magic > Level > Add Ball, Shot and Avatar.
    /// </summary>
    internal static class PondInvalidZone
    {
        public static readonly Vector3 TriggerCenter = new(0f, -0.25f, 0f);
        public static readonly Vector3 TriggerSize = new(8f, 1.5f, 8f);
        static readonly Vector3 OldCenter = new(0f, -0.5f, 0f);
        static readonly Vector3 OldSize = new(8f, 1f, 8f);

        public static string PrefabPath => PlaceholderPrefabsMenu.Folder + "/Course/Pond.prefab";

        public static void Ensure()
        {
            if (!File.Exists(PrefabPath))
                return;
            var root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                var changed = false;
                var box = root.GetComponent<BoxCollider>();
                if (box != null && box.isTrigger && box.center == OldCenter && box.size == OldSize)
                {
                    box.center = TriggerCenter;
                    box.size = TriggerSize;
                    MagicMenuUtil.Log("Raised the Pond trigger to 0.5 m above the water");
                    changed = true;
                }
                if (box != null && root.GetComponent<InvalidZone>() == null)
                {
                    root.AddComponent<InvalidZone>();
                    MagicMenuUtil.Log("Added InvalidZone to " + PrefabPath);
                    changed = true;
                }
                if (changed)
                    PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }
    }
}
