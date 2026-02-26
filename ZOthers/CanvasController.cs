using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CanvasController : MonoBehaviour
{
    public static CanvasController Instance { get; private set; }

    [Header("Canvas References")]
    [Tooltip("Canvas chính có sẵn trong scene (không instantiate).")]
    public GameObject mainCanvas;

    [Tooltip("Danh sách PREFAB canvas phụ (shop, win, lose, settings, inventory, ...)")]
    public List<GameObject> otherCanvasPrefabs = new List<GameObject>();

    [Header("Spawn Parent")]
    [Tooltip("Parent để chứa các canvas instantiate. Nếu null sẽ dùng transform của CanvasController.")]
    public Transform canvasesRoot;

    [Header("Settings")]
    public bool hideOthersOnStart = true;
    public float transitionDuration = 0.25f;

    [Tooltip("Bật nếu muốn CanvasController không bị destroy khi load scene khác. Nếu tắt (mặc định), nó sẽ chết theo scene.")]
    public bool dontDestroyOnLoad = false;

    private readonly Dictionary<string, GameObject> _prefabDict = new();

    private readonly Dictionary<string, GameObject> _instanceDict = new();

    private GameObject _currentActiveCanvas;
    private Coroutine _transitionCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (dontDestroyOnLoad)
            DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private void Start()
    {
        if (canvasesRoot == null) canvasesRoot = transform;

        BuildPrefabDictionary();

        if (mainCanvas != null)
            EnsureCanvasGroup(mainCanvas);

        if (hideOthersOnStart)
            HideAllExceptMain();
    }

    private void BuildPrefabDictionary()
    {
        _prefabDict.Clear();

        foreach (var prefab in otherCanvasPrefabs)
        {
            if (prefab == null) continue;

            string key = prefab.name.ToLower();
            if (!_prefabDict.ContainsKey(key))
                _prefabDict.Add(key, prefab);
        }
    }

    public void HideAllExceptMain()
    {
        StopTransition();

        foreach (var kv in _instanceDict)
        {
            if (kv.Value != null)
                kv.Value.SetActive(false);
        }

        if (mainCanvas != null)
            mainCanvas.SetActive(true);

        _currentActiveCanvas = null;
    }

    public void ShowCanvas(string canvasName)
    {
        if (string.IsNullOrEmpty(canvasName)) return;

        var key = canvasName.ToLower();

        if (key == "main")
        {
            ShowMain();
            return;
        }

        var canvas = GetOrCreateCanvasInstance(key);
        if (canvas == null)
        {
            Debug.LogWarning($"[CanvasController] Canvas prefab '{canvasName}' không tìm thấy trong otherCanvasPrefabs!");
            return;
        }

        ShowCanvas(canvas);
    }

    public void HideCanvas(string canvasName)
    {
        if (string.IsNullOrEmpty(canvasName)) return;

        var key = canvasName.ToLower();

        if (key == "main")
        {
            return;
        }

        if (_instanceDict.TryGetValue(key, out var inst) && inst != null)
            HideCanvas(inst);
    }

    private GameObject GetOrCreateCanvasInstance(string key)
    {
        if (_instanceDict.TryGetValue(key, out var existing) && existing != null)
            return existing;

        if (!_prefabDict.TryGetValue(key, out var prefab) || prefab == null)
            return null;

        var inst = Instantiate(prefab, canvasesRoot);
        inst.name = prefab.name;

        EnsureCanvasGroup(inst);
        inst.SetActive(false);

        _instanceDict[key] = inst;
        return inst;
    }

    private void ShowMain()
    {
        StopTransition();

        if (_currentActiveCanvas != null)
            _currentActiveCanvas.SetActive(false);

        foreach (var kv in _instanceDict)
        {
            if (kv.Value != null)
                kv.Value.SetActive(false);
        }

        if (mainCanvas != null)
            mainCanvas.SetActive(true);

        _currentActiveCanvas = null;
    }

    public void ShowCanvas(GameObject canvas)
    {
        if (canvas == null) return;

        StopTransition();

        if (mainCanvas != null)
            mainCanvas.SetActive(false);

        if (_currentActiveCanvas != null && _currentActiveCanvas != canvas)
            _currentActiveCanvas.SetActive(false);

        canvas.SetActive(true);
        _currentActiveCanvas = canvas;

        _transitionCoroutine = StartCoroutine(FadeIn(canvas));
    }

    public void HideCanvas(GameObject canvas)
    {
        if (canvas == null) return;

        StopTransition();

        _transitionCoroutine = StartCoroutine(FadeOut(canvas));
    }

    private IEnumerator FadeIn(GameObject canvas)
    {
        var cg = canvas.GetComponent<CanvasGroup>();
        cg.alpha = 0f;
        canvas.transform.localScale = Vector3.one * 0.9f;

        float t = 0f;
        while (t < transitionDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / transitionDuration);

            cg.alpha = p;
            canvas.transform.localScale = Vector3.Lerp(Vector3.one * 0.9f, Vector3.one, p);
            yield return null;
        }

        cg.alpha = 1f;
        canvas.transform.localScale = Vector3.one;
    }

    private IEnumerator FadeOut(GameObject canvas)
    {
        var cg = canvas.GetComponent<CanvasGroup>();
        float t = 0f;

        while (t < transitionDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(1f - t / transitionDuration);

            cg.alpha = p;
            canvas.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.9f, 1f - p);
            yield return null;
        }

        canvas.SetActive(false);
        cg.alpha = 1f;
        canvas.transform.localScale = Vector3.one;

        if (_currentActiveCanvas == canvas)
            _currentActiveCanvas = null;

        if (mainCanvas != null)
            mainCanvas.SetActive(true);
    }

    private void EnsureCanvasGroup(GameObject canvas)
    {
        if (canvas.GetComponent<CanvasGroup>() == null)
            canvas.AddComponent<CanvasGroup>();
    }

    private void StopTransition()
    {
        if (_transitionCoroutine != null)
        {
            StopCoroutine(_transitionCoroutine);
            _transitionCoroutine = null;
        }
    }

    public void CleanupSpawnedCanvases()
    {
        StopTransition();

        foreach (var kv in _instanceDict)
        {
            if (kv.Value != null)
                Destroy(kv.Value);
        }

        _instanceDict.Clear();
        _currentActiveCanvas = null;

        if (mainCanvas != null)
            mainCanvas.SetActive(true);
    }

    private void OnSceneUnloaded(Scene scene)
    {
        CleanupSpawnedCanvases();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void ShowSpawnCanvas() => ShowCanvas("canvasspawn");
    public void HideSpawnCanvas() => HideCanvas("canvasspawn");
}
