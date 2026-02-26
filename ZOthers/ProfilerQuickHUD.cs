using System;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.Profiling;

public class ProfilerQuickHUD : MonoBehaviour
{
    private const bool ENABLE_DEBUG = false;

    [Header("Optional UI (can be null if using OnGUI only)")]
    public TextMeshProUGUI txtStats;
    public Button btnClose;

    [Header("Refresh Interval (seconds)")]
    public float refreshInterval = 0.5f;

    [Header("Show Options")]
    public bool showDeviceHeader = false;
    public bool showRenderingStats = true;
    public bool showAndroidRam = true;

    [Header("Heat Thresholds (tune as you like)")]
    public float mainMsGood = 6f;
    public float mainMsBad = 20f;
    public float renderMsGood = 4f;
    public float renderMsBad = 16f;

    public float gcKbGood = 0f;
    public float gcKbBad = 200f;

    public float allocPctGood = 0.35f;
    public float allocPctBad = 0.70f;

    public float fpsGood = 58f;
    public float fpsBad = 30f;

    [Header("OnGUI Panel")]
    public bool showHud = true;
    public KeyCode toggleKey = KeyCode.F2;
    public string boxTitle = "Profiler HUD (F2 to hide)";

    [Header("Scene Routing")]
    public string sceneGameName = "Game";
    public string scenePlayName = "Play";

    public float boxX = 10f;
    public float boxY = 10f;

    public float boxW = 420f;
    public float boxH = 260f;

    [Header("Layout")]
    public float leftPadding = 700f;
    public float innerPadding = 10f;
    public float topPadding = 28f;

    [Header("Font Size")]
    public int boxFontSize = 16;
    public int labelFontSize = 15;
    public float lineHeight = 20f;

    [Header("Bottom-Left (Game Scene)")]
    public float gameBottomLeftMarginX = 10f;
    public float gameBottomLeftMarginY = 10f;

    int frameCount;
    float timePassed;
    float fps;

    ProfilerRecorder rMainThread;
    ProfilerRecorder rRenderThread;
    ProfilerRecorder rGcAllocInFrame;

    ProfilerRecorder rBatches;
    ProfilerRecorder rSetPass;
    ProfilerRecorder rTris;
    ProfilerRecorder rVtx;

    static ProfilerQuickHUD instance;

    string _cachedText = "";
    float _nextRefreshAt = 0f;

    Coroutine _refreshCo;

    void Awake()
    {
        if (!ENABLE_DEBUG) { enabled = false; return; }

        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        if (!ENABLE_DEBUG) return;

        if (btnClose != null)
            btnClose.onClick.AddListener(() => gameObject.SetActive(false));

        rMainThread = TryStart(ProfilerCategory.Internal, "Main Thread");
        rRenderThread = TryStart(ProfilerCategory.Internal, "Render Thread");
        rGcAllocInFrame = TryStart(ProfilerCategory.Memory, "GC Allocated In Frame");

        if (showRenderingStats)
        {
            rBatches = TryStart(ProfilerCategory.Render, "Batches Count");
            rSetPass = TryStart(ProfilerCategory.Render, "SetPass Calls Count");
            rTris = TryStart(ProfilerCategory.Render, "Triangles Count");
            rVtx = TryStart(ProfilerCategory.Render, "Vertices Count");
        }

        _nextRefreshAt = Time.unscaledTime;
        _cachedText = BuildText();

        if (_refreshCo != null) StopCoroutine(_refreshCo);
        _refreshCo = StartCoroutine(RefreshRoutine());
    }

    void OnDisable()
    {
        if (!ENABLE_DEBUG) return;

        if (_refreshCo != null)
        {
            StopCoroutine(_refreshCo);
            _refreshCo = null;
        }

        DisposeRecorder(ref rMainThread);
        DisposeRecorder(ref rRenderThread);
        DisposeRecorder(ref rGcAllocInFrame);

        DisposeRecorder(ref rBatches);
        DisposeRecorder(ref rSetPass);
        DisposeRecorder(ref rTris);
        DisposeRecorder(ref rVtx);
    }

