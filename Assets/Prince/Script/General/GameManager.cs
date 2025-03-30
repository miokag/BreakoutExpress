using System.Collections;
using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    public GameObject npcPrefab;
    public float minSpawnDelay = 3f;
    public float maxSpawnDelay = 6f;
    public float spawnYPosition = 0f;
    public float despawnMargin = 2f; // Extra space beyond camera before despawning

    private Camera mainCamera;
    private float cameraHalfWidth;

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
        // Always spawn on the side opposite to camera movement
        // Assuming camera moves right in a side-scroller
        float spawnX = mainCamera.transform.position.x - cameraHalfWidth - despawnMargin;
        
        Vector3 spawnPos = new Vector3(spawnX, spawnYPosition, 0);
        GameObject npc = Instantiate(npcPrefab, spawnPos, Quaternion.identity);
        npc.tag = "NPC";
        
        NPCMovement mover = npc.GetComponent<NPCMovement>();
        if (mover != null)
        {
            mover.Initialize(Vector2.right, mainCamera); // Move right toward camera
        }
    }
}