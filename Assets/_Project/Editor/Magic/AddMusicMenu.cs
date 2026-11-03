using System.Collections.Generic;
using Project3DGP.Game.Effects;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Project3DGP.EditorTools
{
    /// <summary>
    /// Magic > Game > Add Music: background music (docs/audio/sound-list.md) in the open scene, picked by Henry (PO):
    ///   MainMenu    Kevin MacLeod - Carefree
    ///   L1_Garden   PulseBox - Comedy Background (CC BY-ND: never edited, so its level is set here, volume 0.45)
    ///   Ending      Alena Smirnova - Moderate Passion Tango + the Fanfare 2 jingle once at the start
    /// L2/L3 owners add a MusicPlayer themselves and drag in their track. Adds a root "Music" object with a 2D
    /// AudioSource and a MusicPlayer, only if the scene has no MusicPlayer yet. Also sets the music import settings
    /// (Streaming, Vorbis) on every bgm_* file in the music folders. One Undo step; the scene is marked dirty, not saved.
    /// </summary>
    public static class AddMusicMenu
    {
        const string Fma = "Assets/ThirdParty/FreeMusicArchive/";
        const string Freesound = "Assets/ThirdParty/Freesound/";
        static readonly string[] MusicFolders = { Fma.TrimEnd('/'), Freesound.TrimEnd('/') };

        readonly struct Pick
        {
            public readonly string Music, Jingle;
            public readonly float Volume;
            public Pick(string music, float volume, string jingle = null) { Music = music; Volume = volume; Jingle = jingle; }
        }

        static readonly Dictionary<string, Pick> Picks = new()
        {
            ["MainMenu"] = new Pick(Fma + "bgm_menu_carefree_kevin-macleod.mp3", 1f),
            ["L1_Garden"] = new Pick(Fma + "bgm_l1_comedy-background_pulsebox.mp3", 0.45f),
            ["Ending"] = new Pick(Fma + "bgm_ending_happy_tango_alena-smirnova.mp3", 1f, Freesound + "sfx_fanfare_580310.mp3"),
        };

        [MenuItem(MagicMenuUtil.Root + "Game/Add Music", priority = MagicMenuUtil.GamePriority + 18)]
        static void Add()
        {
            SetImportSettings();

            var scene = EditorSceneManager.GetActiveScene();
            if (!Picks.TryGetValue(scene.name, out var pick))
            {
                MagicMenuUtil.Log($"No track picked for {scene.name}. Add a GameObject with a MusicPlayer and drag in your track.");
                return;
            }
            var existing = Object.FindAnyObjectByType<MusicPlayer>(FindObjectsInactive.Include);
            if (existing != null)
            {
                MagicMenuUtil.Log($"Skipped: {scene.name} already has a MusicPlayer ({existing.name}).");
                return;
            }
            var music = AssetDatabase.LoadAssetAtPath<AudioClip>(pick.Music);
            if (music == null)
            {
                MagicMenuUtil.Log($"Missing {pick.Music} (pull the branch).");
                return;
            }

            var go = new GameObject("Music");
            var audio = go.AddComponent<AudioSource>();
            audio.playOnAwake = false;
            audio.spatialBlend = 0f;
            audio.priority = 0;   // music is never cut for SFX voices
            var player = go.AddComponent<MusicPlayer>();
            var so = new SerializedObject(player);
            so.FindProperty("music").objectReferenceValue = music;
            so.FindProperty("volume").floatValue = pick.Volume;
            if (pick.Jingle != null)
                so.FindProperty("jingle").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(pick.Jingle);
            so.ApplyModifiedPropertiesWithoutUndo();
            Undo.RegisterCreatedObjectUndo(go, "Add Music");
            EditorSceneManager.MarkSceneDirty(scene);
            MagicMenuUtil.Log($"Added Music to {scene.name}: {music.name} (volume {pick.Volume})"
                              + (pick.Jingle != null ? " + jingle once" : "") + ". Save the scene.");
        }

        // Long tracks stream from disk instead of sitting decompressed in memory.
        static void SetImportSettings()
        {
            foreach (var guid in AssetDatabase.FindAssets("bgm_ t:AudioClip", MusicFolders))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (AssetImporter.GetAtPath(path) is not AudioImporter importer)
                    continue;
                var settings = importer.defaultSampleSettings;
                if (settings.loadType == AudioClipLoadType.Streaming && settings.compressionFormat == AudioCompressionFormat.Vorbis)
                    continue;
                settings.loadType = AudioClipLoadType.Streaming;
                settings.compressionFormat = AudioCompressionFormat.Vorbis;
                settings.quality = 0.7f;
                importer.defaultSampleSettings = settings;
                importer.SaveAndReimport();
                MagicMenuUtil.Log($"Music import settings (Streaming, Vorbis) on {path}");
            }
        }
    }
}
