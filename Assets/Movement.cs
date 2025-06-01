using UnityEngine;

/// <summary>
/// Handles 2D platformer character movement including running and jumping
/// </summary>
public class Movement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float acceleration = 50f;
    [SerializeField] private float deceleration = 50f;
    [SerializeField] private float velocityPower = 0.9f;
    [SerializeField] private bool useSmoothMovement = true;

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 16f;
    [SerializeField] private float jumpCutMultiplier = 0.5f;
    [SerializeField] private float fallGravityMultiplier = 1.7f;
    [SerializeField] private float coyoteTime = 0.2f;
    [SerializeField] private float jumpBufferTime = 0.2f;

    [Header("Ground Detection")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.9f, 0.2f);
    [SerializeField] private LayerMask groundLayer;

    // Private variables
    private Rigidbody2D rb;
    private float moveInput;
    private bool isJumpPressed;
    private bool isJumping;
    private bool isGrounded;
    private float lastGroundedTime;
    private float lastJumpPressedTime;
    private bool facingRight = true;
    private bool jumpCut;

    // Constants
    private const float MIN_JUMP_SPEED = 0.1f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        // Validate components
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.freezeRotation = true;
            Debug.Log("Added missing Rigidbody2D component to " + gameObject.name);
        }

        // Create a ground check transform if none is assigned
        if (groundCheckPoint == null)
        {
            GameObject checkObject = new GameObject("GroundCheck");
            checkObject.transform.parent = transform;
            checkObject.transform.localPosition = new Vector3(0, -0.95f, 0); // Position at feet
            groundCheckPoint = checkObject.transform;
            Debug.Log("Created ground check point at bottom of character");
        }

        // Warn if ground layer is not set
        if (groundLayer.value == 0)
        {
            Debug.LogError("Ground Layer not set! Please assign it in the inspector (Layer 8 is recommended)");
            // Auto-assign to layer 8 as a fallback
            groundLayer = 1 << 8;
        }
    }

    private void Update()
    {
        // Handle player input
        moveInput = Input.GetAxisRaw("Horizontal");
        
        // Direct key detection as backup
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            moveInput = -1;
        }
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            moveInput = 1;
        }

        // Jump input processing with buffer time
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            isJumpPressed = true;
            lastJumpPressedTime = Time.time;
            Debug.Log("Jump button pressed. isGrounded: " + isGrounded + ", Time: " + Time.time);
        }        // Jump cut (shorter jump when button released)
        if (Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.UpArrow))
        {
            if (rb.linearVelocity.y > 0 && isJumping)
            {
                jumpCut = true;
            }
        }

        // Coyote time and jump buffer logic
        if (CanJump() && isJumpPressed)
        {
            ExecuteJump();
        }
    }

    private void FixedUpdate()
    {
        // Check if character is grounded
        CheckGrounded();
        
        // Apply the appropriate movement
        MoveCharacter();
        
        // Handle gravity modification for better jump feel
        ApplyGravityModifier();
        
        // Flip the character sprite based on movement direction
        HandleSpriteFlip();
    }    private void CheckGrounded()
    {
        // Store previous state for transition detection
        bool wasGrounded = isGrounded;
        
        // Try multiple ground detection methods in case one fails
        
        // METHOD 1: Box cast (most reliable)
        RaycastHit2D hit = Physics2D.BoxCast(
            groundCheckPoint.position,
            groundCheckSize,
            0f,
            Vector2.down,
            0.2f,  // Increased distance
            groundLayer
        );
        
        isGrounded = hit.collider != null;
        
        // METHOD 2: If box cast fails, try circle overlap
        if (!isGrounded)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckSize.x/2, groundLayer);
        }
        
        // METHOD 3: If all else fails, try direct raycast
        if (!isGrounded)
        {
            isGrounded = Physics2D.Raycast(groundCheckPoint.position, Vector2.down, 0.3f, groundLayer);
        }
          // FORCE GROUNDED FOR TESTING - Uncomment this line if ground detection isn't working
        isGrounded = true; // TEMPORARY FIX - REMOVE AFTER TESTING
        
        // Update the last grounded time for coyote time
        if (isGrounded)
        {
            lastGroundedTime = Time.time;
            
            // Reset jump state when landing
            if (rb.linearVelocity.y <= 0)
            {
                isJumping = false;
                jumpCut = false;
            }
        }

        // ALWAYS log ground detection status for debugging
        Debug.Log("Ground check: " + isGrounded + 
                 " | Position: " + groundCheckPoint.position +
                 " | Layer: " + groundLayer.value);

        // Log ground state changes
        if (wasGrounded != isGrounded)
        {
            Debug.LogWarning("Grounded state changed to: " + isGrounded);
        }
    }private void MoveCharacter()
    {
        // Calculate target speed
        float targetSpeed = moveInput * moveSpeed;
        
        if (useSmoothMovement)
        {
            // Calculate speed difference
            float speedDiff = targetSpeed - rb.linearVelocity.x;
            
            // Calculate acceleration rate based on direction
            float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
            
            // Apply movement force with non-linear acceleration for smoother feel
            float movement = Mathf.Pow(Mathf.Abs(speedDiff) * accelRate, velocityPower) * Mathf.Sign(speedDiff);
            rb.AddForce(movement * Vector2.right);
        }
        else
        {
            // Simple movement - directly set velocity
            rb.linearVelocity = new Vector2(targetSpeed, rb.linearVelocity.y);
        }
        
        // Cap maximum speed
        if (Mathf.Abs(rb.linearVelocity.x) > moveSpeed)
        {
            rb.linearVelocity = new Vector2(moveSpeed * Mathf.Sign(rb.linearVelocity.x), rb.linearVelocity.y);
        }
    }

    private bool CanJump()
    {
        // Can jump if within coyote time or grounded
        float timeSinceGrounded = Time.time - lastGroundedTime;
        return timeSinceGrounded <= coyoteTime && !isJumping;
    }    private void ExecuteJump()
    {
        // Reset velocity y to ensure consistent jump height
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        
        // Apply the jump force
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        
        // Reset jump pressed flag
        isJumpPressed = false;
        
        // Set jumping state
        isJumping = true;
        
        // Debug
        Debug.Log("JUMP executed with force: " + jumpForce);
    }    private void ApplyGravityModifier()
    {
        // Increase gravity when falling for better feel
        if (rb.linearVelocity.y < 0)
        {
            rb.gravityScale = fallGravityMultiplier;
        }
        // Apply jump cut (shorter jump when button released early)
        else if (rb.linearVelocity.y > 0 && jumpCut)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
            jumpCut = false;
        }
        else
        {
            rb.gravityScale = 1f;
        }
    }

    private void HandleSpriteFlip()
    {
        // Flip the character based on movement direction
        if (moveInput > 0 && !facingRight)
        {
            FlipCharacter();
        }
        else if (moveInput < 0 && facingRight)
        {
            FlipCharacter();
        }
    }

    private void FlipCharacter()
    {
        facingRight = !facingRight;
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
    }    // Visual debugging
    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            // Draw ground check area
            Gizmos.color = Application.isPlaying ? (isGrounded ? Color.green : Color.red) : Color.yellow;
            Gizmos.DrawWireCube(
                groundCheckPoint.position, 
                new Vector3(groundCheckSize.x, groundCheckSize.y, 0.1f)
            );
        }
    }
}
