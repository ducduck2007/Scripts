using UnityEngine;

public class NetworkDebugUI : MonoBehaviour
{
    private const bool ENABLE_DEBUG = false;

    private bool showDebug = true;

    private const float LEFT_PADDING = 330f;

    private static int gamePacketsThisSecond = 0;
    private static int gamePacketsLastSecond = 0;
    private static float gamePacketTimer = 0f;
    private static float lastGameSnapshotTime = 0f;
    private static float maxGameGap = 0f;
    private static float avgGameGap = 0f;
    private static int gameGapSamples = 0;

    private static int resourcePacketsThisSecond = 0;
    private static int resourcePacketsLastSecond = 0;
    private static float resourcePacketTimer = 0f;
    private static float lastResourceSnapshotTime = 0f;
    private static float maxResourceGap = 0f;
    private static float avgResourceGap = 0f;
    private static int resourceGapSamples = 0;

    public static void OnGameSnapshotReceived()
    {
        if (!ENABLE_DEBUG) return;

        gamePacketsThisSecond++;

        float now = Time.time;
        float gap = now - lastGameSnapshotTime;

        if (lastGameSnapshotTime > 0 && gap < 2f)
        {
            if (gap > maxGameGap) maxGameGap = gap;

            gameGapSamples++;
            avgGameGap = avgGameGap + (gap - avgGameGap) / Mathf.Min(gameGapSamples, 100);
        }

        lastGameSnapshotTime = now;
    }

    public static void OnResourceSnapshotReceived()
    {
        if (!ENABLE_DEBUG) return;

        resourcePacketsThisSecond++;

        float now = Time.time;
        float gap = now - lastResourceSnapshotTime;

        if (lastResourceSnapshotTime > 0 && gap < 2f)
        {
            if (gap > maxResourceGap) maxResourceGap = gap;

            resourceGapSamples++;
            avgResourceGap = avgResourceGap + (gap - avgResourceGap) / Mathf.Min(resourceGapSamples, 100);
        }

        lastResourceSnapshotTime = now;
    }

    public static void ResetStats()
    {
        if (!ENABLE_DEBUG) return;

        gamePacketsThisSecond = 0;
        gamePacketsLastSecond = 0;
        gamePacketTimer = 0f;
        lastGameSnapshotTime = 0f;
        maxGameGap = 0f;
        avgGameGap = 0f;
        gameGapSamples = 0;

        resourcePacketsThisSecond = 0;
        resourcePacketsLastSecond = 0;
        resourcePacketTimer = 0f;
        lastResourceSnapshotTime = 0f;
        maxResourceGap = 0f;
        avgResourceGap = 0f;
        resourceGapSamples = 0;
    }

    void Awake()
    {
        if (!ENABLE_DEBUG) { enabled = false; return; }
    }

    void Update()
    {
        if (!ENABLE_DEBUG) return;

        if (Input.GetKeyDown(KeyCode.F1))
            showDebug = !showDebug;

        gamePacketTimer += Time.deltaTime;
        if (gamePacketTimer >= 1f)
        {
            gamePacketsLastSecond = gamePacketsThisSecond;
            gamePacketsThisSecond = 0;
            gamePacketTimer = 0f;
        }

        resourcePacketTimer += Time.deltaTime;
        if (resourcePacketTimer >= 1f)
        {
            resourcePacketsLastSecond = resourcePacketsThisSecond;
            resourcePacketsThisSecond = 0;
            resourcePacketTimer = 0f;
        }
    }

