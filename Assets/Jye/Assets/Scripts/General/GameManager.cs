using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public string TargetScene { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadSceneWithLoading(string sceneName)
    {
        TargetScene = sceneName;
        StartCoroutine(LoadWithFade());
    }

    private IEnumerator LoadWithFade()
    {
        // Only fade out if FadeManager exists
        if (FadeManager.Instance != null)
        {
            yield return FadeManager.Instance.FadeOut();
        }
        
        SceneManager.LoadScene("LoadingScene");
    }
}