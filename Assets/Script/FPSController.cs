using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float lookSpeed = 3f;
    public float jumpSpeed = 5f;
    public float gravity = 9.81f;

    private CharacterController controller;
    private float verticalVelocity;

    // We'll store these to handle mouse look
    private float rotationX;  // up-down
    private float rotationY;  // left-right

    void Start()
    {
        controller = GetComponent<CharacterController>();

        // Lock and hide cursor (optional for FPS)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // --- Mouse Look ---
        rotationX -= Input.GetAxis("Mouse Y") * lookSpeed;
        rotationY += Input.GetAxis("Mouse X") * lookSpeed;

        // Clamp vertical rotation so you can't flip upside down
        rotationX = Mathf.Clamp(rotationX, -90f, 90f);

        // Apply rotations to the player
        transform.localRotation = Quaternion.Euler(rotationX, rotationY, 0f);

        // --- Movement ---
        float moveX = Input.GetAxis("Horizontal");  // A/D
        float moveZ = Input.GetAxis("Vertical");    // W/S

        // Move direction relative to player orientation
        Vector3 move = transform.right * moveX + transform.forward * moveZ;

        // Handle jumping/vertical velocity
        if (controller.isGrounded)
        {
            // Small downward force to keep grounded
            verticalVelocity = -1f;

            // Jump
            if (Input.GetButtonDown("Jump"))
            {
                verticalVelocity = jumpSpeed;
            }
        }
        else
        {
            // Apply gravity when not grounded
            verticalVelocity -= gravity * Time.deltaTime;
        }

        // Add vertical movement
        move.y = verticalVelocity;

        // Move the controller
        controller.Move(move * moveSpeed * Time.deltaTime);
    }
}
