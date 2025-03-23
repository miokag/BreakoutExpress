using System.Collections.Generic;
using UnityEngine;

namespace BreakoutExpress
{
    public class PlayerController : MonoBehaviour
    {
        public float WalkSpeed = 6f;
        public float RunSpeed = 4f;

        private float walkSpeed => WalkSpeed;
        private float runSpeed => RunSpeed;
        [SerializeField] private float crouchSpeed = 3f;
        [SerializeField] private float jumpForce = 8f;
        [SerializeField] private float gravity = 20f;
        [SerializeField] private float groundCheckDistance = 0.2f;
        [SerializeField] private float crouchHeight = 1f;
        [SerializeField] private Transform cameraTransform;

        private CharacterController controller;
        private Vector3 velocity;
        private bool isGrounded;
        private bool isCrouching;
        private bool isRunning;
        private float originalHeight;
        private Vector3 originalCenter;

        // Input variables
        private float horizontal;
        private float vertical;
        private bool jumpInput;
        private bool crouchInput;
        private bool runInput;

        private List<Effect> activeEffects = new List<Effect>();

        void Start()
        {
            controller = GetComponent<CharacterController>();
            originalHeight = controller.height;
            originalCenter = controller.center; // Store the original center

            // If no camera transform is assigned, use the main camera
            if (cameraTransform == null)
            {
                cameraTransform = Camera.main.transform;
            }
        }

        void Update()
        {
            // Gather all inputs
            GatherInputs();

            // Check if the player is grounded
            bool wasGrounded = isGrounded; // Store the previous grounded state
            isGrounded = CheckGrounded();

            // Handle landing (reset horizontal velocity when landing)
            if (!wasGrounded && isGrounded)
            {
                velocity.x = 0f;
                velocity.z = 0f;
            }

            // Handle movement
            HandleMovement();

            // Handle jumping (only allow jumping if not crouching and grounded)
            if (jumpInput && isGrounded && !isCrouching)
            {
                velocity.y = jumpForce;
            }

            // Handle crouching
            if (crouchInput)
            {
                if (!isCrouching)
                {
                    StartCrouch();
                }
            }
            else
            {
                if (isCrouching)
                {
                    StopCrouch();
                }
            }

            // Handle running (prevent running while crouching)
            isRunning = runInput && !isCrouching;

            // Apply gravity
            if (!isGrounded)
            {
                velocity.y -= gravity * Time.deltaTime;
            }

            // Move the player
            controller.Move(velocity * Time.deltaTime);

            // Effect on player
            HandleEffects();
        }

        public void ApplyEffect(Effect effect)
        {
            effect.ApplyEffect(this);
            activeEffects.Add(effect);
        }

        private void HandleEffects()
        {
            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                Effect effect = activeEffects[i];
                if (Time.time > effect.StartTime + effect.Duration)
                {
                    effect.RemoveEffect(this);
                    activeEffects.RemoveAt(i);
                }
            }
        }

        public void ApplyForce(Vector3 force)
        {
            // Apply a force to the player (e.g., for pushback effects)
            GetComponent<CharacterController>().Move(force * Time.deltaTime);
        }

        private void GatherInputs()
        {
            // Movement inputs
            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");

            // Action inputs
            jumpInput = Input.GetButtonDown("Jump");
            crouchInput = Input.GetKey(KeyCode.LeftControl); // Hold Ctrl to crouch
            runInput = Input.GetKey(KeyCode.LeftShift);
        }

        private void HandleMovement()
        {
            // Get the camera's forward and right vectors, ignoring vertical rotation
            Vector3 cameraForward = cameraTransform.forward;
            Vector3 cameraRight = cameraTransform.right;
            cameraForward.y = 0f; // Flatten the vector to ignore camera tilt
            cameraRight.y = 0f;
            cameraForward.Normalize();
            cameraRight.Normalize();

            // Calculate movement direction relative to the camera
            Vector3 moveDirection = (cameraForward * vertical + cameraRight * horizontal).normalized;

            // Adjust speed based on crouching or running
            float currentSpeed = isCrouching ? crouchSpeed : (isRunning ? runSpeed : walkSpeed);

            // Update velocity based on movement direction
            velocity.x = moveDirection.x * currentSpeed;
            velocity.z = moveDirection.z * currentSpeed;

            // Apply friction when grounded
            if (isGrounded)
            {
                velocity.x *= 0.9f; // Reduce horizontal velocity by 10% each frame
                velocity.z *= 0.9f;
            }

            // Rotate the player to face the movement direction
            if (moveDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }
        }

        private void StartCrouch()
        {
            isCrouching = true;
            controller.height = crouchHeight;
            controller.center =
                new Vector3(0, crouchHeight / 2, 0); // Adjust the center to avoid sinking into the ground
        }

        private void StopCrouch()
        {
            isCrouching = false;
            controller.height = originalHeight;
            controller.center = originalCenter; // Reset to the original center
        }

        private bool CheckGrounded()
        {
            // Adjust the raycast starting position to be at the bottom of the player's collider
            Vector3 raycastStart = transform.position + controller.center - Vector3.up * (controller.height / 2);

            // Perform the raycast
            RaycastHit hit;
            bool isGrounded = Physics.Raycast(raycastStart, Vector3.down, out hit, groundCheckDistance);

            return isGrounded;
        }
    }
}