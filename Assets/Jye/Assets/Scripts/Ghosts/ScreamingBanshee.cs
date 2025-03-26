using UnityEngine;
using System.Diagnostics;

namespace BreakoutExpress
{
    public class ScreamingBanshee : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float pushForce = 15f;
        [SerializeField] private float screamRange = 5f;
        [SerializeField] private float windupTime = 1f;
        [SerializeField] private float screamDuration = 0.5f;
        [SerializeField] private float cooldownTime = 3f;
        
        [Header("References")]
        [SerializeField] private ParticleSystem screamParticles;
        [SerializeField] private AudioSource screamSound;
        [SerializeField] private PlayerController playerController; 

        private float timer;
        private BansheeState state = BansheeState.Cooldown;

        private enum BansheeState { Windup, Screaming, Cooldown }

        void Start()
        {
            if (playerController == null)
            {
                playerController = FindObjectOfType<PlayerController>();
            }
        }

        void Update()
        {
            if (playerController == null) return;

            timer -= Time.deltaTime;

            switch (state)
            {
                case BansheeState.Cooldown when timer <= 0 && PlayerInRange():
                    StartWindup();
                    break;
                
                case BansheeState.Windup when timer <= 0:
                    StartScreaming();
                    break;
                
                case BansheeState.Screaming when timer <= 0:
                    StartCooldown();
                    break;
            }
        }

        bool PlayerInRange()
        {
            float distance = Vector3.Distance(transform.position, playerController.transform.position);
            bool inRange = distance <= screamRange;
            return inRange;
        }

        void StartWindup()
        {
            state = BansheeState.Windup;
            timer = windupTime;
        }

        void StartScreaming()
        {
            state = BansheeState.Screaming;
            timer = screamDuration;
            
            if (screamParticles != null) screamParticles.Play();
            if (screamSound != null) screamSound.Play();
            
            Vector3 pushDirection = (playerController.transform.position - transform.position).normalized;
            playerController.ApplyEffect(new PushbackEffect(screamDuration, pushDirection, pushForce));
        }

        void StartCooldown()
        {
            state = BansheeState.Cooldown;
            timer = cooldownTime;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, screamRange);
        }
    }
}