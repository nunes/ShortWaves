using UnityEngine;
using System.Collections;

namespace ShortWaves.MiniGame
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioFeedbackGenerator : MonoBehaviour
    {
        [Header("Audio Settings")]
        [SerializeField] private float noiseVolume = 0.1f;
        [SerializeField] private float sineVolume = 0.3f;
        [SerializeField] private float lowerOctaveVolume = 0.2f; // Volume of the octave-below sine wave
        [SerializeField] private float lowerOctaveMultiplier = 0.5f; // Frequency multiplier for lower tone
        [SerializeField] private float higherOctaveVolume = 0.0f; // Volume of the octave-above sine wave
        [SerializeField] private float higherOctaveMultiplier = 2.0f; // Frequency multiplier for higher tone
        [SerializeField] private float fadeOutDuration = 0.5f; // Duration of fade out in seconds
        
        [Header("Frequency Range")]
        [SerializeField] private float minFrequency = 200f; // Low frequency when far from match
        [SerializeField] private float maxFrequency = 700f; // High frequency when close to match
        
        [Header("Wobbliness Settings")]
        [SerializeField] private float minWobbleFrequency = 2f; // Min LFO frequency for wobble effect (Hz)
        [SerializeField] private float maxWobbleFrequency = 10f; // Max LFO frequency for wobble effect (Hz)
        [SerializeField] private float wobbleDepth = 0.02f; // How much the frequency varies (as percentage of current frequency)
        
        private AudioSource audioSource;
        private float currentFrequency;
        private float currentWobbleFrequency;
        private float phase;
        private float lowerOctavePhase; // Phase for the lower octave
        private float higherOctavePhase; // Phase for the higher octave
        private float wobblePhase; // Phase for the LFO wobble
        private bool isPlaying;
        private int sampleRate;
        private System.Random random;
        private float volumeMultiplier = 1f; // Used for fading
        private Coroutine fadeCoroutine;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            audioSource.loop = true;
            audioSource.playOnAwake = false;
            sampleRate = AudioSettings.outputSampleRate;
            random = new System.Random();
            currentFrequency = minFrequency;
            currentWobbleFrequency = minWobbleFrequency;
        }

        /// <summary>
        /// Starts playing the audio feedback.
        /// </summary>
        public void StartAudio()
        {
            // Stop any ongoing fade
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
                fadeCoroutine = null;
            }
            
            if (!isPlaying)
            {
                isPlaying = true;
                phase = 0f;
                lowerOctavePhase = 0f;
                higherOctavePhase = 0f;
                wobblePhase = 0f;
                volumeMultiplier = 1f;
                audioSource.Play();
            }
        }

        /// <summary>
        /// Stops playing the audio feedback with a fade out.
        /// </summary>
        public void StopAudio()
        {
            if (isPlaying && fadeCoroutine == null)
            {
                fadeCoroutine = StartCoroutine(FadeOutAndStop());
            }
        }
        
        /// <summary>
        /// Coroutine that fades out the audio over the configured duration.
        /// </summary>
        private IEnumerator FadeOutAndStop()
        {
            float elapsed = 0f;
            
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                volumeMultiplier = 1f - (elapsed / fadeOutDuration);
                yield return null;
            }
            
            // Ensure we end at 0
            volumeMultiplier = 0f;
            isPlaying = false;
            audioSource.Stop();
            fadeCoroutine = null;
        }

        /// <summary>
        /// Updates the sine wave frequency based on match score.
        /// </summary>
        /// <param name="matchScore">Match score from 0 to 1</param>
        public void UpdateFrequency(float matchScore)
        {
            // Map match score to frequency range
            // 0 = minFrequency, 1 = maxFrequency
            currentFrequency = Mathf.Lerp(minFrequency, maxFrequency, matchScore);
            currentWobbleFrequency = Mathf.Lerp(minWobbleFrequency, maxWobbleFrequency, matchScore);
        }

        private void OnAudioFilterRead(float[] data, int channels)
        {
            if (!isPlaying)
            {
                // Fill with silence
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = 0f;
                }
                return;
            }

            // Calculate the increment for the sine wave phase
            // Apply wobble using a low-frequency oscillator (LFO)
            float wobble = Mathf.Sin(wobblePhase) * wobbleDepth;
            float wobbledFrequency = currentFrequency * (1f + wobble);
            
            float increment = wobbledFrequency * 2f * Mathf.PI / sampleRate;
            float lowerOctaveIncrement = (wobbledFrequency * lowerOctaveMultiplier) * 2f * Mathf.PI / sampleRate;
            float higherOctaveIncrement = (wobbledFrequency * higherOctaveMultiplier) * 2f * Mathf.PI / sampleRate;
            float wobbleIncrement = currentWobbleFrequency * 2f * Mathf.PI / sampleRate;

            for (int i = 0; i < data.Length; i += channels)
            {
                // Generate noise (-1 to 1)
                float noise = ((float)random.NextDouble() * 2f - 1f) * noiseVolume;

                // Generate primary sine wave
                float sine = Mathf.Sin(phase) * sineVolume;
                
                // Generate lower octave sine wave
                float lowerOctave = Mathf.Sin(lowerOctavePhase) * lowerOctaveVolume;

                // Generate higher octave sine wave
                float higherOctave = Mathf.Sin(higherOctavePhase) * higherOctaveVolume;

                // Combine all audio sources and apply volume multiplier for fade-out
                float sample = (noise + sine + lowerOctave + higherOctave) * volumeMultiplier;

                // Apply to all channels
                for (int c = 0; c < channels; c++)
                {
                    data[i + c] = sample;
                }

                // Increment phases for next sample
                phase += increment;
                lowerOctavePhase += lowerOctaveIncrement;
                higherOctavePhase += higherOctaveIncrement;
                wobblePhase += wobbleIncrement;
                
                // Keep phases in reasonable range to prevent floating point errors
                if (phase > 2f * Mathf.PI)
                {
                    phase -= 2f * Mathf.PI;
                }
                if (lowerOctavePhase > 2f * Mathf.PI)
                {
                    lowerOctavePhase -= 2f * Mathf.PI;
                }
                if (higherOctavePhase > 2f * Mathf.PI)
                {
                    higherOctavePhase -= 2f * Mathf.PI;
                }
                if (wobblePhase > 2f * Mathf.PI)
                {
                    wobblePhase -= 2f * Mathf.PI;
                }
            }
        }
    }
}
