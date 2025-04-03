using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace BreakoutExpress
{
    public class WailingWraith : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float moveDistance = 5f;
        [SerializeField] private bool moveHorizontally = true;
        [SerializeField] private float rotationSpeed = 10f;
        [SerializeField] private Vector3 movementPivotOffset = Vector3.zero;
        [SerializeField] private Transform visualModel;

        [Header("Effect Settings")]
        [SerializeField] private float slowAmount = 0.3f;
        [SerializeField] private Vector3 effectSize = new Vector3(5f, 5f, 5f);
        [SerializeField] private Vector3 effectOffset = Vector3.zero;

        [Header("Effects")]
        [SerializeField] private ParticleSystem slowZoneEffect;
        
        [Header("Vignette Settings")]
        [SerializeField] private float maxVignetteIntensity = 0.5f;
        [SerializeField] private float vignetteChangeSpeed = 2f;

        private PlayerController affectedPlayer;
        private float movementTimer;
        private bool playerInRange;
        private Vector3 currentDirection;
        
        // Movement tracking
        private Vector3 movementPathCenter;
        private Vector3 currentPivotWorldPosition;
        private Vector3 previousPivotWorldPosition;
        private Vector3 detectionCenter;
        
        // Vignette
        private Volume globalVolume;
        private Vignette vignette;
        private float originalVignetteIntensity;
        private float targetVignetteIntensity;

        private void Awake()
        {
            // Initialize movement path center at current pivot position
            movementPathCenter = visualModel.TransformPoint(movementPivotOffset);
            
            FindGlobalVolume();
        }
        
        private void FindGlobalVolume()
        {
            globalVolume = FindObjectOfType<Volume>();
            if (globalVolume != null && globalVolume.profile != null)
            {
                if (!globalVolume.profile.TryGet(out vignette))
                {
                    Debug.LogWarning("No Vignette effect found in Global Volume");
                }
                else
                {
                    originalVignetteIntensity = vignette.intensity.value;
                }
            }
            else
            {
                Debug.LogWarning("No Volume component found in scene");
            }
        }

        private void Update()
        {
            // Store previous pivot position
            previousPivotWorldPosition = currentPivotWorldPosition;

            // Calculate movement along path
            movementTimer += Time.deltaTime * moveSpeed;
            float pingPongValue = Mathf.PingPong(movementTimer, 1f) * 2f - 1f;
            
            // Calculate target pivot position along axis
            currentPivotWorldPosition = movementPathCenter;
            if (moveHorizontally)
            {
                currentPivotWorldPosition.x += pingPongValue * moveDistance;
            }
            else
            {
                currentPivotWorldPosition.z += pingPongValue * moveDistance;
            }

            // Move the entire object to maintain pivot position
            transform.position = currentPivotWorldPosition - visualModel.TransformDirection(movementPivotOffset);

            // Calculate movement direction
            currentDirection = (currentPivotWorldPosition - previousPivotWorldPosition).normalized;
            
            // Rotate the visual model to face movement direction
            if (currentDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(currentDirection);
                visualModel.rotation = Quaternion.Slerp(visualModel.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            // Update detection center (based on current pivot position)
            detectionCenter = currentPivotWorldPosition + effectOffset;

            // Check for player in range
            bool wasInRange = playerInRange;
            playerInRange = CheckForPlayerInRange();

            // Handle state changes
            if (playerInRange && !wasInRange)
            {
                OnPlayerEnteredRange();
            }
            else if (!playerInRange && wasInRange)
            {
                OnPlayerExitedRange();
            }
            
            if (vignette != null)
            {
                vignette.intensity.value = Mathf.Lerp(
                    vignette.intensity.value, 
                    targetVignetteIntensity, 
                    vignetteChangeSpeed * Time.deltaTime
                );
            }
        }

        private bool CheckForPlayerInRange()
        {
            // Find all players in scene
            PlayerController[] players = FindObjectsOfType<PlayerController>();
            
            foreach (PlayerController player in players)
            {
                if (player.CompareTag("Player") && IsInEffectRange(player.transform.position))
                {
                    affectedPlayer = player;
                    return true;
                }
            }
            
            return false;
        }

        private bool IsInEffectRange(Vector3 position)
        {
            Vector3 relativePos = position - detectionCenter;
            return Mathf.Abs(relativePos.x) < effectSize.x * 0.5f &&
                   Mathf.Abs(relativePos.y) < effectSize.y * 0.5f &&
                   Mathf.Abs(relativePos.z) < effectSize.z * 0.5f;
        }

        private void OnPlayerEnteredRange()
        {
            if (affectedPlayer == null) return;

            PlayerEffects effects = GetPlayerEffects(affectedPlayer);
            if (effects == null) return;

            // Disable running
            affectedPlayer.CanRun = false;
    
            effects.ApplyEffect(new PlayerEffect {
                type = PlayerEffect.EffectType.Slow,
                duration = float.MaxValue, // Infinite while in range
                magnitude = slowAmount
            });

            if (slowZoneEffect != null) 
            {
                slowZoneEffect.Play();
            }
            
            targetVignetteIntensity = maxVignetteIntensity;

            Debug.Log("Player entered slow zone");
        }

        private void OnPlayerExitedRange()
        {
            if (affectedPlayer == null) return;

            PlayerEffects effects = GetPlayerEffects(affectedPlayer);
            if (effects == null) return;

            // Re-enable running
            affectedPlayer.CanRun = true;
    
            effects.CancelEffect(PlayerEffect.EffectType.Slow);
    
            if (slowZoneEffect != null) 
            {
                slowZoneEffect.Stop();
            }
            
            targetVignetteIntensity = originalVignetteIntensity;

            Debug.Log("Player exited slow zone");
            affectedPlayer = null;
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

        void OnDrawGizmosSelected()
        {
            // Calculate current pivot point
            Vector3 currentPivot = visualModel != null ? 
                visualModel.TransformPoint(movementPivotOffset) : 
                transform.position + movementPivotOffset;

            // Draw movement range
            Gizmos.color = new Color(1f, 0f, 1f, 0.5f);
            Gizmos.DrawWireCube(currentPivot + effectOffset, effectSize);
            
            // Draw movement direction
            Gizmos.color = Color.red;
            Gizmos.DrawRay(currentPivot, currentDirection * 2f);
            
            // Draw pivot point and connection
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(currentPivot, 0.2f);
            Gizmos.DrawLine(transform.position, currentPivot);
            
            // Draw movement path
            Gizmos.color = Color.cyan;
            Vector3 pathStart = movementPathCenter;
            Vector3 pathEnd = movementPathCenter;
            if (moveHorizontally)
            {
                pathStart.x -= moveDistance;
                pathEnd.x += moveDistance;
            }
            else
            {
                pathStart.z -= moveDistance;
                pathEnd.z += moveDistance;
            }
            Gizmos.DrawLine(pathStart, pathEnd);
        }

    }
}