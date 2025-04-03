// LoadingScreen.cs (attach to main object in LoadingScene)
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class LoadingScreen : MonoBehaviour
{
    [Header("Visuals")]
    public Image walkingCharacter;
    public TMP_Text loadingText;
    public float typingSpeed = 0.1f;
    public string[] loadingPhrases = { "Loading", "Loading.", "Loading..", "Loading..." };

    [Header("Settings")]
    public float walkAnimationSpeed = 1f;
    public float walkDistance = 100f;

    private AsyncOperation loadingOperation;
    private Vector2 startPos;
    private string targetScene;

    void Start()
    {
        startPos = walkingCharacter.rectTransform.anchoredPosition;
        targetScene = PlayerPrefs.GetString("TargetScene");
        StartCoroutine(AnimateLoading());
        StartCoroutine(LoadTargetScene());
    }

    IEnumerator AnimateLoading()
    {
        int phraseIndex = 0;
        float walkTimer = 0f;
        bool walkingRight = true;

        while (true)
        {
            // Typing text animation
            loadingText.text = loadingPhrases[phraseIndex];
            phraseIndex = (phraseIndex + 1) % loadingPhrases.Length;

            // Walking animation
            walkTimer += Time.deltaTime * walkAnimationSpeed;
            float progress = Mathf.PingPong(walkTimer, 1f);
            float newX = Mathf.Lerp(startPos.x, startPos.x + walkDistance, progress);
            walkingCharacter.rectTransform.anchoredPosition = new Vector2(newX, startPos.y);

            yield return new WaitForSeconds(typingSpeed);
        }
    }

    IEnumerator LoadTargetScene()
    {
        // Small delay to show loading screen
        yield return new WaitForSeconds(1f);

        loadingOperation = SceneManager.LoadSceneAsync(targetScene);
        loadingOperation.allowSceneActivation = false;

        while (!loadingOperation.isDone)
        {
            // Wait until loading is almost complete (90% is typical for scene activation)
            if (loadingOperation.progress >= 0.9f)
            {
                // Short delay before activation to ensure everything is ready
                yield return new WaitForSeconds(0.5f);
                loadingOperation.allowSceneActivation = true;
            }
            yield return null;
        }
    }
}