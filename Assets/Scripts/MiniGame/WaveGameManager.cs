using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ShortWaves.UI;
using System.Collections;

namespace ShortWaves.MiniGame
{
    public class WaveGameManager : MonoBehaviour
    {
        [Header("Wave Renderers")]
        [SerializeField] private WaveRenderer templateWaveRenderer;
        [SerializeField] private WaveRenderer playerWaveRenderer;

        [Header("UI Controls")]
        [SerializeField] private Slider frequencySlider;
        [SerializeField] private Slider amplitudeSlider;
        [SerializeField] private Button sineButton;
        [SerializeField] private Button squareButton;
        [SerializeField] private GameObject waveformSelector; // Parent containing wave buttons
        [SerializeField] private Button exitButton;

        [Header("UI Display")]
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI matchText;
        [SerializeField] private TextMeshProUGUI instructionText;
        [SerializeField] private TextMeshProUGUI winMessageText;
        [SerializeField] private GameObject winPanel;

        [Header("Game Settings")]
        [SerializeField] private int currentLevel = 1;
        [SerializeField] private float matchThreshold = 0.9f; // 90% match to win
        [SerializeField] private AudioFeedbackGenerator audioFeedback;

        private WaveData templateWave;
        private WaveData playerWave;

        private void Start()
        {
            // Get day from GameLoopManager if available
            if (GameLoopManager.Instance != null)
            {
                currentLevel = GameLoopManager.Instance.CurrentDay;
            }
            
            SetupLevel(currentLevel);
            SetupUI();
            
            // Start audio feedback
            if (audioFeedback != null)
            {
                audioFeedback.StartAudio();
            }
        }

        private void SetupUI()
        {
            // Setup slider listeners
            frequencySlider.onValueChanged.AddListener(OnFrequencyChanged);
            amplitudeSlider.onValueChanged.AddListener(OnAmplitudeChanged);

            // Setup button listeners
            if (sineButton != null)
                sineButton.onClick.AddListener(() => OnWaveTypeChanged(WaveType.Sine));
            if (squareButton != null)
                squareButton.onClick.AddListener(() => OnWaveTypeChanged(WaveType.Square));
            if (exitButton != null)
                exitButton.onClick.AddListener(ExitMinigame);

            // Initialize Sine button as pressed (disabled) and Square as unpressed (enabled)
            UpdateButtonStates(WaveType.Sine);

            UpdatePlayerWave();
        }

