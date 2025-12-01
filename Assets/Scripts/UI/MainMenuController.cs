using UnityEngine;
using UnityEngine.SceneManagement;

namespace ShortWaves.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("Scene Configuration")]
        [Tooltip("The name of the game scene to load when Start is clicked.")]
        [SerializeField] private string sceneToLoad = "GameScene";

        [Header("UI References")]
        [Tooltip("The Settings Panel GameObject to toggle.")]
        [SerializeField] private GameObject settingsPanel;
        [Tooltip("The Credits Panel GameObject to toggle.")]
        [SerializeField] private GameObject creditsPanel;

        private void Start()
        {
            // Ensure settings panel is closed on start
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }
            // Ensure credits panel is closed on start
            if (creditsPanel != null)
            {
                creditsPanel.SetActive(false);
            }
        }

        public void StartGame()
        {
            if (GameLoopManager.Instance != null)
            {
                GameLoopManager.Instance.NewGame();
            }
            else if (!string.IsNullOrEmpty(sceneToLoad))
            {
                Debug.LogWarning("GameLoopManager not found, falling back to direct scene load.");
                SceneManager.LoadScene(sceneToLoad);
            }
            else
            {
                Debug.LogError("MainMenuController: Scene to load is not specified!");
            }
        }

        public void OpenSettings()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(true);
            }
        }

        public void CloseSettings()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }
        }

        public void OpenCredits()
        {
            if (creditsPanel != null)
            {
                creditsPanel.SetActive(true);
            }
        }

        public void CloseCredits()
        {
            if (creditsPanel != null)
            {
                creditsPanel.SetActive(false);
            }
        }

        public void ExitGame()
        {
            Debug.Log("MainMenuController: Exit Game requested.");
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
