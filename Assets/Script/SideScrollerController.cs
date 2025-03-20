using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SideScrollerController : MonoBehaviour
{
    public float moveSpeed = 5f;    // Movement speed
    public float jumpSpeed = 5f;    // Jump force
    public float gravity = 9.81f;   // Gravity

    private CharacterController controller;
    private float verticalVelocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        // Orient the player so that transform.forward is the desired forward direction.
        transform.rotation = Quaternion.Euler(0f, 0f, 0f);
    }

    void Update()
    {
        // Use the "Vertical" axis for forward/backward movement (W = +1, S = -1)
        float moveInput = Input.GetAxis("Vertical");

        // Move along transform.forward
        Vector3 move = transform.right * moveInput;

        // Handle jumping:
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

        // Apply vertical movement (jumping/falling)
        move.y = verticalVelocity;

        // Move the controller
        controller.Move(move * moveSpeed * Time.deltaTime);
    }
}
