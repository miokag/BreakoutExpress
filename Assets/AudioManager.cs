using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Music Clips")]
    [SerializeField] private AudioClip mainMenuMusic;
    [SerializeField] private AudioClip music2DLevel;
    [SerializeField] private AudioClip music3DLevel;
    [SerializeField] private float musicVolume = 0.7f;
    [SerializeField] private float musicFadeDuration = 1.5f;

    [Header("Ambience Clips")]
    [SerializeField] private AudioClip trainAmbience;
    [SerializeField] private float ambienceVolume = 0.5f;
    [SerializeField] private float ambienceFadeDuration = 1f;

    [Header("Sound Effects")]
    [SerializeField] private float sfxVolume = 0.8f;

    [Header("Mixer Groups")]
    [SerializeField] private AudioMixerGroup musicMixerGroup;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;
    [SerializeField] private AudioMixerGroup ambienceMixerGroup;

    private AudioSource musicSource;
    private AudioSource sfxSource;
    private AudioSource ambienceSource;
    private string currentSceneName;
    private bool isCartScene;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Create audio sources
        musicSource = gameObject.AddComponent<AudioSource>();
        sfxSource = gameObject.AddComponent<AudioSource>();
        ambienceSource = gameObject.AddComponent<AudioSource>();

        // Configure music source
        musicSource.loop = true;
        musicSource.volume = musicVolume;
        musicSource.outputAudioMixerGroup = musicMixerGroup;

        // Configure SFX source
        sfxSource.loop = false;
        sfxSource.volume = sfxVolume;
        sfxSource.outputAudioMixerGroup = sfxMixerGroup;

        // Configure ambience source
        ambienceSource.loop = true;
        ambienceSource.volume = 0f; // Start silent
        ambienceSource.outputAudioMixerGroup = ambienceMixerGroup;

        // Subscribe to scene changes
        SceneManager.activeSceneChanged += OnSceneChanged;
    }

    private void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        SceneManager.activeSceneChanged -= OnSceneChanged;
    }

    private void Start()
    {
        currentSceneName = SceneManager.GetActiveScene().name;
        PlaySceneMusic(currentSceneName);
        CheckForCartScene(currentSceneName);
    }

    private void OnSceneChanged(Scene current, Scene next)
    {
        currentSceneName = next.name;
        PlaySceneMusic(currentSceneName);
        CheckForCartScene(currentSceneName);
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
        if (isCartScene)
        {
            PlayTrainAmbience();
        }
        else
        {
            StopTrainAmbience();
        }
    }

    private void PlayTrainAmbience()
    {
        if (trainAmbience == null)
        {
            Debug.LogWarning("Train ambience clip not assigned!");
            return;
        }

        if (ambienceSource.clip == trainAmbience && ambienceSource.isPlaying)
            return;

        StartCoroutine(FadeAmbience(trainAmbience, ambienceVolume));
    }
    
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
    
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    private void StopTrainAmbience()
    {
        StartCoroutine(FadeAmbience(null, 0f));
    }

    private IEnumerator FadeAmbience(AudioClip newClip, float targetVolume)
    {
        // Fade out current ambience if playing
        if (ambienceSource.isPlaying)
        {
            float startVolume = ambienceSource.volume;
            float timer = 0f;

            while (timer < ambienceFadeDuration)
            {
                timer += Time.deltaTime;
                ambienceSource.volume = Mathf.Lerp(startVolume, 0f, timer / ambienceFadeDuration);
                yield return null;
            }

            ambienceSource.Stop();
        }

        // Play new ambience if provided
        if (newClip != null)
        {
            ambienceSource.clip = newClip;
            ambienceSource.Play();

            float timer2 = 0f;
            while (timer2 < ambienceFadeDuration)
            {
                timer2 += Time.deltaTime;
                ambienceSource.volume = Mathf.Lerp(0f, targetVolume, timer2 / ambienceFadeDuration);
                yield return null;
            }
        }
    }

    private void PlaySceneMusic(string sceneName)
    {
        AudioClip clipToPlay = null;

        if (sceneName.Contains("Menu"))
        {
            clipToPlay = mainMenuMusic;
        }
        else if (sceneName.Contains("2D"))
        {
            clipToPlay = music2DLevel;
        }
        else if (sceneName.Contains("3D"))
        {
            clipToPlay = music3DLevel;
        }

        if (clipToPlay != null && clipToPlay != musicSource.clip)
        {
            StartCoroutine(FadeMusic(clipToPlay));
        }
    }

    public void PlayMainMenuMusic()
    {
        if (mainMenuMusic == null)
        {
            Debug.LogWarning("Main menu music clip not assigned!");
            return;
        }

        if (musicSource.clip == mainMenuMusic && musicSource.isPlaying)
            return;

        StartCoroutine(FadeMusic(mainMenuMusic));
    }

    private IEnumerator FadeMusic(AudioClip newClip)
    {
        // Fade out current music if playing
        if (musicSource.isPlaying)
        {
            float startVolume = musicSource.volume;
            float timer = 0f;

            while (timer < musicFadeDuration)
            {
                timer += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, 0f, timer / musicFadeDuration);
                yield return null;
            }

            musicSource.Stop();
            musicSource.volume = startVolume;
        }

        // Play new music with fade in
        musicSource.clip = newClip;
        musicSource.Play();

        float targetVolume = musicSource.volume;
        musicSource.volume = 0f;
        float timer2 = 0f;

        while (timer2 < musicFadeDuration)
        {
            timer2 += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0f, targetVolume, timer2 / musicFadeDuration);
            yield return null;
        }
    }

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

    public void StopAllMusic()
    {
        musicSource.Stop();
        ambienceSource.Stop();
    }
}