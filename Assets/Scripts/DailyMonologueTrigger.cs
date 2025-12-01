using UnityEngine;
using Fungus;

public class DailyMonologueTrigger : MonoBehaviour
{
    [Tooltip("The Flowchart containing the monologue blocks. If null, will attempt to find one automatically.")]
    [SerializeField]
    private Flowchart flowchart;

    [Tooltip("The suffix for the block name (will be prefixed with Day number, e.g., Day1_AlexIntro).")]
    [SerializeField]
    private string blockNameSuffix = "AlexIntro";

    private void Start()
    {
        // 1. Get GameLoopManager and Current Day
        if (GameLoopManager.Instance == null)
        {
            Debug.LogWarning("DailyMonologueTrigger: GameLoopManager instance not found. Cannot determine day.");
            return;
        }

        int currentDay = GameLoopManager.Instance.CurrentDay;

        // 2. Check if already played
        if (GameLoopManager.Instance.HasPlayedMonologueForDay(currentDay))
        {
            Debug.Log($"DailyMonologueTrigger: Monologue for Day {currentDay} already played. Skipping.");
            return;
        }

        // 3. Find Flowchart if needed
        if (flowchart == null)
        {
            // Try to find by name "Flowchart" first
            GameObject flowchartObj = GameObject.Find("Flowchart");
            if (flowchartObj != null)
            {
                flowchart = flowchartObj.GetComponent<Flowchart>();
            }

            // Fallback to finding any Flowchart
            if (flowchart == null)
            {
                flowchart = FindFirstObjectByType<Flowchart>();
            }

            if (flowchart == null)
            {
                Debug.LogError("DailyMonologueTrigger: No Flowchart found in scene.");
                return;
            }
        }

        // 4. Construct Block Name
        string blockName = $"Day{currentDay}_{blockNameSuffix}";

        // 5. Set Fungus variable for previous night's minigame result
        int previousDay = currentDay - 1;
        bool wonLastNight = previousDay > 0 && GameLoopManager.Instance.DidWinMinigameOnDay(previousDay);
        flowchart.SetBooleanVariable("RadioMiniGameWon", wonLastNight);
        Debug.Log($"DailyMonologueTrigger: Set RadioMiniGameWon = {wonLastNight} (previous day: {previousDay})");

        // 6. Execute Block
        if (flowchart.HasBlock(blockName))
        {
            Debug.Log($"DailyMonologueTrigger: Executing block '{blockName}'");
            flowchart.ExecuteBlock(blockName);
            
            // 7. Mark as played
            GameLoopManager.Instance.SetMonologuePlayedForDay(currentDay);
        }
        else
        {
            Debug.LogWarning($"DailyMonologueTrigger: Block '{blockName}' not found in Flowchart.");
        }
    }
}
