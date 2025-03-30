using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DoorTriggerTo3D : MonoBehaviour
{
    [Header("Camera Settings")]
    public Camera camera3D;           // Your 3D camera (to be re-enabled)
    public Camera camera2D;           // Your 2D camera (currently active in 2D mode)

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
            StartCoroutine(CrossFadeTo3D());
        }
    }

    private IEnumerator CrossFadeTo3D()
    {
        transitioning = true;
        // Disable both movement scripts to prevent input during transition
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
        camera2D.gameObject.SetActive(false);
        camera3D.gameObject.SetActive(true);

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

        // Enable only the 3D movement controller now
        fpsController.enabled = true;

        transitioning = false;
    }
}
