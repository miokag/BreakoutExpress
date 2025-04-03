using System.Collections;
using BreakoutExpress2D;
using UnityEngine;

public class NPCMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 2f;
    public float pauseDuration = 1f;

    [Header("Detection Settings")]
    public float detectionWidth = 30f;
    public float detectionHeight = 2f;
    public Vector2 detectionOffset = Vector2.zero;

    [Header("Behavior Settings")]
    [Range(0, 100)] public int pauseChance = 30;
    public int maxPauses = 3;
    public float minPauseInterval = 2f;
    public float movementThreshold = 0.1f;
    public float detectionStartDelay = 0.2f;

    [Header("Gizmo Settings")]
    public Color detectionColor = Color.yellow;
    public bool showGizmos = true;

    private Vector2 direction;
    private Camera mainCamera;
    
    private Transform player;
    private PlayerController2D playerController;
    
    private bool isPaused = false;
    private bool isCheckingPlayer = false;
    private int pauseCount = 0;
    private float lastPauseTime;
    
    private float cameraHalfWidth;
    private Vector2 playerPositionAtPause;
    
    private EyeFollow[] eyeFollows;
    private bool playerDetectedDuringPause = false;


    void Start()
    {
        mainCamera = Camera.main;
        cameraHalfWidth = mainCamera.orthographicSize * mainCamera.aspect;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerController = player.GetComponent<PlayerController2D>();
        
        // Get all EyeFollow components in children and disable them initially
        eyeFollows = GetComponentsInChildren<EyeFollow>(true);
        SetEyeFollowsEnabled(false);
    }


    public void Initialize(Vector2 moveDirection, Camera cam)
    {
        direction = moveDirection;
        mainCamera = cam;
        if (direction.x < 0) GetComponent<SpriteRenderer>().flipX = true;
    }

    void Update()
    {
        if (isPaused && isCheckingPlayer)
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
        if (pauseCount >= maxPauses) return false;
        if (Time.time - lastPauseTime < minPauseInterval) return false;
        if (Random.Range(0, 100) >= pauseChance) return false;

        return IsPlayerInDetectionZone();
    }

    bool IsPlayerInDetectionZone()
    {
        Vector2 playerPos = player.position;
        Vector2 zoneCenter = (Vector2)transform.position + detectionOffset;
        
        // Adjust zone direction based on NPC facing
        float directionMultiplier = direction.x > 0 ? 1 : -1;
        Vector2 adjustedOffset = new Vector2(detectionOffset.x * directionMultiplier, detectionOffset.y);
        zoneCenter = (Vector2)transform.position + adjustedOffset;

        return Mathf.Abs(playerPos.x - zoneCenter.x) < detectionWidth * 0.5f &&
               Mathf.Abs(playerPos.y - zoneCenter.y) < detectionHeight * 0.5f;
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
        bool playerMoved = Vector2.Distance(player.position, playerPositionAtPause) > movementThreshold ||
                           Mathf.Abs(playerController.rb.linearVelocity.x) > movementThreshold;

        if (playerMoved)
        {
            playerDetectedDuringPause = true;
            Debug.Log($"NPC noticed player moving after stopping!");
            NPCSpawner.Instance.IncreaseDetection();
            
            // Enable all eye follows when player is detected
            SetEyeFollowsEnabled(true);
        }
    }

    IEnumerator PauseMovement()
    {
        isPaused = true;
        playerDetectedDuringPause = false;
        pauseCount++;
        lastPauseTime = Time.time;
        float originalSpeed = speed;
        speed = 0f;

        yield return new WaitForSeconds(detectionStartDelay);
        
        playerPositionAtPause = player.position;
        isCheckingPlayer = true;

        yield return new WaitForSeconds(pauseDuration - detectionStartDelay);

        speed = originalSpeed;
        isPaused = false;
        isCheckingPlayer = false;
        
        // Disable all eye follows when pause ends if player wasn't detected
        if (!playerDetectedDuringPause)
        {
            SetEyeFollowsEnabled(false);
        }
    }
    
    private void SetEyeFollowsEnabled(bool enabled)
    {
        if (eyeFollows != null)
        {
            foreach (var eyeFollow in eyeFollows)
            {
                if (eyeFollow != null)
                {
                    eyeFollow.enabled = enabled;
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        Gizmos.color = detectionColor;
        
        float directionMultiplier = direction.x > 0 ? 1 : -1;
        Vector2 adjustedOffset = new Vector2(detectionOffset.x * directionMultiplier, detectionOffset.y);
        Vector2 zoneCenter = (Vector2)transform.position + adjustedOffset;

        Gizmos.DrawWireCube(zoneCenter, new Vector3(detectionWidth, detectionHeight, 0));
        
        Gizmos.color = Color.red;
        Vector3 forwardLineEnd = transform.position + (Vector3)(direction * 0.5f);
        Gizmos.DrawLine(transform.position, forwardLineEnd);
    }
}
