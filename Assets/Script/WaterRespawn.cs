using UnityEngine;
using System.Collections;

public class WaterRespawn : MonoBehaviour
{
    [Tooltip("Assign the respawn point Transform here")]
    public Transform respawnPoint;

    [Tooltip("Layer to detect as Water (set to match Water layer number)")]
    public LayerMask waterLayer;

    private Rigidbody2D rb2D;
    private Movement movement;

    private void Awake()
    {
        rb2D = GetComponent<Rigidbody2D>();
        movement = GetComponent<Movement>();
        
        if (respawnPoint == null)
        {
            Debug.LogError("[WaterRespawn] No respawn point assigned! Please assign a Transform in the inspector.");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleWaterContact(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleWaterContact(other.gameObject);
    }

    private void HandleWaterContact(GameObject hitObject)
    {
        // Check if the object is in the Water layer
        if (((1 << hitObject.layer) & waterLayer.value) != 0 && respawnPoint != null)
        {
            StartCoroutine(TeleportNextFrame());
        }
    }

    private IEnumerator TeleportNextFrame()
    {
        // Wait for the next frame to ensure all physics calculations are complete
        yield return null;

        // Reset velocity if we have a Rigidbody2D
        if (rb2D != null)
        {
            rb2D.linearVelocity = Vector2.zero;
            rb2D.angularVelocity = 0f;
            rb2D.position = respawnPoint.position;
        }
        else
        {
            transform.position = respawnPoint.position;
        }

        // If we have the Movement component, make sure it's reset too
        if (movement != null)
        {
            movement.ResetToStartPosition();
        }
    }
}
