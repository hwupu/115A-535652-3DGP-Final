using System.IO;
using Project3DGP.Game.Golf;
using UnityEditor;
using UnityEngine;

namespace Project3DGP.EditorTools
{
    /// <summary>
    /// Magic > Game > Create Club Profiles (#15, #16): creates the missing ClubProfile assets in Settings/Clubs:
    /// Standard, Sledgehammer and Mysterious (architecture §4.2). Existing assets are never changed
    /// (balancing happens in the Inspector, #46), except an EMPTY ToolPrefab field, which is filled with the
    /// matching Prefabs/Tools prefab once Magic > Prefabs > Create Placeholder Prefabs has made it (#88).
    /// </summary>
    public static class ClubProfilesMenu
    {
        public const string Folder = "Assets/_Project/Settings/Clubs";
        public const string StandardPath = Folder + "/Standard.asset";
        public const string SledgehammerPath = Folder + "/Sledgehammer.asset";
        public const string MysteriousPath = Folder + "/Mysterious.asset";

        /// <summary>All clubs in slot order (keys 1, 2, 3).</summary>
        public static readonly string[] AllPaths = { StandardPath, SledgehammerPath, MysteriousPath };

        [MenuItem(MagicMenuUtil.Root + "Game/Create Club Profiles", priority = MagicMenuUtil.GamePriority + 20)]
        public static void CreateAll()
        {
            MagicMenuUtil.EnsureFolder(Folder);
            // Standard golf club: mid range, mid accuracy (architecture §4.2). The only club at the start (#70).
            Ensure(StandardPath, "StandardClub", "Standard Club", minSpeed: 4f, maxSpeed: 16f, cone: 10f, cycle: 1.6f, pitch: 25f, noise: 0.3f);
            // Sledgehammer: high range, low accuracy (wide cone), a faster marker (harder to time), loud.
            Ensure(SledgehammerPath, "Sledgehammer", "Sledgehammer", minSpeed: 8f, maxSpeed: 26f, cone: 30f, cycle: 1.2f, pitch: 20f, noise: 1f);
            // Mysterious (codename, real tool TBD): low range, high accuracy (narrow cone), a slow calm marker, quiet.
            Ensure(MysteriousPath, "Mysterious", "Mysterious", minSpeed: 2f, maxSpeed: 10f, cone: 3f, cycle: 2f, pitch: 30f, noise: 0.1f);
            AssetDatabase.SaveAssets();
        }

        static void Ensure(string path, string toolName, string displayName, float minSpeed, float maxSpeed, float cone,
            float cycle, float pitch, float noise)
        {
            var tool = AssetDatabase.LoadAssetAtPath<GameObject>($"{PlaceholderPrefabsMenu.Folder}/Tools/{toolName}.prefab");
            if (File.Exists(path))
            {
                MagicMenuUtil.Log("Skipped (exists) " + path);
                var existing = AssetDatabase.LoadAssetAtPath<ClubProfile>(path);
                if (existing != null && existing.ToolPrefab == null && tool != null)
                {
                    existing.ToolPrefab = tool;
                    EditorUtility.SetDirty(existing);
                    MagicMenuUtil.Log($"Linked {path} ToolPrefab -> {tool.name}");
                }
                return;
            }

            var club = ScriptableObject.CreateInstance<ClubProfile>();
            club.ToolPrefab = tool;
            club.DisplayName = displayName;
            club.MinSpeed = minSpeed;
            club.MaxSpeed = maxSpeed;
            club.AimConeDegrees = cone;
            club.MarkerCycleSeconds = cycle;
            club.LaunchPitch = pitch;
            club.Noise = noise;
            AssetDatabase.CreateAsset(club, path);
            MagicMenuUtil.Log("Created " + path);
        }
    }
}
