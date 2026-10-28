using System.IO;
using Project3DGP.Game.Golf;
using Project3DGP.Game.Session;
using UnityEditor;
using UnityEngine;

namespace Project3DGP.EditorTools
{
    /// <summary>
    /// Magic > Game > Set Up Leveling (#70). Creates what is missing:
    ///   Settings/Skills/SkillA, SkillB      placeholder skills (names TBD; the team decides the real ones)
    ///   GameSession prefab > Unlocks        if empty: Lv2 Sledgehammer, Lv3 Skill A, Lv4 Mysterious, Lv5 Skill B
    ///   Prefabs/Shared/XpSeed.prefab        the XP collectible level owners place (ball rolls through it: +20 XP)
    /// Existing assets and a filled unlock table are never changed. No scene is touched.
    /// </summary>
    public static class LevelingMenu
    {
        public const string SkillsFolder = "Assets/_Project/Settings/Skills";
        public const string SkillAPath = SkillsFolder + "/SkillA.asset";
        public const string SkillBPath = SkillsFolder + "/SkillB.asset";
        public const string SeedPath = GolfBallPrefabMenu.Folder + "/XpSeed.prefab";

        [MenuItem(MagicMenuUtil.Root + "Game/Set Up Leveling", priority = MagicMenuUtil.GamePriority + 16)]
        static void SetUp()
        {
            EnsureSkills();
            var prefab = GameSessionMenu.CreatePrefab();
            if (prefab != null)
                FillUnlocksInPrefab();
            CreateSeedPrefab();
            MagicMenuUtil.Log("Leveling set up. Place XpSeed prefabs in your level (Gameplay); tune XP and unlocks on the GameSession prefab.");
        }

        static void EnsureSkills()
        {
            EnsureSkill(SkillAPath, "Skill A", "Placeholder skill, unlocked at level 3. The real skill is TBD.");
            EnsureSkill(SkillBPath, "Skill B", "Placeholder skill, unlocked at level 5. The real skill is TBD.");
        }

        static void EnsureSkill(string path, string displayName, string description)
        {
            if (File.Exists(path))
                return;
            MagicMenuUtil.EnsureFolder(SkillsFolder);
            var skill = ScriptableObject.CreateInstance<SkillProfile>();
            skill.DisplayName = displayName;
            skill.Description = description;
            AssetDatabase.CreateAsset(skill, path);
            MagicMenuUtil.Log("Created " + path);
        }

        /// <summary>Fills an empty "unlocks" list (GameSession serialized object) with the placeholder table.</summary>
        internal static bool FillUnlocks(SerializedObject session)
        {
            var list = session.FindProperty("unlocks");
            if (list.arraySize > 0)
                return false;
            EnsureSkills();
            (int Level, string Club, string Skill)[] rows =
            {
                (2, ClubProfilesMenu.SledgehammerPath, null),
                (3, null, SkillAPath),
                (4, ClubProfilesMenu.MysteriousPath, null),
                (5, null, SkillBPath),
            };
            list.arraySize = rows.Length;
            for (var i = 0; i < rows.Length; i++)
            {
                var row = list.GetArrayElementAtIndex(i);
                row.FindPropertyRelative("Level").intValue = rows[i].Level;
                row.FindPropertyRelative("Club").objectReferenceValue =
                    rows[i].Club != null ? AssetDatabase.LoadAssetAtPath<ClubProfile>(rows[i].Club) : null;
                row.FindPropertyRelative("Skill").objectReferenceValue =
                    rows[i].Skill != null ? AssetDatabase.LoadAssetAtPath<SkillProfile>(rows[i].Skill) : null;
            }
            session.ApplyModifiedPropertiesWithoutUndo();
            return true;
        }

        static void FillUnlocksInPrefab()
        {
            var root = PrefabUtility.LoadPrefabContents(GameSessionMenu.PrefabPath);
            try
            {
                if (!FillUnlocks(new SerializedObject(root.GetComponent<GameSession>())))
                {
                    MagicMenuUtil.Log("Skipped: the GameSession prefab already has an unlock table.");
                    return;
                }
                PrefabUtility.SaveAsPrefabAsset(root, GameSessionMenu.PrefabPath);
                MagicMenuUtil.Log("GameSession prefab: unlock table filled (Lv2 Sledgehammer, Lv3 Skill A, Lv4 Mysterious, Lv5 Skill B)");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        // A trigger sphere with the collectible on the root and a small spinning "seed" mesh as a child.
        static void CreateSeedPrefab()
        {
            if (File.Exists(SeedPath))
            {
                MagicMenuUtil.Log("Skipped (exists) " + SeedPath);
                return;
            }
            var root = new GameObject("XpSeed");
            try
            {
                var trigger = root.AddComponent<SphereCollider>();
                trigger.isTrigger = true;
                trigger.radius = 0.5f;
                trigger.center = new Vector3(0f, 0.3f, 0f);

                var visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                visual.name = "Visual";
                Object.DestroyImmediate(visual.GetComponent<Collider>());
                visual.transform.SetParent(root.transform, false);
                visual.transform.localPosition = new Vector3(0f, 0.3f, 0f);
                visual.transform.localRotation = Quaternion.Euler(0f, 0f, 20f);
                visual.transform.localScale = new Vector3(0.2f, 0.32f, 0.2f);
                visual.GetComponent<Renderer>().sharedMaterial = PlaceholderMaterials.Get("Seed", new Color(0.95f, 0.8f, 0.3f));

                var collectible = root.AddComponent<XpCollectible>();
                var so = new SerializedObject(collectible);
                so.FindProperty("visual").objectReferenceValue = visual.transform;
                so.ApplyModifiedPropertiesWithoutUndo();

                MagicMenuUtil.EnsureFolder(GolfBallPrefabMenu.Folder);
                PrefabUtility.SaveAsPrefabAsset(root, SeedPath);
                MagicMenuUtil.Log("Created " + SeedPath + " (+20 XP when the ball rolls through)");
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }
    }
}
