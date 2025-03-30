using UnityEngine;

public class NPCMovement : MonoBehaviour
{
    public float speed = 2f;
    private Vector2 direction;
    private Camera mainCamera;
    private float cameraHalfWidth;
    private float spawnTime;
    private float minLifetime = 3f; // Minimum time before allowed to despawn

    void Start()
    {
        spawnTime = Time.time;
    }

    public void Initialize(Vector2 moveDirection, Camera cam)
    {
        direction = moveDirection;
        mainCamera = cam;
        cameraHalfWidth = mainCamera.orthographicSize * mainCamera.aspect;
        
        // Flip sprite if needed
        if (direction.x < 0)
        {
            GetComponent<SpriteRenderer>().flipX = true;
        }
    }

    void Update()
    {
        // Move NPC
        transform.Translate(direction * speed * Time.deltaTime);

        // Calculate camera bounds with margin
        float cameraRightEdge = mainCamera.transform.position.x + cameraHalfWidth + 2f;
        float cameraLeftEdge = mainCamera.transform.position.x - cameraHalfWidth - 2f;

        // Check if NPC has been alive long enough and is behind camera
        if (Time.time - spawnTime > minLifetime)
        {
            if (direction.x > 0 && transform.position.x > cameraRightEdge)
            {
                Destroy(gameObject); // NPC passed camera right edge
            }
            else if (direction.x < 0 && transform.position.x < cameraLeftEdge)
            {
                Destroy(gameObject); // NPC passed camera left edge
            }
        }
    }
}