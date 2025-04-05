using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public string TargetScene { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private GameObject controlsUI3D;
    [SerializeField] private GameObject controlsUI2D;
    [SerializeField] private GameObject settingsUI;

    [Header("Input Settings")]
    [SerializeField] private float controlsHoldDuration = 1f;
    private float tabHoldTimer = 0f;
    private bool isHoldingTab = false;

    private bool isPaused = false;
    private bool cursorLocked = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeUI();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        HandleUIInput();
    }

    private void InitializeUI()
    {
        if (gameOverUI) gameOverUI.SetActive(false);
        if (controlsUI3D) controlsUI3D.SetActive(false);
        if (controlsUI2D) controlsUI2D.SetActive(false);
        if (settingsUI) settingsUI.SetActive(false);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateCursorState();
        InitializeUI(); // Reset UI on scene load
    }

    private void HandleUIInput()
    {
        // Handle Tab hold for controls UI
        if (Keyboard.current.tabKey.isPressed)
        {
            if (!isHoldingTab)
            {
                tabHoldTimer += Time.unscaledDeltaTime;
                
                if (tabHoldTimer >= controlsHoldDuration)
                {
                    isHoldingTab = true;
                    ShowControlsUI(true);
                }
            }
        }
        else if (isHoldingTab)
        {
            ShowControlsUI(false);
            tabHoldTimer = 0f;
            isHoldingTab = false;
        }

        // Handle Escape press for settings
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (settingsUI.activeSelf)
            {
                ToggleSettingsUI(false);
            }
            else
            {
                ToggleSettingsUI(true);
            }
        }
    }

    public void ShowControlsUI(bool show)
    {
        GameObject activeControlsUI = GetActiveControlsUI();
        if (activeControlsUI == null) return;

        activeControlsUI.SetActive(show);
        
        if (show) 
        {
            settingsUI.SetActive(false);
            StopTime();
        }
        else if (!settingsUI.activeSelf)
        {
            ResumeTime();
        }
    }

    private GameObject GetActiveControlsUI()
    {
        bool is3DScene = SceneManager.GetActiveScene().name.Contains("3D") || 
                        !SceneManager.GetActiveScene().name.Contains("2D");

        return is3DScene ? controlsUI3D : controlsUI2D;
    }

    public void ToggleSettingsUI(bool show)
    {
        if (!SceneManager.GetActiveScene().name.Contains("Cart"))
        {
            if (settingsUI != null) settingsUI.SetActive(false);
            return;
        }

        if (settingsUI == null) return;

        settingsUI.SetActive(show);
    
        if (show)
        {
            if (controlsUI3D) controlsUI3D.SetActive(false);
            if (controlsUI2D) controlsUI2D.SetActive(false);
            StopTime();
        }
        else
        {
            ResumeTime();
        }
    }

    public void UpdateCursorState()
    {
        bool shouldLockCursor = !SceneManager.GetActiveScene().name.Contains("Menu");
        SetCursorLocked(shouldLockCursor);
    }

    public void SetCursorLocked(bool locked)
    {
        cursorLocked = locked;
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
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
        SetCursorLocked(false);
    }

    public void ResumeTime()
    {
        Time.timeScale = 1f;
        isPaused = false;
        UpdateCursorState();
    }

    public void GameOver()
    {
        StopTime();
        if (gameOverUI) gameOverUI.SetActive(true);
    }
    
    public void RestartScene()
    {
        ResumeTime();
        if (gameOverUI) gameOverUI.SetActive(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void CompleteLastCart()
    {
        StartCoroutine(EndGameSequence());
    }

    private IEnumerator EndGameSequence()
    {
        if (FadeManager.Instance != null)
        {
            yield return FadeManager.Instance.FadeOut();
        }
        SceneManager.LoadScene("EndScene");
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (isPaused) Time.timeScale = 1f;
    }
}