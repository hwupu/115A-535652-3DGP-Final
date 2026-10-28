using System.Collections;
using Project3DGP.Game.Flow;
using Project3DGP.Game.Session;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Project3DGP.Game.UI
{
    /// <summary>
    /// The dialogs of a level (#20, architecture §4.8), driven by the level's ILevelFlow:
    ///   pause dialog    (PausedChanged)  Resume, Retry level, Settings, Save, Main menu, Quit
    ///   confirm dialog  (Save, a save exists)  Overwrite, Cancel
    ///   settings panel  (Settings)  Back
    ///   time-up dialog  (LevelFailed, after the fail scene)  Retry, Main menu, Close
    /// The public methods are the button targets (wired in the scene by Magic > Level > Add HUD and Menus).
    /// Esc opens the pause dialog, and closes the open dialog. Saving goes through GameSession's ISaveStore (#44);
    /// without one the Save button is disabled.
    /// </summary>
    public class LevelMenus : MonoBehaviour
    {
        [Tooltip("The level's ILevelFlow (LevelManager, #27). Empty = the first one in the scene at Start.")]
        [SerializeField] MonoBehaviour flow;
        [SerializeField] GameObject pausePanel;
        [SerializeField] GameObject confirmPanel;
        [SerializeField] GameObject timeUpPanel;
        [SerializeField] SettingsPanel settingsPanel;
        [SerializeField] Button saveButton;
        [Tooltip("Short message on the pause dialog, e.g. \"Saved.\"")]
        [SerializeField] Text messageLabel;
        [Tooltip("Seconds of the level's fail scene (the cat waking up) before the time-up dialog shows.")]
        [SerializeField] float timeUpDelay = 3f;

        ILevelFlow _flow;
        ISaveStore _testSaveStore;
        Coroutine _timeUp;

        public bool IsPauseOpen => pausePanel != null && pausePanel.activeSelf;
        public bool IsConfirmOpen => confirmPanel != null && confirmPanel.activeSelf;
        public bool IsTimeUpOpen => timeUpPanel != null && timeUpPanel.activeSelf;
        public bool IsSettingsOpen => settingsPanel != null && settingsPanel.IsOpen;

        ISaveStore SaveStore => _testSaveStore ?? (GameSession.Instance != null ? GameSession.Instance.SaveStore : null);

        void Awake() => HideAll();

        void Start()
        {
            if (_flow == null)
                Bind(LevelFlowFinder.Find(flow));
        }

        void OnDestroy() => Unbind();

        /// <summary>Connects the dialogs to a level (Start does it from the scene; tests call it directly).</summary>
        public void Bind(ILevelFlow levelFlow)
        {
            Unbind();
            _flow = levelFlow;
            if (_flow == null)
                return;
            _flow.PausedChanged += OnPausedChanged;
            _flow.LevelFailed += OnLevelFailed;
            _flow.LevelStarted += OnLevelStarted;
        }

        void Unbind()
        {
            if (_flow == null)
                return;
            _flow.PausedChanged -= OnPausedChanged;
            _flow.LevelFailed -= OnLevelFailed;
            _flow.LevelStarted -= OnLevelStarted;
            _flow = null;
        }

        void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null || !keyboard.escapeKey.wasPressedThisFrame || _flow == null || IsTimeUpOpen)
                return;
            if (IsSettingsOpen) CloseSettings();
            else if (IsConfirmOpen) CancelOverwrite();
            else if (_flow.IsPaused) Resume();
            else if (_flow.Phase == LevelPhase.Playing) _flow.SetPaused(true);
        }

        // ---------- Button targets ----------

        public void Resume() => _flow?.SetPaused(false);

        public void Retry()
        {
            HideAll();
            _flow?.Retry();
        }

        public void OpenSettings()
        {
            Show(pausePanel, false);
            if (settingsPanel != null)
                settingsPanel.Open();
        }

        public void CloseSettings()
        {
            if (settingsPanel != null)
                settingsPanel.Close();
            Show(pausePanel, _flow != null && _flow.IsPaused);
        }

        /// <summary>Saves this level and the session; asks first if that would replace a save.</summary>
        public void Save()
        {
            var store = SaveStore;
            if (store == null)
                return;
            if (store.HasSave)
            {
                Show(pausePanel, false);
                Show(confirmPanel, true);
                return;
            }
            WriteSave(store);
        }

        public void ConfirmOverwrite()
        {
            Show(confirmPanel, false);
            Show(pausePanel, true);
            if (SaveStore != null)
                WriteSave(SaveStore);
        }

        public void CancelOverwrite()
        {
            Show(confirmPanel, false);
            Show(pausePanel, true);
        }

        public void GoToMainMenu()
        {
            HideAll();
            _flow?.GoToMainMenu();
        }

        public void Quit() => GameQuit.Quit();

        // ---------- Level events ----------

        void OnPausedChanged(bool paused)
        {
            if (!paused)
            {
                Show(pausePanel, false);
                Show(confirmPanel, false);
                if (settingsPanel != null)
                    settingsPanel.Close();
                return;
            }
            SetMessage("");
            if (saveButton != null)
                saveButton.interactable = SaveStore != null;
            Show(pausePanel, true);
        }

        void OnLevelFailed()
        {
            HideAll();
            if (_timeUp != null)
                StopCoroutine(_timeUp);
            _timeUp = StartCoroutine(ShowTimeUpLater());
        }

        void OnLevelStarted()
        {
            if (_timeUp != null)
                StopCoroutine(_timeUp);
            _timeUp = null;
            HideAll();
        }

        IEnumerator ShowTimeUpLater()
        {
            yield return new WaitForSecondsRealtime(timeUpDelay);   // realtime: the level may pause the game
            _timeUp = null;
            Show(timeUpPanel, true);
        }

        void WriteSave(ISaveStore store)
        {
            var session = GameSession.Instance;
            store.Save(new SaveGame
            {
                LevelScene = SceneManager.GetActiveScene().name,
                Session = session != null ? session.Data : new GameSessionData(),
            });
            SetMessage("Saved.");
        }

        void HideAll()
        {
            Show(pausePanel, false);
            Show(confirmPanel, false);
            Show(timeUpPanel, false);
            if (settingsPanel != null)
                settingsPanel.Close();
        }

        void SetMessage(string text)
        {
            if (messageLabel != null)
                messageLabel.text = text;
        }

        static void Show(GameObject panel, bool visible)
        {
            if (panel != null)
                panel.SetActive(visible);
        }

        internal void SetForTests(GameObject pause, GameObject confirm, GameObject timeUp, SettingsPanel settings,
            ISaveStore saveStore, float delay)
        {
            pausePanel = pause;
            confirmPanel = confirm;
            timeUpPanel = timeUp;
            settingsPanel = settings;
            _testSaveStore = saveStore;
            timeUpDelay = delay;
            HideAll();
        }
    }
}
