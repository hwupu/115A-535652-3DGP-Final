using System;

namespace Project3DGP.Game.Session
{
    /// <summary>
    /// The single save slot (PO, 2026-10-28). Interface by Henry (#20); Save &amp; Load (#44, Pongpat) implements it
    /// as a component on the GameSession prefab (e.g. JsonUtility to a file in Application.persistentDataPath).
    /// Saving is manual, from the pause dialog (which asks before overwriting); a new game does not save;
    /// Continue in the main menu loads it.
    /// </summary>
    public interface ISaveStore
    {
        bool HasSave { get; }
        /// <summary>Writes the slot, replacing any earlier save.</summary>
        void Save(SaveGame save);
        /// <summary>False if there is no save or it can't be read.</summary>
        bool TryLoad(out SaveGame save);
    }

    /// <summary>What a save holds: the session and the level to restart (from the tee, not mid-shot).</summary>
    [Serializable]
    public class SaveGame
    {
        public string LevelScene;                 // e.g. "L1_Garden"
        public GameSessionData Session = new();
    }
}
