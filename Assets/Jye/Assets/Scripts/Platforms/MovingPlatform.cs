using UnityEngine;

namespace BreakoutExpress
{
    public class MovingPlatform : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private Vector3 movementAxis = Vector3.right;
        [SerializeField] private float movementDistance = 5f;
        [SerializeField] private float movementSpeed = 2f;
        [SerializeField] private bool pingPongMovement = true;

        private Vector3 startPosition;
        private float movementProgress;

        private void Awake()
        {
            startPosition = transform.position;
        }

        private void Update()
        {
            movementProgress += Time.deltaTime * movementSpeed;
            
            if (pingPongMovement)
            {
                float pingPongValue = Mathf.PingPong(movementProgress, 1f);
                transform.position = startPosition + movementAxis * (pingPongValue * movementDistance);
            }
            else
            {
                float repeatValue = Mathf.Repeat(movementProgress, 1f);
                transform.position = startPosition + movementAxis * (repeatValue * movementDistance);
            }
        }
    }
}