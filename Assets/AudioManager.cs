using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Music Settings")]
    [SerializeField] private AudioClip mainMenuMusic;
    [SerializeField] private AudioClip music2DLevel;
    [SerializeField] private AudioClip music3DLevel;
    [SerializeField] private float musicVolume = 0.7f;
    [SerializeField] private float musicFadeDuration = 1.5f;
    [SerializeField] private AudioMixerGroup musicMixerGroup;

    [Header("Ambience Settings")]
    [SerializeField] private AudioClip trainAmbience;
    [SerializeField] private float ambienceVolume = 0.5f;
    [SerializeField] private float ambienceFadeDuration = 1f;
    [SerializeField] private AudioMixerGroup ambienceMixerGroup;

    [Header("SFX Settings")]
    [SerializeField] private float sfxVolume = 0.8f;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;

    private AudioSource musicSource;
    private AudioSource sfxSource;
    private AudioSource ambienceSource;
    private string currentSceneName;
    private bool isCartScene;

    #region Initialization
    private void Awake()
    {
        InitializeSingleton();
        CreateAudioSources();
        SceneManager.activeSceneChanged += OnSceneChanged;
    }

    private void InitializeSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void CreateAudioSources()
    {
        musicSource = CreateAudioSource("Music Source", musicMixerGroup, true, musicVolume);
        sfxSource = CreateAudioSource("SFX Source", sfxMixerGroup, false, sfxVolume);
        ambienceSource = CreateAudioSource("Ambience Source", ambienceMixerGroup, true, 0f);
    }

    private AudioSource CreateAudioSource(string name, AudioMixerGroup mixerGroup, bool loop, float volume)
    {
        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.name = name;
        source.outputAudioMixerGroup = mixerGroup;
        source.loop = loop;
        source.volume = volume;
        return source;
    }
    #endregion

    #region Scene Management
    private void Start()
    {
        // Force play music when first initialized
        PlaySceneMusic(SceneManager.GetActiveScene().name);
        CheckForCartScene(SceneManager.GetActiveScene().name);
    }

    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnSceneChanged;
    }

    private void OnSceneChanged(Scene current, Scene next)
    {
        // Delay the scene change handling slightly to ensure scene is fully loaded
        StartCoroutine(DelayedSceneChange(next.name));
    }
    
    private IEnumerator DelayedSceneChange(string sceneName)
    {
        // Wait for end of frame to ensure scene is loaded
        yield return new WaitForEndOfFrame();
        HandleSceneChange(sceneName);
    }

    private void HandleSceneChange(string sceneName)
    {
        currentSceneName = sceneName;
        PlaySceneMusic(sceneName);
        CheckForCartScene(sceneName);
    }

    private void CheckForCartScene(string sceneName)
    {
        bool newCartState = sceneName.Contains("Cart");
        if (newCartState != isCartScene)
        {
            isCartScene = newCartState;
            HandleCartSceneChange();
        }
    }

    private void HandleCartSceneChange()
    {
        if (isCartScene) PlayTrainAmbience();
        else StopTrainAmbience();
    }
    #endregion

    #region Music Control
    private void PlaySceneMusic(string sceneName)
    {
        AudioClip clipToPlay = GetMusicClipForScene(sceneName);
        
        // Always play music if clip is found, even if same as current
        if (clipToPlay != null)
        {
            // If same clip is already playing, just ensure it's not faded out
            if (musicSource.clip == clipToPlay && musicSource.isPlaying)
            {
                musicSource.volume = musicVolume;
            }
            else
            {
                StartCoroutine(CrossFadeMusic(clipToPlay));
            }
        }
        else
        {
            // No music for this scene - fade out
            StartCoroutine(FadeAudioSource(musicSource, 0f, musicFadeDuration));
        }
    }

    private AudioClip GetMusicClipForScene(string sceneName)
    {
        if (sceneName.Contains("Menu")) return mainMenuMusic;
        if (sceneName.Contains("2D")) return music2DLevel;
        if (sceneName.Contains("3D")) return music3DLevel;
        return null;
    }

    public void PlayMainMenuMusic()
    {
        if (mainMenuMusic == null)
        {
            Debug.LogWarning("Main menu music clip not assigned!");
            return;
        }
        if (!(musicSource.clip == mainMenuMusic && musicSource.isPlaying))
        {
            StartCoroutine(CrossFadeMusic(mainMenuMusic));
        }
    }

    private IEnumerator CrossFadeMusic(AudioClip newClip)
    {
        yield return FadeAudioSource(musicSource, 0f, musicFadeDuration);
        
        musicSource.Stop();
        musicSource.clip = newClip;
        musicSource.Play();
        
        yield return FadeAudioSource(musicSource, musicVolume, musicFadeDuration);
    }
    #endregion

    #region Ambience Control
    private void PlayTrainAmbience()
    {
        if (trainAmbience == null)
        {
            Debug.LogWarning("Train ambience clip not assigned!");
            return;
        }
        if (!(ambienceSource.clip == trainAmbience && ambienceSource.isPlaying))
        {
            StartCoroutine(FadeAmbience(trainAmbience, ambienceVolume));
        }
    }

    private void StopTrainAmbience() => StartCoroutine(FadeAmbience(null, 0f));

    private IEnumerator FadeAmbience(AudioClip newClip, float targetVolume)
    {
        yield return FadeAudioSource(ambienceSource, 0f, ambienceFadeDuration);
        
        if (newClip != null)
        {
            ambienceSource.clip = newClip;
            ambienceSource.Play();
            yield return FadeAudioSource(ambienceSource, targetVolume, ambienceFadeDuration);
        }
        else
        {
            ambienceSource.Stop();
        }
    }
    #endregion

    #region SFX Control
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }
    #endregion

    #region Volume Control
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume;
    }

    public void SetAmbienceVolume(float volume)
    {
        ambienceVolume = Mathf.Clamp01(volume);
        ambienceSource.volume = ambienceVolume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        sfxSource.volume = sfxVolume;
    }

    public void StopAllAudio()
    {
        musicSource.Stop();
        ambienceSource.Stop();
    }
    #endregion

    #region Helper Methods
    private IEnumerator FadeAudioSource(AudioSource source, float targetVolume, float duration)
    {
        float startVolume = source.volume;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, targetVolume, timer / duration);
            yield return null;
        }
        
        source.volume = targetVolume;
    }
    #endregion
}