        private void SetupLevel(int level)
        {
            // Clamp level for template generation logic, but keep actual level number for display
            int templateLevel = Mathf.Clamp(level, 1, 4);
            
            // Update level display
            if (levelText != null)
                levelText.text = $"Day {level}";

            // Calculate noise strength based on level (1x at level 1, ~20x at level 5)
            // Formula: Base * (1 + (level - 1) * 4.75f)
            float baseNoiseStrength = 0.05f; // Default base strength
            float noiseMultiplier = 1f + (level - 1) * 4.75f;
            float currentNoiseStrength = baseNoiseStrength * noiseMultiplier;

            // Apply noise strength to renderers
            if (templateWaveRenderer != null)
                templateWaveRenderer.HighFreqNoiseStrength = currentNoiseStrength;
            
            if (playerWaveRenderer != null)
                playerWaveRenderer.HighFreqNoiseStrength = currentNoiseStrength;

            // Generate template wave based on level
            switch (templateLevel)
            {
                case 1:
                    // Level 1: Simple sine wave
                    templateWave = new WaveData(2f, 1f, WaveType.Sine, 0f);
                    waveformSelector?.SetActive(false); // Hide waveform selector
                    playerWave = new WaveData(1f, 0.5f, WaveType.Sine, 0f);
                    if (instructionText != null)
                        instructionText.text = "Match the sine wave using Frequency and Amplitude controls. You have to get a match of over 90%";
                    break;

                case 2:
                    // Level 2: Square wave
                    templateWave = new WaveData(3f, 1.2f, WaveType.Square, 0f);
                    waveformSelector?.SetActive(true); // Show waveform selector
                    playerWave = new WaveData(1f, 0.5f, WaveType.Sine, 0f);
                    if (instructionText != null)
                        instructionText.text = "Match the square wave using Waveform, Frequency and Amplitude";
                    break;

                case 3:
                    // Level 3: Complex sine wave
                    templateWave = new WaveData(5f, 0.8f, WaveType.Sine, Mathf.PI / 4);
                    waveformSelector?.SetActive(true);
                    playerWave = new WaveData(1f, 0.5f, WaveType.Sine, 0f);
                    if (instructionText != null)
                        instructionText.text = "Match the wave - try different combinations!";
                    break;

                case 4:
                    // Level 4: Complex square wave
                    templateWave = new WaveData(4.5f, 1.5f, WaveType.Square, Mathf.PI / 6);
                    waveformSelector?.SetActive(true);
                    playerWave = new WaveData(1f, 0.5f, WaveType.Sine, 0f);
                    if (instructionText != null)
                        instructionText.text = "Final challenge - match the complex square wave!";
                    break;

                default:
                    templateWave = new WaveData(2f, 1f, WaveType.Sine, 0f);
                    playerWave = new WaveData(1f, 0.5f, WaveType.Sine, 0f);
                    break;
            }

            // Set slider ranges based on template
            if (frequencySlider != null)
            {
                frequencySlider.minValue = 0.5f;
                frequencySlider.maxValue = 10f;
                frequencySlider.value = playerWave.frequency;
            }

            if (amplitudeSlider != null)
            {
                amplitudeSlider.minValue = 0.1f;
                amplitudeSlider.maxValue = 2f;
                amplitudeSlider.value = playerWave.amplitude;
            }

            // Initial render will happen in Update
        }

        private void OnFrequencyChanged(float value)
        {
            playerWave.frequency = value;
            UpdatePlayerWave();
        }

        private void OnAmplitudeChanged(float value)
        {
            playerWave.amplitude = value;
            UpdatePlayerWave();
        }

        private void OnWaveTypeChanged(WaveType type)
        {
            playerWave.type = type;
            UpdateButtonStates(type);
            UpdatePlayerWave();
        }

        private void UpdateButtonStates(WaveType selectedType)
        {
            // The pressed button becomes non-interactable (appears pressed)
            // The unpressed button becomes interactable (appears normal)
            if (sineButton != null)
                sineButton.interactable = (selectedType != WaveType.Sine);
            
            if (squareButton != null)
                squareButton.interactable = (selectedType != WaveType.Square);
        }

        private void Update()
        {
            // Animate waves with time
            if (templateWaveRenderer != null)
                templateWaveRenderer.Render(templateWave, Time.time);
            
            if (playerWaveRenderer != null)
                playerWaveRenderer.Render(playerWave, Time.time);
        }

        private void UpdatePlayerWave()
        {
            // We don't need to call Render here anymore as Update handles it every frame
            // But we still need to check for a match
            CheckMatch();
        }

        private void CheckMatch()
        {
            float matchScore = CalculateMatchScore();
            
            // Update audio frequency based on match score
            if (audioFeedback != null)
            {
                audioFeedback.UpdateFrequency(matchScore);
            }
            
            if (matchText != null)
            {
                matchText.text = $"Match: {matchScore * 100f:F1}%";
                
                // Color code the match text
                if (matchScore >= matchThreshold)
                    matchText.color = Color.green;
                else if (matchScore >= 0.7f)
                    matchText.color = Color.yellow;
                else
                    matchText.color = Color.red;
            }

            // Check for win condition
            if (matchScore >= matchThreshold)
            {
                OnLevelComplete();
            }
        }

        private float CalculateMatchScore()
        {
            float score = 1f;

            // Check wave type match
            if (playerWave.type != templateWave.type)
                return 0f; // Wrong wave type = 0 match

            // Calculate frequency match (within 10% tolerance)
            float freqDiff = Mathf.Abs(playerWave.frequency - templateWave.frequency);
            float freqTolerance = templateWave.frequency * 0.1f;
            float freqScore = 1f - Mathf.Clamp01(freqDiff / freqTolerance);

            // Calculate amplitude match (within 10% tolerance)
            float ampDiff = Mathf.Abs(playerWave.amplitude - templateWave.amplitude);
            float ampTolerance = templateWave.amplitude * 0.1f;
            float ampScore = 1f - Mathf.Clamp01(ampDiff / ampTolerance);

            // Combined score (weighted equally)
            score = (freqScore + ampScore) / 2f;

            return score;
        }

