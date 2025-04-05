using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DoorTriggerTo2D : MonoBehaviour
{
    [Header("Camera Settings")]
    public Camera camera3D;           // Your 3D camera (active initially)
    public Camera camera2D;           // Your 2D camera (inactive initially)

    [Header("Fade Settings")]
    public Image fadeOverlay;         // Full-screen UI Image for fade (set to black, alpha = 0 initially)
    public float fadeDuration = 1.0f;   // Duration of the fade in seconds

    [Header("Player Movement Scripts")]
    public MonoBehaviour fpsController;    // The 3D movement script (FPSController)
    public MonoBehaviour sideScroller;     // The 2D movement script (SideScrollerController)

    private bool transitioning = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !transitioning)
        {
            StartCoroutine(CrossFadeTo2D());
        }
    }

    private IEnumerator CrossFadeTo2D()
    {
        transitioning = true;
        // Disable both movement scripts so no input is processed during the transition
        fpsController.enabled = false;
        sideScroller.enabled = false;

        Color overlayColor = fadeOverlay.color;
        overlayColor.a = 0f;
        fadeOverlay.color = overlayColor;

        // Fade to black
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            overlayColor.a = Mathf.Lerp(0f, 1f, t);
            fadeOverlay.color = overlayColor;
            yield return null;
        }
        overlayColor.a = 1f;
        fadeOverlay.color = overlayColor;

        // Switch cameras
        camera3D.gameObject.SetActive(false);
        camera2D.gameObject.SetActive(true);

        // Fade back to clear
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            overlayColor.a = Mathf.Lerp(1f, 0f, t);
            fadeOverlay.color = overlayColor;
            yield return null;
        }
        overlayColor.a = 0f;
        fadeOverlay.color = overlayColor;

        // Enable only the 2D movement controller now
        sideScroller.enabled = true;

        transitioning = false;
    }
}