    void OnGUI()
    {
        if (!ENABLE_DEBUG) return;
        if (!showDebug) return;

        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.fontSize = 14;
        style.alignment = TextAnchor.UpperLeft;

        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = 14;
        labelStyle.normal.textColor = Color.white;
        labelStyle.richText = true;

        float x0 = 10f + LEFT_PADDING;

        GUI.Box(new Rect(x0, 10, 350, 250), "Network Debug (F1 to hide)", style);

        int y = 35;
        int lineHeight = 22;

        GUI.Label(new Rect(x0 + 10, y, 330, 25), "=== GAME_SNAPSHOT (CMD 50) ===", labelStyle);
        y += lineHeight;

        string gamePpsColor = gamePacketsLastSecond >= 25 ? "green" :
                              (gamePacketsLastSecond >= 15 ? "yellow" : "red");

        GUI.Label(new Rect(x0 + 10, y, 330, 25),
            $"  Packets/sec: <color={gamePpsColor}>{gamePacketsLastSecond}</color> (target: 30)",
            labelStyle);
        y += lineHeight;

        float gameAvgMs = avgGameGap * 1000f;
        string gameAvgColor = gameAvgMs <= 40 ? "green" :
                              (gameAvgMs <= 80 ? "yellow" : "red");

        GUI.Label(new Rect(x0 + 10, y, 330, 25),
            $"  Avg gap: <color={gameAvgColor}>{gameAvgMs:F1}ms</color> (target: 33ms)",
            labelStyle);
        y += lineHeight;

        float gameMaxMs = maxGameGap * 1000f;
        string gameMaxColor = gameMaxMs <= 100 ? "green" :
                              (gameMaxMs <= 300 ? "yellow" : "red");

        GUI.Label(new Rect(x0 + 10, y, 330, 25),
            $"  Max gap: <color={gameMaxColor}>{gameMaxMs:F0}ms</color>",
            labelStyle);
        y += lineHeight;

        float timeSinceLastGame = Time.time - lastGameSnapshotTime;
        string gameLastColor = timeSinceLastGame <= 0.1f ? "green" :
                               (timeSinceLastGame <= 0.3f ? "yellow" : "red");

        GUI.Label(new Rect(x0 + 10, y, 330, 25),
            $"  Since last: <color={gameLastColor}>{timeSinceLastGame * 1000:F0}ms</color>",
            labelStyle);
        y += lineHeight;

        GUI.Label(new Rect(x0 + 10, y, 330, 25), "=== RESOURCE_SNAPSHOT (CMD 51) ===", labelStyle);
        y += lineHeight;

        string resourcePpsColor =
            resourcePacketsLastSecond >= 2 ? "green" :
            (resourcePacketsLastSecond == 1 ? "yellow" : "red");

        GUI.Label(new Rect(x0 + 10, y, 330, 25),
            $"  Packets/sec: <color={resourcePpsColor}>{resourcePacketsLastSecond}</color> (target: 1-2)",
            labelStyle);
        y += lineHeight;

        float resourceAvgMs = avgResourceGap * 1000f;
        string resourceAvgColor = resourceAvgMs <= 120 ? "green" :
                                  (resourceAvgMs <= 300 ? "yellow" : "red");

        GUI.Label(new Rect(x0 + 10, y, 330, 25),
            $"  Avg gap: <color={resourceAvgColor}>{resourceAvgMs:F1}ms</color> (target: 100ms)",
            labelStyle);
        y += lineHeight;

        float resourceMaxMs = maxResourceGap * 1000f;
        string resourceMaxColor = resourceMaxMs <= 300 ? "green" :
                                  (resourceMaxMs <= 1000 ? "yellow" : "red");

        GUI.Label(new Rect(x0 + 10, y, 330, 25),
            $"  Max gap: <color={resourceMaxColor}>{resourceMaxMs:F0}ms</color>",
            labelStyle);
        y += lineHeight;

        float timeSinceLastResource = Time.time - lastResourceSnapshotTime;
        string resourceLastColor = timeSinceLastResource <= 0.2f ? "green" :
                                   (timeSinceLastResource <= 0.5f ? "yellow" : "red");

        GUI.Label(new Rect(x0 + 10, y, 330, 25),
            $"  Since last: <color={resourceLastColor}>{timeSinceLastResource * 1000:F0}ms</color>",
            labelStyle);
    }
}
