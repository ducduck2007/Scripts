using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LowHpVignetteController : MonoBehaviour
{
    public Image img;
    private Material _mat;

    private const float PulseIntensity = 0.75f;
    private const float PulseCooldown = 0.45f;
    private const float FadeInDuration = 0.12f;  // thời gian xuất hiện
    private const float HoldDuration = 0.15f;  // giữ ở đỉnh
    private const float FadeOutDuration = 0.55f;  // thời gian tắt dần

    private float _lastPulseTime = -999f;
    private Coroutine _pulseCo;

    private void Awake()
    {
        if (!img) img = GetComponent<Image>();
        if (img == null)
        {
            Debug.LogWarning("[LowHpVignetteController] Missing Image reference.");
            enabled = false;
            return;
        }

        _mat = Instantiate(img.material);
        img.material = _mat;
        SetIntensity(0f);
    }

    public void SetHpPercent(float hp01) { }

    public void PulseDamageOnce()
    {
        if (!gameObject.activeInHierarchy) return;

        float now = Time.unscaledTime;
        if (now - _lastPulseTime < PulseCooldown) return;
        _lastPulseTime = now;

        if (_pulseCo != null) { StopCoroutine(_pulseCo); _pulseCo = null; }
        _pulseCo = StartCoroutine(CoPulse());
    }

    private IEnumerator CoPulse()
    {
        float peak = Mathf.Clamp01(PulseIntensity);

        float t = 0f;
        while (t < FadeInDuration)
        {
            t += Time.unscaledDeltaTime;
            float u = Mathf.Clamp01(t / FadeInDuration);
            SetIntensity(Mathf.Lerp(0f, peak, u));
            yield return null;
        }

        SetIntensity(peak);
        t = 0f;
        while (t < HoldDuration)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        t = 0f;
        while (t < FadeOutDuration)
        {
            t += Time.unscaledDeltaTime;
            float u = Mathf.Clamp01(t / FadeOutDuration);
            float eased = 1f - (u * u);
            SetIntensity(peak * eased);
            yield return null;
        }

        SetIntensity(0f);
        _pulseCo = null;
    }

    private void SetIntensity(float intensity)
    {
        if (_mat == null) return;
        intensity = Mathf.Clamp01(intensity);
        _mat.SetFloat("_Intensity", intensity);
        if (img != null) img.enabled = intensity > 0.001f;
    }
}