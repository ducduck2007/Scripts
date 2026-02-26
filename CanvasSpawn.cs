using System.Collections;
using TMPro;
using UnityEngine;

public class CanvasSpawn : MonoBehaviour
{
    public TMP_Text txtTimeSpawn;

    private Coroutine _co;
    private float _remain;

    private void OnDisable()
    {
        StopCountdown();
    }

    public void StartCountdown(float seconds)
    {
        seconds = Mathf.Max(0f, seconds);
        _remain = seconds;

        StopCountdown();
        _co = StartCoroutine(CoCountdown());
    }

    public void StopCountdown()
    {
        if (_co != null)
        {
            StopCoroutine(_co);
            _co = null;
        }
    }

    private IEnumerator CoCountdown()
    {
        while (_remain > 0f)
        {
            if (txtTimeSpawn != null)
                txtTimeSpawn.text = $"{Mathf.CeilToInt(_remain)}s";

            _remain -= Time.unscaledDeltaTime;
            yield return null;
        }

        if (txtTimeSpawn != null)
            txtTimeSpawn.text = "0s";

        _co = null;
    }
}
