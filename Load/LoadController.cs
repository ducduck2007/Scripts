using System.Collections;
using UnityEngine;

public class LoadController : ManualSingleton<LoadController>
{
    private GameObject Load(string namePath) => Resources.Load<GameObject>(namePath);

    private GameObject _loadWait;
    public GameObject LoadWait
    {
        get
        {
            if (_loadWait == null)
            {
                var prefab = Load(PathResource.LoadWait);
                if (prefab != null) _loadWait = AgentUnity.InstanceObject(prefab, transform);
                if (_loadWait != null) _loadWait.SetActive(false);
            }
            return _loadWait;
        }
    }

    private GameObject _loadWaitData;
    public GameObject LoadWaitData
    {
        get
        {
            if (_loadWaitData == null)
            {
                var prefab = Load(PathResource.LoadWaitData);
                if (prefab != null) _loadWaitData = AgentUnity.InstanceObject(prefab, transform);
                if (_loadWaitData != null) _loadWaitData.SetActive(false);
            }
            return _loadWaitData;
        }
    }

    private LoadPercentChangeInfo _loadPercenChangeInfo;
    public LoadPercentChangeInfo LoadPercentChangeInfo
    {
        get
        {
            if (_loadPercenChangeInfo == null)
            {
                var prefab = Load(PathResource.LoadPercent);
                if (prefab != null)
                    _loadPercenChangeInfo = AgentUnity.InstanceObject<LoadPercentChangeInfo>(prefab, transform);

                if (_loadPercenChangeInfo != null)
                    _loadPercenChangeInfo.gameObject.SetActive(false);
            }
            return _loadPercenChangeInfo;
        }
    }

    private bool _isLoad;
    private Coroutine _coLoadWaitData;
    private Coroutine _coLoadWait;
    private Coroutine _coCheckConnect;

    private bool _prewarmed;
    private bool _lowMemoryTriggered;

    protected override void Awake()
    {
        base.Awake();
        Application.lowMemory += OnLowMemory;
    }

    private void OnDestroy()
    {
        Application.lowMemory -= OnLowMemory;
    }

    private void OnLowMemory()
    {
        _lowMemoryTriggered = true;

        // Tắt ngay UI loading nếu đang bật để giảm áp lực
        HideLoadWait();
        HideLoadWaitData();

        // Dọn rác mạnh (đỡ chết ngay trên máy yếu)
        StartCoroutine(CoEmergencyCleanup());
    }

    private IEnumerator CoEmergencyCleanup()
    {
        // chờ 1 frame cho Unity ổn định
        yield return null;

        // Unload + GC
        yield return Resources.UnloadUnusedAssets();
        System.GC.Collect();
    }

    /// <summary>
    /// Prewarm có điều kiện (máy yếu / vừa bị lowMemory => skip)
    /// </summary>
    public void PrewarmSafe()
    {
        if (_prewarmed) return;
        _prewarmed = true;

        // Nếu vừa bị low memory thì đừng đụng Resources nữa
        if (_lowMemoryTriggered) return;

        // Gate theo RAM thiết bị (Tab A7 thường 3GB/4GB; bạn tuỳ chỉnh ngưỡng)
        // Nếu máy <= 4096MB thì bỏ prewarm để tránh spike lúc vào trận.
        int ramMb = SystemInfo.systemMemorySize;
        if (ramMb <= 4096)
            return;

        _ = LoadWait;
        _ = LoadWaitData;
        _ = LoadPercentChangeInfo;
    }

    // ===== LoadWaitData (delay show) =====
    public void ShowCheckLoadWait(bool val, int time = 10, float timecheck = 0.2f)
    {
        _isLoad = val;

        if (_coLoadWaitData != null)
        {
            StopCoroutine(_coLoadWaitData);
            _coLoadWaitData = null;
        }

        if (!val)
        {
            HideLoadWaitData();
            return;
        }

        _coLoadWaitData = StartCoroutine(CoShowLoadWaitData(time, timecheck));
    }

    private IEnumerator CoShowLoadWaitData(int time, float timecheck)
    {
        yield return new WaitForSeconds(timecheck);

        if (!_isLoad) { _coLoadWaitData = null; yield break; }

        // low mem thì đừng instantiate mới
        if (_lowMemoryTriggered) { _coLoadWaitData = null; yield break; }

        var o = LoadWaitData;
        if (o != null) o.SetActive(true);

        yield return new WaitForSeconds(time);

        HideLoadWaitData();
        _coLoadWaitData = null;
    }

    private void HideLoadWaitData()
    {
        if (_loadWaitData != null) _loadWaitData.SetActive(false);
    }

    // ===== LoadWait (global spinner) =====
    public void ShowLoadWait(bool val = true, float time = 15f)
    {
        if (_coLoadWait != null)
        {
            StopCoroutine(_coLoadWait);
            _coLoadWait = null;
        }

        if (!val)
        {
            HideLoadWait();
            return;
        }

        // low mem thì đừng instantiate mới
        if (_lowMemoryTriggered) return;

        var o = LoadWait;
        if (o == null) return;

        o.SetActive(true);
        _coLoadWait = StartCoroutine(CoAutoHideLoadWait(time));
    }

    private IEnumerator CoAutoHideLoadWait(float time)
    {
        yield return new WaitForSeconds(time);
        HideLoadWait();
        _coLoadWait = null;
    }

    private void HideLoadWait()
    {
        if (_loadWait != null) _loadWait.SetActive(false);
    }

    // ===== Connect wait =====
    public void ShowLoadWaitConnectServer(bool val = true, float time = 8f)
    {
        if (_coCheckConnect != null)
        {
            StopCoroutine(_coCheckConnect);
            _coCheckConnect = null;
        }

        if (val)
        {
            if (_lowMemoryTriggered) return;

            var o = LoadWait;
            if (o != null) o.SetActive(true);
            _coCheckConnect = StartCoroutine(CoCheckConnect(time));
        }
        else
        {
            HideLoadWait();
        }
    }

    private IEnumerator CoCheckConnect(float time)
    {
        yield return new WaitForSeconds(time);
        HideLoadWait();
        _coCheckConnect = null;

        if (!B.Instance.isConnectServerSuccess)
            NetworkControler.Instance.OnDisconnectServer("");
    }

    // ===== Utilities =====
    public void DestroyAllChildsToTime(Transform parent, float time = 1f, bool val = true)
    {
        if (!val || parent == null) return;
        StartCoroutine(CoDestroyChildren(parent, time));
    }

    private IEnumerator CoDestroyChildren(Transform parent, float time)
    {
        yield return new WaitForSeconds(time);
        if (parent == null) yield break;

        foreach (Transform t in parent)
            if (t != null) Object.Destroy(t.gameObject);
    }

    public void ShowLoadPercentChangeInfo(bool isShow)
    {
        C.SetBusy(isShow);

        if (_lowMemoryTriggered && isShow)
            return;

        if (LoadPercentChangeInfo != null)
            LoadPercentChangeInfo.ShowLoadPercent(isShow);
    }
}
