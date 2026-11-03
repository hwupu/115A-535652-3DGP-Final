using System.Collections.Generic;
using System.IO;
using System.Linq;
using Project3DGP.Game.Effects;
using Project3DGP.Game.Golf;
using Project3DGP.Game.Levels.Garden;
using Project3DGP.Game.Player;
using Project3DGP.Game.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Project3DGP.EditorTools
{
    /// <summary>
    /// Magic > Game > Add Sounds (#72): the Kenney Impact Sounds (CC0, Assets/ThirdParty/Kenney/ImpactSounds) wired
    /// into the open scene. Shared framework: run it in every scene (each owner in their own level).
    ///   Settings/Audio/Sounds_L1_Garden, _L2_Desert, _L3_Lake   one SoundSet per level, created once with the picks
    ///                                                          below (grass / sand / ice), then edited in the Inspector
    ///   Systems/Sounds   (levels: scenes with a ShotController) GameplaySounds + 8 3D voices, the level's SoundSet
    ///                    L1 also: GardenSounds (cat) and the splash on the pond's BallTriggerEffect
    ///   every canvas     UiSounds + a 2D AudioSource (button clicks, slider ticks)
    /// Only what is missing; one Undo step; the scene is marked dirty, not saved. The full event -> sound list is in
    /// docs/audio/sound-list.md.
    /// </summary>
    public static class AddSoundsMenu
    {
        const string ClipFolder = "Assets/ThirdParty/Kenney/ImpactSounds";
        const string SetFolder = "Assets/_Project/Settings/Audio";
        const int Voices = 8;

        // UI sounds (all scenes).
        static readonly (string Group, float Volume) UiClick = ("impactWood_light", 0.6f);
        static readonly (string Group, float Volume) UiBack = ("impactSoft_medium", 0.6f);
        static readonly (string Group, float Volume) UiSlider = ("impactPlate_light", 0.35f);

        // Level picks: SoundSet field (or "extra:Name") -> Kenney group and volume. Shared events first.
        static readonly Dictionary<string, (string Field, string Group, float Volume)[]> LevelPicks = new()
        {
            ["L1_Garden"] = Shared("impactSoft_medium", "impactWood_light", "footstep_grass", "impactWood_heavy").Concat(new[]
            {
                ("extra:" + GardenSounds.CatSuspicious, "impactPlank_medium", 0.6f),
                ("extra:" + GardenSounds.CatWake, "impactMining", 0.9f),
                ("extra:PondSplash", "impactSoft_heavy", 1f),
            }).ToArray(),
            ["L2_Desert"] = Shared("impactSoft_heavy", "impactMining", "footstep_carpet", "impactWood_heavy").Concat(new[]
            {
                ("extra:ScarabHit", "impactPunch_medium", 0.9f),
                ("extra:SandLanding", "impactSoft_heavy", 1f),
            }).ToArray(),
            ["L3_Lake"] = Shared("impactGlass_light", "impactGlass_medium", "footstep_snow", "impactMetal_medium").Concat(new[]
            {
                ("extra:IceCrack", "impactGlass_heavy", 1f),
                ("extra:GuardianHit", "impactMetal_medium", 0.9f),
                ("extra:SnowLanding", "impactSoft_medium", 0.9f),
            }).ToArray(),
        };

        static IEnumerable<(string, string, float)> Shared(string bounceGround, string bounceHard, string footstep, string gate) => new[]
        {
            ("ShotClub", "impactWood_medium", 0.9f), ("ShotHammer", "impactMetal_heavy", 0.8f), ("ShotTap", "impactPlate_light", 0.7f),
            ("BounceGround", bounceGround, 0.8f), ("BounceHard", bounceHard, 0.9f), ("OutOfBounds", "impactGeneric_light", 0.8f),
            ("Footstep", footstep, 0.45f),
            ("GoalReached", "impactTin_medium", 1f), ("GateOpen", gate, 0.9f), ("XpCollect", "impactGlass_light", 0.8f),
            ("LevelUp", "impactBell_heavy", 0.8f), ("LevelFailed", "impactPunch_heavy", 0.9f),
        };

        [MenuItem(MagicMenuUtil.Root + "Game/Add Sounds", priority = MagicMenuUtil.GamePriority + 17)]
        static void Add()
        {
            if (Clips("impactWood_light").Length == 0)
            {
                MagicMenuUtil.Log($"No sounds in {ClipFolder} (pull the branch).");
                return;
            }
            foreach (var level in LevelPicks.Keys)
                EnsureSet(level);

            var scene = EditorSceneManager.GetActiveScene();
            Undo.SetCurrentGroupName("Add Sounds");
            var undoGroup = Undo.GetCurrentGroup();
            var added = 0;

            added += AddUiSounds(scene);
            var shot = Object.FindAnyObjectByType<ShotController>(FindObjectsInactive.Include);
            if (shot != null)
                added += AddGameplaySounds(scene, shot);

            Undo.CollapseUndoOperations(undoGroup);
            if (added > 0)
                EditorSceneManager.MarkSceneDirty(scene);
            MagicMenuUtil.Log($"Sounds in {scene.name}: {added} added. Save the scene.");
        }

        // ---------- sound sets ----------

        static string SetPath(string level) => $"{SetFolder}/Sounds_{level}.asset";

        static SoundSet EnsureSet(string level)
        {
            var path = SetPath(level);
            var set = AssetDatabase.LoadAssetAtPath<SoundSet>(path);
            if (set != null)
                return set;
            MagicMenuUtil.EnsureFolder(SetFolder);
            set = ScriptableObject.CreateInstance<SoundSet>();
            foreach (var (field, group, volume) in LevelPicks[level])
            {
                var cue = Cue(group, volume);
                if (field.StartsWith("extra:"))
                    set.Extras.Add(new NamedSoundCue { Name = field.Substring(6), Cue = cue });
                else
                    typeof(SoundSet).GetField(field).SetValue(set, cue);
            }
            AssetDatabase.CreateAsset(set, path);
            MagicMenuUtil.Log($"Created {path}");
            return set;
        }

        static SoundCue Cue(string group, float volume) => new() { Clips = Clips(group), Volume = volume };

        static AudioClip[] Clips(string group) => Directory.Exists(ClipFolder)
            ? Directory.GetFiles(ClipFolder, group + "_*.ogg").OrderBy(p => p)
                .Select(p => AssetDatabase.LoadAssetAtPath<AudioClip>(p.Replace('\\', '/'))).Where(c => c != null).ToArray()
            : new AudioClip[0];

        // ---------- scene ----------

        static int AddUiSounds(UnityEngine.SceneManagement.Scene scene)
        {
            var added = 0;
            foreach (var root in scene.GetRootGameObjects())
            foreach (var canvas in root.GetComponentsInChildren<Canvas>(true))
            {
                if (!canvas.isRootCanvas || canvas.GetComponent<UiSounds>() != null)
                    continue;
                if (!canvas.TryGetComponent(out AudioSource audio))   // not ??: Unity's fake null
                    audio = Undo.AddComponent<AudioSource>(canvas.gameObject);
                audio.playOnAwake = false;
                audio.spatialBlend = 0f;
                var sounds = Undo.AddComponent<UiSounds>(canvas.gameObject);
                var so = new SerializedObject(sounds);
                SetCue(so.FindProperty("click"), UiClick.Group, UiClick.Volume);
                SetCue(so.FindProperty("back"), UiBack.Group, UiBack.Volume);
                SetCue(so.FindProperty("slider"), UiSlider.Group, UiSlider.Volume);
                so.ApplyModifiedPropertiesWithoutUndo();
                MagicMenuUtil.Log($"Added UiSounds to {canvas.name}");
                added++;
            }
            return added;
        }

        static void SetCue(SerializedProperty cue, string group, float volume)
        {
            var clips = Clips(group);
            var list = cue.FindPropertyRelative("Clips");
            list.arraySize = clips.Length;
            for (var i = 0; i < clips.Length; i++)
                list.GetArrayElementAtIndex(i).objectReferenceValue = clips[i];
            cue.FindPropertyRelative("Volume").floatValue = volume;
        }

        static int AddGameplaySounds(UnityEngine.SceneManagement.Scene scene, ShotController shot)
        {
            var added = 0;
            var sounds = Object.FindAnyObjectByType<GameplaySounds>(FindObjectsInactive.Include);
            if (sounds == null)
            {
                var level = LevelPicks.ContainsKey(scene.name) ? scene.name : "L1_Garden";
                var go = new GameObject("Sounds");
                var systems = LevelSkeleton.Find(scene, "Systems");
                if (systems != null)
                    go.transform.SetParent(systems.transform, false);
                var voices = new AudioSource[Voices];
                for (var i = 0; i < Voices; i++)
                {
                    var voice = new GameObject($"Voice_{i + 1}").AddComponent<AudioSource>();
                    voice.transform.SetParent(go.transform, false);
                    voice.playOnAwake = false;
                    voice.spatialBlend = 0.7f;   // mostly 3D, never inaudible
                    voice.rolloffMode = AudioRolloffMode.Linear;
                    voice.minDistance = 6f;
                    voice.maxDistance = 70f;
                    voices[i] = voice;
                }
                sounds = go.AddComponent<GameplaySounds>();
                var so = new SerializedObject(sounds);
                so.FindProperty("sounds").objectReferenceValue = AssetDatabase.LoadAssetAtPath<SoundSet>(SetPath(level));
                so.FindProperty("shot").objectReferenceValue = shot;
                so.FindProperty("ball").objectReferenceValue = Object.FindAnyObjectByType<GolfBall>(FindObjectsInactive.Include);
                var avatar = Object.FindAnyObjectByType<GolferAvatar>(FindObjectsInactive.Include);
                so.FindProperty("avatar").objectReferenceValue = avatar != null ? avatar.transform : null;
                var list = so.FindProperty("voices");
                list.arraySize = Voices;
                for (var i = 0; i < Voices; i++)
                    list.GetArrayElementAtIndex(i).objectReferenceValue = voices[i];
                so.ApplyModifiedPropertiesWithoutUndo();
                Undo.RegisterCreatedObjectUndo(go, "Add Sounds");
                MagicMenuUtil.Log($"Added Systems/Sounds (GameplaySounds with Sounds_{level}, {Voices} voices)");
                added++;
            }

            if (scene.name == "L1_Garden")
            {
                var cat = Object.FindAnyObjectByType<GardenCat>(FindObjectsInactive.Include);
                if (cat != null && sounds.GetComponent<GardenSounds>() == null)
                {
                    var garden = Undo.AddComponent<GardenSounds>(sounds.gameObject);
                    var so = new SerializedObject(garden);
                    so.FindProperty("cat").objectReferenceValue = cat;
                    so.ApplyModifiedPropertiesWithoutUndo();
                    MagicMenuUtil.Log("Added GardenSounds (cat suspicious / wakes up)");
                    added++;
                }
                foreach (var effect in Object.FindObjectsByType<BallTriggerEffect>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                {
                    if (!effect.name.StartsWith("Pond"))
                        continue;
                    var so = new SerializedObject(effect);
                    var cue = so.FindProperty("sound");
                    if (cue.FindPropertyRelative("Clips").arraySize > 0)
                        continue;
                    SetCue(cue, "impactSoft_heavy", 1f);
                    so.ApplyModifiedProperties();
                    MagicMenuUtil.Log($"Added the splash sound to {effect.name}");
                    added++;
                }
            }
            return added;
        }
    }
}
