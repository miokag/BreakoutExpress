using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public string TargetScene { get; private set; }

    [Header("Game Over Settings")]
    [SerializeField] private GameObject gameOverUI;

    private bool isPaused = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeGameOverUI();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeGameOverUI()
    {
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(false);
        }
    }

    public void LoadSceneWithLoading(string sceneName)
    {
        TargetScene = sceneName;
        StartCoroutine(LoadWithFade());
    }

    private IEnumerator LoadWithFade()
    {
        if (FadeManager.Instance != null)
        {
            yield return FadeManager.Instance.FadeOut();
        }
        
        SceneManager.LoadScene("LoadingScene");
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
    }

    public void StopTime()
    {
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ResumeTime()
    {
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void GameOver()
    {
        StopTime();
        
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
        }
    }
    
    public void RestartScene()
    {
        ResumeTime();
        
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(false);
        }
    }

    void OnDestroy()
    {
        // Ensure time is reset when destroyed to prevent frozen state
        if (isPaused)
        {
            Time.timeScale = 1f;
        }
    }
}