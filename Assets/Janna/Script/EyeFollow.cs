using UnityEngine;

public class EyeFollow : MonoBehaviour
{   
    private Transform player;

    void OnEnable()
    {
        // Find player only when enabled
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            enabled = false;
        }
    }

    void Update()
    {
        if (player != null)
        {
            EyeFollowPlayer();
        }
    }

    void EyeFollowPlayer()
    {
        Vector3 playerPos = player.position;
        Vector2 direction = new Vector2(
            (playerPos.x - transform.position.x),
            (playerPos.y - transform.position.y)     
        );
        
        // Only update rotation if direction is significant
        if (direction.sqrMagnitude > 0.01f)
        {
            transform.up = direction;
        }
    }
}