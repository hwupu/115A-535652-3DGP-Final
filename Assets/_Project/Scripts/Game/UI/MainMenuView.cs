using Project3DGP.Core.UI;
using Project3DGP.Game.Flow;
using Project3DGP.Game.Session;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Project3DGP.Game.UI
{
    /// <summary>
    /// The main menu (#30, architecture §4.8): Start (new game → L1, nothing saved), Continue, Settings, Quit.
    /// Continue stays disabled for now: Save &amp; Load (#44, Pongpat) enables it when a save exists and loads it.
    /// Start plays the story intro first (#51, StoryPages; texts in its Inspector) if there is one.
    /// The public methods are the button targets (wired by Magic > Scenes > Add Main Menu UI).
    /// </summary>
    public class MainMenuView : MonoBehaviour
    {
        [SerializeField] GameObject menuPanel;
        [SerializeField] Button continueButton;
        [SerializeField] SettingsPanel settingsPanel;
        [Tooltip("Story intro before the first level (#51). Empty = straight to the level.")]
        [SerializeField] StoryPages intro;

        void Start()
        {
            if (continueButton != null)
                continueButton.interactable = false;   // #44: enable when GameSession.Instance.SaveStore.HasSave
            CloseSettings();
        }

        /// <summary>A fresh session (no save is written), the story intro, then the first level.</summary>
        public void StartGame()
        {
            var session = GameSession.Instance;
            if (session != null)
                session.Load(session.NewGame());
            if (intro == null)
            {
                LoadFirstLevel();
                return;
            }
            if (menuPanel != null)
                menuPanel.SetActive(false);
            intro.Play(LoadFirstLevel);
        }

        static void LoadFirstLevel() => SceneManager.LoadScene(GameScenes.FirstLevel);

        public void OpenSettings()
        {
            if (menuPanel != null)
                menuPanel.SetActive(false);
            if (settingsPanel != null)
                settingsPanel.Open();
        }

        public void CloseSettings()
        {
            if (settingsPanel != null)
                settingsPanel.Close();
            if (menuPanel != null)
                menuPanel.SetActive(true);
        }

        public void Quit() => GameQuit.Quit();
    }
}
