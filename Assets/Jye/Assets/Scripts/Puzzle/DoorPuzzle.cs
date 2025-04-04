using UnityEngine;
using TMPro;
using System.Collections;
using BreakoutExpress;
using BreakoutExpress2D;

public class DoorPuzzle : MonoBehaviour
{
    [Header("UI")]
    public GameObject interactPrompt;
    public GameObject puzzleUI;
    public TMP_Text questionText;
    public TMP_InputField answerInput;
    public TMP_Text triesText; // Added for tries display

    [Header("Config")]
    public DoorQuestion questionData;
    public float doorOpenDelay = 1f;
    public string nextSceneName;
    public int maxTries = 3; // Added max tries setting
    
    [Header("Audio")]
    public AudioClip correctAnswerSFX;
    public AudioClip wrongAnswerSFX;
    public AudioClip gameOverSFX; // Added for game over sound
    public Color normalTextColor = Color.white;
    public Color wrongAnswerColor = Color.red;

    [Header("Ghost Control")]
    public TicketTaker3D[] ghostsToPause;

    private bool puzzleActive;
    private bool playerInRange;
    private bool puzzleCompleted;
    private MonoBehaviour playerController;
    private bool wasControllerEnabled;
    private GameObject freeLookCamera;
    private bool wasCameraActive;
    private GameObject npcManager;
    private bool wasNPCManagerActive;
    private bool isProcessingAnswer;
    private int remainingTries; // Track remaining attempts

    void Start()
    {
        tag = "Interactable";
        HideAllUI();
        freeLookCamera = GameObject.Find("FreeLook Camera");
        npcManager = GameObject.Find("NPC Manager");
        remainingTries = maxTries; // Initialize tries
    }

    void HideAllUI()
    {
        if (interactPrompt) interactPrompt.SetActive(false);
        if (puzzleUI) puzzleUI.SetActive(false);
    }

    public void ShowInteractPrompt(bool show)
    {
        if (puzzleCompleted || isProcessingAnswer) return;
        playerInRange = show;
        if (interactPrompt) interactPrompt.SetActive(show);
    }

    public void StartPuzzle()
    {
        if (puzzleActive || !playerInRange || puzzleCompleted || isProcessingAnswer) return;
        
        // Reset tries when starting puzzle
        remainingTries = maxTries;
        UpdateTriesDisplay();

        // Pause all ghosts
        if (ghostsToPause != null)
        {
            foreach (var ghost in ghostsToPause)
            {
                if (ghost != null) ghost.PauseMovement(true);
            }
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerController = player.GetComponent<PlayerController>();
            if (playerController == null)
            {
                playerController = player.GetComponent<PlayerController2D>();
                
                if (npcManager != null)
                {
                    wasNPCManagerActive = npcManager.activeSelf;
                    var spawner = npcManager.GetComponent<NPCSpawner>();
                    if (spawner != null) spawner.StopAllCoroutines();
                    npcManager.SetActive(false);
                }
            }
            
            if (playerController != null)
            {
                wasControllerEnabled = playerController.enabled;
                playerController.enabled = false;
            }
        }
        
        if (freeLookCamera != null)
        {
            wasCameraActive = freeLookCamera.activeSelf;
            freeLookCamera.SetActive(false);
        }
        
        puzzleActive = true;
        ShowInteractPrompt(false);
        
        if (puzzleUI) puzzleUI.SetActive(true);
        if (questionText) questionText.text = questionData.question;
        if (answerInput)
        {
            answerInput.text = "";
            answerInput.Select();
        }
    }

    private void UpdateTriesDisplay()
    {
        if (triesText != null)
        {
            triesText.text = $"Tries: {remainingTries}";
        }
    }

    public void CheckAnswer()
    {
        if (puzzleCompleted || isProcessingAnswer) return;
        
        isProcessingAnswer = true;
        answerInput.interactable = false;
        
        if (int.TryParse(answerInput.text, out int answer) && answer == questionData.correctAnswer)
        {
            if (correctAnswerSFX != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(correctAnswerSFX);
                StartCoroutine(WaitForSFXThenOpenDoor(correctAnswerSFX.length));
            }
            else
            {
                StartCoroutine(OpenDoor());
            }
        }
        else
        {
            remainingTries--; // Decrement tries on wrong answer
            UpdateTriesDisplay();

            if (remainingTries <= 0)
            {
                // No more tries left - game over
                StartCoroutine(GameOverSequence());
            }
            else
            {
                // Still have tries remaining
                if (questionText != null)
                {
                    answerInput.text = "<color=#" + ColorUtility.ToHtmlStringRGBA(wrongAnswerColor) + ">Try Again</color>";
                }
                
                if (wrongAnswerSFX != null && AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX(wrongAnswerSFX);
                    StartCoroutine(WaitForSFXThenReset(wrongAnswerSFX.length));
                }
                else
                {
                    StartCoroutine(ResetAfterDelay(0.5f));
                }
            }
        }
    }

    private IEnumerator GameOverSequence()
    {
        // Play game over sound if available
        if (gameOverSFX != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(gameOverSFX);
            yield return new WaitForSeconds(gameOverSFX.length);
        }

        // Trigger game over
        GameManager.Instance.GameOver();

        // Clean up puzzle UI
        if (puzzleUI) puzzleUI.SetActive(false);
        
        // Resume player control
        if (playerController != null && wasControllerEnabled)
        {
            playerController.enabled = true;
        }
        
        // Resume camera
        if (freeLookCamera != null && wasCameraActive)
        {
            freeLookCamera.SetActive(true);
        }
        
        // Resume NPCs
        if (npcManager != null && wasNPCManagerActive)
        {
            npcManager.SetActive(true);
        }
        
        // Resume ghosts
        if (ghostsToPause != null)
        {
            foreach (var ghost in ghostsToPause)
            {
                if (ghost != null) ghost.PauseMovement(false);
            }
        }
    }

    private IEnumerator WaitForSFXThenOpenDoor(float sfxLength)
    {
        yield return new WaitForSeconds(sfxLength);
        StartCoroutine(OpenDoor());
    }

    private IEnumerator WaitForSFXThenReset(float sfxLength)
    {
        yield return new WaitForSeconds(sfxLength);
        StartCoroutine(ResetAfterDelay(0.1f));
    }

    private IEnumerator ResetAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        answerInput.text = "";
        answerInput.interactable = true;
        answerInput.Select();
        
        if (questionText != null)
        {
            questionText.text = questionData.question;
        }
        
        isProcessingAnswer = false;
    }

    IEnumerator OpenDoor()
    {
        yield return new WaitForSeconds(doorOpenDelay);
    
        puzzleCompleted = true;
        if (puzzleUI) puzzleUI.SetActive(false);
    
        // Resume all ghosts
        if (ghostsToPause != null)
        {
            foreach (var ghost in ghostsToPause)
            {
                if (ghost != null) ghost.PauseMovement(false);
            }
        }

        if (playerController != null && wasControllerEnabled)
        {
            playerController.enabled = true;
        }
    
        if (freeLookCamera != null && wasCameraActive)
        {
            freeLookCamera.SetActive(true);
        }
    
        if (npcManager != null && wasNPCManagerActive)
        {
            npcManager.SetActive(true);
        }
    
        tag = "Untagged";
        if (interactPrompt) interactPrompt.SetActive(false);

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            GameManager.Instance.LoadSceneWithLoading(nextSceneName);
        }
        else
        {
            Debug.LogWarning("No next scene name specified in DoorPuzzle");
        }
    }
}