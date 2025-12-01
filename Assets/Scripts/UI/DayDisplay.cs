using UnityEngine;
using TMPro;

namespace ShortWaves.UI
{
    /// <summary>
    /// Displays the current day from GameLoopManager.
    /// Place this on a UI Text element in scenes where you want to show the day.
    /// </summary>
    public class DayDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI dayText;
        [SerializeField] private string textFormat = "Day {0}";
        [SerializeField] private bool updateEveryFrame = false;

        private void Start()
        {
            // Find the text component if not assigned
            if (dayText == null)
            {
                dayText = GetComponent<TextMeshProUGUI>();
            }

            UpdateDayDisplay();
        }

        private void Update()
        {
            if (updateEveryFrame)
            {
                UpdateDayDisplay();
            }
        }

        private void UpdateDayDisplay()
        {
            if (dayText == null)
            {
                Debug.LogWarning("DayDisplay: TextMeshProUGUI component not assigned!");
                return;
            }

            if (GameLoopManager.Instance != null)
            {
                int currentDay = GameLoopManager.Instance.CurrentDay;
                dayText.text = string.Format(textFormat, currentDay);
            }
            else
            {
                dayText.text = "Day ?";
                Debug.LogWarning("DayDisplay: GameLoopManager instance not found!");
            }
        }

        /// <summary>
        /// Call this method to manually refresh the display.
        /// Useful if you want to update it when you know the day has changed.
        /// </summary>
        public void RefreshDisplay()
        {
            UpdateDayDisplay();
        }
    }
}
