using System.Collections;
using BreakoutExpress2D;
using UnityEngine;

public class NPCMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 2f;
    public float pauseDuration = 1f;
    public float detectionRange = 3f;

    [Header("Behavior Settings")]
    [Range(0, 100)] public int pauseChance = 30;
    public int maxPauses = 3;
    public float minPauseInterval = 2f;
    public float movementThreshold = 0.1f; // Minimum player movement to detect

    [Header("Gizmo Settings")]
    public Color detectionColor = Color.yellow;

    private Vector2 direction;
    private Camera mainCamera;
    private Transform player;
    private PlayerController2D playerController;
    private bool isPaused = false;
    private int pauseCount = 0;
    private float lastPauseTime;
    private float cameraHalfWidth;
    private Vector2 playerPositionAtPause;

    void Start()
    {
        mainCamera = Camera.main;
        cameraHalfWidth = mainCamera.orthographicSize * mainCamera.aspect;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerController = player.GetComponent<PlayerController2D>();
    }

    public void Initialize(Vector2 moveDirection, Camera cam)
    {
        direction = moveDirection;
        mainCamera = cam;
        if (direction.x < 0) GetComponent<SpriteRenderer>().flipX = true;
    }

    void Update()
    {
        if (isPaused)
        {
            CheckPlayerMovementDuringPause();
            return;
        }

        if (ShouldPause())
        {
            StartCoroutine(PauseMovement());
            return;
        }

        MoveAndDespawn();
    }

    bool ShouldPause()
    {
        return Vector2.Distance(transform.position, player.position) < detectionRange &&
               pauseCount < maxPauses &&
               Time.time - lastPauseTime > minPauseInterval &&
               Random.Range(0, 100) < pauseChance;
    }

    void MoveAndDespawn()
    {
        transform.Translate(direction * speed * Time.deltaTime);
        
        float cameraRightEdge = mainCamera.transform.position.x + cameraHalfWidth + 2f;
        float cameraLeftEdge = mainCamera.transform.position.x - cameraHalfWidth - 2f;
        
        if ((direction.x > 0 && transform.position.x > cameraRightEdge) ||
            (direction.x < 0 && transform.position.x < cameraLeftEdge))
        {
            Destroy(gameObject);
        }
    }

    void CheckPlayerMovementDuringPause()
    {
        // Check both position change and velocity
        bool playerMoved = Vector2.Distance(player.position, playerPositionAtPause) > movementThreshold ||
                          Mathf.Abs(playerController.rb.linearVelocity.x) > movementThreshold;

        if (playerMoved)
        {
            Debug.Log($"NPC noticed player moving! (Position change: {Vector2.Distance(player.position, playerPositionAtPause):F2}, " +
                     $"Velocity: {playerController.rb.linearVelocity.x:F2})");
        }
    }

    IEnumerator PauseMovement()
    {
        isPaused = true;
        pauseCount++;
        lastPauseTime = Time.time;
        playerPositionAtPause = player.position;
        float originalSpeed = speed;
        speed = 0f;

        Debug.Log($"NPC paused ({pauseCount}/{maxPauses}) at player position: {playerPositionAtPause}");

        yield return new WaitForSeconds(pauseDuration);

        speed = originalSpeed;
        isPaused = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = detectionColor;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}