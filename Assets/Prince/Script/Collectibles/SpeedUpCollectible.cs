using UnityEngine;

namespace BreakoutExpress2D
{
    public class SpeedUpCollectible : MonoBehaviour
    {
        public float speedBoost = 2f;
        public float duration = 5f; // Duration of the boost in seconds

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                PlayerController2D player = collision.GetComponent<PlayerController2D>();
                if (player != null)
                {
                    player.IncreaseSpeed(speedBoost, duration);
                }
                Destroy(gameObject);
            }
        }
    }
}

