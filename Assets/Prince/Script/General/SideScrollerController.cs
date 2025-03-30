using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SideScrollerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;         // Base movement speed
    public float jumpSpeed = 5f;         // Jump force
    public float gravity = 9.81f;        // Gravity applied when not grounded

    [Header("Crouch Settings")]
    public float crouchSpeedMultiplier = 0.5f;  // Movement speed multiplier while crouching
    public float crouchHeightFactor = 0.5f;       // Percentage of original height when crouched

    [Header("Climb Settings")]
    public float climbSpeed = 3f;        // Speed of climbing movement

    private CharacterController controller;
    private float verticalVelocity;

    // For crouching, store original height and center
    private float originalHeight;
    private Vector3 originalCenter;
    private bool isCrouching = false;

    // Climbing flag – set true when colliding with an object tagged "Climb"
    private bool isClimbing = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        // Store original values for crouch restoration
        originalHeight = controller.height;
        originalCenter = controller.center;

        // Orient the player so that transform.right is the direction of movement
        // (Adjust as needed for your scene)
        transform.rotation = Quaternion.Euler(0f, 0f, 0f);
    }

    void Update()
    {
        // --- Crouch Logic ---
        if (Input.GetKey(KeyCode.C))
        {
            if (!isCrouching)
            {
                isCrouching = true;
                // Lower the CharacterController height and adjust its center
                controller.height = originalHeight * crouchHeightFactor;
                controller.center = originalCenter * crouchHeightFactor;
            }
        }
        else
        {
            if (isCrouching)
            {
                // Optional: check for headroom before standing up
                isCrouching = false;
                controller.height = originalHeight;
                controller.center = originalCenter;
            }
        }

        // --- Climb Logic ---
        if (isClimbing)
        {
            // When climbing, use the Vertical input to move upward or downward
            float climbInput = Input.GetAxis("Vertical");
            Vector3 climbMove = Vector3.up * climbSpeed * climbInput;
            // Apply climbing movement (ignore gravity and jump)
            controller.Move(climbMove * Time.deltaTime);
            return; // Skip the rest of Update while climbing
        }

        // --- Normal Movement (Forward/Backward) ---
        // Here we use the "Vertical" axis (W/S) for movement along transform.right.
        float moveInput = Input.GetAxis("Vertical");
        // If crouching, reduce the movement speed
        float currentSpeed = isCrouching ? moveSpeed * crouchSpeedMultiplier : moveSpeed;
        Vector3 move = transform.right * moveInput;

        // --- Jump and Gravity ---
        if (controller.isGrounded)
        {
            verticalVelocity = -1f;
            if (Input.GetButtonDown("Jump"))
            {
                verticalVelocity = jumpSpeed;
            }
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }

        // Apply vertical velocity (for jump/fall)
        move.y = verticalVelocity;

        // Move the CharacterController
        controller.Move(move * currentSpeed * Time.deltaTime);
    }

    // --- Climbing Trigger Detection ---
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Climb"))
        {
            isClimbing = true;
            // Optional: reset vertical velocity when starting to climb
            verticalVelocity = 0f;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Climb"))
        {
            isClimbing = false;
        }
    }
}