        private void OnLevelComplete()
        {
            // Stop audio feedback on successful match
            if (audioFeedback != null)
            {
                audioFeedback.StopAudio();
            }
            
            // Hide instruction text
            if (instructionText != null)
                instructionText.gameObject.SetActive(false);

            // Show win panel if assigned
            if (winPanel != null)
                winPanel.SetActive(true);

            // Show win message with console effect
            if (winMessageText != null)
            {
                winMessageText.gameObject.SetActive(true);
                ConsoleEffect consoleEffect = winMessageText.GetComponent<ConsoleEffect>();
                
                // If the component is missing, try to add it or just set text
                if (consoleEffect == null)
                    consoleEffect = winMessageText.gameObject.AddComponent<ConsoleEffect>();

                string winMsg = "SIGNAL LOCKED // UPLINK ESTABLISHED";
                consoleEffect.PlayTypewriter(winMsg, 0.05f, () => {
                    consoleEffect.StartBlinking(0.5f);
                });
            }

            // Fade out signals
            StartCoroutine(FadeOutSignals(1.0f));

            // Record victory in GameLoopManager
            if (GameLoopManager.Instance != null)
            {
                GameLoopManager.Instance.RecordMinigameVictory();
            }

            // Disable controls to prevent further changes
            if (frequencySlider != null) frequencySlider.interactable = false;
            if (amplitudeSlider != null) amplitudeSlider.interactable = false;
            if (waveformSelector != null) waveformSelector.SetActive(false);

            // Auto-advance to next level after delay (increased delay to allow reading message)
            Invoke(nameof(ReturnToNight), 4f);
        }

        private void ReturnToNight()
        {
            if (GameLoopManager.Instance != null)
            {
                GameLoopManager.Instance.LoadScene("Night1");
            }
            else
            {
                Debug.LogWarning("GameLoopManager not found! Reloading current scene for testing.");
                UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            }
        }

        // Public method to reset to specific level
        public void SetLevel(int level)
        {
            CancelInvoke(); // Cancel any pending level transitions
            SetupLevel(Mathf.Clamp(level, 1, 4));
        }

        /// <summary>
        /// Exits the minigame and returns to the previous scene.
        /// TODO: Add logic to handle consequences of not completing the minigame.
        /// </summary>
        public void ExitMinigame()
        {
            CancelInvoke(); // Cancel any pending level transitions
            
            // Stop audio feedback when exiting
            if (audioFeedback != null)
            {
                audioFeedback.StopAudio();
            }

            if (GameLoopManager.Instance != null)
            {
                // Return to the previous scene stored in GameLoopManager
                string previousScene = GameLoopManager.Instance.PreviousSceneName;
                
                if (!string.IsNullOrEmpty(previousScene))
                {
                    Debug.Log($"Exiting minigame, returning to: {previousScene}");
                    GameLoopManager.Instance.LoadScene(previousScene);
                }
                else
                {
                    Debug.LogWarning("No previous scene found, falling back to Night1");
                    GameLoopManager.Instance.LoadScene("Night1");
                }
            }
            else
            {
                Debug.LogWarning("GameLoopManager not found! Cannot exit minigame properly.");
            }
        }
        private IEnumerator FadeOutSignals(float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
                
                if (templateWaveRenderer != null) templateWaveRenderer.SetFade(alpha);
                if (playerWaveRenderer != null) playerWaveRenderer.SetFade(alpha);
                
                yield return null;
            }
            
            if (templateWaveRenderer != null) templateWaveRenderer.SetFade(0f);
            if (playerWaveRenderer != null) playerWaveRenderer.SetFade(0f);
        }
    }
}
