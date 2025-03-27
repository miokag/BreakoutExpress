using UnityEngine;

namespace BreakoutExpress
{
    public class WailingWraith : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float moveDistance = 5f;
        [SerializeField] private bool moveHorizontally = true;

        [Header("Effect Settings")]
        [SerializeField] private float slowAmount = 0.3f;
        [SerializeField] private float pushSlowAmount = 0.5f;
        [SerializeField] private float effectDuration = 2f;

        [Header("Effects")]
        [SerializeField] private ParticleSystem slowZoneEffect;

        private Collider rangeTrigger;
        private bool isPlayerInRange;
        private PlayerController affectedPlayer;
        private Vector3 startPosition;
        private float movementTimer;

        private void Awake()
        {
            startPosition = transform.position;
            
            rangeTrigger = GetComponentInChildren<Collider>();
            if (rangeTrigger != null)
            {
                rangeTrigger.isTrigger = true;
            }
        }

        private void Update()
        {
            movementTimer += Time.deltaTime * moveSpeed;
            float pingPongValue = Mathf.PingPong(movementTimer, 1f) * 2f - 1f; // Returns -1 to 1
            
            Vector3 newPosition = startPosition;
            if (moveHorizontally)
            {
                newPosition.x += pingPongValue * moveDistance;
            }
            else
            {
                newPosition.z += pingPongValue * moveDistance;
            }
            
            transform.position = newPosition;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.transform.root.CompareTag("Player"))
            {
                PlayerController player = other.transform.root.GetComponent<PlayerController>();
                if (player != null)
                {
                    isPlayerInRange = true;
                    affectedPlayer = player;
                    ApplyZoneEffect(true);
                    Debug.Log("Player entered slow zone");
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.transform.root.CompareTag("Player"))
            {
                isPlayerInRange = false;
                ApplyZoneEffect(false);
                affectedPlayer = null;
                Debug.Log("Player exited slow zone");
            }
        }

        private void ApplyZoneEffect(bool entering)
        {
            if (affectedPlayer == null) return;

            PlayerEffects effects = GetPlayerEffects(affectedPlayer);
            if (effects == null) return;

            if (entering)
            {
                // Disable running
                affectedPlayer.CanRun = false;
        
                effects.ApplyEffect(new PlayerEffect {
                    type = PlayerEffect.EffectType.Slow,
                    duration = float.MaxValue,
                    magnitude = slowAmount
                });

                if (slowZoneEffect != null) 
                {
                    slowZoneEffect.Play();
                }
            }
            else
            {
                // Re-enable running
                affectedPlayer.CanRun = true;
        
                effects.CancelEffect(PlayerEffect.EffectType.Slow);
        
                if (slowZoneEffect != null) 
                {
                    slowZoneEffect.Stop();
                }
            }
        }

        private PlayerEffects GetPlayerEffects(PlayerController player)
        {
            PlayerEffects effects = player.GetComponent<PlayerEffects>();
            if (effects == null) 
            {
                effects = player.gameObject.AddComponent<PlayerEffects>();
                Debug.Log("Added PlayerEffects component to player");
            }
            return effects;
        }
    }
}