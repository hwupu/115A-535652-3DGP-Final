using System.IO;
using Project3DGP.Game.Golf;
using UnityEditor;
using UnityEngine;

namespace Project3DGP.EditorTools
{
    /// <summary>
    /// Magic > Game > Create Club Profiles (#15): creates the missing ClubProfile assets in Settings/Clubs.
    /// #15 ships the Standard club; #16 adds the Sledgehammer and the Mysterious tool to the list below.
    /// Existing assets are never changed (balancing happens in the Inspector).
    /// </summary>
    public static class ClubProfilesMenu
    {
        public const string Folder = "Assets/_Project/Settings/Clubs";
        public const string StandardPath = Folder + "/Standard.asset";

        [MenuItem(MagicMenuUtil.Root + "Game/Create Club Profiles", priority = MagicMenuUtil.GamePriority + 20)]
        public static void CreateAll()
        {
            MagicMenuUtil.EnsureFolder(Folder);
            // Standard golf club: mid range, mid accuracy (architecture §4.2). The only club at the start (#70).
            Ensure(StandardPath, "Standard Club", minSpeed: 4f, maxSpeed: 16f, cone: 10f, cycle: 1.6f, pitch: 25f, noise: 0.3f);
            AssetDatabase.SaveAssets();
        }

        static void Ensure(string path, string displayName, float minSpeed, float maxSpeed, float cone, float cycle,
            float pitch, float noise)
        {
            if (File.Exists(path))
            {
                MagicMenuUtil.Log("Skipped (exists) " + path);
                return;
            }

            var club = ScriptableObject.CreateInstance<ClubProfile>();
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
