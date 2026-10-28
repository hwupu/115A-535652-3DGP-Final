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

        GameSessionData _data;
        readonly List<ClubProfile> _unlocked = new();

        public static GameSession Instance { get; private set; }

        public GameSessionData Data => _data;
        public IReadOnlyList<ClubProfile> UnlockedClubs => _unlocked;
        public IReadOnlyList<ClubProfile> AllClubs => allClubs;
        public int PlayerLevel => _data.PlayerLevel;
        public int Xp => _data.Xp;

        /// <summary>Anything changed (unlock, result, load): HUD and save can refresh.</summary>
        public event Action Changed;

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

        /// <summary>Leveling (#70) sets level and XP; the XP curve lives there.</summary>
        public void SetProgress(int playerLevel, int xp)
        {
            _data.PlayerLevel = playerLevel;
            _data.Xp = xp;
            Changed?.Invoke();
        }

        /// <summary>LevelManager (#27) reports every finished level. Returns true if it is a new best.</summary>
        public bool RecordResult(LevelResult result)
        {
            var best = _data.RecordResult(result);
            Changed?.Invoke();
            return best;
        }

        public LevelResult? GetBestResult(string levelId) => _data.GetBestResult(levelId);

        // Unlocked clubs in the order of allClubs, so the key slots stay the same as clubs unlock.
        void RefreshUnlocked()
        {
            _unlocked.Clear();
            foreach (var club in allClubs)
                if (club != null && _data.IsUnlocked(club.name)) _unlocked.Add(club);
        }

        internal void ConfigureForTests(List<ClubProfile> all, List<ClubProfile> starting)
        {
            allClubs = all;
            startingClubs = starting;
        }
    }
}
