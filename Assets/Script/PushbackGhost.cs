using UnityEngine;

public class PushbackGhost : MonoBehaviour
{
    public float pushbackForce = 5f;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                // Push the player away from the ghost
                Vector2 pushDirection = (collision.transform.position - transform.position).normalized;
                playerRb.AddForce(pushDirection * pushbackForce, ForceMode2D.Impulse);
            }
        }
    }
}
