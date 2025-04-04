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
    private bool cursorLocked = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeGameOverUI();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateCursorState();
    }

    void InitializeGameOverUI()
    {
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(false);
        }
    }

    public void UpdateCursorState()
    {
        bool isMenuScene = SceneManager.GetActiveScene().name.Contains("Menu");
        bool isCartScene = SceneManager.GetActiveScene().name.Contains("Cart");
        
        SetCursorLocked(!isMenuScene || isCartScene);
    }

    public void SetCursorLocked(bool locked)
    {
        cursorLocked = locked;
        
        if (locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
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
        SetCursorLocked(!isPaused);
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
        UpdateCursorState();
    }
    
    public void CompleteLastCart()
    {
        StartCoroutine(EndGameSequence());
    }

    private IEnumerator EndGameSequence()
    {
        // Fade out
        if (FadeManager.Instance != null)
        {
            yield return FadeManager.Instance.FadeOut();
        }
    
        // Load end scene directly (no loading screen)
        SceneManager.LoadScene("EndScene");
    
        // Fade in will be handled by the EndScene's own script
    }

    public void GameOver()
    {
        StopTime();
        
        if (gameOverUI != null)
        {
            SetCursorLocked(false);
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
        SceneManager.sceneLoaded -= OnSceneLoaded;
        
        // Ensure time is reset when destroyed to prevent frozen state
        if (isPaused)
        {
            Time.timeScale = 1f;
        }
    }
}