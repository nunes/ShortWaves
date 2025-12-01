using UnityEngine;
using Fungus;
using System.Collections;
using TMPro;

/// <summary>
/// Interactable object that triggers the game ending on Day 5.
/// Calculates and displays the ending based on player's choices throughout the game.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class EndingTrigger : MonoBehaviour, IInteractable
{
    [SerializeField] private string promptText = "Interact";
    [SerializeField] private bool onlyAvailableOnFinalDay = true;

    [Header("Fungus Integration")]
    [Tooltip("The Flowchart containing the ending dialogue.")]
    [SerializeField] private Flowchart endingFlowchart;
    [Tooltip("The name of the Block to execute for the ending.")]
    [SerializeField] private string endingBlockName = "EndingDialogue";


    [Header("Effects")]
    [Tooltip("The GameObject containing the Fire FX to enable if the antenna is destroyed.")]
    [SerializeField] private GameObject fireFX;
    [Tooltip("The GameObject containing the Fireworks FX to enable if the antenna is celebrated.")]
    [SerializeField] private GameObject fireworksFX;
    [Tooltip("The UI Panel to display credits.")]
    [SerializeField] private GameObject creditsPanel;
    [Tooltip("The TextMeshProUGUI component to display the ending status.")]
    [SerializeField] private TextMeshProUGUI endingStatusText;

    private string endingMessage = "";

    private bool hasTriggered = false;

    public string InteractionPrompt
    {
        get
        {
            if (hasTriggered) return "";

            if (onlyAvailableOnFinalDay)
            {
                if (GameLoopManager.Instance != null && GameLoopManager.Instance.IsLastDay())
                {
                    return promptText;
                }
                // Return empty to hide prompt if not on final day
                return "";
            }
            return promptText;
        }
    }

    private void Reset()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    public void Interact(PlayerControl player)
    {
        Debug.Log($"EndingTrigger: Interact called. hasTriggered: {hasTriggered}");
        if (hasTriggered) 
        {
            Debug.Log("EndingTrigger: Already triggered, ignoring.");
            return;
        }

        if (player != null)
        {
            // Check if we should trigger ending
            if (onlyAvailableOnFinalDay)
            {
                if (GameLoopManager.Instance != null)
                {
                    if (!GameLoopManager.Instance.IsLastDay())
                    {
                        Debug.Log("Ending trigger is only available on the final day.");
                        return;
                    }
                }
            }

            hasTriggered = true;
            Debug.Log("EndingTrigger: Starting interaction sequence.");
            player.StartInteractionSequence(transform, StartEndingSequence);
        }
    }

    private void StartEndingSequence()
    {
        Debug.Log("EndingTrigger: StartEndingSequence called.");
        if (endingFlowchart != null && endingFlowchart.HasBlock(endingBlockName))
        {
            // Pass game state to Fungus
            if (GameLoopManager.Instance != null)
            {
                int wins = GameLoopManager.Instance.WaveMinigamesBeaten;
                endingFlowchart.SetIntegerVariable("MinigameWins", wins);
                endingFlowchart.SetBooleanVariable("CanHack", wins >= 2);
                Debug.Log($"EndingTrigger: Set Fungus variables - MinigameWins: {wins}, CanHack: {wins >= 2}");
            }

            // Execute the Fungus block
            Debug.Log($"EndingTrigger: Executing Fungus block '{endingBlockName}'.");
            endingFlowchart.ExecuteBlock(endingBlockName);
        }
        else
        {
            Debug.LogError($"EndingTrigger: Flowchart is missing or Block '{endingBlockName}' not found!");
            
            // Fallback to old logic if Fungus fails
            if (GameLoopManager.Instance != null)
            {
                string ending = GameLoopManager.Instance.CalculateEnding();
                Debug.Log($"Ending triggered (Fallback): {ending}");
            }
        }
    }

    // This method should be called by the Fungus Block using the "Call Method" command
    // when the player chooses to destroy the antenna.
    public void OnDestroyAntenna()
    {
        Debug.Log("EndingTrigger: OnDestroyAntenna called by Fungus.");
        endingMessage = "You found Ending 1 of 3";
        ActivateFireFX();
    }

    // This method should be called by the Fungus Block using the "Call Method" command
    // when the player chooses to improve/tune the antenna (The Synthesis).
    public void OnImproveAntenna()
    {
        Debug.Log("EndingTrigger: OnImproveAntenna called by Fungus.");
        endingMessage = "You found Ending 2 of 3";
        // Reusing Fireworks FX for the "Good" ending visual for now
        ActivateFireworksFX();
    }

    // This method should be called by the Fungus Block using the "Call Method" command
    // when the player chooses to leave the antenna alone (The Bliss).
    public void OnLeaveAntenna()
    {
        Debug.Log("EndingTrigger: OnLeaveAntenna called by Fungus.");
        endingMessage = "You found Ending 3 of 3";
        // No specific FX for leaving it, just the credits eventually
    }

    // Legacy method, keeping for compatibility if needed, redirects to Improve
    public void OnCelebrateAntenna()
    {
        OnImproveAntenna();
    }

    // This method should be called by the Fungus Block using the "Call Method" command
    // when the ending sequence is finished to show credits.
    public void ShowCredits()
    {
        Debug.Log("EndingTrigger: ShowCredits called by Fungus.");
        if (creditsPanel != null)
        {
            creditsPanel.SetActive(true);
            if (endingStatusText != null)
            {
                endingStatusText.text = endingMessage;
            }
        }
        else
        {
            Debug.LogWarning("EndingTrigger: Credits Panel is not assigned!");
        }
    }

    private void ActivateFireFX()
    {
        if (fireFX != null)
        {
            fireFX.SetActive(true);
            Debug.Log("EndingTrigger: Fire FX activated!");
        }
        else
        {
            Debug.LogWarning("EndingTrigger: Fire FX GameObject is not assigned!");
        }
    }

    private void ActivateFireworksFX()
    {
        if (fireworksFX != null)
        {
            fireworksFX.SetActive(true);
            Debug.Log("EndingTrigger: Fireworks FX activated!");
        }
        else
        {
            Debug.LogWarning("EndingTrigger: Fireworks FX GameObject is not assigned!");
        }
    }
}
