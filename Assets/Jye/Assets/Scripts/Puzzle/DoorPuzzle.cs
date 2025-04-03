using UnityEngine;
using TMPro;
using System.Collections;
using BreakoutExpress;
using BreakoutExpress2D;
using UnityEngine.SceneManagement;

public class DoorPuzzle : MonoBehaviour
{
    [Header("UI")]
    public GameObject interactPrompt;
    public GameObject puzzleUI;
    public TMP_Text questionText;
    public TMP_InputField answerInput;

    [Header("Config")]
    public DoorQuestion questionData;
    public float doorOpenDelay = 1f;
    [Tooltip("Name of the scene to load when puzzle is solved")]
    public string nextSceneName;
    
    [Header("Audio")]
    public AudioClip correctAnswerSFX;
    public AudioClip wrongAnswerSFX;
    public Color normalTextColor = Color.white;
    public Color wrongAnswerColor = Color.red;

    private bool puzzleActive;
    private bool playerInRange;
    private bool puzzleCompleted;
    private MonoBehaviour playerController;
    private bool wasControllerEnabled;
    private GameObject freeLookCamera;
    private bool wasCameraActive;
    private GameObject npcManager;
    private bool wasNPCManagerActive;
    private bool isProcessingAnswer; // New flag to track answer processing

    void Start()
    {
        tag = "Interactable";
        HideAllUI();
        freeLookCamera = GameObject.Find("FreeLook Camera");
        npcManager = GameObject.Find("NPC Manager");
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

    public void CheckAnswer()
    {
        if (puzzleCompleted || isProcessingAnswer) return;
        
        isProcessingAnswer = true;
        answerInput.interactable = false;
        
        if (int.TryParse(answerInput.text, out int answer) && answer == questionData.correctAnswer)
        {
            // Correct answer flow
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
            // Wrong answer flow
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

    private IEnumerator WaitForSFXThenOpenDoor(float sfxLength)
    {
        yield return new WaitForSeconds(sfxLength);
        StartCoroutine(OpenDoor());
    }

    private IEnumerator WaitForSFXThenReset(float sfxLength)
    {
        yield return new WaitForSeconds(sfxLength);
        StartCoroutine(ResetAfterDelay(0.1f)); // Small additional delay
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
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("No next scene name specified in DoorPuzzle");
        }
    }
}