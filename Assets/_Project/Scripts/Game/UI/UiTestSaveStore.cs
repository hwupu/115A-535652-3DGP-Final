using Project3DGP.Game.Session;
using UnityEngine;

namespace Project3DGP.Game.UI
{
    /// <summary>
    /// An in-memory save slot for the UI Test Area and tests (#20), so Save and its overwrite question can be
    /// tried before Save &amp; Load (#44) exists. Forgotten when Play stops. Not for real scenes.
    /// </summary>
    public class UiTestSaveStore : MonoBehaviour, ISaveStore
    {
        SaveGame _save;

        public bool HasSave => _save != null;
        public int SaveCount { get; private set; }

        public void Save(SaveGame save)
        {
            _save = JsonUtility.FromJson<SaveGame>(JsonUtility.ToJson(save));   // a copy, like a file would be
            SaveCount++;
            Debug.Log($"[UI test] Saved {save.LevelScene} (in memory only; #44 writes a file).");
        }

        public bool TryLoad(out SaveGame save)
        {
            save = _save;
            return save != null;
        }
    }
}
