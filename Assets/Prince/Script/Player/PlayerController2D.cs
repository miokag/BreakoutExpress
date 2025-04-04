using System.Collections;
using UnityEngine;

namespace BreakoutExpress2D
{
    public class PlayerController2D : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float walkSpeed = 8f;
        public float runSpeed = 12f;
        [SerializeField] private float crouchSpeed = 4f;
        [SerializeField] private float jumpForce = 15f;
        [SerializeField] private float airControlFactor = 0.8f; 

        [Header("Jump Settings")]
        [SerializeField] private float jumpCutMultiplier = 0.5f; 
        [SerializeField] private float fallMultiplier = 2.5f; 
        [SerializeField] private float lowJumpMultiplier = 2f; 

        [Header("Ground Check")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundCheckRadius = 0.2f;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float groundRememberTime = 0.1f; 
        [SerializeField] private float crouchedGroundCheckRadius = 0.1f;
        private float groundRememberCounter;
        
        [Header("Crouch Settings")]
        [SerializeField] private float crouchHeight = 0.5f;
        [SerializeField] private Vector2 crouchOffset = new Vector2(0, -0.25f);
        [SerializeField] private Sprite crouchSprite;
        private Sprite originalSprite;
        private float originalHeight;
        private Vector2 originalOffset;
        
        [Header("Visual Effects")]
        [SerializeField] private float idleFadeOpacity = 0.7f;
        [SerializeField] private float fadeTransitionSpeed = 5f;
        
        private SpriteRenderer spriteRenderer;
        private BoxCollider2D playerCollider;
        private Color defaultColor;
        private Color fadedColor;
        private bool isMoving;
        private bool isJumping;
        private bool isCrouching;
        private bool isRunning;
        private bool canRun = true;

        internal Rigidbody2D rb;
        private bool facingRight = true;
        private bool isGrounded;
        private bool wasGrounded;

        private Animator animator;
        private bool animatorWasEnabled;
        
        [SerializeField] private Vector2 groundCheckOffset = Vector2.zero;
        [SerializeField] private Vector2 crouchedGroundCheckOffset = new Vector2(0, -0.2f);

        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            playerCollider = GetComponent<BoxCollider2D>();
            
            // Store original collider values
            originalHeight = playerCollider.size.y;
            originalOffset = playerCollider.offset;
            
            // Get the SpriteRenderer and store default color
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                defaultColor = spriteRenderer.color;
                fadedColor = new Color(defaultColor.r, defaultColor.g, defaultColor.b, idleFadeOpacity);
                originalSprite = spriteRenderer.sprite;
            }
            
