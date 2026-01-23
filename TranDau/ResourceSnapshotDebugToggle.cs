using System.Threading;
using UnityEngine;

public class ResourceSnapshotDebugToggle : MonoBehaviour
{
    public bool enableDebug = true;

    private void Awake()
    {
        UdpResourceSnapshotSystem.DebugLog = enableDebug;
        Debug.Log($"[CMD51 DEBUG] Awake | mainThreadId={Thread.CurrentThread.ManagedThreadId} | enableDebug={enableDebug}");
    }
}