    void Update()
    {
        if (!ENABLE_DEBUG) return;

        if (Input.GetKeyDown(toggleKey))
            showHud = !showHud;

        frameCount++;
        timePassed += Time.unscaledDeltaTime;
        if (timePassed >= 1f)
        {
            fps = frameCount / timePassed;
            frameCount = 0;
            timePassed = 0f;
        }

        if (Time.unscaledTime >= _nextRefreshAt)
        {
            _cachedText = BuildText();
            _nextRefreshAt = Time.unscaledTime + Mathf.Max(0.05f, refreshInterval);
        }
    }

    IEnumerator RefreshRoutine()
    {
        if (!ENABLE_DEBUG) yield break;

        var wait = new WaitForSecondsRealtime(refreshInterval);
        while (ENABLE_DEBUG)
        {
            if (txtStats != null)
                txtStats.text = _cachedText;
            yield return wait;
        }
    }

    void OnGUI()
    {
        if (!ENABLE_DEBUG) return;
        if (!showHud) return;

        GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.fontSize = boxFontSize;
        boxStyle.alignment = TextAnchor.UpperLeft;

        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = labelFontSize;
        labelStyle.normal.textColor = Color.white;
        labelStyle.richText = true;

        string sceneName = SceneManager.GetActiveScene().name;

        float px, py;

        if (!string.IsNullOrEmpty(sceneGameName) &&
            sceneName.Equals(sceneGameName, StringComparison.OrdinalIgnoreCase))
        {
            px = gameBottomLeftMarginX;
            py = Screen.height - boxH - gameBottomLeftMarginY;
        }
        else
        {
            px = boxX + leftPadding;
            py = boxY;
        }

        Rect r = new Rect(px, py, boxW, boxH);
        GUI.Box(r, boxTitle, boxStyle);

        float x = px + innerPadding;
        float y = py + topPadding;
        float w = boxW - innerPadding * 2f;

        if (!string.IsNullOrEmpty(_cachedText))
        {
            var lines = _cachedText.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                if (y + lineHeight > py + boxH) break;

                string line = lines[i].TrimEnd('\r');
                GUI.Label(new Rect(x, y, w, lineHeight), line, labelStyle);
                y += lineHeight;
            }
        }
    }

    string BuildText()
    {
        if (!ENABLE_DEBUG) return "";

        var sb = new StringBuilder(256);

        if (showDeviceHeader)
        {
            sb.AppendLine($"Device: {SystemInfo.deviceModel}");
            sb.AppendLine($"CPU Threads: {SystemInfo.processorCount} | GPU: {SystemInfo.graphicsDeviceName}");
        }

        float mainMs = RecorderNsToMs(rMainThread);
        float renderMs = RecorderNsToMs(rRenderThread);

        string fpsStr = HeatValueHigherBetter(fps, fpsGood, fpsBad, "F0");
        string mainStr = HeatValue(mainMs, mainMsGood, mainMsBad, "F2");
        string renderStr = HeatValue(renderMs, renderMsGood, renderMsBad, "F2");

        sb.AppendLine($"FPS: {fpsStr} | Main: {mainStr} ms | Render: {renderStr} ms");

        long gcAllocBytes = RecorderLong(rGcAllocInFrame);
        if (gcAllocBytes >= 0)
        {
            float gcKb = gcAllocBytes / 1024f;
            string gcStr = HeatValue(gcKb, gcKbGood, gcKbBad, "F1");
            sb.AppendLine($"GC Alloc/Frame: {gcStr} KB");
        }

        long allocMb = Profiler.GetTotalAllocatedMemoryLong() / (1024 * 1024);
        long resMb = Profiler.GetTotalReservedMemoryLong() / (1024 * 1024);
        long monoMb = Profiler.GetMonoUsedSizeLong() / (1024 * 1024);
        long gcHeapMb = Profiler.GetMonoHeapSizeLong() / (1024 * 1024);

        float systemRamMb = Mathf.Max(1, SystemInfo.systemMemorySize);
        float allocPct = allocMb / systemRamMb;

        string allocColored = HeatValue(allocPct, allocPctGood, allocPctBad, "P0");
        sb.AppendLine($"Alloc: {allocMb} MB ({allocColored}) | Res: {resMb} MB");
        sb.AppendLine($"Mono: {monoMb} MB | GC Heap: {gcHeapMb} MB");
        sb.AppendLine($"System RAM Total: {SystemInfo.systemMemorySize} MB");

        if (showRenderingStats)
        {
            long batches = RecorderLong(rBatches);
            long setpass = RecorderLong(rSetPass);
            long tris = RecorderLong(rTris);
            long vtx = RecorderLong(rVtx);

            if (batches >= 0 || setpass >= 0 || tris >= 0 || vtx >= 0)
                sb.AppendLine($"Batches: {batches} | SetPass: {setpass} | Tris: {tris} | Vtx: {vtx}");
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        if (showAndroidRam)
        {
            long avail = GetAndroidAvailRamMb(out bool lowMem);
            sb.AppendLine($"Android Avail: {avail} MB | LowMem: {lowMem}");
        }
#endif

        return sb.ToString();
    }

    static string HeatValue(float value, float good, float bad, string format)
    {
        float sev = SeverityHigherWorse(value, good, bad);
        Color c = HeatColor(sev);
        return WrapColor(value.ToString(format), c);
    }

    static string HeatValueHigherBetter(float value, float good, float bad, string format)
    {
        float sev = SeverityHigherBetter(value, good, bad);
        Color c = HeatColor(sev);
        return WrapColor(value.ToString(format), c);
    }

    static float SeverityHigherWorse(float v, float good, float bad)
    {
        if (bad <= good) bad = good + 0.0001f;
        return Mathf.Clamp01(Mathf.InverseLerp(good, bad, v));
    }

    static float SeverityHigherBetter(float v, float good, float bad)
    {
        if (good <= bad) good = bad + 0.0001f;
        float t = Mathf.Clamp01(Mathf.InverseLerp(bad, good, v));
        return 1f - t;
    }

    static Color HeatColor(float severity01)
    {
        severity01 = Mathf.Clamp01(severity01);
        if (severity01 < 0.5f)
            return Color.Lerp(Color.green, Color.yellow, severity01 * 2f);
        return Color.Lerp(Color.yellow, Color.red, (severity01 - 0.5f) * 2f);
    }

    static string WrapColor(string text, Color c)
    {
        return $"<color=#{ColorUtility.ToHtmlStringRGB(c)}>{text}</color>";
    }

    static ProfilerRecorder TryStart(ProfilerCategory category, string counterName, int capacity = 15)
    {
        try { return ProfilerRecorder.StartNew(category, counterName, capacity); }
        catch { return default; }
    }

    static void DisposeRecorder(ref ProfilerRecorder r)
    {
        if (r.Valid) r.Dispose();
        r = default;
    }

    static long RecorderLong(ProfilerRecorder r) => r.Valid ? r.LastValue : -1;

    static float RecorderNsToMs(ProfilerRecorder r)
    {
        if (!r.Valid) return 0f;
        return r.LastValue / 1_000_000f;
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    long GetAndroidAvailRamMb(out bool lowMemory)
    {
        lowMemory = false;
        try
        {
            AndroidJavaObject activity =
                new AndroidJavaClass("com.unity3d.player.UnityPlayer")
                    .GetStatic<AndroidJavaObject>("currentActivity");

            AndroidJavaObject am = activity.Call<AndroidJavaObject>("getSystemService", "activity");
            AndroidJavaObject memInfo = new AndroidJavaObject("android.app.ActivityManager$MemoryInfo");
            am.Call("getMemoryInfo", memInfo);

            long availMem = memInfo.Get<long>("availMem") / (1024 * 1024);
            lowMemory = memInfo.Get<bool>("lowMemory");
            return availMem;
        }
        catch { return -1; }
    }
#endif
}
