using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class BedInteraction : MonoBehaviour, IInteractable
{
    [SerializeField] private string promptText = "Sleep";
    [SerializeField] private string finalDayPrompt = "Sleep (Final Day)";

    [Header("Fungus Integration")]
    [SerializeField] private Fungus.Flowchart flowchart;
    [SerializeField] private string blockName = "BedConfirm";


    public string InteractionPrompt
    {
        get
        {
            if (GameLoopManager.Instance != null && GameLoopManager.Instance.IsLastDay())
            {
                return finalDayPrompt;
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
        if (player != null)
        {
            player.StartInteractionSequence(transform, () => StartCoroutine(InteractRoutine()));
        }
    }

    private System.Collections.IEnumerator InteractRoutine()
    {
        if (flowchart != null && flowchart.HasBlock(blockName))
        {
            // Execute the block. The block MUST contain a 'Call Method' command 
            // that calls OnConfirm() when the user chooses 'Yes'.
            flowchart.ExecuteBlock(blockName);
        }
        else
        {
            OnConfirm();
        }
        yield return null;
    }

    // This method should be called by the Fungus Block using the "Call Method" command
    public void OnConfirm()
    {
        GoToSleep();
    }

    private void GoToSleep()
    {
        Debug.Log("Going to sleep...");
        
        if (GameLoopManager.Instance != null)
        {
            if (GameLoopManager.Instance.IsLastDay())
            {
                Debug.Log("Already on the final day. Use the ending trigger to see your ending!");
                // Optionally could still allow sleeping but not advance day
                return;
            }

            GameLoopManager.Instance.AdvanceToNextDay();
        }
        else
        {
            Debug.LogWarning("GameLoopManager not found! Cannot advance day.");
        }
    }
}
