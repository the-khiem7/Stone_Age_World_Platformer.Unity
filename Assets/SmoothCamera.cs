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
    }
      private void AdjustZoomBasedOnBackgroundEdges()
    {
        float targetSize = defaultOrthographicSize; // Start with default zoom level
        float closestEdgeDistance = float.MaxValue; // Track the closest edge distance
        
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
        float maxDetectionDistance = 5f; // Maximum distance to consider for zoom adjustment
        
        foreach (Vector3 dir in checkDirections)
        {
            // Calculate the point to check at the camera edge
            Vector3 checkPoint = transform.position;
            
            if (dir == Vector3.right)
                checkPoint.x += horizExtent;
            else if (dir == Vector3.left)
                checkPoint.x -= horizExtent;
            else if (dir == Vector3.up)
                checkPoint.y += vertExtent;
            else if (dir == Vector3.down)
                checkPoint.y -= vertExtent;
            
            // Cast a ray to check for background
            RaycastHit2D hit = Physics2D.Raycast(checkPoint, dir, maxDetectionDistance);
            if (hit.collider != null && hit.collider.CompareTag(backgroundTag))
            {
                // We found an edge, calculate distance
                float distance = hit.distance;
                
                // Update closest distance if this one is smaller
                if (distance < closestEdgeDistance)
                {
                    closestEdgeDistance = distance;
                }
            }
        }
        
        // Adjust zoom based on distance to closest edge
        if (closestEdgeDistance < maxDetectionDistance)
        {
            // Calculate zoom level based on distance - closer means more zoomed in
            // Map the distance [0...maxDetectionDistance] to zoom level [minOrthographicSize...defaultOrthographicSize]
            float zoomFactor = Mathf.InverseLerp(0, maxDetectionDistance, closestEdgeDistance);
            targetSize = Mathf.Lerp(minOrthographicSize, defaultOrthographicSize, zoomFactor);
        }
        
        // Apply the zoom smoothly
        cam.orthographicSize = Mathf.SmoothDamp(
            cam.orthographicSize, 
            targetSize, 
            ref zoomVelocity, 
            zoomSmoothTime
        );
    }
}