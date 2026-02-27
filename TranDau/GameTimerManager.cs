using UnityEngine;

public class GameTimerManager : MonoBehaviour
{
    public static GameTimerManager Instance;

    private float _startTime;
    private bool _started = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
    }

    public void StartTimer()
    {
        _startTime = Time.time;
        _started = true;
    }

    public float GetTimeElapsed()
    {
        if (!_started) return 0f;
        return Time.time - _startTime;
    }

    public void ResetTimer()
    {
        _startTime = Time.time;
    }
}
