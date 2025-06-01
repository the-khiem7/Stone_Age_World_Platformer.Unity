using UnityEngine;

/// <summary>
/// A simplified version of character movement with reliable jumping
/// </summary>
public class SimpleMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;
    
    [Header("Ground Detection")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    
    // Private variables
    private Rigidbody2D rb;
    private bool isGrounded;
    
    private void Start()
    {
        // Get or add required components
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.freezeRotation = true;
        }
        
        // Create ground check point if missing
        if (groundCheckPoint == null)
        {
            GameObject groundCheck = new GameObject("GroundCheck");
            groundCheck.transform.parent = transform;
            groundCheck.transform.localPosition = new Vector3(0, -0.9f, 0);
            groundCheckPoint = groundCheck.transform;
            Debug.Log("Ground check point created.");
        }
        
        // Layer check
        if (groundLayer.value == 0)
        {
            Debug.LogError("Ground layer not assigned! Jumping will not work.");
            groundLayer = 1 << 8; // Set to layer 8 as a default
        }
    }
    
    private void Update()
    {
        // Movement Input
        float moveInput = Input.GetAxis("Horizontal");
        
        // Apply horizontal movement
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        
        // Ground check
        isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);
        
        // Display ground state (for debugging)
        if (isGrounded)
        {
            Debug.Log("Grounded: TRUE");
        }
        
        // Jump Input (Space, W, Up arrow)
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (isGrounded)
            {
                // Apply jump force
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                Debug.Log("JUMP!");
            }
            else
            {
                Debug.LogWarning("Cannot jump - not grounded!");
            }
        }
        
        // Flip character based on direction
        if (moveInput > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (moveInput < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }
    
    // Visual debugging
    private void OnDrawGizmos()
    {
        if (groundCheckPoint != null)
        {
            // Show ground check area
            Gizmos.color = (Application.isPlaying && isGrounded) ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }
    }
}
