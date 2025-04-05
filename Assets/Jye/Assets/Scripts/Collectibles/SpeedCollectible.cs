using UnityEngine;

namespace BreakoutExpress
{
    public class SpeedCollectible3D : MonoBehaviour
    {
        [Header("Boost Settings")] [SerializeField]
        private float speedBoostAmount = 2f; // How much to increase speed

        [SerializeField] private float boostDuration = 5f;
        [SerializeField] private AudioClip collectSound;
        [SerializeField] private ParticleSystem collectEffect;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                // Apply speed boost
                PlayerEffects playerEffects = other.GetComponent<PlayerEffects>();
                if (playerEffects != null)
                {
                    playerEffects.ApplyEffect(new PlayerEffect
                    {
                        type = PlayerEffect.EffectType.SpeedBoost,
                        duration = boostDuration,
                        magnitude = speedBoostAmount
                    });
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

                // Destroy the collectible
                Destroy(gameObject);
            }
        }
    }
}