using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class GameLoopManager : MonoBehaviour
{
    public static GameLoopManager Instance { get; private set; }

    [Header("Game State")]
    [SerializeField] private int currentDay = 1;
    [SerializeField] private int waveMinigamesBeaten = 0;
    [SerializeField] private int npcInteractionsCount = 0;
    
    // Track which days had minigame wins
    private HashSet<int> daysWithMinigameWins = new HashSet<int>();
    // Track which days the player refused the radio interaction
    private HashSet<int> daysWithRadioRefusals = new HashSet<int>();

    public int CurrentDay => currentDay;
    public int WaveMinigamesBeaten => waveMinigamesBeaten;
    public int NpcInteractionsCount => npcInteractionsCount;
    public bool IsLoading { get; private set; }
    public string PreviousSceneName { get; private set; }

    private int lastMonologueDay = 0;

    private const int MaxDays = 5;
    private const string TownSceneName = "Town1";
    private const string menuSceneName = "Menu";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        // Use new Input System's Keyboard API for pause menu
        // This works even when Time.timeScale = 0
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            // Only toggle pause if not in the main menu
            if (SceneManager.GetActiveScene().name != "Menu")
            {
                // Delegate to the scene-local PauseMenuController
                if (ShortWaves.UI.PauseMenuController.Instance != null)
                {
                    ShortWaves.UI.PauseMenuController.Instance.TogglePause();
                }
                else
                {
                    // Attempt to find it even if it is inactive (e.g. scene start)
                    // Note: FindObjectOfType<T>(true) includes inactive objects
                    var controller = FindFirstObjectByType<ShortWaves.UI.PauseMenuController>(FindObjectsInactive.Include);
                    if (controller != null)
                    {
                        controller.TogglePause();
                    }
                    else
                    {
                        Debug.LogWarning("GameLoopManager: No PauseMenuController found in scene. Add PauseMenuController to your pause menu.");
                    }
                }
            }
        }
    }



    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"GameLoopManager: Scene loaded '{scene.name}'. Resetting IsLoading to false.");
        IsLoading = false;
        
        // Reset game state when scene loads
        Time.timeScale = 1f;
    }





    public void ReturnToMainMenu()
    {
        ResumeGame(); // Ensure time scale is reset
        LoadScene(menuSceneName);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
    }

    public void QuitGame()
    {
        Debug.Log("GameLoopManager: Quit Game requested.");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void RecordNpcInteraction()
    {
        npcInteractionsCount++;
        Debug.Log($"NPC interaction recorded. Total: {npcInteractionsCount}");
    }

    public void RecordMinigameVictory()
    {
        waveMinigamesBeaten++;
        daysWithMinigameWins.Add(currentDay);
        Debug.Log($"Wave minigame victory recorded for Day {currentDay}. Total beaten: {waveMinigamesBeaten}");
    }
    
    public bool DidWinMinigameOnDay(int day)
    {
        return daysWithMinigameWins.Contains(day);
    }

    public void RecordRadioRefusal()
    {
        daysWithRadioRefusals.Add(currentDay);
        Debug.Log($"Radio refusal recorded for Day {currentDay}.");
    }

    public bool HasRefusedRadioOnDay(int day)
    {
        return daysWithRadioRefusals.Contains(day);
    }

    // Track which NPCs have been interacted with today
    private HashSet<string> _dailyInteractedNpcs = new HashSet<string>();

    public bool HasInteractedWith(string npcId)
    {
        return _dailyInteractedNpcs.Contains(npcId);
    }

    public void MarkInteraction(string npcId)
    {
        if (!_dailyInteractedNpcs.Contains(npcId))
        {
            _dailyInteractedNpcs.Add(npcId);
            Debug.Log($"Marked interaction with NPC '{npcId}' for Day {currentDay}");
        }
    }

    public void AdvanceToNextDay()
    {
        if (currentDay < MaxDays)
        {
            currentDay++;
            _dailyInteractedNpcs.Clear(); // Reset daily interactions
            Debug.Log($"Advanced to Day {currentDay}");
            LoadScene(TownSceneName);
        }
        else
        {
            Debug.Log($"Already on final day (Day {MaxDays}). Cannot advance further.");
        }
    }

    public bool IsLastDay()
    {
        return currentDay >= MaxDays;
    }

    public bool HasPlayedMonologueForDay(int day)
    {
        return lastMonologueDay >= day;
    }

    public void SetMonologuePlayedForDay(int day)
    {
        if (day > lastMonologueDay)
        {
            lastMonologueDay = day;
            Debug.Log($"Monologue recorded for Day {day}");
        }
    }

    public string CalculateEnding()
    {
        string ending;

        if (waveMinigamesBeaten == 0 && npcInteractionsCount == 0)
        {
            ending = "Ending 1: Complete Isolation - You avoided everyone and everything.";
        }
        else if (waveMinigamesBeaten >= 1 && waveMinigamesBeaten <= 2)
        {
            ending = "Ending 2: Tentative Connection - You made some effort to engage with the waves.";
        }
        else if (waveMinigamesBeaten >= 3)
        {
            ending = "Ending 3: Wave Master - You conquered the waves and embraced the challenge.";
        }
        else
        {
            // Fallback for other cases (e.g., only talked to NPCs but no minigames)
            ending = "Ending 1: Complete Isolation - You avoided everyone and everything.";
        }

        Debug.Log($"=== GAME ENDING ===");
        Debug.Log($"Days completed: {currentDay}");
        Debug.Log($"Wave minigames beaten: {waveMinigamesBeaten}");
        Debug.Log($"NPC interactions: {npcInteractionsCount}");
        Debug.Log($"{ending}");
        Debug.Log($"==================");

        return ending;
    }

    public void LoadScene(string sceneName)
    {
        PreviousSceneName = SceneManager.GetActiveScene().name;
        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    private System.Collections.IEnumerator LoadSceneRoutine(string sceneName)
    {
        IsLoading = true;
        yield return null; // Wait for one frame to allow UI updates
        SceneManager.LoadScene(sceneName);
    }

    public void NewGame()
    {
        currentDay = 1;
        waveMinigamesBeaten = 0;
        npcInteractionsCount = 0;
        daysWithMinigameWins.Clear();
        daysWithRadioRefusals.Clear();
        _dailyInteractedNpcs.Clear(); // Reset daily interactions
        Debug.Log("Starting New Game - Day 1");
        LoadScene(TownSceneName);
    }
}
