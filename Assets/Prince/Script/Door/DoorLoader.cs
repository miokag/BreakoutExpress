using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorLoader : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("The name of the scene to load when the player enters the door.")]
    public string sceneToLoad;

    [Header("Door Settings")]
    [Tooltip("If true, this door can only be used once.")]
    public bool oneTimeUse = true;

    private bool hasBeenUsed = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the colliding object is the player and if the door hasn't been used (if oneTimeUse is true)
        if (collision.CompareTag("Player") && (!hasBeenUsed || !oneTimeUse))
        {
            if (oneTimeUse)
                hasBeenUsed = true;

            // Optionally, disable the door's collider after it's been used:
            // GetComponent<Collider2D>().enabled = false;

            // Load the specified scene.
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
