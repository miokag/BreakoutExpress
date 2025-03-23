using UnityEngine;

public class CustomPlayerController : MonoBehaviour
{
    public float moveSpeed = 6f;
    public float jumpForce = 8f;
    public float gravity = 20f;
    public float groundCheckDistance = 0.1f;
    public Transform cameraTransform; // Reference to the Cinemachine FreeLook camera's transform

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        // If no camera transform is assigned, use the main camera
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    void Update()
    {
        // Check if the player is grounded
        isGrounded = CheckGrounded();

        // Handle movement
        HandleMovement();

        // Handle jumping
        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            velocity.y = jumpForce;
        }

        // Apply gravity
        if (!isGrounded)
        {
            velocity.y -= gravity * Time.deltaTime;
        }

        // Move the player
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Get the camera's forward and right vectors, ignoring vertical rotation
        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;
        cameraForward.y = 0f; // Flatten the vector to ignore camera tilt
        cameraRight.y = 0f;
        cameraForward.Normalize();
        cameraRight.Normalize();

        // Calculate movement direction relative to the camera
        Vector3 moveDirection = (cameraForward * vertical + cameraRight * horizontal).normalized;

        // Update velocity based on movement direction
        velocity.x = moveDirection.x * moveSpeed;
        velocity.z = moveDirection.z * moveSpeed;

        // Rotate the player to face the movement direction
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }

    private bool CheckGrounded()
    {
        // Raycast to check if the player is grounded
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, groundCheckDistance))
        {
            return true;
        }
        return false;
    }
}