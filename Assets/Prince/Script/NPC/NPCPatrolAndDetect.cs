using System.Collections;
using UnityEngine;

public class NPCMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 2f;
    public float pauseDuration = 1f;
    public float detectionRange = 3f;

    [Header("Behavior Settings")]
    [Range(0, 100)] public int pauseChance = 30; // 30% chance to pause when in range
    public int maxPauses = 3; // Max times an NPC will pause
    public float minPauseInterval = 2f; // Minimum time between possible pauses

    [Header("Gizmo Settings")]
    public Color detectionColor = Color.yellow;

    private Vector2 direction;
    private Camera mainCamera;
    private Transform player;
    private bool isPaused = false;
    private int pauseCount = 0;
    private float lastPauseTime;
    private float cameraHalfWidth;

    void Start()
    {
        mainCamera = Camera.main;
        cameraHalfWidth = mainCamera.orthographicSize * mainCamera.aspect;
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    public void Initialize(Vector2 moveDirection, Camera cam)
    {
        direction = moveDirection;
        mainCamera = cam;
        if (direction.x < 0) GetComponent<SpriteRenderer>().flipX = true;
    }

    void Update()
    {
        if (isPaused) return;

        // Check for player in range and pause conditions
        if (Vector2.Distance(transform.position, player.position) < detectionRange &&
            pauseCount < maxPauses &&
            Time.time - lastPauseTime > minPauseInterval &&
            Random.Range(0, 100) < pauseChance)
        {
            StartCoroutine(PauseMovement());
            return;
        }

        // Normal movement
        transform.Translate(direction * speed * Time.deltaTime);

        // Despawn check
        float cameraRightEdge = mainCamera.transform.position.x + cameraHalfWidth + 2f;
        float cameraLeftEdge = mainCamera.transform.position.x - cameraHalfWidth - 2f;
        
        if ((direction.x > 0 && transform.position.x > cameraRightEdge) ||
            (direction.x < 0 && transform.position.x < cameraLeftEdge))
        {
            Destroy(gameObject);
        }
    }

    IEnumerator PauseMovement()
    {
        isPaused = true;
        pauseCount++;
        lastPauseTime = Time.time;
        float originalSpeed = speed;
        speed = 0f;

        Debug.Log($"NPC paused ({pauseCount}/{maxPauses})");

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