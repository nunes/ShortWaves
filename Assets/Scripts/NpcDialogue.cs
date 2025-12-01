using UnityEngine;
using Fungus;

public class NpcDialogue : MonoBehaviour, IInteractable
{
    [Tooltip("The Flowchart containing the dialogue block.")]
    [SerializeField]
    private Flowchart flowchart;

    [Tooltip("The base name of the Block to execute (will be prefixed with Day number, e.g., Day1_BlockName).")]
    [SerializeField]
    private string blockName = "New Block";

    [Tooltip("The action text shown to the player (e.g., 'Talk (E)', 'Talk to Sara (E)').")]
    [SerializeField]
    private string actionDialogue = "Talk (E)";

    private void Start()
    {
        if (GameLoopManager.Instance != null && GameLoopManager.Instance.IsLastDay())
        {
            gameObject.SetActive(false);
        }
    }

    public string InteractionPrompt
    {
        get
        {
            if (GetValidBlockName() != null)
            {
                return actionDialogue;
            }
            return "";
        }
    }

    public void Interact(PlayerControl player)
    {
        string targetBlock = GetValidBlockName();

        if (!string.IsNullOrEmpty(targetBlock))
        {
            if (player != null)
            {
                // Record NPC interaction
                if (GameLoopManager.Instance != null)
                {
                    GameLoopManager.Instance.RecordNpcInteraction();
                    
                    // Mark this specific NPC as interacted with for the day
                    // We mark it regardless of which block we played, to ensure consistency
                    // But strictly speaking, if we played the "Day" block, we definitely want to mark it.
                    GameLoopManager.Instance.MarkInteraction(blockName);
                }

                player.StartInteractionSequence(transform, () => flowchart.ExecuteBlock(targetBlock));
            }
            else
            {
                Debug.LogError("PlayerControl is null in NpcDialogue.Interact");
            }
        }
        // If neither block exists, do nothing (no error) as requested
    }

    private string GetValidBlockName()
    {
        if (flowchart == null || string.IsNullOrEmpty(blockName))
        {
            return null;
        }

        // Check if we have already interacted with this NPC today
        bool alreadyInteracted = false;
        if (GameLoopManager.Instance != null)
        {
            alreadyInteracted = GameLoopManager.Instance.HasInteractedWith(blockName);
        }

        // If already interacted, we prefer the base block (generic/repeat dialogue)
        if (alreadyInteracted)
        {
            if (flowchart.HasBlock(blockName))
            {
                return blockName;
            }
            // If base block missing, maybe fall back to day block? Or just nothing?
            // User request: "interact with the default interaction without day"
            // So if that's missing, we probably shouldn't replay the day one if we want to avoid repetition.
            // But let's see if the day one is available as a last resort? 
            // Actually, if they want to avoid repeating the day dialogue, we should NOT return the day block here.
            return null; 
        }

        // If NOT interacted yet, try the day-specific block first
        string daySpecificBlock = GetDaySpecificBlockName();

        if (flowchart.HasBlock(daySpecificBlock))
        {
            return daySpecificBlock;
        }
        
        // Fallback: If day specific block is missing, use the base block
        if (flowchart.HasBlock(blockName))
        {
            return blockName;
        }

        return null;
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
}
