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

        private CharacterController controller;
        private Vector3 velocity;
        private bool isGrounded;
        private bool isCrouching;
        private bool isRunning;
        private float originalHeight;
        private Vector3 originalCenter;

        private PlayerInput input;
        private List<Effect> activeEffects = new List<Effect>();

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            originalHeight = controller.height;
            originalCenter = controller.center;
            
            input = new PlayerInput();
            
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
            
            HandleLanding(wasGrounded);
            HandleMovement();
            HandleJumping();
            HandleCrouching();
            HandleRunning();
            HandleGravity();
            
            controller.Move(velocity * Time.deltaTime);
            HandleEffects();
        }

        #region Public Methods
        public void ApplyEffect(Effect effect)
        {
            effect.ApplyEffect(this);
            activeEffects.Add(effect);
        }

        public void ApplyForce(Vector3 force)
        {
            controller.Move(force * Time.deltaTime);
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
                velocity.x = 0f;
                velocity.z = 0f;
            }
        }

        private void HandleMovement()
        {
            Vector3 moveDirection = CalculateMovementDirection();
            float currentSpeed = GetCurrentSpeed();
            
            UpdateVelocity(moveDirection, currentSpeed);
            ApplyGroundFriction();
            RotateToMovementDirection(moveDirection);
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
                StartCrouch();
            }
            else if (!input.Crouch && isCrouching)
            {
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
            isRunning = input.Run && !isCrouching;
        }

        private void HandleGravity()
        {
            if (!isGrounded)
            {
                velocity.y -= gravity * Time.deltaTime;
            }
        }
        #endregion

        #region Effects
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
        #endregion

        #region Ground Check
        private bool CheckGrounded()
        {
            Vector3 raycastStart = transform.position + controller.center - Vector3.up * (controller.height / 2);
            return Physics.Raycast(raycastStart, Vector3.down, out _, groundCheckDistance);
        }
        #endregion
    }
}