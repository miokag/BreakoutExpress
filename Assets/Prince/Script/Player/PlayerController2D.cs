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

        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        void Update()
        {
            HandleMovement();
            HandleJump();
            HandlePhysicsAdjustments();
            FlipCharacter();
        }

        private void HandleMovement()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float currentSpeed = moveSpeed * (isGrounded ? 1f : airControlFactor);
            rb.linearVelocity = new Vector2(horizontal * currentSpeed, rb.linearVelocity.y);
        }

        private void HandleJump()
        {
            // Coyote time - remember grounded state briefly after leaving ground
            bool wasGrounded = isGrounded;
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

            if (isGrounded)
            {
                groundRememberCounter = groundRememberTime;
            }
            else
            {
                groundRememberCounter -= Time.deltaTime;
            }

            // Jump if grounded and pressed jump
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (groundRememberCounter > 0)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                    groundRememberCounter = 0; // Reset coyote time
                }
            }

            // Variable jump height
            if (Input.GetKeyUp(KeyCode.Space))
            {
                if (rb.linearVelocity.y > 0)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
                }
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
