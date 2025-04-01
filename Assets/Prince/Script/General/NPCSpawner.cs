using System.Collections;
using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    public static NPCSpawner Instance;
    
    public GameObject npcPrefab;
    public float minSpawnDelay = 3f;
    public float maxSpawnDelay = 6f;
    public float spawnYPosition = 0f;
    public float despawnMargin = 2f;
    
    [Header("Detection Scaling")]
    public int detectionCount = 0;
    public float baseDetectionWidth = 15f;
    
    [Header("Progressive Scaling")]
    public int[] detectionThresholds = { 3, 6, 9 }; // Tier 0:0-2, Tier 1:3-5, Tier 2:6-8, Tier 3:9+
    public float[] widthIncreasePerTier = { 0f, 0.5f, 1f, 1.5f };
    public int[] maxPausesPerTier = { 3, 4, 5, 6 }; 
    public int[] pauseChancePerTier = { 30, 45, 60, 75 }; 

    private Camera mainCamera;
    private float cameraHalfWidth;
    private Coroutine spawnCoroutine;
    private bool isSpawning;

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        mainCamera = Camera.main;
        cameraHalfWidth = mainCamera.orthographicSize * mainCamera.aspect;
        isSpawning = false; // Reset spawning state when enabled
        spawnCoroutine = StartCoroutine(SpawnRoutine());
    }
    
    void OnDisable()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
        isSpawning = false;
    
        // Clean up any existing NPCs when disabled
        var npcs = GameObject.FindGameObjectsWithTag("NPC");
        foreach (var npc in npcs)
        {
            Destroy(npc);
        }
    }


    public IEnumerator SpawnRoutine()
    {
        while (true)
        {
            // Wait until all NPCs are gone and we're not already spawning
            yield return new WaitUntil(() => 
                GameObject.FindGameObjectsWithTag("NPC").Length == 0 && !isSpawning);
            
            isSpawning = true; // Mark that we're starting a spawn cycle
            
            float spawnDelay = Random.Range(minSpawnDelay, maxSpawnDelay);
            yield return new WaitForSeconds(spawnDelay);
            
            if (this.isActiveAndEnabled)
            {
                SpawnNPC();
            }
            
            isSpawning = false;
        }
    }

    void SpawnNPC()
    {
        float spawnX = mainCamera.transform.position.x - cameraHalfWidth - despawnMargin;
        Vector3 spawnPos = new Vector3(spawnX, spawnYPosition, 0);
        GameObject npc = Instantiate(npcPrefab, spawnPos, Quaternion.identity);
        npc.tag = "NPC";
    
        NPCMovement mover = npc.GetComponent<NPCMovement>();
        if (mover != null)
        {
            int currentTier = GetCurrentTier();
            int detectionsSinceLastThreshold = detectionCount - (currentTier > 0 ? detectionThresholds[currentTier-1] : 0);
            
            // Calculate width increase
            float widthIncrease = widthIncreasePerTier[currentTier] * detectionsSinceLastThreshold;
            float scaledWidth = baseDetectionWidth + widthIncrease;
            
            // Apply tier-based behavior
            mover.maxPauses = maxPausesPerTier[currentTier];
            mover.pauseChance = pauseChancePerTier[currentTier];
            mover.detectionWidth = scaledWidth;
            
            mover.Initialize(Vector2.right, mainCamera);
        }
    }

    int GetCurrentTier()
    {
        for (int i = 0; i < detectionThresholds.Length; i++)
        {
            if (detectionCount < detectionThresholds[i])
            {
                return i;
            }
        }
        return detectionThresholds.Length; 
    }

    public void IncreaseDetection()
    {
        detectionCount++;
        Debug.Log($"Detection increased to {detectionCount} (Tier {GetCurrentTier()})");
    }
}