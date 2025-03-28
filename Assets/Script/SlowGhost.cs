using UnityEngine;

public class SlowGhost : MonoBehaviour
{
    public float speed = 2f;
    public Transform target; // Usually the player

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (target == null)
            target = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        // Move toward the player at a slow pace
        Vector2 direction = (target.position - transform.position).normalized;
        rb.linearVelocity = direction * speed;
    }
}