            originalHeight = playerCollider.size.y;
            originalOffset = playerCollider.offset;
            animatorWasEnabled = animator.enabled;
        }

        void Update()
        {
            wasGrounded = isGrounded;
            HandleGroundCheck();
            HandleCrouching();
            HandleRunning();
            HandleMovement();
            HandleJump();
            HandlePhysicsAdjustments();
            FlipCharacter();
            UpdateAnimations();
            HandleOpacity();
        }

        private void HandleGroundCheck()
        {
            // Adjust ground check position based on crouch state
            Vector2 checkPosition = (Vector2)groundCheck.position + 
                                    (isCrouching ? crouchedGroundCheckOffset : groundCheckOffset);
            float radius = isCrouching ? crouchedGroundCheckRadius : groundCheckRadius;

            isGrounded = Physics2D.OverlapCircle(checkPosition, radius, groundLayer);

            if (isGrounded)
            {
                groundRememberCounter = groundRememberTime;
            }
            else
            {
                groundRememberCounter -= Time.deltaTime;
            }
        }

        private void HandleMovement()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            
            // Determine current speed based on state
            float currentSpeed;
            if (isCrouching)
                currentSpeed = crouchSpeed;
            else if (isRunning && canRun)
                currentSpeed = runSpeed;
            else
                currentSpeed = walkSpeed;
            
            // Apply air control factor if not grounded
            currentSpeed *= (isGrounded ? 1f : airControlFactor);
            
            rb.linearVelocity = new Vector2(horizontal * currentSpeed, rb.linearVelocity.y);
            
            // Update movement state
            isMoving = Mathf.Abs(horizontal) > 0.1f || !isGrounded;
            animator.SetFloat("xVelocity", Mathf.Abs(rb.linearVelocity.x));
        }

        private void HandleJump()
        {
            // Can't jump while crouching
            if (isCrouching) return;

            // Jump if grounded and pressed jump
            if (Input.GetKeyDown(KeyCode.Space) && groundRememberCounter > 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                groundRememberCounter = 0;
            }

            // Variable jump height
            if (Input.GetKeyUp(KeyCode.Space) && rb.linearVelocity.y > 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
            }
        }

        private void HandleCrouching()
        {
            bool wantsToCrouch = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.DownArrow);
            
            // Only crouch if grounded
            if (wantsToCrouch && isGrounded && !isCrouching)
            {
                StartCrouch();
            }
            else if (!wantsToCrouch && isCrouching)
            {
                // Check if there's room to stand up
                if (!CheckHeadClearance())
                {
                    return; // Can't stand up if something is above
                }
                StopCrouch();
            }
        }

        private void StartCrouch()
        {
            isCrouching = true;
            playerCollider.size = new Vector2(playerCollider.size.x, crouchHeight);
            playerCollider.offset = crouchOffset;
            
            // Change to crouch sprite and disable animator
            if (crouchSprite != null)
            {
                spriteRenderer.sprite = crouchSprite;
            }
            
            // Disable animator if it exists
            if (animator != null)
            {
                animator.enabled = false;
            }
            
            // Move the ground check down slightly when crouching
            groundCheck.localPosition += (Vector3)crouchedGroundCheckOffset;
        }

        private void StopCrouch()
        {
            isCrouching = false;
            playerCollider.size = new Vector2(playerCollider.size.x, originalHeight);
            playerCollider.offset = originalOffset;
            
            // Restore original sprite and animator
            spriteRenderer.sprite = originalSprite;
            if (animator != null)
            {
                animator.enabled = animatorWasEnabled;
            }
            
            // Reset the ground check position
            groundCheck.localPosition -= (Vector3)crouchedGroundCheckOffset;
        }


        private bool CheckHeadClearance()
        {
            // Cast a ray upward to check if there's space to stand up
            float rayLength = originalHeight - crouchHeight;
            RaycastHit2D hit = Physics2D.Raycast(
                transform.position, 
                Vector2.up, 
                rayLength, 
                groundLayer);
            
            return hit.collider == null;
        }

        private void HandleRunning()
        {
            isRunning = Input.GetKey(KeyCode.LeftShift) && !isCrouching && canRun;
            animator.SetBool("isRunning", isRunning);
        }

        private void UpdateAnimations()
        {
            // Update jumping/falling state
            if (!isGrounded)
            {
                isJumping = true;
                animator.SetBool("isJumping", true);
                animator.SetFloat("yVelocity", rb.linearVelocity.y);
            }
            else if (wasGrounded != isGrounded) // Just landed
            {
                isJumping = false;
                animator.SetBool("isJumping", false);
            }
        }

        private void HandlePhysicsAdjustments()
        {
            // Faster falling
            if (rb.linearVelocity.y < 0)
            {
                rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
            }
            // Short hop control
            else if (rb.linearVelocity.y > 0 && !Input.GetKey(KeyCode.Space))
            {
                rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
            }
        }

        private void FlipCharacter()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            if (horizontal > 0 && !facingRight)
            {
                Flip();
            }
            else if (horizontal < 0 && facingRight)
            {
                Flip();
            }
        }

        private void Flip()
        {
            facingRight = !facingRight;
            Vector3 scaler = transform.localScale;
            scaler.x *= -1;
            transform.localScale = scaler;
        }
        
        private void HandleOpacity()
        {
            if (spriteRenderer == null) return;

            // Determine target opacity based on player activity
            float targetAlpha = (isMoving || isJumping) ? defaultColor.a : idleFadeOpacity;
            
            // Smoothly transition between opacities
            Color currentColor = spriteRenderer.color;
            Color targetColor = new Color(currentColor.r, currentColor.g, currentColor.b, targetAlpha);
            
            spriteRenderer.color = Color.Lerp(currentColor, targetColor, fadeTransitionSpeed * Time.deltaTime);
        }

        public void IncreaseSpeed(float boost, float duration)
        {
            StartCoroutine(SpeedBoost(boost, duration));
        }

        IEnumerator SpeedBoost(float boost, float duration)
        {
            walkSpeed += boost;
            runSpeed += boost;
            yield return new WaitForSeconds(duration);
            walkSpeed -= boost;
            runSpeed -= boost;
        }
    }
}