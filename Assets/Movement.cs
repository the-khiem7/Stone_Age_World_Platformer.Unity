using UnityEngine;

public class Movement : MonoBehaviour
{
    [Header("Movement Settings")]    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 15f;
    
    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private bool showDebug = true;

    private Rigidbody2D rb;
    private bool isGrounded;
    private float horizontalInput;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.freezeRotation = true;
        }
          // Create a ground check object if not assigned
        if (groundCheck == null)
        {
            GameObject checkObject = new GameObject("GroundCheck");
            checkObject.transform.parent = transform;
            // Position it lower to ensure it's below the character's feet
            checkObject.transform.localPosition = new Vector3(0, -0.5f, 0);
            groundCheck = checkObject.transform;
            Debug.Log("Ground check created at position Y=-0.5. Please adjust its position if needed.");
        }
        
        // Verify ground layer is assigned
        if (groundLayer.value == 0)
        {
            Debug.LogError("Ground Layer not assigned! Please set it in the inspector. Jumping will not work without this!");
            
            // Set a default layer (Layer 8 is often used for ground)
            // Note: You still need to set up this layer in Unity's Layer settings
            groundLayer = 1 << 8; // This sets it to layer 8
            Debug.Log("Temporarily set ground layer to Layer 8. Please configure this properly in the Inspector.");
        }
    }
      // Update is called once per frame
    void Update()
    {
        // Get horizontal input (A, D keys or left, right arrow keys)
        horizontalInput = Input.GetAxisRaw("Horizontal");
        
        // Alternatively, use direct key detection for A and D keys
        if (Input.GetKey(KeyCode.A))
        {
            horizontalInput = -1;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            horizontalInput = 1;
        }
        
        // Jump when W or Space is pressed
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space))
        {
            Debug.LogWarning("Jump key pressed!");
            // Print debug info if showDebug is enabled
            if (showDebug)
                Debug.Log("Jump key pressed! isGrounded = " + isGrounded + ", position = " + transform.position);
            
            if (isGrounded)
            {
                Jump();
            }
            else
            {
                Debug.LogWarning("Jump failed - character not grounded!");
            }
        }
    }    void FixedUpdate()
    {
        // Check if grounded - increase the radius for better detection
        bool wasGrounded = isGrounded;
        
        // Reset isGrounded before checking
        isGrounded = false;
        
        // FIRST METHOD: Try using OverlapCircle with larger radius
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius * 1.5f, groundLayer);
        
        // SECOND METHOD: If that fails, try a boxcast with larger area
        if (!isGrounded) {
            // Use a wider and taller box for better detection
            isGrounded = Physics2D.BoxCast(
                groundCheck.position,  // Origin
                new Vector2(1.0f, 0.2f),  // Size (wider box)
                0f,  // Angle
                Vector2.down,  // Direction
                0.2f,  // Distance
                groundLayer  // Layer mask
            );
        }
        
        // THIRD METHOD: If that fails too, use multiple raycasts
        if (!isGrounded) {
            float rayDist = 0.3f;  // Increased ray distance
            // Center raycast
            isGrounded = Physics2D.Raycast(groundCheck.position, Vector2.down, rayDist, groundLayer);
            
            // Left raycast
            if (!isGrounded) {
                Vector2 leftPos = groundCheck.position + new Vector3(-0.3f, 0f, 0f);
                isGrounded = Physics2D.Raycast(leftPos, Vector2.down, rayDist, groundLayer);
            }
            
            // Right raycast
            if (!isGrounded) {
                Vector2 rightPos = groundCheck.position + new Vector3(0.3f, 0f, 0f);
                isGrounded = Physics2D.Raycast(rightPos, Vector2.down, rayDist, groundLayer);
            }
        }
        
        // DEBUG: Always log ground check status in FixedUpdate
        if (showDebug)
        {
            Debug.Log("Ground check: " + isGrounded + 
                     " at position " + groundCheck.position +
                     " with layer mask: " + groundLayer.value);
        }
        
        // Log when grounded state changes
        if (wasGrounded != isGrounded)
        {
            Debug.Log("Grounded state changed to: " + isGrounded + " at position " + transform.position);
        }
        
        // Apply horizontal movement
        Move();
    }
    
    void Move()
    {
        // Calculate velocity
        Vector2 targetVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
        
        // Apply the velocity to the rigidbody
        rb.linearVelocity = targetVelocity;
          // Flip character sprite based on direction (if needed)
        if (horizontalInput != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(horizontalInput), 1, 1);
        }
    }
    void Jump()
    {
        // Apply jump force directly to the Y velocity component
        // rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        
        // Method 2 (Alternative): Uncomment this if the above method doesn't work
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        
        // Debug jump action
        Debug.Log("JUMP! Applied force: " + jumpForce + ", new velocity: " + rb.linearVelocity);
        
        // Optional: Add a jumping sound effect
        // if (jumpSound != null) AudioSource.PlayClipAtPoint(jumpSound, transform.position);
        
        // Force the isGrounded to false immediately after jumping to prevent multiple jumps
        isGrounded = false;
    }    // Visual debugging
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            // Draw the ground check circle with appropriate color
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius * 1.5f);
            
            // Draw the box cast area
            Gizmos.DrawWireCube(groundCheck.position + new Vector3(0, -0.1f, 0), new Vector3(1.0f, 0.2f, 0.1f));
            
            // Draw the raycasts
            float rayDist = 0.3f;
            // Center raycast
            Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * rayDist);
            // Left raycast
            Vector3 leftPos = groundCheck.position + new Vector3(-0.3f, 0f, 0f);
            Gizmos.DrawLine(leftPos, leftPos + Vector3.down * rayDist);
            // Right raycast  
            Vector3 rightPos = groundCheck.position + new Vector3(0.3f, 0f, 0f);
            Gizmos.DrawLine(rightPos, rightPos + Vector3.down * rayDist);
        }
    }
}
