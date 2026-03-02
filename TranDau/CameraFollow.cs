using UnityEngine;
using UnityEngine.EventSystems;

public class CameraFollow : MonoBehaviour
{
    public static CameraFollow Instance;

    public Transform target;
    public Vector3 offset = new Vector3(50, 200, -250);
    public float followSmooth = 5f;
    public float dragSpeed = 0.02f;

    [Header("Lane-Based Camera Rotation")]
    public float midLaneRotationX = 58.5f;
    public float botLaneRotationX = 66f;
    public float topLaneRotationX = 48f;
    public float baseRotationY = -18f;
    public float baseRotationZ = 5f;

    [Header("Lane Z Boundaries")]
    public float midLaneZ = 0f;
    public float topLaneZ = 150f;
    public float botLaneZ = -150f;

    [Header("Smooth Transition")]
    public float rotationTransitionSpeed = 3f;

    [Header("Intro Orbit Settings")]
    [Tooltip("Bán kính quỹ đạo quay quanh vị trí spawn")]
    public float introOrbitRadius = 300f;
    [Tooltip("Độ cao camera khi quay")]
    public float introOrbitHeight = 250f;
    [Tooltip("Góc quay tổng cộng (độ). VD: 270 = quay 3/4 vòng")]
    public float introOrbitDegrees = 270f;
    [Tooltip("Góc bắt đầu quay (độ). 0 = phía trước, 90 = bên phải")]
    public float introStartAngle = -90f;
    [Tooltip("Tỷ lệ thời gian dành cho orbit (0-1), phần còn lại settle vào follow cam")]
    public float introOrbitPortion = 0.7f;

    [Header("Intro Easing")]
    public float introEaseInPower = 2f;
    public float introEaseOutPower = 2f;

    private bool isFollow = true;
    private bool isDragging = false;
    private Vector3 lastMousePos;
    private float currentDynamicRotationX;

    // Intro state
    private bool _isPlayingIntro = false;
    private float _introDuration = 0f;
    private float _introElapsed = 0f;
    private Vector3 _introOrbitCenter;
    private Vector3 _introEndPos;
    private Vector3 _introEndRot;
    private Vector3 _orbitLastPos;
    private Quaternion _orbitLastRot;

    private void Awake()
    {
        Instance = this;
        currentDynamicRotationX = midLaneRotationX;
    }

    public void SetTarget(Transform tran)
    {
        target = tran;
        transform.rotation = Quaternion.Euler(midLaneRotationX, baseRotationY, baseRotationZ);
    }

    public void PlayIntroFlyTo(float duration, Vector3 spawnPos)
    {
        if (target == null) return;

        _introDuration = Mathf.Max(0.1f, duration);
        _introElapsed = 0f;
        _isPlayingIntro = true;
        isFollow = false;

        _introOrbitCenter = spawnPos;

        _introEndRot = new Vector3(midLaneRotationX, baseRotationY, baseRotationZ);
        Quaternion endRot = Quaternion.Euler(_introEndRot);
        _introEndPos = spawnPos
                       + endRot * Vector3.right * offset.x
                       + Vector3.up * offset.y
                       + endRot * Vector3.forward * offset.z;

        float startAngleRad = introStartAngle * Mathf.Deg2Rad;
        Vector3 startPos = _introOrbitCenter + new Vector3(
            Mathf.Sin(startAngleRad) * introOrbitRadius,
            introOrbitHeight,
            Mathf.Cos(startAngleRad) * introOrbitRadius
        );

        transform.position = startPos;
        transform.LookAt(_introOrbitCenter + Vector3.up * 10f);
        currentDynamicRotationX = transform.eulerAngles.x;
    }

    private void LateUpdate()
    {
        if (_isPlayingIntro)
        {
            UpdateIntro();
            return;
        }

        if (isFollow && target != null)
        {
            UpdateDynamicRotation();
            FollowTarget();
        }
    }

