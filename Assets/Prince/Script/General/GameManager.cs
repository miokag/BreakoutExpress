using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Wave Settings")]
    public int currentWave = 1;
    public float timeBetweenWaves = 3f;  // Extra wait time after the wave finishes spawning

    [Header("NPC Spawning Settings")]
    public GameObject npcPrefab;         // Prefab for NPC to spawn (assign in Inspector)
    public float offscreenOffset = 1f;     // How far outside the camera view to spawn NPCs
    public float spawnY = 0f;            // Fixed y-coordinate for NPC spawning
    public float spawnDelay = 0.5f;       // Delay between each NPC spawn within a wave

    [Header("Game Over Settings")]
    public int detectionCountThreshold = 3;  // When reached, triggers game over
    public int detectionCount = 0;
    public bool isGameOver = false;

    void Awake()
    {
        // Singleton pattern for GameManager
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        StartCoroutine(WaveSpawner());
    }

    /// <summary>
    /// Continuously spawns waves until the game is over.
    /// Waits until all NPCs from the previous wave are gone before starting the next wave.
    /// </summary>
    IEnumerator WaveSpawner()
    {
        // Optional: initial delay before waves start.
        yield return new WaitForSeconds(1f);

        while (!isGameOver)
        {
            // Wait until there are no NPCs in the scene.
            yield return new WaitUntil(() => GameObject.FindGameObjectsWithTag("NPC").Length == 0);

            // Determine number of NPCs for this wave (capped at 3).
            int waveCount = Mathf.Min(currentWave, 3);
            Debug.Log("Starting Wave " + currentWave + " with " + waveCount + " NPC(s).");

            // Spawn the wave with sequential spawning and delays.
            yield return StartCoroutine(SpawnWaveCoroutine(waveCount));

            // Wait extra time after the wave spawns.
            yield return new WaitForSeconds(timeBetweenWaves);

            // Move to the next wave; if past wave 3, reset back to wave 1.
            currentWave++;
            if (currentWave > 3)
                currentWave = 1;
        }
    }

    /// <summary>
    /// Coroutine that spawns NPCs sequentially with a delay between each.
    /// NPCs are split between left and right spawns.
    /// </summary>
    IEnumerator SpawnWaveCoroutine(int numberToSpawn)
    {
        // Calculate how many NPCs spawn from the left and right.
        int leftCount = numberToSpawn / 2;
        int rightCount = numberToSpawn / 2;
        if (numberToSpawn % 2 != 0)
        {
            if (Random.value < 0.5f)
                leftCount++;
            else
                rightCount++;
        }

        // Spawn NPCs from the left side sequentially.
        for (int i = 0; i < leftCount; i++)
        {
            Vector3 spawnPoint = GetOffscreenSpawnPointFromSide(0, offscreenOffset);
            GameObject npc = Instantiate(npcPrefab, spawnPoint, Quaternion.identity);
            // Set the NPC's patrol direction to right.
            NPCPatrolAndDetect npcScript = npc.GetComponent<NPCPatrolAndDetect>();
            if (npcScript != null)
            {
                npcScript.patrolDirection = Vector2.right;
            }
            yield return new WaitForSeconds(spawnDelay);
        }

        // Spawn NPCs from the right side sequentially.
        for (int i = 0; i < rightCount; i++)
        {
            Vector3 spawnPoint = GetOffscreenSpawnPointFromSide(1, offscreenOffset);
            GameObject npc = Instantiate(npcPrefab, spawnPoint, Quaternion.identity);
            // Set the NPC's patrol direction to left.
            NPCPatrolAndDetect npcScript = npc.GetComponent<NPCPatrolAndDetect>();
            if (npcScript != null)
            {
                npcScript.patrolDirection = Vector2.left;
            }
            yield return new WaitForSeconds(spawnDelay);
        }
    }

    /// <summary>
    /// Calculates an offscreen spawn point along the x-axis.
    /// Side: 0 = left, 1 = right.
    /// </summary>
    Vector3 GetOffscreenSpawnPointFromSide(int side, float offset)
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("No main camera found! Make sure your camera is tagged 'MainCamera'.");
            return Vector3.zero;
        }
        Vector3 camPos = cam.transform.position;
        float horizExtent = cam.orthographicSize * cam.aspect;
        Vector3 spawnPoint = Vector3.zero;
        if (side == 0) // left side
        {
            spawnPoint.x = camPos.x - horizExtent - offset;
        }
        else // right side
        {
            spawnPoint.x = camPos.x + horizExtent + offset;
        }
        spawnPoint.y = spawnY;
        spawnPoint.z = 0;
        return spawnPoint;
    }

    /// <summary>
    /// Called by NPCs when they detect the player.
    /// </summary>
    public void RegisterDetection()
    {
        detectionCount++;
        Debug.Log("Detection Count: " + detectionCount);
        if (detectionCount >= detectionCountThreshold)
        {
            GameOver();
        }
    }

    void GameOver()
    {
        isGameOver = true;
        Debug.Log("Game Over!");
        // Add additional game over logic or UI handling here.
    }
}
