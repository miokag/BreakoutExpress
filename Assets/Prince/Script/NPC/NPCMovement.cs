using System.Collections;
using BreakoutExpress2D;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

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
    
    [Header("Vignette Settings")]
    [SerializeField]
    internal float maxVignetteIntensity = 0.3f;
    [SerializeField] private float vignetteChangeSpeed = 2f;

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
    
    // Vignette
    
    private Volume globalVolume;
    private Vignette vignette;
    private float originalVignetteIntensity;
    private float targetVignetteIntensity;
    private bool playerInRange;


    void Start()
    {
        mainCamera = Camera.main;
        cameraHalfWidth = mainCamera.orthographicSize * mainCamera.aspect;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerController = player.GetComponent<PlayerController2D>();
        
        // Get all EyeFollow components in children and disable them initially
        eyeFollows = GetComponentsInChildren<EyeFollow>(true);
        SetEyeFollowsEnabled(false);
        
        FindAndSetupVignette();
    }
    
    private void FindAndSetupVignette()
    {
        globalVolume = FindObjectOfType<Volume>();
        if (globalVolume == null || globalVolume.profile == null) return;

        if (!globalVolume.profile.TryGet(out vignette))
        {
            Debug.Log("Adding Vignette override to Volume profile");
            vignette = globalVolume.profile.Add<Vignette>(true);
            vignette.active = true;
        }

        if (vignette != null)
        {
            originalVignetteIntensity = vignette.intensity.value;
        }
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
        
        // Update vignette effect
        if (vignette != null)
        {
            vignette.intensity.value = Mathf.Lerp(
                vignette.intensity.value, 
                targetVignetteIntensity, 
                vignetteChangeSpeed * Time.deltaTime
            );
        }
        
        // Reset vignette when NPC moves out of range
        if (!IsPlayerInDetectionZone() && !playerDetectedDuringPause)
        {
            targetVignetteIntensity = originalVignetteIntensity;
        }
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
            // Only intensify vignette if player is detected moving
            targetVignetteIntensity = maxVignetteIntensity;
            playerDetectedDuringPause = true;
            Debug.Log($"NPC noticed player moving after stopping!");
            NPCSpawner.Instance.IncreaseDetection();
            
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

