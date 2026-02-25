using UnityEngine;

public class SwipeRotateCharacter : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Nếu để trống: xoay chính GameObject này.")]
    public Transform target;

    [Header("Control")]
    [Tooltip("Độ nhạy theo pixel. Ví dụ 0.15f - 0.4f")]
    public float yawPerPixel = 0.25f;

    [Tooltip("Giới hạn góc yaw (độ)")]
    public float yawMax = 35f;

    [Tooltip("Tự trả về góc 0 khi thả tay")]
    public bool springBack = true;

    [Tooltip("Tốc độ spring back")]
    public float springSpeed = 10f;

    Vector2 _lastPos;
    bool _dragging;

    float _yaw;
    float _yawVel;
    Quaternion _baseRot;

    void Awake()
    {
        if (target == null) target = transform;
        _baseRot = target.localRotation;
    }

    void OnEnable()
    {
        if (target == null) target = transform;
        _baseRot = target.localRotation;
        _yaw = 0f;
        _yawVel = 0f;
    }

    void Update()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetMouseButtonDown(0))
        {
            _dragging = true;
            _lastPos = Input.mousePosition;
            _baseRot = target.localRotation;
        }
        else if (Input.GetMouseButton(0) && _dragging)
        {
            Rotate(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            _dragging = false;
        }
#else
        if (Input.touchCount == 0)
        {
            _dragging = false;
        }
        else
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began)
            {
                _dragging = true;
                _lastPos = t.position;
                _baseRot = target.localRotation;
            }
            else if (t.phase == TouchPhase.Moved && _dragging)
            {
                Rotate(t.position);
            }
            else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
            {
                _dragging = false;
            }
        }
#endif

        if (springBack && !_dragging)
        {
            _yaw = Mathf.SmoothDamp(_yaw, 0f, ref _yawVel, 1f / Mathf.Max(1f, springSpeed), Mathf.Infinity, Time.unscaledDeltaTime);
            ApplyYaw();
        }
    }

    void Rotate(Vector2 pos)
    {
        Vector2 delta = pos - _lastPos;

        _yaw += delta.x * yawPerPixel;
        _yaw = Mathf.Clamp(_yaw, -yawMax, yawMax);

        ApplyYaw();

        _lastPos = pos;
    }

    void ApplyYaw()
    {
        if (target == null) return;
        target.localRotation = _baseRot * Quaternion.Euler(0f, _yaw, 0f);
    }

    public void ResetRotation()
    {
        if (target == null) return;
        _yaw = 0f;
        _yawVel = 0f;
        target.localRotation = _baseRot;
    }
}