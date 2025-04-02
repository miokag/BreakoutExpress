using UnityEngine;

public class TrainCameraShake : MonoBehaviour
{
    [Header("Shake Settings")]
    [SerializeField] private float baseShakeAmount = 0.05f;
    [SerializeField] private float shakeFrequency = 1f;
    [SerializeField] private float movementMultiplier = 0.2f;
    [SerializeField] private float maxShakeOffset = 0.3f;

    [Header("Movement Detection")]
    [SerializeField] private float speedThreshold = 1f;
    [SerializeField] private float movementSmoothing = 5f;

    private Vector3 originalPosition;
    private float shakeTimer;
    private Vector3 currentVelocity;
    private Vector3 lastPosition;
    private float currentSpeed;

    void Start()
    {
        originalPosition = transform.localPosition;
        lastPosition = transform.position;
    }

    void Update()
    {
        // Calculate camera movement speed
        Vector3 positionDelta = transform.position - lastPosition;
        currentSpeed = positionDelta.magnitude / Time.deltaTime;
        lastPosition = transform.position;

        // Adjust shake amount based on movement speed
        float speedFactor = Mathf.Clamp01(currentSpeed / speedThreshold);
        float shakeAmount = baseShakeAmount + (speedFactor * movementMultiplier);
        shakeAmount = Mathf.Min(shakeAmount, maxShakeOffset);

        // Only shake when moving (optional)
        if (currentSpeed > 0.1f)
        {
            shakeTimer += Time.deltaTime * shakeFrequency;
            
            // Perlin noise for smoother shaking
            float offsetX = Mathf.PerlinNoise(shakeTimer, 0) * 2 - 1;
            float offsetY = Mathf.PerlinNoise(0, shakeTimer) * 2 - 1;
            
            Vector3 targetOffset = new Vector3(
                offsetX * shakeAmount,
                offsetY * shakeAmount,
                0
            );

            // Smoothly apply the shake
            transform.localPosition = Vector3.SmoothDamp(
                transform.localPosition,
                originalPosition + targetOffset,
                ref currentVelocity,
                movementSmoothing * Time.deltaTime
            );
        }
        else
        {
            // Smoothly return to original position when not moving
            transform.localPosition = Vector3.SmoothDamp(
                transform.localPosition,
                originalPosition,
                ref currentVelocity,
                movementSmoothing * Time.deltaTime
            );
        }
    }

    public void AddBump(float intensity)
    {
        // Add an immediate bump to the camera
        float bumpX = Random.Range(-intensity, intensity);
        float bumpY = Random.Range(-intensity, intensity);
        transform.localPosition += new Vector3(bumpX, bumpY, 0);
    }
}