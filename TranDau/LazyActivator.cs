using UnityEngine;

public class LazyActivator : MonoBehaviour
{
    public Camera cam;
    public GameObject targetObject;
    public float expansion = -1f;
    public int checkEvery = 6;

    private bool _vis;
    private Bounds _bounds;
    private int _frame;
    private Transform _camTransform;
    private Plane[] _frustumPlanes;

    void Awake()
    {
        if (!cam) cam = Camera.main;
        if (!targetObject) { enabled = false; return; }

        if (cam != null)
            _camTransform = cam.transform;

        bool wasOn = targetObject.activeSelf;
        targetObject.SetActive(true);

        var rends = targetObject.GetComponentsInChildren<Renderer>(true);
        var cols = targetObject.GetComponentsInChildren<Collider>(true);

        _bounds = rends.Length > 0 ? rends[0].bounds :
                  cols.Length > 0 ? cols[0].bounds :
                  new Bounds(targetObject.transform.position, Vector3.one);

        foreach (var r in rends) _bounds.Encapsulate(r.bounds);
        foreach (var c in cols) _bounds.Encapsulate(c.bounds);

        targetObject.SetActive(wasOn);
        
        _frustumPlanes = new Plane[6];
    }

    void LateUpdate()
    {
        if (++_frame < checkEvery) return;
        _frame = 0;

        if (_camTransform == null)
        {
            if (cam != null) _camTransform = cam.transform;
            return;
        }

        var b = _bounds;
        if (expansion != 0f)
            b.Expand(expansion * -1f);

        GeometryUtility.CalculateFrustumPlanes(cam, _frustumPlanes);
        bool vis = GeometryUtility.TestPlanesAABB(_frustumPlanes, b);

        if (_vis != vis)
        {
            _vis = vis;
            targetObject.SetActive(vis);
        }
    }
}