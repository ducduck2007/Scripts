using UnityEngine;

public class LazyActivator : MonoBehaviour
{
    public Camera cam;
    public GameObject targetObject;
    public float expansion = 100f;
    public int checkEvery = 20;

    [Header("Behavior")]
    [Tooltip("Nếu true: bounds sẽ rebuild theo runtime (đắt). Nếu false: chỉ build 1 lần.")]
    public bool dynamicBounds = false;

    [Tooltip("Không SetActive root. Thay vào đó tắt/bật Renderer + Collider để tránh spike Awake/OnEnable.")]
    public bool toggleRenderersAndColliders = true;

    [Tooltip("Chống flap: chỉ đổi trạng thái nếu khác nhau liên tục trong X lần check.")]
    public int stableChecksRequired = 2;

    private bool _vis;
    private Bounds _bounds;
    private int _frame;
    private Transform _camTransform;
    private Plane[] _frustumPlanes;

    private Renderer[] _renderers;
    private Collider[] _colliders;

    private int _stableCount = 0;

    void Awake()
    {
        if (!cam) cam = Camera.main;
        if (!targetObject) { enabled = false; return; }

        if (cam != null)
            _camTransform = cam.transform;

        _renderers = targetObject.GetComponentsInChildren<Renderer>(true);
        _colliders = targetObject.GetComponentsInChildren<Collider>(true);

        _frustumPlanes = new Plane[6];

        RebuildBounds();

        // 🔥 QUAN TRỌNG: KHÔNG apply visible state ở đây nữa
        _vis = true; // assume visible ban đầu
    }

    void LateUpdate()
    {
        if (++_frame < checkEvery) return;
        _frame = 0;

        if (_camTransform == null)
        {
            if (cam != null) _camTransform = cam.transform;
            if (_camTransform == null) return;
        }

        if (dynamicBounds)
            RebuildBounds();

        var b = _bounds;
        if (expansion > 0f) b.Expand(expansion);

        GeometryUtility.CalculateFrustumPlanes(cam, _frustumPlanes);
        bool vis = GeometryUtility.TestPlanesAABB(_frustumPlanes, b);

        // chống flap
        if (vis == _vis)
        {
            _stableCount = 0;
            return;
        }

        _stableCount++;
        if (_stableCount < stableChecksRequired) return;
        _stableCount = 0;

        if (_vis != vis)
        {
            _vis = vis;
            ApplyVisibleState(_vis);
        }
    }

    private void ApplyVisibleState(bool visible)
    {
        if (!targetObject) return;

        if (!toggleRenderersAndColliders)
        {
            // nếu bạn vẫn muốn SetActive root (không khuyến nghị)
            if (targetObject.activeSelf != visible)
                targetObject.SetActive(visible);
            return;
        }

        // Tắt/bật renderers + colliders: nhẹ hơn rất nhiều so với SetActive cả root
        if (_renderers != null)
        {
            for (int i = 0; i < _renderers.Length; i++)
            {
                var r = _renderers[i];
                if (r) r.enabled = visible;
            }
        }

        if (_colliders != null)
        {
            for (int i = 0; i < _colliders.Length; i++)
            {
                var c = _colliders[i];
                if (c) c.enabled = visible;
            }
        }
    }

    private void RebuildBounds()
    {
        bool hasAny = false;

        if (_renderers != null && _renderers.Length > 0)
        {
            for (int i = 0; i < _renderers.Length; i++)
            {
                var r = _renderers[i];
                if (!r) continue;

                if (!hasAny) { _bounds = r.bounds; hasAny = true; }
                else _bounds.Encapsulate(r.bounds);
            }
        }

        if (!hasAny && _colliders != null && _colliders.Length > 0)
        {
            for (int i = 0; i < _colliders.Length; i++)
            {
                var c = _colliders[i];
                if (!c) continue;

                if (!hasAny) { _bounds = c.bounds; hasAny = true; }
                else _bounds.Encapsulate(c.bounds);
            }
        }

        if (!hasAny)
        {
            _bounds.center = targetObject.transform.position;
            _bounds.size = Vector3.one;
        }
    }
}
