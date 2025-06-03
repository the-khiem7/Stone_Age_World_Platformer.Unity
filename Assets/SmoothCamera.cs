using UnityEngine;

public class SmoothCameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothTime = 0.25f;
    
    private Vector3 velocity = Vector3.zero;
    
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
    }
}