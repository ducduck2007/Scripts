using UnityEngine;
using UnityEngine.EventSystems;

public class CameraFollow : MonoBehaviour
{
    public static CameraFollow Instance;

    public Transform target;
    public Vector3 offset = new Vector3(50, 200, -250);
    public float followSmooth = 5f;
    public float dragSpeed = 0.02f;

    // ========== DYNAMIC CAMERA ROTATION BASED ON LANE ==========
    [Header("Lane-Based Camera Rotation")]
    [Tooltip("Rotation X khi ở mid lane (vùng giữa)")]
    public float midLaneRotationX = 58.5f;
    
    [Tooltip("Rotation X khi ở bot lane (đường dưới - nhìn xuống nhiều hơn)")]
    public float botLaneRotationX = 66f; // Tăng lên để nhìn xuống
    
    [Tooltip("Rotation X khi ở top lane (đường trên - nhìn xuống ít hơn)")]
    public float topLaneRotationX = 48f; // Giảm xuống để nhìn ngang hơn

    [Tooltip("Rotation Y (giữ nguyên hoặc điều chỉnh nếu cần)")]
    public float baseRotationY = -18f;
    
    [Tooltip("Rotation Z (giữ nguyên hoặc điều chỉnh nếu cần)")]
    public float baseRotationZ = 5f;

    [Header("Lane Z Boundaries")]
    [Tooltip("Vị trí Z của mid lane center")]
    public float midLaneZ = 0f;
    
    [Tooltip("Vị trí Z của top lane (z cao)")]
    public float topLaneZ = 150f;
    
    [Tooltip("Vị trí Z của bot lane (z thấp)")]
    public float botLaneZ = -150f;

    [Header("Smooth Transition")]
    [Tooltip("Tốc độ chuyển đổi camera rotation")]
    public float rotationTransitionSpeed = 3f;

    private bool isFollow = true;
    private bool isDragging = false;
    private Vector3 lastMousePos;
    private float currentDynamicRotationX; // Rotation X hiện tại (smooth)

    private void Awake()
    {
        Instance = this;
        currentDynamicRotationX = midLaneRotationX; // Khởi tạo ở mid
    }

    public void SetTarget(Transform tran)
    {
        target = tran;
        // Khởi tạo rotation từ mid lane
        transform.rotation = Quaternion.Euler(midLaneRotationX, baseRotationY, baseRotationZ);
    }

    private void LateUpdate()
    {
        // HandleDrag();

        if (isFollow && target != null)
        {
            UpdateDynamicRotation(); // Cập nhật rotation dựa vào vị trí player
            FollowTarget();
        }
    }

    // ========== DYNAMIC ROTATION BASED ON PLAYER POSITION ==========
    void UpdateDynamicRotation()
    {
        if (target == null) return;

        float targetZ = target.position.z;
        float targetRotationX;

        // Tính rotation X dựa vào vị trí Z của player
        if (targetZ >= midLaneZ)
        {
            // Đang ở giữa đi lên top lane (z cao)
            // Lerp từ midLaneRotationX (57) → topLaneRotationX (52)
            float t = Mathf.InverseLerp(midLaneZ, topLaneZ, targetZ);
            targetRotationX = Mathf.Lerp(midLaneRotationX, topLaneRotationX, t);
        }
        else
        {
            // Đang ở giữa đi xuống bot lane (z thấp)
            // Lerp từ midLaneRotationX (57) → botLaneRotationX (63)
            float t = Mathf.InverseLerp(midLaneZ, botLaneZ, targetZ);
            targetRotationX = Mathf.Lerp(midLaneRotationX, botLaneRotationX, t);
        }

        // Smooth transition
        currentDynamicRotationX = Mathf.Lerp(
            currentDynamicRotationX, 
            targetRotationX, 
            rotationTransitionSpeed * Time.deltaTime
        );

        // Cập nhật rotation
        transform.rotation = Quaternion.Euler(
            currentDynamicRotationX, 
            baseRotationY, 
            baseRotationZ
        );
    }

    void FollowTarget()
    {
        Vector3 desiredPos = target.position
                             + transform.right * offset.x
                             + Vector3.up * offset.y
                             + transform.forward * offset.z;

        transform.position = Vector3.Lerp(transform.position, desiredPos, followSmooth * Time.deltaTime);
    }

    public float minDragDistance = 20f;
    private bool isDraggingActive = false;
    
    void HandleDrag()
    {
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            isDraggingActive = false;
            isFollow = false;
            lastMousePos = Input.mousePosition;
        }

        if (Input.GetMouseButton(0) && isDragging)
        {
            Vector3 delta = Input.mousePosition - lastMousePos;

            if (!isDraggingActive)
            {
                if (delta.magnitude >= minDragDistance)
                {
                    isDraggingActive = true;
                }
                else
                {
                    return;
                }
            }

            lastMousePos = Input.mousePosition;

            Vector3 right = transform.right;
            right.y = 0;
            right.Normalize();
            Vector3 forward = Vector3.Cross(Vector3.up, right);

            transform.position += right * (-delta.x * dragSpeed) + forward * (-delta.y * dragSpeed);
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            isDraggingActive = false;
            isFollow = true;
        }
    }

    public void SetFollow(bool value)
    {
        isFollow = value;
    }

    // ========== DEBUG GIZMOS ==========
    private void OnDrawGizmosSelected()
    {
        if (target == null) return;

        // Vẽ 3 vùng lane
        Gizmos.color = Color.green;
        Gizmos.DrawLine(
            new Vector3(-200, 0, topLaneZ), 
            new Vector3(200, 0, topLaneZ)
        ); // Top lane

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(
            new Vector3(-200, 0, midLaneZ), 
            new Vector3(200, 0, midLaneZ)
        ); // Mid lane

        Gizmos.color = Color.red;
        Gizmos.DrawLine(
            new Vector3(-200, 0, botLaneZ), 
            new Vector3(200, 0, botLaneZ)
        ); // Bot lane
    }
}