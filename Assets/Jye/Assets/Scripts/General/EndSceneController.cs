using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class EndSceneController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform walkTarget;
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private TMP_Text storyText;
    [SerializeField] private float textTypeSpeed = 0.05f; // Time between characters
    [SerializeField] private float textFadeDuration = 1f;
    [SerializeField] private string message = "Your story continues...";

    private string fullText;
    private bool isTyping = false;

    private void Start()
    {
        fullText = storyText.text;
        storyText.text = "";
        storyText.color = new Color(1, 1, 1, 0);
        StartCoroutine(PlayEndingSequence());
    }

    private IEnumerator PlayEndingSequence()
    {
        if (FadeManager.Instance != null)
        {
            yield return FadeManager.Instance.FadeIn();
        }

        StartCoroutine(WalkToTarget());
        StartCoroutine(TypeText());

        // Wait until both walking and typing are complete
        while (isTyping || Vector3.Distance(player.position, walkTarget.position) > 0.1f)
        {
            yield return null;
        }

        yield return new WaitForSeconds(2f); // Additional pause at end
        SceneManager.LoadScene("MainMenu");
    }

    private IEnumerator WalkToTarget()
    {
        float journeyLength = Vector3.Distance(player.position, walkTarget.position);
        float startTime = Time.time;
        
        while (Vector3.Distance(player.position, walkTarget.position) > 0.1f)
        {
            float distanceCovered = (Time.time - startTime) * walkSpeed;
            float fractionOfJourney = distanceCovered / journeyLength;
            player.position = Vector3.Lerp(player.position, walkTarget.position, fractionOfJourney);
            yield return null;
        }
    }

    private IEnumerator TypeText()
    {
        isTyping = true;
        
        // Fade in text box
        float fadeTimer = 0f;
        while (fadeTimer < textFadeDuration)
        {
            fadeTimer += Time.deltaTime;
            storyText.color = new Color(1, 1, 1, Mathf.Lerp(0, 1, fadeTimer/textFadeDuration));
            yield return null;
        }

        for (int i = 0; i <= fullText.Length; i++)
        {
            storyText.text = fullText.Substring(0, i);
            yield return new WaitForSeconds(textTypeSpeed);
        }

        isTyping = false;
    }
}