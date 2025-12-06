using UnityEngine;

/// <summary>
/// Là nơi trung gian xử lý các Corountine
/// How to use: Gắn vào đối tượng không bao giờ bị disable trong game
/// </summary>
public class ProcessActionDelayTime : MonoBehaviour
{
    internal static ProcessActionDelayTime Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }

    internal void RegisterAction(UnityEngine.Events.UnityAction action, long timeMiliSeconds = 0L)
    {
        StartCoroutine(ProcessAction(action, timeMiliSeconds));
    }
    private System.Collections.IEnumerator ProcessAction(UnityEngine.Events.UnityAction action, long timeMiliSeconds)
    {
        yield return new WaitForSeconds((float)timeMiliSeconds / 1000F);
        if ((action != null))
            action.Invoke();
    }
    internal void RegisterAction(UnityEngine.Events.UnityAction action, float secondsDelay)
    {
        StartCoroutine(ProcessAction(action, secondsDelay));
    }
    private System.Collections.IEnumerator ProcessAction(UnityEngine.Events.UnityAction action, float secondsDelay)
    {
        yield return new WaitForSeconds(secondsDelay);
        if ((action != null))
            action.Invoke();
    }
    public void DisableAllCoroutine()
    {
        this.StopAllCoroutines();
    }
}
