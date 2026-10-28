using System;
using System.Collections.Generic;
using Project3DGP.Game.Golf;
using Project3DGP.Game.Scoring;
using UnityEngine;

namespace Project3DGP.Game.Session
{
    /// <summary>
    /// Cross-scene data (#89, architecture §4.9). Every scene that needs it has the GameSession prefab
    /// (Magic > Game > Add Game Session), so pressing Play in any level works. The first one to wake becomes
    /// Instance and survives scene loads (DontDestroyOnLoad); copies in later scenes remove themselves.
    /// The data is a plain GameSessionData: save/load (#44) writes Data and calls Load.
    /// </summary>
    public class GameSession : MonoBehaviour
    {
        [Tooltip("Every club in the game, in slot order (keys 1-3). Unlocked ones are taken from this list.")]
        [SerializeField] List<ClubProfile> allClubs = new();
        [Tooltip("Clubs a new player has. PO: the standard club only; the rest unlock by leveling (#70).")]
        [SerializeField] List<ClubProfile> startingClubs = new();
        [Tooltip("Total XP needed to reach level 2, 3, ... (one entry per level after the first). Max level = entries + 1.")]
        [SerializeField] int[] xpToReachLevel = { 100, 250, 450, 700 };

        GameSessionData _data;
        readonly List<ClubProfile> _unlocked = new();

        public static GameSession Instance { get; private set; }

        public GameSessionData Data => _data;
        public IReadOnlyList<ClubProfile> UnlockedClubs => _unlocked;
        public IReadOnlyList<ClubProfile> AllClubs => allClubs;
        public int PlayerLevel => _data.PlayerLevel;
        /// <summary>Total XP earned in this game (it does not reset at a level-up).</summary>
        public int Xp => _data.Xp;
        public int MaxLevel => xpToReachLevel.Length + 1;
        public bool IsMaxLevel => PlayerLevel >= MaxLevel;
        /// <summary>The save slot (#44) on the same GameSession object, or null until it exists.</summary>
        public ISaveStore SaveStore => GetComponent<ISaveStore>();

        /// <summary>Anything changed (unlock, XP, result, load): HUD and save can refresh.</summary>
        public event Action Changed;
        /// <summary>Raised with the new level after XP pushed the player up one or more levels (#70 unlocks clubs here).</summary>
        public event Action<int> LeveledUp;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);   // the session from an earlier scene is already running
                return;
            }
            Instance = this;
            if (transform.parent != null)
                transform.SetParent(null);   // DontDestroyOnLoad works on root objects only
            DontDestroyOnLoad(gameObject);
            Load(NewGame());
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        /// <summary>A fresh start: level 1, no XP, the starting clubs.</summary>
        public GameSessionData NewGame()
        {
            var data = new GameSessionData();
            foreach (var club in startingClubs)
                if (club != null) data.Unlock(club.name);
            return data;
        }

        /// <summary>Replaces all data (new game, or a loaded save from #44).</summary>
        public void Load(GameSessionData data)
        {
            _data = data ?? NewGame();
            ApplySettings(_data.Settings);
            RefreshUnlocked();
            Changed?.Invoke();
        }

        public bool IsUnlocked(ClubProfile club) => club != null && _data.IsUnlocked(club.name);

        /// <summary>Leveling (#70) unlocks a club. Returns true if it was locked.</summary>
        public bool Unlock(ClubProfile club)
        {
            if (club == null || !_data.Unlock(club.name))
                return false;
            RefreshUnlocked();
            Changed?.Invoke();
            return true;
        }

        /// <summary>Sets level and XP directly (debug keys, tests). Normal play uses AddXp.</summary>
        public void SetProgress(int playerLevel, int xp)
        {
            _data.PlayerLevel = playerLevel;
            _data.Xp = xp;
            Changed?.Invoke();
        }

        /// <summary>Total XP needed to reach the level (level 1 = 0). Beyond the max level: the max level's XP.</summary>
        public int XpForLevel(int level)
        {
            if (level <= 1 || xpToReachLevel.Length == 0)
                return 0;
            return xpToReachLevel[Mathf.Min(level, MaxLevel) - 2];
        }

        /// <summary>How far the player is from this level to the next (0..1); 1 at the max level. The HUD XP bar.</summary>
        public float LevelProgress01
        {
            get
            {
                if (IsMaxLevel)
                    return 1f;
                var from = XpForLevel(PlayerLevel);
                var to = XpForLevel(PlayerLevel + 1);
                return to > from ? Mathf.Clamp01((float)(Xp - from) / (to - from)) : 1f;
            }
        }

        /// <summary>
        /// Leveling (#70) gives XP (holes, collectibles). Levels up as many times as the XP allows, then raises
        /// Changed and, if the level went up, LeveledUp.
        /// </summary>
        public void AddXp(int amount)
        {
            if (amount <= 0)
                return;
            _data.Xp += amount;
            var level = _data.PlayerLevel;
            while (level < MaxLevel && _data.Xp >= XpForLevel(level + 1))
                level++;
            var leveledUp = level > _data.PlayerLevel;
            _data.PlayerLevel = level;
            Changed?.Invoke();
            if (leveledUp)
                LeveledUp?.Invoke(level);
        }

        /// <summary>LevelManager (#27) reports every finished level. Returns true if it is a new best.</summary>
        public bool RecordResult(LevelResult result)
        {
            var best = _data.RecordResult(result);
            Changed?.Invoke();
            return best;
        }

        public LevelResult? GetBestResult(string levelId) => _data.GetBestResult(levelId);

        /// <summary>Applies the master volume (settings panel, load). Music and SFX read their own volume.</summary>
        public static void ApplySettings(Settings settings)
        {
            if (settings != null)
                AudioListener.volume = Mathf.Clamp01(settings.MasterVolume);
        }

        // Unlocked clubs in the order of allClubs, so the key slots stay the same as clubs unlock.
        void RefreshUnlocked()
        {
            _unlocked.Clear();
            foreach (var club in allClubs)
                if (club != null && _data.IsUnlocked(club.name)) _unlocked.Add(club);
        }

        internal void ConfigureForTests(List<ClubProfile> all, List<ClubProfile> starting, int[] xpTable = null)
        {
            allClubs = all;
            startingClubs = starting;
            if (xpTable != null)
                xpToReachLevel = xpTable;
        }
    }
}
