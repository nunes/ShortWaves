using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BoxCollider))]
public class SceneTransitionInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string targetSceneName;
    [SerializeField] private string promptText = "Enter";
    [Header("Fungus Integration")]
    [SerializeField] private Fungus.Flowchart flowchart;
    [SerializeField] private string blockName = "LeaveConfirm";

    [SerializeField] private bool requiresConfirmation = false;

    public string InteractionPrompt => promptText;

    private void Reset()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    public void Interact(PlayerControl player)
    {
        if (player != null)
        {
            // Move to the object and then transition
            player.StartInteractionSequence(transform, () => StartCoroutine(InteractRoutine()));
        }
    }

    private System.Collections.IEnumerator InteractRoutine()
    {
        if (requiresConfirmation && flowchart != null && flowchart.HasBlock(blockName))
        {
            // Execute the block. The block MUST contain a 'Call Method' command 
            // that calls OnConfirm() when the user chooses 'Yes'.
            flowchart.ExecuteBlock(blockName);
        }
        else
        {
            // No confirmation needed, proceed immediately
            OnConfirm();
        }
        yield return null;
    }

    // This method should be called by the Fungus Block using the "Call Method" command
    public void OnConfirm()
    {
        TransitionToScene();
    }

    private void TransitionToScene()
    {
        Debug.Log($"Transitioning to scene: {targetSceneName}");
        
        if (GameLoopManager.Instance != null)
        {
            GameLoopManager.Instance.LoadScene(targetSceneName);
        }
        else
        {
            // Fallback if GameLoopManager is not present
            SceneManager.LoadScene(targetSceneName);
        }
    }
}
