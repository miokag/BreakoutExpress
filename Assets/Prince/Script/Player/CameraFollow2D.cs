using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    // The target (usually your Player GameObject's Transform)
    public Transform target;
    // The desired offset from the target
    public Vector3 offset = new Vector3(0f, 2f, -10f);
    // How quickly the camera moves toward the target position
    public float smoothSpeed = 0.125f;

    void LateUpdate()
    {
        if (target == null)
            return;

        // Calculate the desired position based on target and offset
        Vector3 desiredPosition = target.position + offset;
        // Smoothly interpolate to the desired position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
}
