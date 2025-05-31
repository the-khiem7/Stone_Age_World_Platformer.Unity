using UnityEngine;

public class Movement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

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
        {
            GameObject checkObject = new GameObject("GroundCheck");
            checkObject.transform.parent = transform;
            checkObject.transform.localPosition = new Vector3(0, -1f, 0);
            groundCheck = checkObject.transform;
            Debug.Log("Ground check created. Please adjust its position in the inspector.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Get horizontal input (A, D keys or left, right arrow keys)
        horizontalInput = Input.GetAxisRaw("Horizontal");
        
        // Jump when W or Space is pressed
        if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space)) && isGrounded)
        {
            Jump();
        }
    }

    void FixedUpdate()
    {
        // Check if grounded
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        
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
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
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
