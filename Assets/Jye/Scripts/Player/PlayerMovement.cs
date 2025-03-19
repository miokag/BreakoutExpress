using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 playerVelocity;
    private bool groundedPlayer;
    private float playerSpeed = 5.0f;
    private float runSpeed = 10.0f; 
    private float jumpHeight = 1.5f;
    private float gravityValue = -20f;
    private float jumpCooldown = 0.2f; 
    private float lastJumpTime = -1f; 
    
    public Transform cameraTransform; 

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        if (controller == null)
        {
            Debug.LogError("Character controller not found");
        }
    }

    void Update()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        // Ground check and reset vertical velocity
        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f;
        }

        // Get movement input
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // Calculate movement direction relative to the camera
        Vector3 move = cameraTransform.right * moveX + cameraTransform.forward * moveZ;
        move.y = 0;
        move.Normalize();

        // Apply run speed if Shift is held
        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : playerSpeed;
        controller.Move(move * Time.deltaTime * currentSpeed);

        // Rotate the player to face the movement direction
        if (move != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(move);
        }

        // Jumping with cooldown
        if (Input.GetButtonDown("Jump") && groundedPlayer && Time.time > lastJumpTime + jumpCooldown)
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2.0f * gravityValue);
            lastJumpTime = Time.time;
        }

        // Apply gravity
        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }
}