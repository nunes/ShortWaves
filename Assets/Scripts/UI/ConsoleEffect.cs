using UnityEngine;
using TMPro;
using System.Collections;

namespace ShortWaves.UI
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class ConsoleEffect : MonoBehaviour
    {
        private TextMeshProUGUI textComponent;
        private Coroutine currentRoutine;
        private bool isBlinking = false;

        private void Awake()
        {
            textComponent = GetComponent<TextMeshProUGUI>();
        }

        public void PlayTypewriter(string text, float speed = 0.05f, System.Action onComplete = null)
        {
            if (currentRoutine != null) StopCoroutine(currentRoutine);
            currentRoutine = StartCoroutine(TypewriterRoutine(text, speed, onComplete));
        }

        private IEnumerator TypewriterRoutine(string text, float speed, System.Action onComplete)
        {
            textComponent.text = "";
            foreach (char c in text)
            {
                textComponent.text += c;
                yield return new WaitForSeconds(speed);
            }
            
            currentRoutine = null;
            onComplete?.Invoke();
        }

        public void StartBlinking(float interval = 0.5f)
        {
            StopBlinking();
            isBlinking = true;
            currentRoutine = StartCoroutine(BlinkRoutine(interval));
        }

        public void StopBlinking()
        {
            isBlinking = false;
            if (currentRoutine != null)
            {
                StopCoroutine(currentRoutine);
                currentRoutine = null;
            }
            // Ensure text is visible when stopping
            if (textComponent != null)
                textComponent.alpha = 1f;
        }

        private IEnumerator BlinkRoutine(float interval)
        {
            while (isBlinking)
            {
                textComponent.alpha = (textComponent.alpha > 0.1f) ? 0f : 1f;
                yield return new WaitForSeconds(interval);
            }
        }
        
        private void OnDisable()
        {
            StopBlinking();
        }
    }
}
