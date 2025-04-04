using UnityEngine;

public class TicketTaker3D : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float heightAboveGround = 0.5f;
    [SerializeField] private Vector3 modelForwardAxis = Vector3.right;

    [Header("Activation Settings")]
    [SerializeField] private string trainTriggerTag = "TrainTrigger";
    private bool isActivated = false;
    private bool isPaused = false;

    [Header("Visual Settings")]
    [SerializeField] private bool startInvisible = true;
    [SerializeField] private Renderer[] modelRenderers;

    [Header("Collision Settings")]
    [SerializeField] private string[] collidableTags = { "Player", "Wall", "Obstacle" };
    [SerializeField] private Collider[] colliders;

    private Transform player;
    private Rigidbody rb;
    private Quaternion forwardRotationOffset;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody>();
        
        if (rb != null)
        {
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }

        forwardRotationOffset = Quaternion.FromToRotation(modelForwardAxis, Vector3.forward);
        SetVisibility(!startInvisible);
        SetCollision(false);
    }

    public void ActivateTicketTaker()
    {
        if (!isActivated)
        {
            isActivated = true;
            SetVisibility(true);
            SetCollision(true);
        }
    }

    public void PauseMovement(bool pause)
    {
        isPaused = pause;
    }

    private void Update()
    {
        if (!isActivated || isPaused || player == null) return;

        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;

        if (direction.magnitude > 0.01f)
        {
            Quaternion targetLookRotation = Quaternion.LookRotation(direction);
            Quaternion targetRotation = targetLookRotation * forwardRotationOffset;
            
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        Vector3 targetPosition = player.position;
        targetPosition.y = transform.position.y;
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        MaintainFloatingHeight();
    }

    private void SetVisibility(bool visible)
    {
        if (modelRenderers != null)
        {
            foreach (Renderer renderer in modelRenderers)
            {
                renderer.enabled = visible;
            }
        }
    }

    private void SetCollision(bool enabled)
    {
        if (colliders != null)
        {
            foreach (Collider collider in colliders)
            {
                collider.enabled = enabled;
            }
        }
    }

    private void MaintainFloatingHeight()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 2f))
        {
            transform.position = new Vector3(
                transform.position.x,
                hit.point.y + heightAboveGround,
                transform.position.z
            );
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isActivated || isPaused) return;

        bool shouldCollide = false;
        foreach (string tag in collidableTags)
        {
            if (collision.gameObject.CompareTag(tag))
            {
                shouldCollide = true;
                GameManager.Instance.GameOver();
                break;
            }
        }

        if (!shouldCollide)
        {
            Physics.IgnoreCollision(GetComponent<Collider>(), collision.collider, true);
        }
    }
}