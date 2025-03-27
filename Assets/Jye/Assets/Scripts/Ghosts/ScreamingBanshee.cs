using BreakoutExpress;
using UnityEngine;

public class ScreamingBanshee : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float pushForce = 10f;
    [SerializeField] private float cooldown = 1f;
    [SerializeField] private string playerTag = "Player";
    
    private Collider triggerZone;
    private float nextPushTime;
    
    private void Awake()
    {
        triggerZone = GetComponentInChildren<Collider>();
        if (triggerZone == null)
        {
            Debug.LogError("Missing trigger collider on child object!", this);
            enabled = false;
            return;
        }
        triggerZone.isTrigger = true;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (Time.time < nextPushTime) return;
        
        PushPlayer(other.transform);
        nextPushTime = Time.time + cooldown;
    }

    private void PushPlayer(Transform player)
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0;
        
        if (direction.sqrMagnitude > 0.1f) 
        {
            direction.Normalize();
        }
        else
        {
            direction = player.forward;
        }
        
        bool wasPushed = false;
        
        if (player.TryGetComponent<CharacterController>(out var cc))
        {
             cc.Move(direction * pushForce * Time.deltaTime);
            wasPushed = true;
        }
        
        if (!wasPushed)
        {
            Debug.LogWarning("No compatible physics component found!", player);
        }
    }
}