using BreakoutExpress;
using UnityEngine;
using TMPro;

public class TimeCollectible : MonoBehaviour
{
    [Header("Time Settings")]
    [SerializeField] private float timeToAdd = 10f;
    [SerializeField] private AudioClip collectSound;
    [SerializeField] private ParticleSystem collectEffect;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Add time
            EscapeTimer timer = FindObjectOfType<EscapeTimer>();
            if (timer != null)
            {
                timer.ModifyTime(timeToAdd);
            }

            // Play effects
            if (collectSound != null)
            {
                AudioManager.Instance.PlaySFX(collectSound);
            }

            if (collectEffect != null)
            {
                Instantiate(collectEffect, transform.position, Quaternion.identity);
            }

            Destroy(gameObject);
        }
    }
}