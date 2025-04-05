using UnityEngine;

namespace BreakoutExpress
{
    public class RotatingPlatform : MonoBehaviour
    {
        [Header("Rotation Settings")]
        [SerializeField] private Vector3 rotationAxis = Vector3.up;
        [SerializeField] private float rotationSpeed = 30f;
        [SerializeField] private bool reverseDirection = false;
        [SerializeField] private bool randomizeDirection = false;

        [Header("Optional Settings")]
        [SerializeField] private bool startOnAwake = true;
        [SerializeField] private bool smoothAcceleration = true;
        [SerializeField] private float accelerationTime = 1f;

        private float currentSpeed;
        private float targetSpeed;
        private float accelerationTimer;

        private void Awake()
        {
            if (randomizeDirection && Random.value > 0.5f)
            {
                reverseDirection = !reverseDirection;
            }

            targetSpeed = reverseDirection ? -rotationSpeed : rotationSpeed;
            currentSpeed = startOnAwake ? targetSpeed : 0f;
        }

        private void Update()
        {
            if (smoothAcceleration)
            {
                HandleAcceleration();
            }
            else
            {
                currentSpeed = targetSpeed;
            }

            transform.Rotate(rotationAxis * currentSpeed * Time.deltaTime);
        }

        private void HandleAcceleration()
        {
            if (Mathf.Approximately(currentSpeed, targetSpeed)) return;

            accelerationTimer += Time.deltaTime;
            float t = Mathf.Clamp01(accelerationTimer / accelerationTime);
            currentSpeed = Mathf.Lerp(0f, targetSpeed, t);
        }

        public void SetRotationActive(bool shouldRotate)
        {
            targetSpeed = shouldRotate ? 
                (reverseDirection ? -rotationSpeed : rotationSpeed) : 
                0f;
            accelerationTimer = 0f;
        }

        public void ReverseRotation()
        {
            reverseDirection = !reverseDirection;
            targetSpeed = reverseDirection ? -rotationSpeed : rotationSpeed;
            accelerationTimer = 0f;
        }
    }
}