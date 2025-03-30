using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GhostPlatformTilemap : MonoBehaviour
{
    [Header("Platform Settings")]
    public float delayBeforeDisappearing = 1f; // Time before the platform disappears after contact
    public float respawnTime = 3f;             // Minimum time before the platform attempts to reappear
    public LayerMask playerLayerMask;          // Layer mask for detecting the player

    private Collider2D platformCollider;
    private TilemapRenderer tilemapRenderer;
    private bool isTriggered = false;          // Prevents multiple triggers

    void Start()
    {
        platformCollider = GetComponent<Collider2D>();
        tilemapRenderer = GetComponent<TilemapRenderer>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Only trigger once per cycle.
        if (!isTriggered && collision.gameObject.CompareTag("Player"))
        {
            isTriggered = true;
            StartCoroutine(DisappearAndRespawn());
        }
    }

    IEnumerator DisappearAndRespawn()
    {
        // Wait a moment before disappearing
        yield return new WaitForSeconds(delayBeforeDisappearing);

        // Capture the platform's bounds before disabling.
        Bounds storedBounds = platformCollider.bounds;

        // Disable both the collider and tilemap renderer so the player cannot stand on it.
        platformCollider.enabled = false;
        tilemapRenderer.enabled = false;

        // Wait for the respawn time.
        yield return new WaitForSeconds(respawnTime);

        // Wait until the player is no longer overlapping the platform's area.
        while (Physics2D.OverlapBox(storedBounds.center, storedBounds.size, 0f, playerLayerMask) != null)
        {
            yield return null;
        }

        // Re-enable the platform.
        platformCollider.enabled = true;
        tilemapRenderer.enabled = true;
        isTriggered = false;
    }
}
