using UnityEngine;
using UnityEngine.SceneManagement;

namespace ShortWaves.UI
{
    /// <summary>
    /// Handles pause menu UI interactions. This script should be attached to the Pause Menu Canvas GameObject
    /// in each scene. It works together with GameLoopManager to handle pause functionality.
    /// </summary>
    public class PauseMenuController : MonoBehaviour
    {
        public static PauseMenuController Instance { get; private set; }

        [Header("UI References")]
        [Tooltip("The panel/canvas that contains the pause menu UI")]
        [SerializeField] private GameObject pauseMenuPanel;

        private bool isPaused = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                // IMPORTANT: We must persist the ROOT object (the Canvas), not just this panel.
                // If this script is on a child object, DontDestroyOnLoad(gameObject) would detach it from the Canvas.
                DontDestroyOnLoad(transform.root.gameObject);

                // Ensure pause menu is hidden on start
                if (pauseMenuPanel != null)
                {
                    pauseMenuPanel.SetActive(false);
                }
                else
                {
                    // If not set, assume this GameObject is the pause menu panel
                    pauseMenuPanel = gameObject;
                    pauseMenuPanel.SetActive(false);
                }
                
                // Validate Canvas
                Canvas canvas = GetComponentInParent<Canvas>();
                if (canvas == null)
                {
                    Debug.LogError("PauseMenuController: No Canvas found in parent hierarchy! The Pause Menu will not be visible.");
                }
                else
                {
                    // Optional: Ensure sorting order is high enough
                    canvas.sortingOrder = 999; 
                }
            }
            else
            {
                // If we are a duplicate, destroy the ROOT object of this duplicate, not just the script holder.
                // This prevents having multiple Canvases.
                Destroy(transform.root.gameObject);
            }
        }

        private void OnDestroy()
        {
            // Only clear if this was the singleton instance
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// Toggles the pause state (called by GameLoopManager when Escape is pressed)
        /// </summary>
        public void TogglePause()
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }

        /// <summary>
        /// Pauses the game
        /// </summary>
        public void Pause()
        {
            isPaused = true;
            
            if (pauseMenuPanel == null)
            {
                pauseMenuPanel = gameObject;
            }
            
            if (pauseMenuPanel != null)
            {
                pauseMenuPanel.SetActive(true);
            }
            Time.timeScale = 0f;
        }

        /// <summary>
        /// Resumes the game (can be called by Resume button)
        /// </summary>
        public void Resume()
        {
            isPaused = false;
            if (pauseMenuPanel != null)
            {
                pauseMenuPanel.SetActive(false);
            }
            Time.timeScale = 1f;
        }

        /// <summary>
        /// Returns to main menu (can be called by Main Menu button)
        /// </summary>
        public void ReturnToMainMenu()
        {
            Resume(); // Ensure time scale is reset
            if (GameLoopManager.Instance != null)
            {
                GameLoopManager.Instance.ReturnToMainMenu();
            }
            else
            {
                SceneManager.LoadScene("Menu");
            }
        }

        /// <summary>
        /// Quits the game (can be called by Quit button)
        /// </summary>
        public void QuitGame()
        {
            Debug.Log("PauseMenuController: Quit Game requested.");
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        public bool IsPaused => isPaused;
    }
}
