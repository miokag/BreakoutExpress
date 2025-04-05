using BreakoutExpress;
using UnityEngine;

public class TimeCollectible2D : MonoBehaviour
{
    [Header("Time Settings")]
    [SerializeField] private float timeToAdd = 10f;
    [SerializeField] private AudioClip collectSound;
    [SerializeField] private ParticleSystem collectEffect;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Add time to the escape timer
            EscapeTimer timer = FindObjectOfType<EscapeTimer>();
            if (timer != null)
            {
                timer.ModifyTime(timeToAdd);
            }

            // Play effects
            if (collectSound != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(collectSound);
            }

            if (collectEffect != null)
            {
                Instantiate(collectEffect, transform.position, Quaternion.identity);
            }

            // Destroy the collectible
            Destroy(gameObject);
        }
    }
}