using UnityEngine;
using System.Collections;

namespace BreakoutExpress2D
{
    public class ScreamingBanshee2D : MonoBehaviour
    {
        [Header("Movement Settings")] 
        public float speed = 2f;
        public float patrolRange = 5f;
        public bool startFacingRight = true;

        [Header("Scream Attack")]
        public float screamRange = 4f;
        public float pushbackForce = 10f;
        public float screamCooldown = 3f;
        public float screamDuration = 1f;
        public GameObject screamEffectPrefab;

        private Rigidbody2D rb;
        private Vector2 startingPosition;
        private bool movingRight;
        private float lastScreamTime;
        private bool isScreaming;
        private Transform player;

        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            startingPosition = transform.position;
            movingRight = startFacingRight;
            player = GameObject.FindGameObjectWithTag("Player").transform;
            Debug.Log("Banshee initialized. Player found: " + (player != null));
        }

        void Update()
        {
            if (!isScreaming)
            {
                PatrolMovement();
                CheckForScreamAttack();
            }
        }

        private void PatrolMovement()
        {
            if (movingRight && transform.position.x >= startingPosition.x + patrolRange)
                movingRight = false;
            else if (!movingRight && transform.position.x <= startingPosition.x - patrolRange)
                movingRight = true;

            float moveDirection = movingRight ? 1f : -1f;
            rb.linearVelocity = new Vector2(moveDirection * speed, rb.linearVelocity.y);

            if ((moveDirection > 0 && transform.localScale.x < 0) || 
                (moveDirection < 0 && transform.localScale.x > 0))
            {
                Flip();
            }
        }

        private void CheckForScreamAttack()
        {
            if (Time.time < lastScreamTime + screamCooldown) return;

            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            Debug.Log("Distance to player: " + distanceToPlayer);

            if (distanceToPlayer > screamRange) return;

            bool playerInFront = (movingRight && player.position.x > transform.position.x) || 
                               (!movingRight && player.position.x < transform.position.x);
            
            Debug.Log("Player in front: " + playerInFront);
            if (playerInFront)
            {
                StartCoroutine(PerformScreamAttack());
            }
        }

        private IEnumerator PerformScreamAttack()
        {
            isScreaming = true;
            lastScreamTime = Time.time;
            rb.linearVelocity = Vector2.zero;
            Debug.Log("SCREAM ATTACK STARTED!");

            // Effect
            if (screamEffectPrefab != null)
            {
                GameObject effect = Instantiate(screamEffectPrefab, transform.position, Quaternion.identity);
                effect.transform.localScale = new Vector3(movingRight ? 1 : -1, 1, 1);
                Destroy(effect, screamDuration);
            }

            // Get player Rigidbody2D first
            Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
            if (playerRb == null)
            {
                Debug.LogError("Player Rigidbody2D missing!");
                yield break;
            }

            // Calculate direction - simpler and more reliable
            Vector2 pushDirection = movingRight ? Vector2.right : Vector2.left;
            Debug.Log("Push direction: " + pushDirection + " | Force: " + pushbackForce);

            // Reset player velocity first for consistent knockback
            playerRb.linearVelocity = Vector2.zero;
    
            // Apply force in one frame using Impulse
            playerRb.AddForce(pushDirection * pushbackForce, ForceMode2D.Impulse);
    
            // Optional: Add slight upward force for arc effect
            playerRb.AddForce(Vector2.up * (pushbackForce * 0.3f), ForceMode2D.Impulse);

            Debug.Log("Force applied! Player velocity: " + playerRb.linearVelocity);

            yield return new WaitForSeconds(screamDuration);
            isScreaming = false;
        }

        private void Flip()
        {
            movingRight = !movingRight;
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
            Debug.Log("Flipped! Now facing: " + (movingRight ? "right" : "left"));
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Vector3 startPos = Application.isPlaying ? startingPosition : transform.position;
            Gizmos.DrawLine(startPos + Vector3.left * patrolRange, startPos + Vector3.right * patrolRange);

            Gizmos.color = Color.red;
            Vector2 screamDir = movingRight ? Vector2.right : Vector2.left;
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)screamDir * screamRange);
        }
    }
}