using UnityEngine;

namespace BreakoutExpress
{
    public class WailingWraith : MonoBehaviour
    {
        public float MoveSpeed = 3f;
        public float MoveDistance = 5f;
        public float SlowDuration = 2f;
        public float SlowAmount = 2f;
        
        public float PushForce = 10f;
        public float PushDuration = 1f;

        private Vector3 startPosition;
        private bool movingRight = true;

        void Start()
        {
            startPosition = transform.position;
        }

        void Update()
        {
            if (movingRight)
            {
                transform.Translate(Vector3.right * MoveSpeed * Time.deltaTime);
                if (transform.position.x > startPosition.x + MoveDistance)
                {
                    movingRight = false;
                }
            }
            else
            {
                transform.Translate(Vector3.left * MoveSpeed * Time.deltaTime);
                if (transform.position.x < startPosition.x - MoveDistance)
                {
                    movingRight = true;
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                PlayerController player = other.GetComponent<PlayerController>();
                if (player != null)
                {
                    // Apply a slow effect
                    player.ApplyEffect(new SlowEffect(SlowDuration, SlowAmount));
                    
                    // Apply a pushback effect
                    Vector3 pushDirection = (player.transform.position - transform.position).normalized;
                    player.ApplyEffect(new PushbackEffect(PushDuration, pushDirection, PushForce));
                }
            }
        }
    }
}