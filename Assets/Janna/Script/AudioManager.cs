using UnityEngine;
using UnityEngine.SceneManagement;

// JUST ADD: AudioManager audiomanager; into other scripts to access this
// ALSO ADD TO VOID AWAKE: audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>(); --- make sure audiomanager is tagged as Audio
// TO CALL SFX: audioManager.PlaySFX(audioManager.walking)

public class AudioManager : MonoBehaviour
{

[Header("---------- Audio Source ----------")]
    [SerializeField] AudioSource firstMusicSource;
    [SerializeField] AudioSource secondMusicSource;
    [SerializeField] AudioSource SFXSource;


[Header("---------- Audio Clip ----------")]
    public AudioClip mainMenuBGM;
    public AudioClip firstCarBGM;
    public AudioClip secondCarBGM;
    public AudioClip thirdCarBGM;
    public AudioClip trainBackground;
    public AudioClip walking;
    public AudioClip running;
    public AudioClip landing;
    public AudioClip interacting;
    public AudioClip trainDoor;
    public AudioClip failKeyCard;
    public AudioClip successKeyCard;
    public AudioClip typing;

    [Header("---------- Volume Control ----------")]
    [Range(0f, 1f)] public float firstmusicVolume = 1f;
    [Range(0f, 1f)] public float secondMusicVolume = 1f;
    [Range(0f, 1f)] public float SFXVolume = 1f;


// Singleton pattern to ensure only one AudioManager exists
    public static AudioManager instance;

    private void Awake()
    {
        // Implement singleton pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // initial music
        firstMusicSource.clip = trainBackground;
        firstMusicSource.volume = firstmusicVolume;
        firstMusicSource.Play();
    }

    private void Start()
    {
        // initial music based on starting scene
        SetSceneMusic(SceneManager.GetActiveScene().name);
        
        // scene change events
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SetSceneMusic(scene.name);
    }

    private void SetSceneMusic(string sceneName)
    {
        AudioClip newClip = null;

        // determine which music to play based on scene name
        switch (sceneName)
        {
            case "Janna": // Assuming this is your main menu
                newClip = mainMenuBGM;
                break;
            case "Jye": // Assuming this is your first car
                newClip = firstCarBGM;
                break;
            case "sxftrix": // Assuming this is your second car
                newClip = secondCarBGM;
                break;
            case "Prince": // Assuming this is your third car
                newClip = thirdCarBGM;
                break;
        }

        // if we found a matching clip and it's different from current, change it
        if (newClip != null && secondMusicSource.clip != newClip)
        {
            secondMusicSource.Stop();
            secondMusicSource.clip = newClip;
            secondMusicSource.volume = secondMusicVolume;
            secondMusicSource.loop = true;
            secondMusicSource.Play();
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        SFXSource.PlayOneShot(clip);
    }

    private void OnDestroy()
    {
        // unsubscribe from events when destroyed to prevent memory leaks
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}