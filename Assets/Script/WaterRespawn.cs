using UnityEngine;

public class WaterRespawn : MonoBehaviour
{
    private Movement movementComponent;

    private void Start()
    {
        // Get the Movement component from this game object
        movementComponent = GetComponent<Movement>();
        
        if (movementComponent == null)
        {
            Debug.LogError("WaterRespawn script requires a Movement component on the same GameObject!");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the collided object is in the Water layer
        if (collision.gameObject.layer == LayerMask.NameToLayer("Water"))
        {
            // Reset character position using the existing Movement script method
            movementComponent.ResetToStartPosition();
        }
    }
}
