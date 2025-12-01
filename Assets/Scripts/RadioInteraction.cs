using UnityEngine;
using UnityEngine.SceneManagement;

public class RadioInteraction : MonoBehaviour, IInteractable
{
    [SerializeField] private string miniGameSceneName = "WaveMiniGame";
    [SerializeField] private string promptText = "Use Radio";

    [Header("Fungus Integration")]
    [SerializeField] private Fungus.Flowchart flowchart;
    [SerializeField] private string blockName = "Radio";


    public string InteractionPrompt
    {
        get
        {
            if (GameLoopManager.Instance != null && GameLoopManager.Instance.HasRefusedRadioOnDay(GameLoopManager.Instance.CurrentDay))
            {
                return "";
            }
            return promptText;
        }
    }

    public void Interact(PlayerControl player)
    {
        if (player != null)
        {
            // Move to the radio and then start the interaction
            player.StartInteractionSequence(transform, () => StartCoroutine(InteractRoutine()));
        }
    }
    private System.Collections.IEnumerator InteractRoutine()
    {
        int currentDay = 1;
        if (GameLoopManager.Instance != null)
        {
            currentDay = GameLoopManager.Instance.CurrentDay;
            if (GameLoopManager.Instance.HasRefusedRadioOnDay(currentDay))
            {
                Debug.Log("Player has already refused the radio today.");
                yield break;
            }

            if (GameLoopManager.Instance.DidWinMinigameOnDay(currentDay))
            {
                string wonBlock = "RadioWon";
                if (flowchart != null && flowchart.HasBlock(wonBlock))
                {
                    flowchart.ExecuteBlock(wonBlock);
                    yield break;
                }
            }
        }

        string targetBlock = GetDaySpecificBlockName();
        if (flowchart != null && flowchart.HasBlock(targetBlock))
        {
            // Execute the block. The block MUST contain a 'Call Method' command 
            // that calls OnConfirm() when the user chooses 'Yes', and OnRefuse() when 'No'.
            flowchart.ExecuteBlock(targetBlock);
        }
        else
        {
            // Fallback if no Fungus block found
            OnConfirm();
        }
        yield return null;
    }

    // This method should be called by the Fungus Block using the "Call Method" command
    public void OnConfirm()
    {
        StartMiniGame();
    }

    // This method should be called by the Fungus Block using the "Call Method" command
    public void OnRefuse()
    {
        Debug.Log("Radio interaction refused.");
        if (GameLoopManager.Instance != null)
        {
            GameLoopManager.Instance.RecordRadioRefusal();
        }
    }

    private string GetDaySpecificBlockName()
    {
        int currentDay = 1;
        if (GameLoopManager.Instance != null)
        {
            currentDay = GameLoopManager.Instance.CurrentDay;
        }
        return $"Day{currentDay}_{blockName}";
    }

    private void StartMiniGame()
    {
        Debug.Log($"Starting Radio Mini Game: {miniGameSceneName}");
        if (GameLoopManager.Instance != null)
        {
            GameLoopManager.Instance.LoadScene(miniGameSceneName);
        }
        else
        {
            SceneManager.LoadScene(miniGameSceneName);
        }
    }
}
