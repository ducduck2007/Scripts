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

    [Header("Intro Cinematic")]
    public Vector3 introStartPos = new Vector3(268f, 233f, -31f);
    public Vector3 introStartRot = new Vector3(38.174f, -11.487f, 0.674f);
    public Vector3 introCenterPos = new Vector3(341.719f, 352f, 38f);
    public Vector3 introCenterRot = new Vector3(44.585f, -21.13f, 0.201f);
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
    private Vector3 _introEndPos;
    private Vector3 _introEndRot;

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

    /// <summary>
    /// Gọi từ TranDauControl khi bắt đầu spawn effect.
    /// Camera sẽ bay từ introStart → end (vị trí follow thực) trong duration giây.
    /// </summary>
    public void PlayIntroFlyTo(float duration)
    {
        if (target == null) return;

        _introDuration = Mathf.Max(0.1f, duration);
        _introElapsed = 0f;
        _isPlayingIntro = true;
        isFollow = false;

        // Đặt camera về điểm start ngay lập tức
        transform.position = introStartPos;
        transform.rotation = Quaternion.Euler(introStartRot);
        currentDynamicRotationX = introStartRot.x;

        // Tính end position: vị trí follow thực của player tại thời điểm này
        _introEndPos = ComputeDesiredFollowPos();
        _introEndRot = new Vector3(midLaneRotationX, baseRotationY, baseRotationZ);
    }

    private Vector3 ComputeDesiredFollowPos()
    {
        if (target == null) return introStartPos;

        // Dùng rotation end để tính offset
        Quaternion endRot = Quaternion.Euler(_introEndRot != Vector3.zero
            ? _introEndRot
            : new Vector3(midLaneRotationX, baseRotationY, baseRotationZ));

        Vector3 right = endRot * Vector3.right;
        Vector3 up = Vector3.up;
        Vector3 forward = endRot * Vector3.forward;

        return target.position
               + right * offset.x
               + up * offset.y
               + forward * offset.z;
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

        // Ease in-out cubic
        float tEased = EaseInOutCustom(t, introEaseInPower, introEaseOutPower);

        // Cubic Bezier: P0=start, P1=center, P2=center, P3=end (dạng quadratic bọc cubic)
        Vector3 pos = CubicBezier(introStartPos, introCenterPos, introCenterPos, _introEndPos, tEased);
        Quaternion rot = Quaternion.Slerp(
            Quaternion.Euler(introStartRot),
            Quaternion.Euler(_introEndRot),
            tEased
        );

        transform.position = pos;
        transform.rotation = rot;
        currentDynamicRotationX = rot.eulerAngles.x;

        if (t >= 1f)
        {
            _isPlayingIntro = false;
            isFollow = true;
            // Snap về đúng rotation follow
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

    private Vector3 CubicBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float u = 1f - t;
        return u * u * u * p0
             + 3f * u * u * t * p1
             + 3f * u * t * t * p2
             + t * t * t * p3;
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

        // Vẽ đường intro bezier để preview trong editor
        Gizmos.color = Color.cyan;
        Vector3 prev = introStartPos;
        for (int i = 1; i <= 20; i++)
        {
            float t = i / 20f;
            Vector3 next = CubicBezier(introStartPos, introCenterPos, introCenterPos,
                new Vector3(344.1235f, 420.2056f, 89.26767f), t);
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }
}