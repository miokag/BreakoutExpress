using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject fadeCanvasPrefab; // Assign your prefab in inspector
    [SerializeField] private float fadeSpeed = 1f;

    private Canvas fadeCanvas;
    private Image fadeImage;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeFadeSystem();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeFadeSystem()
    {
        // Instantiate the fade canvas
        fadeCanvas = Instantiate(fadeCanvasPrefab, transform).GetComponent<Canvas>();
        fadeImage = fadeCanvas.GetComponentInChildren<Image>();
        
        // Set up canvas properties
        fadeCanvas.sortingOrder = 9999;
        fadeImage.color = new Color(0, 0, 0, 0); 
        fadeCanvas.gameObject.SetActive(false);
    }

    public IEnumerator FadeOut()
    {
        fadeCanvas.gameObject.SetActive(true);
        float alpha = 0f;
        
        while (alpha < 1f)
        {
            alpha += Time.deltaTime * fadeSpeed;
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
    }

    public IEnumerator FadeIn()
    {
        float alpha = 1f;
        
        while (alpha > 0f)
        {
            alpha -= Time.deltaTime * fadeSpeed;
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        
        fadeCanvas.gameObject.SetActive(false);
    }

    public IEnumerator TransitionToScene(string sceneName)
    {
        yield return StartCoroutine(FadeOut());
        SceneManager.LoadScene(sceneName);
        yield return StartCoroutine(FadeIn());
    }
    
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Reset fade state on new scene
        if (fadeCanvas != null)
        {
            fadeCanvas.gameObject.SetActive(false);
            fadeImage.color = new Color(0, 0, 0, 0);
        }
    }

    // Quick static access methods
    public static void FadeOutStatic() => Instance.StartCoroutine(Instance.FadeOut());
    public static void FadeInStatic() => Instance.StartCoroutine(Instance.FadeIn());
    public static void TransitionToSceneStatic(string sceneName) => Instance.StartCoroutine(Instance.TransitionToScene(sceneName));
}