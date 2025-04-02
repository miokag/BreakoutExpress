using UnityEngine;

public class ScrollingBackground : MonoBehaviour
{
    [SerializeField] private GameObject[] backgroundLayers;
    [SerializeField] private float[] parallaxSpeeds;
    
    private Camera mainCamera;
    private Vector3 lastCameraPosition;
    private float[] textureUnitSizesX;

    void Start()
    {
        mainCamera = Camera.main;
        lastCameraPosition = mainCamera.transform.position;
        
        // Initialize arrays if not set in inspector
        if (parallaxSpeeds == null || parallaxSpeeds.Length != backgroundLayers.Length)
        {
            parallaxSpeeds = new float[backgroundLayers.Length];
            for (int i = 0; i < parallaxSpeeds.Length; i++)
            {
                parallaxSpeeds[i] = 0.5f; // Default speed
            }
        }
        
        textureUnitSizesX = new float[backgroundLayers.Length];
        
        // Create copies for each background layer
        for (int i = 0; i < backgroundLayers.Length; i++)
        {
            GameObject layer = backgroundLayers[i];
            float spriteWidth = layer.GetComponent<SpriteRenderer>().bounds.size.x;
            textureUnitSizesX[i] = spriteWidth;
            
            // Create left and right copies
            GameObject leftCopy = Instantiate(layer, layer.transform.parent);
            GameObject rightCopy = Instantiate(layer, layer.transform.parent);
            
            leftCopy.transform.position = new Vector3(
                layer.transform.position.x - spriteWidth,
                layer.transform.position.y,
                layer.transform.position.z);
                
            rightCopy.transform.position = new Vector3(
                layer.transform.position.x + spriteWidth,
                layer.transform.position.y,
                layer.transform.position.z);
                
            leftCopy.name = layer.name + "_Left";
            rightCopy.name = layer.name + "_Right";
        }
    }

    void LateUpdate()
    {
        Vector3 deltaMovement = mainCamera.transform.position - lastCameraPosition;
        
        for (int i = 0; i < backgroundLayers.Length; i++)
        {
            GameObject layer = backgroundLayers[i];
            
            // Move the layer with parallax effect
            layer.transform.position += new Vector3(
                deltaMovement.x * parallaxSpeeds[i],
                0,
                0);
            
            // Check if we need to reposition the layer
            float cameraDistance = mainCamera.transform.position.x - layer.transform.position.x;
            float textureUnitSize = textureUnitSizesX[i];
            
            if (cameraDistance > textureUnitSize * 0.5f)
            {
                layer.transform.position += new Vector3(
                    textureUnitSize,
                    0,
                    0);
            }
            else if (cameraDistance < -textureUnitSize * 0.5f)
            {
                layer.transform.position -= new Vector3(
                    textureUnitSize,
                    0,
                    0);
            }
        }
        
        lastCameraPosition = mainCamera.transform.position;
    }
}