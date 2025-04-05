using System.Collections.Generic;
using UnityEngine;

namespace BreakoutExpress
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        public float WalkSpeed = 6f;
        public float RunSpeed = 4f;
        
        [Header("Movement Settings")]
        private float walkSpeed => WalkSpeed;
        private float runSpeed => RunSpeed;
        [SerializeField] private float crouchSpeed = 3f;
        [SerializeField] private float jumpForce = 8f;
        [SerializeField] private float gravity = 20f;
        [SerializeField] private float groundCheckDistance = 0.2f;
        [SerializeField] private float crouchHeight = 1f;
        [SerializeField] private float movementSmoothing = 10f;
        [SerializeField] private float groundFriction = 0.9f;
        
        [Header("References")]
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private PlayerEffects playerEffects;
        
        [Header("State Flags")]
        [SerializeField] private bool canRun = true;
        public bool CanRun {
            get => canRun;
            set => canRun = value;
        }

        private CharacterController controller;
        private Vector3 velocity;
        private bool isGrounded;
        private bool isCrouching;
        private bool isRunning;
        private float originalHeight;
        private Vector3 originalCenter;
        
        private Transform currentPlatform;
        private Vector3 lastPlatformPosition;
        private Vector3 platformVelocity;

        private PlayerInput input;

        private void Start()
        {
            controller = GetComponent<CharacterController>();
            originalHeight = controller.height;
            originalCenter = controller.center;
    
            input = new PlayerInput();
            playerEffects = GetComponent<PlayerEffects>();
    
            if (cameraTransform == null)
            {
                cameraTransform = Camera.main?.transform;
            }
        }

        private void Update()
        {
            input.Gather();
            
            bool wasGrounded = isGrounded;
            isGrounded = CheckGrounded();
            
            HandlePlatformMovement();
            HandleLanding(wasGrounded);
            HandleMovement();
            HandleJumping();
            HandleCrouching();
            HandleRunning();
            HandleGravity();
            
            controller.Move(velocity * Time.deltaTime);
        }
        
        #region Platform Movement
        private void HandlePlatformMovement()
        {
            if (isGrounded)
            {
                // Check for moving platform
                if (Physics.SphereCast(transform.position + controller.center, 
                        controller.radius * 0.9f, Vector3.down, 
                        out RaycastHit hit, controller.height / 2 + groundCheckDistance))
                {
                    if (hit.transform != currentPlatform)
                    {
                        currentPlatform = hit.transform;
                        lastPlatformPosition = hit.transform.position;
                        return;
                    }

                    if (currentPlatform != null)
                    {
                        platformVelocity = currentPlatform.position - lastPlatformPosition;
                
                        // Only apply horizontal platform movement (optional)
                        platformVelocity.y = 0;
                
                        if (platformVelocity.magnitude > 0.001f)
                        {
                            controller.Move(platformVelocity);
                        }
                
                        lastPlatformPosition = currentPlatform.position;
                    }
                }
                else
                {
                    currentPlatform = null;
                }
            }
            else
            {
                currentPlatform = null;
            }
        }
        
        #endregion

        #region Input Handling
        private class PlayerInput
        {
            public float Horizontal { get; private set; }
            public float Vertical { get; private set; }
            public bool Jump { get; private set; }
            public bool Crouch { get; private set; }
            public bool Run { get; private set; }

            public void Gather()
            {
                Horizontal = Input.GetAxis("Horizontal");
                Vertical = Input.GetAxis("Vertical");
                Jump = Input.GetButtonDown("Jump");
                Crouch = Input.GetKey(KeyCode.LeftControl);
                Run = Input.GetKey(KeyCode.LeftShift);
            }
        }
        #endregion

        #region Movement
        private void HandleLanding(bool wasGrounded)
        {
            if (!wasGrounded && isGrounded)
            {
                // Reduce horizontal velocity more aggressively when landing
                velocity.x *= 0.5f;
                velocity.z *= 0.5f;
        
                // Optional: Add a small downward force to "stick" the landing
                velocity.y = -2f;
            }
        }

        private void HandleMovement()
        {
            Vector3 moveDirection = CalculateMovementDirection();
            float currentSpeed = GetCurrentSpeed();
            
            UpdateVelocity(moveDirection, currentSpeed);
            ApplyGroundFriction();
            RotateToMovementDirection(moveDirection);
            
            if (isGrounded && velocity.magnitude < 0.1f)
            {
                velocity = Vector3.zero;
            }
        }

        private Vector3 CalculateMovementDirection()
        {
            Vector3 cameraForward = FlattenVector(cameraTransform.forward);
            Vector3 cameraRight = FlattenVector(cameraTransform.right);
            
            return (cameraForward * input.Vertical + cameraRight * input.Horizontal).normalized;
        }

        private Vector3 FlattenVector(Vector3 vector)
        {
            vector.y = 0f;
            return vector.normalized;
        }

        private float GetCurrentSpeed()
        {
            return isCrouching ? crouchSpeed : (isRunning ? runSpeed : walkSpeed);
        }

        private void UpdateVelocity(Vector3 moveDirection, float speed)
        {
            velocity.x = moveDirection.x * speed;
            velocity.z = moveDirection.z * speed;
        }
        
        public void AddForce(Vector3 force)
        {
            velocity += force;
        }

        private void ApplyGroundFriction()
        {
            if (isGrounded)
            {
                velocity.x *= groundFriction;
                velocity.z *= groundFriction;
            }
        }

        private void RotateToMovementDirection(Vector3 moveDirection)
        {
            if (moveDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * movementSmoothing);
            }
        }
        #endregion

        #region Actions
        private void HandleJumping()
        {
            if (input.Jump && isGrounded && !isCrouching)
            {
                velocity.y = jumpForce;
            }
        }

        private void HandleCrouching()
        {
            if (input.Crouch && !isCrouching)
            {
                Debug.Log("StartCrouching");
                StartCrouch();
            }
            else if (!input.Crouch && isCrouching)
            {
                Debug.Log("EndCrouching");
                StopCrouch();
            }
        }

        private void StartCrouch()
        {
            isCrouching = true;
            controller.height = crouchHeight;
            controller.center = new Vector3(0, crouchHeight / 2, 0);
        }

        private void StopCrouch()
        {
            isCrouching = false;
            controller.height = originalHeight;
            controller.center = originalCenter;
        }

        private void HandleRunning()
        {
            isRunning = input.Run && !isCrouching && canRun;
        }

        private void HandleGravity()
        {
            if (!isGrounded)
            {
                velocity.y -= gravity * Time.deltaTime;
            }
        }
        #endregion
        

        #region Ground Check
        private bool CheckGrounded()
        {
            Vector3 rayStart = transform.position + controller.center;
            float rayLength = controller.height / 2 + groundCheckDistance;
    
            // Cast a sphere slightly smaller than the character controller's radius
            return Physics.SphereCast(rayStart, controller.radius * 0.9f, Vector3.down, 
                out RaycastHit hit, rayLength);
        }
        #endregion
    }
}