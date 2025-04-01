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
    public float baseDetectionWidth = 3f;
    public float maxDetectionWidth = 6f;
    public float widthIncreasePerDetection = 0.5f;

    private Camera mainCamera;
    private float cameraHalfWidth;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        mainCamera = Camera.main;
        cameraHalfWidth = mainCamera.orthographicSize * mainCamera.aspect;
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitUntil(() => GameObject.FindGameObjectsWithTag("NPC").Length == 0);
            
            float spawnDelay = Random.Range(minSpawnDelay, maxSpawnDelay);
            yield return new WaitForSeconds(spawnDelay);
            
            SpawnNPC();
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
            // Use the prefab's detectionWidth as base, then add scaling
            float scaledWidth = Mathf.Min(
                mover.detectionWidth + (detectionCount * widthIncreasePerDetection),
                mover.detectionWidth * 2f // Or whatever maximum multiplier you want
            );
            mover.detectionWidth = scaledWidth;
            mover.Initialize(Vector2.right, mainCamera);
        
            Debug.Log($"Spawned NPC with detection width: {scaledWidth} " +
                      $"(Base: {mover.detectionWidth}, Bonus: {detectionCount * widthIncreasePerDetection})");
        }
    }

    public void IncreaseDetection()
    {
        detectionCount++;
        Debug.Log($"Detection increased! Count: {detectionCount}");
    }
}
