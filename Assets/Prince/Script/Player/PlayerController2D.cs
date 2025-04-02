using System.Collections;
using UnityEngine;

namespace BreakoutExpress2D
{
    public class PlayerController2D : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float moveSpeed = 8f;
        public float jumpForce = 15f;
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
        private float groundRememberCounter;

        internal Rigidbody2D rb;
        private bool facingRight = true;
        private bool isGrounded;
        private bool wasGrounded;

        private Animator animator;

        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
        }

        void Update()
        {
            wasGrounded = isGrounded;
            HandleGroundCheck();
            HandleMovement();
            HandleJump();
            HandlePhysicsAdjustments();
            FlipCharacter();
            UpdateAnimations();
        }

        private void HandleGroundCheck()
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

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
            float currentSpeed = moveSpeed * (isGrounded ? 1f : airControlFactor);
            rb.linearVelocity = new Vector2(horizontal * currentSpeed, rb.linearVelocity.y);
            animator.SetFloat("xVelocity", Mathf.Abs(rb.linearVelocity.x));
        }

        private void HandleJump()
        {
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

        private void UpdateAnimations()
        {
            // Update jumping/falling state
            if (!isGrounded)
            {
                animator.SetBool("isJumping", true);
                
                // Optional: You can still use yVelocity if needed in your blend tree
                animator.SetFloat("yVelocity", rb.linearVelocity.y);
            }
            else if (wasGrounded != isGrounded) // Just landed
            {
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

        public void IncreaseSpeed(float boost, float duration)
        {
            StartCoroutine(SpeedBoost(boost, duration));
        }

        IEnumerator SpeedBoost(float boost, float duration)
        {
            moveSpeed += boost;
            yield return new WaitForSeconds(duration);
            moveSpeed -= boost;
        }
    }
}