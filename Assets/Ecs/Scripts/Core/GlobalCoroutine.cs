using System;
using System.Collections;
using UnityEngine;

public class GlobalCoroutine : AutoSingleton<GlobalCoroutine>
{
    private static WaitForEndOfFrame _endOfFrame;

    private void Start()
    {
        _endOfFrame = new WaitForEndOfFrame();
    }

    public static void StopAll()
    {
        Instance.StopAllCoroutines();
    }
    
    public static Coroutine Invoke(IEnumerator ienumerator)
    {
        return Instance.StartCoroutine(ienumerator);
    }

    public static void InvokeDelay(float second, Action onComplete)
    {
        Instance.StartCoroutine(DelayRoutine(second, onComplete));
    }

    public static void InvokeDelay(long miniSecond, Action onComplete)
    {
        Instance.StartCoroutine(DelayRoutine(1f / miniSecond, onComplete));
    }

    public static void InvokeDelayOneFrame(Action onComplete)
    {
        Instance.StartCoroutine(DelayOneFrameRoutine(onComplete));
    }

    static IEnumerator DelayRoutine(float second, Action onComplete)
    {
        yield return new WaitForSeconds(second);
        if(onComplete != null) onComplete.Invoke();
    }

    static IEnumerator DelayOneFrameRoutine(Action onComplete)
    {
        yield return _endOfFrame;
        if(onComplete != null) onComplete.Invoke();
    }
}