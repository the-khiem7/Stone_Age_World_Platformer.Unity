using UnityEngine;

public class SmoothCameraFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private float smoothTime = 0.25f;
    
    [Header("Zoom Settings")]
    [SerializeField] private string backgroundTag = "Background";
    [SerializeField] private float defaultOrthographicSize = 5f;
    [SerializeField] private float minOrthographicSize = 3f;
    [SerializeField] private float zoomSmoothTime = 0.3f;
    [SerializeField] private float edgeCheckDistance = 1f;  // Distance to check for background edges
    
    private Vector3 velocity = Vector3.zero;
    private Camera cam;
    private float zoomVelocity = 0f;
    
    private void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("Camera component not found on this GameObject!");
            enabled = false;
        }
    }
    
    private void LateUpdate()
    {
        if (target == null)
            return;
        
        // Target position with character in the middle of the screen
        Vector3 targetPosition = new Vector3(
            target.position.x, // No offset - character stays in center
            transform.position.y,
            transform.position.z
        );
        
        // Smoothly move the camera to the target position
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            smoothTime
        );
        
        // Check edges and adjust zoom
        AdjustZoomBasedOnBackgroundEdges();
    }      private void AdjustZoomBasedOnBackgroundEdges()
    {
        float targetSize = defaultOrthographicSize; // Start with default zoom level
        float closestEdgeDistance = float.MaxValue; // Track the closest edge distance
        bool edgeDetected = false; // Track if we've detected any edge
        
        // Check in all 4 directions for background edges
        Vector3[] checkDirections = new Vector3[] {
            Vector3.right,   // Right
            Vector3.left,    // Left
            Vector3.up,      // Up
            Vector3.down     // Down
        };
        
        // Calculate viewport bounds in world space
        float vertExtent = cam.orthographicSize;
        float horizExtent = vertExtent * cam.aspect;
        float maxDetectionDistance = 10f; // Increased detection range
        
        // Multiple raycasts per direction for better edge detection
        int raysPerDirection = 3; // Cast multiple rays for better coverage
        
        foreach (Vector3 dir in checkDirections)
        {
            // Cast multiple rays along each edge
            bool backgroundFoundInDirection = false;
            
            for (int i = 0; i < raysPerDirection; i++)
            {
                // Calculate the point to check at the camera edge with offset
                Vector3 checkPoint = transform.position;
                float offset = 0f;
                
                if (dir == Vector3.right)
                {
                    checkPoint.x += horizExtent;
                    offset = (i - (raysPerDirection-1)/2f) * vertExtent / ((raysPerDirection-1) * 0.5f);
                    checkPoint.y += offset;
                }
                else if (dir == Vector3.left)
                {
                    checkPoint.x -= horizExtent;
                    offset = (i - (raysPerDirection-1)/2f) * vertExtent / ((raysPerDirection-1) * 0.5f);
                    checkPoint.y += offset;
                }
                else if (dir == Vector3.up)
                {
                    checkPoint.y += vertExtent;
                    offset = (i - (raysPerDirection-1)/2f) * horizExtent / ((raysPerDirection-1) * 0.5f);
                    checkPoint.x += offset;
                }
                else if (dir == Vector3.down)
                {
                    checkPoint.y -= vertExtent;
                    offset = (i - (raysPerDirection-1)/2f) * horizExtent / ((raysPerDirection-1) * 0.5f);
                    checkPoint.x += offset;
                }
                
                // Display debug ray in scene view
                Debug.DrawRay(checkPoint, dir * maxDetectionDistance, Color.red, 0.1f);
                
                // Cast a ray to check for background
                RaycastHit2D hit = Physics2D.Raycast(checkPoint, dir, maxDetectionDistance);
                if (hit.collider != null && hit.collider.CompareTag(backgroundTag))
                {
                    // We found a background in this direction
                    backgroundFoundInDirection = true;
                    
                    // Update closest distance if this hit is closer
                    float distance = hit.distance;
                    if (distance < closestEdgeDistance)
                    {
                        closestEdgeDistance = distance;
                    }
                }
            }
            
            // If no background found in this entire direction
            if (!backgroundFoundInDirection)
            {
                // No background detected in this direction - immediate action needed
                closestEdgeDistance = 0f; // Force minimum zoom
                edgeDetected = true;
                break; // Stop checking other directions, need to zoom in now
            }
            else
            {
                edgeDetected = true; // We found at least one edge
            }
        }
        
        // Adjust zoom based on edge detection results
        if (edgeDetected)
        {
            // Calculate zoom level based on distance - closer means more zoomed in
            // Using a more aggressive curve with stronger bias toward zoomed-in state
            float zoomFactor = Mathf.InverseLerp(0, maxDetectionDistance * 0.6f, closestEdgeDistance);
            zoomFactor = Mathf.Pow(zoomFactor, 1.5f); // Apply power curve for more aggressive zoom
            targetSize = Mathf.Lerp(minOrthographicSize, defaultOrthographicSize, zoomFactor);
        }
        
        // Apply the zoom smoothly - using a faster zoom time when zooming in to hide empty space
        float currentZoomTime = closestEdgeDistance < 1f ? zoomSmoothTime * 0.3f : zoomSmoothTime;
        cam.orthographicSize = Mathf.SmoothDamp(
            cam.orthographicSize, 
            targetSize, 
            ref zoomVelocity, 
            currentZoomTime
        );
    }
}