using UnityEngine;
using TMPro;
using System.Collections;
using BreakoutExpress;

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

    void Start()
    {
        tag = "Interactable";
        if (interactPrompt) interactPrompt.SetActive(false);
        if (puzzleUI) puzzleUI.SetActive(false);
    }

    public void ShowInteractPrompt(bool show)
    {
        if (interactPrompt) interactPrompt.SetActive(show);
    }

    public void StartPuzzle()
    {
        if (puzzleActive) return;
        
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
        if (int.TryParse(answerInput.text, out int answer) && answer == questionData.correctAnswer)
        {
            StartCoroutine(OpenDoor());
        }
        else
        {
            Debug.Log("Try Again");
        }
    }

    IEnumerator OpenDoor()
    {
        yield return new WaitForSeconds(doorOpenDelay);
        Debug.Log("Correct Answer");
        this.enabled = false;
        DoorInteractor doorInteractor = FindObjectOfType<DoorInteractor>();
        doorInteractor.enabled = false;
        // GetComponent<Animator>().SetTrigger("Open");
        // GetComponent<Collider>().enabled = false;
        puzzleUI.SetActive(false);
    }
}