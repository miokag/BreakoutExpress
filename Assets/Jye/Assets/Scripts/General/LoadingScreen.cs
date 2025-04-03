using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private TMP_Text loadingText;
    [SerializeField] private float typingSpeed = 0.1f;
    
    private string loadingString = "Loading";
    private AsyncOperation loadingOperation;
    private bool isLoading = true;
    private bool hasFadedIn = false;

    void Start()
    {
        if (loadingText == null)
        {
            Debug.LogError("LoadingText reference is missing!");
            return;
        }

        StartCoroutine(LoadingSequence());
    }

    IEnumerator LoadingSequence()
    {
        // Start typing animation
        StartCoroutine(TypeLoadingText());
    
        // Initial delay to allow scene to stabilize
        yield return new WaitForEndOfFrame();
    
        // Fade in if we haven't already
        if (!hasFadedIn && FadeManager.Instance != null)
        {
            yield return FadeManager.Instance.FadeIn();
            hasFadedIn = true;
        }

        // Start loading the target scene in background
        loadingOperation = SceneManager.LoadSceneAsync(GameManager.Instance.TargetScene);
        loadingOperation.allowSceneActivation = false;

        // Create a minimum loading time buffer (2 seconds total)
        float minLoadingTime = 2f;
        float loadingTimer = 0f;
    
        while (!loadingOperation.isDone || loadingTimer < minLoadingTime)
        {
            loadingTimer += Time.deltaTime;
        
            // When 90% loaded and minimum time elapsed
            if (loadingOperation.progress >= 0.9f && loadingTimer >= minLoadingTime)
            {
                // Start fade out (takes 1 second based on fadeSpeed = 1)
                if (FadeManager.Instance != null)
                {
                    yield return FadeManager.Instance.FadeOut();
                }
            
                // Extra buffer after fade completes
                yield return new WaitForSeconds(0.5f);
            
                isLoading = false;
                loadingOperation.allowSceneActivation = true;
                break;
            }
            yield return null;
        }

        // Wait one frame to ensure scene is loaded
        yield return null;

        // Fade in the new scene
        if (FadeManager.Instance != null)
        {
            yield return FadeManager.Instance.FadeIn();
        }
    }

    IEnumerator TypeLoadingText()
    {
        int dots = 0;
        while (isLoading)
        {
            loadingText.text = loadingString + new string('.', dots);
            dots = (dots + 1) % 4;
            yield return new WaitForSeconds(typingSpeed);
        }
    }
}