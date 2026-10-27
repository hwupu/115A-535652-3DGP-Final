using System.IO;
using Project3DGP.Game.Player;
using UnityEditor;
using UnityEngine;

namespace Project3DGP.EditorTools
{
    /// <summary>
    /// Magic > Game > Create Golfer Avatar Prefab (#88): Prefabs/Shared/GolferAvatar.prefab, a prefab VARIANT of
    /// Prefabs/Characters/Hamster with a GolferAvatar component. Because it is a variant, replacing the Hamster's
    /// Visual with the Blender model (#80) updates the avatar too. Only created if missing.
    /// </summary>
    public static class GolferAvatarPrefabMenu
    {
        public const string HamsterPath = PlaceholderPrefabsMenu.Folder + "/Characters/Hamster.prefab";
        public const string AvatarPath = GolfBallPrefabMenu.Folder + "/GolferAvatar.prefab";

        [MenuItem(MagicMenuUtil.Root + "Game/Create Golfer Avatar Prefab", priority = MagicMenuUtil.GamePriority + 13)]
        public static GameObject Create()
        {
            if (File.Exists(AvatarPath))
            {
                MagicMenuUtil.Log("Skipped (exists) " + AvatarPath);
                return AssetDatabase.LoadAssetAtPath<GameObject>(AvatarPath);
            }

            var hamster = AssetDatabase.LoadAssetAtPath<GameObject>(HamsterPath);
            if (hamster == null)
            {
                Debug.LogError($"[Magic] Missing {HamsterPath}. Run Magic > Prefabs > Create Placeholder Prefabs first.");
                return null;
            }

            // Saving an instance of a prefab as a new asset makes a prefab variant of it.
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(hamster);
            try
            {
                instance.name = "GolferAvatar";
                instance.AddComponent<GolferAvatar>();
                MagicMenuUtil.EnsureFolder(GolfBallPrefabMenu.Folder);
                var prefab = PrefabUtility.SaveAsPrefabAsset(instance, AvatarPath);
                MagicMenuUtil.Log("Created " + AvatarPath + " (variant of Characters/Hamster)");
                return prefab;
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
        }
    }
}
