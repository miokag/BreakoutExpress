using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public float speed = 2f;

    private Vector3 target;

    void Start()
    {
        target = pointB.position;
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
        if (Vector3.Distance(transform.position, target) < 0.1f)
        {
            target = (target == pointB.position) ? pointA.position : pointB.position;
        }
    }
}
