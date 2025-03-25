using UnityEngine;

namespace BreakoutExpress
{
    public class ScreamingBanshee : MonoBehaviour
    {
        public float PushForce = 10f;
        public float PushDuration = 1f;

        private Vector3 startPosition;
        private bool movingRight = true;

        void Start()
        {
            startPosition = transform.position;
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                PlayerController player = other.GetComponent<PlayerController>();
                if (player != null)
                {
                    // Apply a pushback effect
                    Vector3 pushDirection = (player.transform.position - transform.position).normalized;
                    player.ApplyEffect(new PushbackEffect(PushDuration, pushDirection, PushForce));
                }
            }
        }
    }
}