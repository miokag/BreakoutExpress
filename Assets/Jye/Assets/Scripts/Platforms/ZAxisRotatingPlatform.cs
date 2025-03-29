using UnityEngine;

namespace BreakbreakExpress
{
    public class ZAxisRotatingPlatform : MonoBehaviour
    {
        [Header("Rotation Settings")]
        [SerializeField] private float rotationSpeed = 30f;
        [SerializeField] private float maxTiltAngle = 25f;
        [SerializeField] private bool startActive = true;
        [SerializeField] private bool smoothMovement = true;
        [SerializeField] private float accelerationTime = 1f;

        [Header("Visual Feedback")]
        [SerializeField] private ParticleSystem tiltParticles;
        [SerializeField] private AudioSource tiltSound;

        private float currentRotation;
        private float currentSpeed;
        private bool isRotating;

        private void Start()
        {
            isRotating = startActive;
            currentSpeed = startActive ? rotationSpeed : 0f;
            
            if (tiltSound != null && startActive)
            {
                tiltSound.Play();
            }
        }

        private void Update()
        {
            if (smoothMovement)
            {
                HandleSmoothAcceleration();
            }
            else
            {
                currentSpeed = isRotating ? rotationSpeed : 0f;
            }

            if (Mathf.Abs(currentSpeed) > 0.01f)
            {
                currentRotation += currentSpeed * Time.deltaTime;
                transform.rotation = Quaternion.Euler(0, 0, Mathf.Sin(currentRotation * Mathf.Deg2Rad) * maxTiltAngle);
                
                UpdateEffects(true);
            }
            else
            {
                UpdateEffects(false);
            }
        }

        private void HandleSmoothAcceleration()
        {
            float targetSpeed = isRotating ? rotationSpeed : 0f;
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, 
                            (rotationSpeed / accelerationTime) * Time.deltaTime);
        }

        private void UpdateEffects(bool active)
        {
            if (tiltParticles != null)
            {
                if (active && !tiltParticles.isPlaying) tiltParticles.Play();
                else if (!active && tiltParticles.isPlaying) tiltParticles.Stop();
            }

            if (tiltSound != null)
            {
                if (active && !tiltSound.isPlaying) tiltSound.Play();
                else if (!active && tiltSound.isPlaying) tiltSound.Stop();
            }
        }

        public void ToggleRotation(bool active)
        {
            isRotating = active;
        }

        public void SetRotationSpeed(float newSpeed)
        {
            rotationSpeed = newSpeed;
        }
    }
}