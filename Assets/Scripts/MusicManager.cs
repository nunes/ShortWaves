using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    [System.Serializable]
    public struct SceneMusic
    {
        public string sceneName;
        public AudioClip musicClip;
    }

    public static MusicManager Instance;

    [Header("Configuration")]
    [SerializeField] private List<SceneMusic> sceneMusicList;
    [SerializeField] private float fadeDuration = 1.0f;
    [SerializeField] private float maxVolume = 0.5f;
    [SerializeField] private UnityEngine.Audio.AudioMixerGroup outputMixerGroup;

    private AudioSource _currentAudioSource;
    private AudioSource _nextAudioSource;
    private Dictionary<string, AudioClip> _sceneMusicMap;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Initialize()
    {
        _sceneMusicMap = new Dictionary<string, AudioClip>();
        foreach (var sm in sceneMusicList)
        {
            if (!string.IsNullOrEmpty(sm.sceneName) && sm.musicClip != null)
            {
                _sceneMusicMap[sm.sceneName] = sm.musicClip;
            }
        }

        // Setup AudioSources
        _currentAudioSource = gameObject.AddComponent<AudioSource>();
        _nextAudioSource = gameObject.AddComponent<AudioSource>();

        _currentAudioSource.loop = true;
        _nextAudioSource.loop = true;
        _currentAudioSource.volume = 0;
        _nextAudioSource.volume = 0;

        if (outputMixerGroup != null)
        {
            _currentAudioSource.outputAudioMixerGroup = outputMixerGroup;
            _nextAudioSource.outputAudioMixerGroup = outputMixerGroup;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicForScene(scene.name);
    }

    public void PlayMusicForScene(string sceneName)
    {
        if (_sceneMusicMap.ContainsKey(sceneName))
        {
            AudioClip newClip = _sceneMusicMap[sceneName];
            if (_currentAudioSource.clip != newClip)
            {
                StartCoroutine(CrossFadeMusic(newClip));
            }
        }
        else
        {
            // Optional: Fade out if no music defined for this scene
            // StartCoroutine(FadeOutCurrent());
        }
    }

    private IEnumerator CrossFadeMusic(AudioClip newClip)
    {
        _nextAudioSource.clip = newClip;
        _nextAudioSource.Play();
        _nextAudioSource.volume = 0;

        float timer = 0;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / fadeDuration;

            _currentAudioSource.volume = Mathf.Lerp(maxVolume, 0, t);
            _nextAudioSource.volume = Mathf.Lerp(0, maxVolume, t);

            yield return null;
        }

        _currentAudioSource.Stop();
        _currentAudioSource.volume = 0;
        _nextAudioSource.volume = maxVolume;

        // Swap sources
        AudioSource temp = _currentAudioSource;
        _currentAudioSource = _nextAudioSource;
        _nextAudioSource = temp;
    }
}
