using UnityEngine;
using UnityEngine.EventSystems;

public class CameraFollow : MonoBehaviour
{
    public static CameraFollow Instance;

    public Transform target;
    public Vector3 offset = new Vector3(100, 400, -900);
    public float followSmooth = 5f; // giảm chút để mượt hơn
    public float dragSpeed = 0.02f; // giảm để tránh giật

    private bool isFollow = true;
    private bool isDragging = false;
    private Vector3 lastMousePos;

    private void Awake()
    {
        Instance = this;
        // Cố định góc ngay từ đầu
        transform.rotation = Quaternion.Euler(50f, -12f, 5f);
    }

    private void LateUpdate()
    {
        HandleDrag();

        if (isFollow && target != null)
        {
            FollowTarget();
        }
    }

    void FollowTarget()
    {
        Vector3 desiredPos = target.position
                             + transform.right * offset.x
                             + Vector3.up * offset.y
                             + transform.forward * offset.z;

        transform.position = Vector3.Lerp(transform.position, desiredPos, followSmooth * Time.deltaTime);
    }

    public float minDragDistance = 20f; // khoảng cần kéo trước khi kích hoạt drag
    private bool isDraggingActive = false;
    void HandleDrag()
    {
        // Nếu đang chạm UI thì bỏ qua
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            isDraggingActive = false; // reset
            isFollow = false;
            lastMousePos = Input.mousePosition;
        }

        if (Input.GetMouseButton(0) && isDragging)
        {
            Vector3 delta = Input.mousePosition - lastMousePos;

            // Kiểm tra xem kéo đủ xa chưa
            if (!isDraggingActive)
            {
                if (delta.magnitude >= minDragDistance)
                {
                    isDraggingActive = true; // bắt đầu drag
                }
                else
                {
                    return; // vẫn coi như tap — không kéo
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
}