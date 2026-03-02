using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Đồng bộ 2 điều kiện trước khi activate scene Play:
///   1. Scene đã load xong assets (progress >= 0.9f)
///   2. CMD GAME_START đã nhận và data đã được ghi vào B.Instance
/// </summary>
public static class SceneReadyGate
{
    private static bool _sceneReady;
    private static bool _gameStartReceived;
    private static AsyncOperation _pendingOp;

    public static void Reset()
    {
        _sceneReady = false;
        _gameStartReceived = false;
        _pendingOp = null;
    }

    /// <summary>
    /// Gọi từ LoadVaoTran khi AsyncOperation.progress >= 0.9f
    /// </summary>
    public static void MarkSceneReady(AsyncOperation op)
    {
        _sceneReady = true;
        _pendingOp = op;
        TryActivate();
    }

    /// <summary>
    /// Gọi từ CommandGameStartSystem sau khi ghi xong data
    /// </summary>
    public static void MarkGameStartReceived()
    {
        _gameStartReceived = true;
        TryActivate();
    }

    private static void TryActivate()
    {
        Debug.Log($"[SceneReadyGate] TryActivate sceneReady={_sceneReady} gameStart={_gameStartReceived} pendingOp={(_pendingOp != null)}");
        if (!_sceneReady || !_gameStartReceived) return;
        if (_pendingOp == null) return;

        _pendingOp.allowSceneActivation = true;
        _pendingOp = null;
        Debug.Log("[SceneReadyGate] allowSceneActivation = true");
    }
}