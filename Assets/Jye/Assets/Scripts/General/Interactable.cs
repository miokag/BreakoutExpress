using UnityEngine;

public class DoorInteractor : MonoBehaviour
{
    [Header("Interaction Settings")]
    public KeyCode interactKey = KeyCode.E;
    public Vector3 interactSize = new Vector3(2f, 2f, 2f); // Dimensions of interaction cube
    public Vector3 interactOffset = new Vector3(0f, 0f, 1f); // Offset from player position

    [Header("Visualization")]
    public Color gizmoColor = new Color(0, 1, 1, 0.5f);
    public bool showGizmo = true;

    private DoorPuzzle currentNearbyDoor;
    private Vector3 detectionCenter;

    void Update()
    {
        detectionCenter = transform.position + interactOffset;
        CheckForDoors();
        
        if (currentNearbyDoor != null && Input.GetKeyDown(interactKey))
        {
            currentNearbyDoor.StartPuzzle();
        }
    }

    void CheckForDoors()
    {
        // Find all potential doors in scene
        DoorPuzzle[] allDoors = FindObjectsOfType<DoorPuzzle>();
        DoorPuzzle closestValidDoor = null;
        float closestDistance = float.MaxValue;

        foreach (DoorPuzzle door in allDoors)
        {
            if (!door.CompareTag("Interactable")) continue;

            Vector3 doorPos = door.transform.position;
            float distance = Vector3.Distance(doorPos, detectionCenter);

            // Check if door is within the 3D interaction cube
            if (IsInInteractionCube(doorPos) && distance < closestDistance)
            {
                closestDistance = distance;
                closestValidDoor = door;
            }
        }

        // Handle door proximity changes
        if (closestValidDoor != currentNearbyDoor)
        {
            // Hide prompt from previous door
            if (currentNearbyDoor != null)
            {
                currentNearbyDoor.ShowInteractPrompt(false);
            }

            // Show prompt on new door
            if (closestValidDoor != null)
            {
                closestValidDoor.ShowInteractPrompt(true);
            }

            currentNearbyDoor = closestValidDoor;
        }
    }

    bool IsInInteractionCube(Vector3 position)
    {
        Vector3 relativePos = position - detectionCenter;
        return Mathf.Abs(relativePos.x) < interactSize.x * 0.5f &&
               Mathf.Abs(relativePos.y) < interactSize.y * 0.5f &&
               Mathf.Abs(relativePos.z) < interactSize.z * 0.5f;
    }

    void OnDrawGizmosSelected()
    {
        if (!showGizmo) return;

        Gizmos.color = gizmoColor;
        Gizmos.DrawWireCube(detectionCenter, interactSize);
    }
}