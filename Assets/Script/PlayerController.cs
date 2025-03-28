using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    public float jumpForce = 15f;

    [Header("Ground Check Settings")]
    public Transform groundCheck;        // Empty child at the player's feet
    public float groundCheckRadius = 0.2f; // Radius for ground detection
    public LayerMask groundLayer;        // Layer assigned to ground objects

    private Rigidbody2D rb;
    private bool facingRight = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
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

    void Update()
    {
        // Get horizontal input and update horizontal velocity.
        float horizontal = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(horizontal * moveSpeed, rb.linearVelocity.y);

        // Use an OverlapCircle for ground detection.
        // Also require that vertical velocity is near zero or negative so that we don't consider the player grounded when jumping.
        bool isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer)
                          && rb.linearVelocity.y <= 0.1f;

        // Jump if Space is pressed and the player is grounded.
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        // For a variable jump height, reduce upward velocity if the jump key is released early.
        if (Input.GetKeyUp(KeyCode.Space) && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
        }

        // Flip the sprite based on the direction of movement.
        if (horizontal > 0 && !facingRight)
            Flip();
        else if (horizontal < 0 && facingRight)
            Flip();
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }
}
