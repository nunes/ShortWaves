using UnityEngine;
using UnityEngine.UI;
using Fungus;

namespace ShortWaves.UI
{
    public class SettingsMenuController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Toggle musicToggle;
        [SerializeField] private Toggle sfxToggle;

        [Header("Audio Configuration")]
        [Tooltip("Name of the exposed Music Volume parameter in the AudioMixer. Ensure this is exposed in the FungusAudioMixer.")]
        [SerializeField] private string musicVolumeParam = "MusicVolume";
        [Tooltip("Name of the exposed SFX Volume parameter in the AudioMixer. Ensure this is exposed in the FungusAudioMixer.")]
        [SerializeField] private string sfxVolumeParam = "SFXVolume";

        private const string MusicKey = "MusicEnabled";
        private const string SFXKey = "SFXEnabled";

        private void Start()
        {
            // Initialize toggles based on saved preferences
            if (musicToggle != null)
            {
                bool musicOn = PlayerPrefs.GetInt(MusicKey, 1) == 1;
                musicToggle.isOn = musicOn;
                // Apply initial state
                OnMusicToggle(musicOn);
                
                musicToggle.onValueChanged.AddListener(OnMusicToggle);
            }

            if (sfxToggle != null)
            {
                bool sfxOn = PlayerPrefs.GetInt(SFXKey, 1) == 1;
                sfxToggle.isOn = sfxOn;
                // Apply initial state
                OnSFXToggle(sfxOn);

                sfxToggle.onValueChanged.AddListener(OnSFXToggle);
            }
        }

        private void OnMusicToggle(bool isOn)
        {
            // Save preference
            PlayerPrefs.SetInt(MusicKey, isOn ? 1 : 0);
            PlayerPrefs.Save();

            // Method 1: Use Fungus MusicManager (Best for Music)
            if (FungusManager.Instance != null && FungusManager.Instance.MusicManager != null)
            {
                // Fade to 1 or 0 over 0.5 seconds
                FungusManager.Instance.MusicManager.SetAudioVolume(isOn ? 1f : 0f, 0.5f, null);
            }

            // Method 2: Try to set Mixer parameter (Backup/Global)
            SetMixerVolume(musicVolumeParam, isOn);
        }

        private void OnSFXToggle(bool isOn)
        {
            // Save preference
            PlayerPrefs.SetInt(SFXKey, isOn ? 1 : 0);
            PlayerPrefs.Save();

            SetMixerVolume(sfxVolumeParam, isOn);
        }

        private void SetMixerVolume(string paramName, bool isOn)
        {
            if (FungusManager.Instance != null && FungusManager.Instance.MainAudioMixer != null)
            {
                var mixer = FungusManager.Instance.MainAudioMixer.Mixer;
                if (mixer != null)
                {
                    float volume = isOn ? 0f : -80f; // 0dB for On, -80dB for Off
                    // Note: This requires the parameter to be exposed in the AudioMixer with the exact name.
                    mixer.SetFloat(paramName, volume);
                }
            }
        }
    }
}
