using UnityEngine;

public class DoorInteractor : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private MeshCollider interactionRange;
    
    private DoorPuzzle nearbyDoor;
    private bool isInRange;

    private void Start()
    {
        if (interactionRange == null) 
        {
            interactionRange = GetComponentInChildren<MeshCollider>();
            if (interactionRange == null) return;
        }

        if (!interactionRange.convex) interactionRange.convex = true;
        if (!interactionRange.isTrigger) interactionRange.isTrigger = true;
    }

    void Update()
    {
        if (isInRange && Input.GetKeyDown(interactKey))
        {
            nearbyDoor?.StartPuzzle();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Interactable")) return;

        if (other.TryGetComponent<DoorPuzzle>(out var door))
        {
            nearbyDoor = door;
            isInRange = true;
            door.ShowInteractPrompt(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Interactable")) return;

        if (nearbyDoor != null && other.gameObject == nearbyDoor.gameObject)
        {
            nearbyDoor.ShowInteractPrompt(false);
            nearbyDoor = null;
            isInRange = false;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (interactionRange != null)
        {
            Gizmos.color = new Color(0, 1, 1, 0.25f);
            Gizmos.DrawMesh(interactionRange.sharedMesh, 
                interactionRange.transform.position,
                interactionRange.transform.rotation,
                interactionRange.transform.lossyScale);
        }
    }
}