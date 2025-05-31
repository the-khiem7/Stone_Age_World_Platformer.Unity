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
        // Get or add Rigidbody2D component
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            // Configure default values
            rb.freezeRotation = true;
        }
        
        // Create a ground check object if not assigned
        if (groundCheck == null)
        {            GameObject checkObject = new GameObject("GroundCheck");
            checkObject.transform.parent = transform;
            // Position it lower to ensure it's below the character's feet
            checkObject.transform.localPosition = new Vector3(0, -0.5f, 0);
            groundCheck = checkObject.transform;
            Debug.Log("Ground check created at position Y=-0.5. Please adjust its position if needed.");
        }
        
        // Verify ground layer is assigned
        if (groundLayer.value == 0)
        {
            Debug.LogWarning("Ground Layer not assigned! Please set it in the inspector.");
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
        {            // Print debug info if showDebug is enabled
            if (showDebug)
                Debug.Log("Jump key pressed! isGrounded = " + isGrounded + ", position = " + transform.position);
            
            if (isGrounded)
            {
                Jump();
            }
        }    }
      void FixedUpdate()
    {
        // Check if grounded - increase the radius for better detection
        bool wasGrounded = isGrounded;
        
        // Try multiple types of ground detection for better reliability
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius * 1.5f, groundLayer);
        
        // Try a second check with a box cast if circle check fails
        if (!isGrounded) {
            // Try wider box cast as a fallback
            isGrounded = Physics2D.BoxCast(groundCheck.position, new Vector2(0.8f, 0.1f), 0f, Vector2.down, 0.1f, groundLayer);
        }
        
        // Try a third check with a raycast if the other methods fail
        if (!isGrounded) {
            float rayDist = 0.2f;
            isGrounded = Physics2D.Raycast(groundCheck.position, Vector2.down, rayDist, groundLayer).collider != null;
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
        }    }
    
    void Jump()
    {
        // Apply jump force directly to the Y velocity component
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        
        // Debug jump action
        Debug.Log("JUMP! Applied force: " + jumpForce + ", new velocity: " + rb.linearVelocity);
        
        // Optional: Add a jumping sound effect
        // if (jumpSound != null) AudioSource.PlayClipAtPoint(jumpSound, transform.position);
    }

    // Visual debugging
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
