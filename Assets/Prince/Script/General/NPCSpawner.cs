using System.Collections;
using BreakoutExpress;
using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    public static NPCSpawner Instance;
    
    [Header("NPC Settings")]
    public GameObject[] npcPrefabs; // Array of possible NPC prefabs to spawn
    public float minSpawnDelay = 3f;
    public float maxSpawnDelay = 6f;
    public float spawnYPosition = 0f;
    public float despawnMargin = 2f;
    
    [Header("Detection Scaling")]
    public int detectionCount = 0;
    
    [Header("Progressive Scaling")]
    public int[] detectionThresholds = { 3, 6, 9 }; // Tier 0:0-2, Tier 1:3-5, Tier 2:6-8, Tier 3:9+
    public int[] maxPausesPerTier = { 3, 4, 5, 6 }; 
    public int[] pauseChancePerTier = { 30, 45, 60, 75 };
    
    [Header("Vignette Scaling")]
    public float[] maxIntensityPerTier = { 0.3f, 0.4f, 0.5f, 0.6f };
    public float[] intensityIncreasePerTier = { 0f, 0.1f, 0.2f, 0.3f };
    
    [Header("Time Penalty Settings")]
    public float[] timePenaltyPerTier = { 3f, 5f, 7f, 10f }; // Tier 0-3 penalties

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
        isSpawning = false;
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
            
            isSpawning = true;
            
            float spawnDelay = Random.Range(minSpawnDelay, maxSpawnDelay);
            yield return new WaitForSeconds(spawnDelay);
            
            if (this.isActiveAndEnabled && npcPrefabs.Length > 0)
            {
                SpawnNPC();
            }
            
            isSpawning = false;
        }
    }

    void SpawnNPC()
    {
        // Select a random NPC prefab from the array
        GameObject npcPrefab = npcPrefabs[Random.Range(0, npcPrefabs.Length)];
        
        float spawnX = mainCamera.transform.position.x - cameraHalfWidth - despawnMargin;
        Vector3 spawnPos = new Vector3(spawnX, spawnYPosition, 0);
        GameObject npc = Instantiate(npcPrefab, spawnPos, Quaternion.identity);
        npc.tag = "NPC";
    
        NPCMovement mover = npc.GetComponent<NPCMovement>();
        if (mover != null)
        {
            int currentTier = GetCurrentTier();
        
            // Apply tier-based vignette scaling
            mover.maxVignetteIntensity = maxIntensityPerTier[currentTier];
            mover.maxVignetteIntensity += intensityIncreasePerTier[currentTier];
            
            // Calculate ADDITIVE width increase (preserves inspector value)
            float widthIncrease = 1;
            mover.detectionWidth += widthIncrease;
        
            // Apply other tier-based behavior
            mover.maxPauses = maxPausesPerTier[currentTier];
            mover.pauseChance = pauseChancePerTier[currentTier];
        
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
        int currentTier = GetCurrentTier();
        float penalty = timePenaltyPerTier[currentTier];
    
        // Find EscapeTimer in the scene
        EscapeTimer timer = FindObjectOfType<EscapeTimer>();
        if (timer != null)
        {
            timer.ModifyTime(-penalty);
        }
    
        Debug.Log($"Detection increased to {detectionCount} (Tier {currentTier}). Time penalty: -{penalty} seconds");
    }
}