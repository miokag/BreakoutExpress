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

    [Header("Config")]
    public DoorQuestion questionData;
    public float doorOpenDelay = 1f;

    private bool puzzleActive;
    private bool playerInRange;
    private bool puzzleCompleted;
    private MonoBehaviour playerController;
    private bool wasControllerEnabled;
    private GameObject freeLookCamera; // For 3D mode camera
    private bool wasCameraActive; // Store camera's original state
    private GameObject npcManager; // For 2D mode NPC management
    private bool wasNPCManagerActive; // Store NPC Manager's original state

    void Start()
    {
        tag = "Interactable";
        HideAllUI();
        
        // Find the FreeLook camera if it exists
        freeLookCamera = GameObject.Find("FreeLook Camera");
        // Find the NPC Manager if it exists
        npcManager = GameObject.Find("NPC Manager");
    }

    void HideAllUI()
    {
        if (interactPrompt) interactPrompt.SetActive(false);
        if (puzzleUI) puzzleUI.SetActive(false);
    }

    public void ShowInteractPrompt(bool show)
    {
        if (puzzleCompleted) return;
        playerInRange = show;
        if (interactPrompt) interactPrompt.SetActive(show);
    }

    public void StartPuzzle()
    {
        if (puzzleActive || !playerInRange || puzzleCompleted) return;
        
        // Handle player controller
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
        
        // Handle 3D camera if it exists
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
        if (puzzleCompleted) return;
        
        if (int.TryParse(answerInput.text, out int answer) && answer == questionData.correctAnswer)
        {
            StartCoroutine(OpenDoor());
        }
        else
        {
            Debug.Log("Try Again");
            answerInput.text = "";
            answerInput.Select();
        }
    }

    IEnumerator OpenDoor()
    {
        yield return new WaitForSeconds(doorOpenDelay);
    
        puzzleCompleted = true;
        Debug.Log("Correct Answer!");
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
            // Don't manually start the coroutine - just enable the GameObject
            npcManager.SetActive(true);
        }
    
        // Disable interaction
        tag = "Untagged";
        if (interactPrompt) interactPrompt.SetActive(false);
    }
}