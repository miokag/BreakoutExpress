using UnityEngine;

namespace BreakoutExpress2D
{
    public class WailingWraith2D : MonoBehaviour
    {
        [Header("Movement Settings")] public float speed = 2f;
        public float patrolRange = 5f; // How far the ghost patrols from its starting point
        public bool startFacingRight = true;

        [Header("Slow Effect Settings")] public float slowRadius = 3f;
        public float slowAmount = 0.5f; // Multiplier for player speed (0.5 = 50% speed)

        [Header("References")] public Transform target; // Player reference
        public LayerMask playerLayer;

        private Rigidbody2D rb;
        private Vector2 startingPosition;
        private bool movingRight;
        private bool isSlowingPlayer = false;
        private PlayerController2D playerController;

        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            startingPosition = transform.position;
            movingRight = startFacingRight;

            if (target == null)
            {
                target = GameObject.FindGameObjectWithTag("Player").transform;
            }

            playerController = target.GetComponent<PlayerController2D>();
        }

        void Update()
        {
            PatrolMovement();
            CheckForPlayerInRange();
        }

        private void PatrolMovement()
        {
            // Change direction if reached patrol boundary
            if (movingRight && transform.position.x >= startingPosition.x + patrolRange)
            {
                movingRight = false;
            }
            else if (!movingRight && transform.position.x <= startingPosition.x - patrolRange)
            {
                movingRight = true;
            }

            // Move in current direction
            float moveDirection = movingRight ? 1f : -1f;
            rb.linearVelocity = new Vector2(moveDirection * speed, rb.linearVelocity.y);

            // Flip sprite if needed
            if ((moveDirection > 0 && transform.localScale.x < 0) ||
                (moveDirection < 0 && transform.localScale.x > 0))
            {
                Flip();
            }
        }

        private void CheckForPlayerInRange()
        {
            float distanceToPlayer = Vector2.Distance(transform.position, target.position);
            bool playerInRange = distanceToPlayer <= slowRadius;

            // Apply or remove slow effect
            if (playerInRange && !isSlowingPlayer)
            {
                ApplySlowEffect();
            }
            else if (!playerInRange && isSlowingPlayer)
            {
                RemoveSlowEffect();
            }
        }

        private void ApplySlowEffect()
        {
            if (playerController != null)
            {
                playerController.walkSpeed *= slowAmount;
                playerController.runSpeed *= slowAmount;
                isSlowingPlayer = true;
            }
        }

        private void RemoveSlowEffect()
        {
            if (playerController != null)
            {
                playerController.walkSpeed /= slowAmount;
                playerController.runSpeed *= slowAmount;
                isSlowingPlayer = false;
            }
        }

        private void Flip()
        {
            Vector3 scaler = transform.localScale;
            scaler.x *= -1;
            transform.localScale = scaler;
        }

        private void OnDisable()
        {
            if (isSlowingPlayer)
            {
                RemoveSlowEffect();
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Draw patrol range
            Gizmos.color = Color.blue;
            Vector3 startPos = Application.isPlaying ? startingPosition : transform.position;
            Gizmos.DrawLine(startPos + Vector3.left * patrolRange, startPos + Vector3.right * patrolRange);

            // Draw slow radius
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, slowRadius);
        }
    }
}