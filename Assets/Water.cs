using UnityEngine;

/// <summary>
/// Handles water collision logic to reset player position when they touch water
/// </summary>
public class Water : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the colliding object is the player
        if (collision.CompareTag("Player"))
        {
            // Get the player's movement script
            var movement = collision.GetComponent<Movement>();
            
            if (movement != null)
            {
                // Reset the player to starting position
                movement.ResetToStartPosition();
            }
            else
            {
                Debug.LogWarning("Player doesn't have a Movement script. Cannot reset position.");
            }
        }
    }
}
