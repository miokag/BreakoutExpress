using System.Collections;
using UnityEngine;

public class NPCPatrolAndDetect : MonoBehaviour
{
    [Header("Patrol Settings")]
    public float patrolSpeed = 2f;
    public Vector2 patrolDirection = Vector2.right;

    [Header("Detection Settings")]
    public float detectionDistance = 5f;
    public float fieldOfViewAngle = 45f; // Detection cone angle in degrees
    public LayerMask obstacleMask;       // Optional: set if obstacles should block view

    [Header("Run Away Settings")]
    public float runSpeed = 5f;
    public float waitTimeBeforeRun = 1f; // NPC stops for 1 second before checking

    [Header("References")]
    public Transform player; // Assign your Player GameObject here (or tag your player "Player")

    private Rigidbody2D rb;
    private bool isScared = false;
    private bool isWaiting = false;
    private bool coroutineRunning = false; // Ensures only one coroutine runs at a time
    private bool hasReportedDetection = false; // Ensures this NPC reports detection only once

    // Variables for despawn behavior
    private float invisibleTimer = 0f;
    private bool isVisible = true; // Updated via OnBecameVisible/Invisible

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // Auto-find player if not assigned in Inspector
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        // Check if the NPC is off-screen and update the timer
        if (!isVisible)
        {
            invisibleTimer += Time.deltaTime;
            if (invisibleTimer >= 3f)
            {
                Destroy(gameObject);
                return;
            }
        }

        // If not scared and not waiting, patrol and check for the player
        if (!isScared && !isWaiting)
        {
            Patrol();
            DetectPlayer();
        }
        // If waiting, stop movement
        else if (isWaiting)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
        // If scared, run away from the player
        else if (isScared)
        {
            RunAway();
        }
    }

    /// <summary>
    /// Called when the renderer is no longer visible by any camera.
    /// </summary>
    void OnBecameInvisible()
    {
        isVisible = false;
        invisibleTimer = 0f; // Reset timer when first becoming invisible.
    }

    /// <summary>
    /// Called when the renderer becomes visible by any camera.
    /// </summary>
    void OnBecameVisible()
    {
        isVisible = true;
        invisibleTimer = 0f; // Reset timer when visible.
    }

    /// <summary>
    /// Makes the NPC patrol in a fixed direction.
    /// </summary>
    void Patrol()
    {
        rb.linearVelocity = new Vector2(patrolDirection.normalized.x * patrolSpeed, rb.linearVelocity.y);
    }

    /// <summary>
    /// Checks if the player is within detection range and in the NPC's field of view.
    /// If the player is moving, starts a wait-and-check routine.
    /// </summary>
    void DetectPlayer()
    {
        if (player == null)
            return;

        // Determine the vector and distance to the player.
        Vector2 toPlayer = player.position - transform.position;
        float distance = toPlayer.magnitude;
        if (distance > detectionDistance)
            return;

        // Check if the player is within the NPC's field of view (detection cone).
        float angleToPlayer = Vector2.Angle(patrolDirection, toPlayer);
        if (angleToPlayer > fieldOfViewAngle)
            return;

        // Optional: Raycast to check for obstacles blocking the view.
        if (obstacleMask != 0)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, toPlayer.normalized, detectionDistance, obstacleMask);
            if (hit.collider != null && hit.collider.transform != player)
                return;
        }

        // Check if the player is moving by examining the player's Rigidbody2D velocity.
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        if (playerRb != null && Mathf.Abs(playerRb.linearVelocity.x) > 0.01f)
        {
            // Start the waiting routine if not already running.
            if (!coroutineRunning)
            {
                StartCoroutine(WaitAndCheck());
            }
        }
    }

    /// <summary>
    /// Waits for a short period and then checks if the player is still moving.
    /// If so, reports detection (if not already done) and the NPC becomes scared;
    /// if not, it resumes patrolling.
    /// </summary>
    IEnumerator WaitAndCheck()
    {
        coroutineRunning = true;
        isWaiting = true;
        // Stop NPC movement immediately.
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        yield return new WaitForSeconds(waitTimeBeforeRun);

        isWaiting = false;
        // Check if the player is still moving.
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        if (playerRb != null && Mathf.Abs(playerRb.linearVelocity.x) > 0.01f)
        {
            // Report detection only once per NPC.
            if (!hasReportedDetection)
            {
                GameManager.Instance.RegisterDetection();
                hasReportedDetection = true;
            }
            isScared = true;
        }
        coroutineRunning = false;
    }

    /// <summary>
    /// Makes the NPC run away from the player.
    /// </summary>
    void RunAway()
    {
        Vector2 runDirection = (transform.position - player.position).normalized;
        rb.linearVelocity = new Vector2(runDirection.x * runSpeed, rb.linearVelocity.y);
    }
}
