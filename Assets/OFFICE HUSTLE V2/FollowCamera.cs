using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Follow Settings")]
    public Vector3 offset = new Vector3(0, 2, -5);
    public float followSpeed = 5f;
    public bool smoothFollow = true;

    [Header("Mouse Look")]
    public float mouseSensitivity = 2f;
    public float verticalLimit = 80f;
    public bool invertY = false;

    [Header("Distance Control")]
    public float minDistance = 2f;
    public float maxDistance = 10f;
    public float zoomSpeed = 2f;

    private float rotationX = 0f;
    private float rotationY = 0f;
    private float currentDistance;
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        currentDistance = offset.magnitude;

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;

        // Initialize rotation based on current camera rotation
        Vector3 angles = transform.eulerAngles;
        rotationY = angles.y;
        rotationX = angles.x;
    }

    void LateUpdate()
    {
        if (target == null) return;

        HandleMouseInput();
        HandleZoom();
        UpdateCameraPosition();
        HandleCursorToggle();
    }

    void HandleMouseInput()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            if (invertY) mouseY = -mouseY;

            rotationY += mouseX;
            rotationX -= mouseY;

            // Clamp vertical rotation
            rotationX = Mathf.Clamp(rotationX, -verticalLimit, verticalLimit);
        }
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        currentDistance -= scroll * zoomSpeed;
        currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
    }

    void UpdateCameraPosition()
    {
        // Calculate desired position
        Quaternion rotation = Quaternion.Euler(rotationX, rotationY, 0);
        Vector3 desiredPosition = target.position - (rotation * Vector3.forward * currentDistance);
        desiredPosition += Vector3.up * offset.y;

        // Apply position (with optional smoothing)
        if (smoothFollow)
        {
            transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
        }
        else
        {
            transform.position = desiredPosition;
        }

        // Look at target
        transform.LookAt(target.position + Vector3.up * offset.y);
    }

    void HandleCursorToggle()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
                Cursor.lockState = CursorLockMode.None;
            else
                Cursor.lockState = CursorLockMode.Locked;
        }
    }

    // Optional: Handle camera collision
    void OnDrawGizmosSelected()
    {
        if (target != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(target.position, 0.5f);
            Gizmos.DrawLine(target.position, transform.position);
        }
    }
}