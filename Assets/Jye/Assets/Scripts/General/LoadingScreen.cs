using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class LoadingScreen : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Text loadingText;
    [SerializeField] private TMP_Text tipsText;
    [SerializeField] private float typingSpeed = 0.1f;
    
    [Header("Tips Content")]
    [SerializeField] private List<string> tips2D = new List<string>
    {
        "Use arrow keys to move in 2D space",
        "Long press your keyboard for higher jumps",
        "Some platforms may disappear after you step on them"
    };
    
    [SerializeField] private List<string> tips3D = new List<string>
    {
        "Use WASD to move and mouse to look around in 3D",
        "Check your surroundings - puzzles might have 3D solutions",
        "Some objects can be interacted with from multiple angles"
    };

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

        // Display appropriate tip based on target scene
        DisplaySceneSpecificTip();
        
        StartCoroutine(LoadingSequence());
    }

    private void DisplaySceneSpecificTip()
    {
        if (tipsText == null) return;
        
        string targetScene = GameManager.Instance.TargetScene;
        List<string> tipsToUse = new List<string>();
        
        if (targetScene.Contains("3D"))
        {
            tipsToUse = tips3D;
        }
        else if (targetScene.Contains("2D"))
        {
            tipsToUse = tips2D;
        }

        if (tipsToUse.Count > 0)
        {
            // Select a random tip from the appropriate list
            int randomIndex = Random.Range(0, tipsToUse.Count);
            tipsText.text = tipsToUse[randomIndex];
        }
        else
        {
            tipsText.text = "Tip: Explore carefully and enjoy the game!";
        }
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