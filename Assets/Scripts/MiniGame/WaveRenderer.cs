using UnityEngine;

namespace ShortWaves.MiniGame
{
    [RequireComponent(typeof(LineRenderer))]
    public class WaveRenderer : MonoBehaviour
    {
        [SerializeField] private int points = 100;
        [SerializeField] private float width = 10f; // Width of the wave display in world units

        [Header("Noise Settings")]
        [SerializeField] private float noiseScale = 1f;
        [SerializeField] private float noiseSpeed = 1f;
        [SerializeField] private float noiseStrength = 0.1f;
        [SerializeField] private float noiseOffset = 0f;

        [Header("High Frequency Noise")]
        [SerializeField] private float highFreqNoiseScale = 20f;
        [SerializeField] private float highFreqNoiseSpeed = 15f;
        [SerializeField] private float highFreqNoiseStrength = 0.05f;

        public float HighFreqNoiseStrength
        {
            get => highFreqNoiseStrength;
            set => highFreqNoiseStrength = value;
        }

        private LineRenderer lineRenderer;

        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.positionCount = points;
            lineRenderer.useWorldSpace = false; // Local space for easier positioning
            
            // Randomize offset if not set, to make waves look different
            if (noiseOffset == 0f) noiseOffset = Random.Range(0f, 100f);
        }

        public void Render(WaveData data, float time = 0f)
        {
            if (lineRenderer == null) lineRenderer = GetComponent<LineRenderer>();

            Vector3[] positions = new Vector3[points];
            float step = width / (points - 1);
            float startX = -width / 2f;

            for (int i = 0; i < points; i++)
            {
                float x = startX + i * step;
                float y = 0f;

                // t goes from 0 to 1 across the width
                float t = (float)i / (points - 1); 
                float angle = t * data.frequency * 2f * Mathf.PI + data.phase;

                switch (data.type)
                {
                    case WaveType.Sine:
                        y = Mathf.Sin(angle) * data.amplitude;
                        break;
                    case WaveType.Square:
                        // Using Sign for square wave. 
                        // Mathf.Sign returns 1 or -1.
                        y = Mathf.Sign(Mathf.Sin(angle)) * data.amplitude;
                        break;
                }

                // Add low frequency "wobble" noise
                // We use x coordinate for spatial noise, and time for temporal variation
                float wobble = Mathf.PerlinNoise(x * noiseScale + time * noiseSpeed + noiseOffset, noiseOffset) * noiseStrength;
                
                // Add high frequency "interference" noise
                // Using a different offset/scale for the high frequency component
                float interference = Mathf.PerlinNoise(x * highFreqNoiseScale - time * highFreqNoiseSpeed + noiseOffset + 50f, noiseOffset + 50f) * highFreqNoiseStrength;
                
                // Combine signal with noise
                y += wobble + interference;

                positions[i] = new Vector3(x, y, 0f);
            }

            lineRenderer.SetPositions(positions);
        }
        private float initialWidthMultiplier = -1f;

        public void SetFade(float fadeValue)
        {
            if (lineRenderer == null) lineRenderer = GetComponent<LineRenderer>();
            
            // Store initial width if not set
            if (initialWidthMultiplier < 0f)
            {
                initialWidthMultiplier = lineRenderer.widthMultiplier;
            }

            // Fade width
            lineRenderer.widthMultiplier = initialWidthMultiplier * fadeValue;

            // Also try to fade alpha (best effort)
            Gradient gradient = lineRenderer.colorGradient;
            GradientAlphaKey[] alphaKeys = gradient.alphaKeys;
            for (int i = 0; i < alphaKeys.Length; i++)
            {
                alphaKeys[i].alpha = fadeValue;
            }
            gradient.alphaKeys = alphaKeys;
            lineRenderer.colorGradient = gradient;
        }
    }
}
