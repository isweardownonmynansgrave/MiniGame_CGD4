using UnityEngine;

public class OrbitCamera : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0, 2, 0);
    
    [Header("Orbit Settings")]
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float minVerticalAngle = -30f;
    [SerializeField] private float maxVerticalAngle = 60f;
    [SerializeField] private float distance = 5f;
    [SerializeField] private float smoothSpeed = 10f;
    
    [Header("Collision Settings")]
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private LayerMask collisionLayers;
    
    private float currentRotationX;
    private float currentRotationY;
    private Vector3 currentRotation;
    private Vector3 desiredRotation;
    private float currentDistance;
    
    private void Start()
    {
        if (target == null)
        {
            Debug.LogError("No target assigned to OrbitCamera!");
            enabled = false;
            return;
        }
        
        // Initialize rotation and position
        transform.position = target.position - transform.forward * distance;
        currentDistance = distance;
    }
    
    private void LateUpdate()
    {
        if (target == null) return;
        
        HandleRotationInput();
        UpdateCameraPosition();
        HandleCollision();
        UpdatePlayerForward();
    }
    
    private void HandleRotationInput()
    {
        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;
        
        // Update rotation angles
        currentRotationY += mouseX;
        currentRotationX -= mouseY;
        
        // Clamp vertical rotation
        currentRotationX = Mathf.Clamp(currentRotationX, minVerticalAngle, maxVerticalAngle);
        
        // Calculate desired rotation
        desiredRotation = new Vector3(currentRotationX, currentRotationY, 0);
        currentRotation = Vector3.Lerp(currentRotation, desiredRotation, Time.deltaTime * smoothSpeed);
    }
    
    private void UpdateCameraPosition()
    {
        // Calculate rotation and position
        Quaternion rotation = Quaternion.Euler(currentRotation);
        Vector3 targetPosition = target.position + offset;
        Vector3 direction = rotation * -Vector3.forward;
        
        // Set camera position and rotation
        transform.position = targetPosition + direction * currentDistance;
        transform.rotation = rotation;
    }
    
    private void HandleCollision()
    {
        Vector3 directionToTarget = (target.position + offset - transform.position).normalized;
        float targetDistance = Vector3.Distance(target.position + offset, transform.position);
        
        // Check for obstacles between camera and target
        RaycastHit hit;
        if (Physics.Raycast(target.position + offset, -directionToTarget, out hit, distance, collisionLayers))
        {
            currentDistance = Mathf.Clamp(hit.distance, minDistance, distance);
        }
        else
        {
            currentDistance = Mathf.Lerp(currentDistance, distance, Time.deltaTime * smoothSpeed);
        }
    }
    
    private void UpdatePlayerForward()
    {
        // Assuming the target is the player, update their forward direction
        // This makes the player's forward direction match the camera's horizontal rotation
        Vector3 forward = transform.forward;
        forward.y = 0; // Remove vertical component
        forward.Normalize();
        
        // Only update the target's rotation if it has a Rigidbody component
        Rigidbody targetRb = target.GetComponent<Rigidbody>();
        if (targetRb != null)
        {
            Quaternion targetRotation = Quaternion.LookRotation(forward);
            target.rotation = targetRotation;
        }
    }
    
    // Public method to set a new target
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
    
    // Public method to set a new distance
    public void SetDistance(float newDistance)
    {
        distance = Mathf.Max(minDistance, newDistance);
    }
}