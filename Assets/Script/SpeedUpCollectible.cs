using UnityEngine;

public class SpeedUpCollectible : MonoBehaviour
{
    public float speedBoost = 2f;
    public float duration = 5f; // Duration of the boost in seconds

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                player.IncreaseSpeed(speedBoost, duration);
            }
            Destroy(gameObject);
        }
    }
}