    private void UpdateIntro()
    {
        _introElapsed += Time.deltaTime;
        float t = Mathf.Clamp01(_introElapsed / _introDuration);

        float orbitEnd = Mathf.Clamp01(introOrbitPortion);

        if (t <= orbitEnd)
        {
            // ===== PHASE 1: Orbit quanh vị trí spawn =====
            float orbitT = t / orbitEnd;
            float easedT = EaseInOutCustom(orbitT, introEaseInPower, introEaseOutPower);

            float angle = (introStartAngle + easedT * introOrbitDegrees) * Mathf.Deg2Rad;

            // Bán kính và độ cao thu nhỏ dần → zoom in effect
            float radius = Mathf.Lerp(introOrbitRadius, introOrbitRadius * 0.7f, easedT);
            float height = Mathf.Lerp(introOrbitHeight, introOrbitHeight * 0.85f, easedT);

            Vector3 pos = _introOrbitCenter + new Vector3(
                Mathf.Sin(angle) * radius,
                height,
                Mathf.Cos(angle) * radius
            );

            transform.position = pos;
            transform.LookAt(_introOrbitCenter + Vector3.up * 10f);

            // Snapshot cuối orbit để settle mượt
            _orbitLastPos = transform.position;
            _orbitLastRot = transform.rotation;
        }
        else
        {
            // ===== PHASE 2: Settle vào vị trí follow gameplay =====
            float settleT = (t - orbitEnd) / (1f - orbitEnd);
            float easedSettle = settleT * settleT * (3f - 2f * settleT); // smoothstep

            transform.position = Vector3.Lerp(_orbitLastPos, _introEndPos, easedSettle);
            transform.rotation = Quaternion.Slerp(_orbitLastRot, Quaternion.Euler(_introEndRot), easedSettle);
        }

        currentDynamicRotationX = transform.eulerAngles.x;

        if (t >= 1f)
        {
            _isPlayingIntro = false;
            isFollow = true;
            transform.position = _introEndPos;
            transform.rotation = Quaternion.Euler(_introEndRot);
            currentDynamicRotationX = midLaneRotationX;
        }
    }

    private float EaseInOutCustom(float t, float easeIn, float easeOut)
    {
        if (t < 0.5f)
            return 0.5f * Mathf.Pow(2f * t, easeIn);
        else
            return 1f - 0.5f * Mathf.Pow(2f * (1f - t), easeOut);
    }

    void UpdateDynamicRotation()
    {
        if (target == null) return;

        float targetZ = target.position.z;
        float targetRotationX;

        if (targetZ >= midLaneZ)
        {
            float t = Mathf.InverseLerp(midLaneZ, topLaneZ, targetZ);
            targetRotationX = Mathf.Lerp(midLaneRotationX, topLaneRotationX, t);
        }
        else
        {
            float t = Mathf.InverseLerp(midLaneZ, botLaneZ, targetZ);
            targetRotationX = Mathf.Lerp(midLaneRotationX, botLaneRotationX, t);
        }

        currentDynamicRotationX = Mathf.Lerp(
            currentDynamicRotationX,
            targetRotationX,
            rotationTransitionSpeed * Time.deltaTime
        );

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
                    isDraggingActive = true;
                else
                    return;
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

    private void OnDrawGizmosSelected()
    {
        if (target == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(new Vector3(-200, 0, topLaneZ), new Vector3(200, 0, topLaneZ));
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(new Vector3(-200, 0, midLaneZ), new Vector3(200, 0, midLaneZ));
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(-200, 0, botLaneZ), new Vector3(200, 0, botLaneZ));
    }

    public void SnapToGameplayPosition(Vector3 focusPos)
    {
        // ✅ hủy intro nếu đang chạy
        _isPlayingIntro = false;

        // ✅ follow ngay
        isFollow = true;

        // set rotation theo lane default (giống SetTarget)
        transform.rotation = Quaternion.Euler(midLaneRotationX, baseRotationY, baseRotationZ);
        currentDynamicRotationX = midLaneRotationX;

        // tính đúng vị trí camera gameplay theo offset hiện tại
        Vector3 desiredPos = focusPos
                             + transform.right * offset.x
                             + Vector3.up * offset.y
                             + transform.forward * offset.z;

        transform.position = desiredPos;
    }
}