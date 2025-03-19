using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public Transform playerTransform; // Reference to the player's transform
    public float mouseSensitivity = 100f; // Sensitivity for mouse movement
    public float distanceFromPlayer = 5f; // Distance of the camera from the player
    public Vector3 cameraOffset = new Vector3(0, 2f, 0); // Offset to position the camera slightly above the player

    private float xRotation = 0f; // Tracks vertical rotation of the camera
    private float yRotation = 0f; // Tracks horizontal rotation of the camera

    private void Start()
    {
        // Lock and hide the cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void LateUpdate()
    {
        HandleCamera();
    }

    private void HandleCamera()
    {
        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Rotate the camera based on mouse input
        yRotation += mouseX; // Horizontal rotation
        xRotation -= mouseY; // Vertical rotation
        xRotation = Mathf.Clamp(xRotation, -30f, 70f); // Clamp vertical rotation to prevent flipping

        // Calculate camera rotation
        Quaternion rotation = Quaternion.Euler(xRotation, yRotation, 0);

        // Calculate camera position
        Vector3 cameraPosition = playerTransform.position - (rotation * Vector3.forward * distanceFromPlayer) + cameraOffset;

        // Apply position and rotation to the camera
        transform.position = cameraPosition;
        transform.rotation = rotation;
    }
}