using UnityEngine;

public class Interactor2D : MonoBehaviour
{
    [Header("Settings")]
    public KeyCode interactKey = KeyCode.E;
    public Vector2 interactSize = new Vector2(2f, 2f);
    public Vector2 interactOffset = Vector2.zero;

    [Header("Debug")]
    public Color gizmoColor = new Color(0, 1, 1, 0.5f);

    private GameObject currentInteractable;
    private DoorPuzzle currentDoorPuzzle;
    private Vector2 detectionCenter;

    void Update()
    {
        detectionCenter = (Vector2)transform.position + interactOffset;
        CheckForInteractables();
        
        if (currentDoorPuzzle != null && Input.GetKeyDown(interactKey))
        {
            currentDoorPuzzle.StartPuzzle();
        }
    }

    void CheckForInteractables()
    {
        Collider2D[] hits = Physics2D.OverlapBoxAll(detectionCenter, interactSize, 0);
        GameObject closest = null;
        float closestDistance = Mathf.Infinity;
        DoorPuzzle nearestDoor = null;

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Interactable"))
            {
                DoorPuzzle door = hit.GetComponent<DoorPuzzle>();
                if (door != null)
                {
                    float dist = Vector2.Distance(hit.transform.position, detectionCenter);
                    if (dist < closestDistance)
                    {
                        closest = hit.gameObject;
                        closestDistance = dist;
                        nearestDoor = door;
                    }
                }
            }
        }

        // Handle door puzzle changes
        if (nearestDoor != currentDoorPuzzle)
        {
            // Hide previous prompt
            if (currentDoorPuzzle != null)
            {
                currentDoorPuzzle.ShowInteractPrompt(false);
            }

            // Show new prompt
            if (nearestDoor != null)
            {
                nearestDoor.ShowInteractPrompt(true);
            }

            currentDoorPuzzle = nearestDoor;
            currentInteractable = closest;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireCube(detectionCenter, interactSize);
        Gizmos.DrawLine(transform.position, detectionCenter);
    }
}