using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class SkillButtonHoldTracker : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public float threshold = 0.25f;

    public int skillIndex = 0;          // 1/2/3
    public float previewDelay = 0.5f;   // yêu cầu 0.5s

    private Coroutine _coPreview;
    private bool _previewShown;
    private bool _isDown;

    public float LastDownTime { get; private set; } = -1f;
    public float LastUpTime { get; private set; } = -1f;

    public void OnPointerDown(PointerEventData eventData)
    {
        LastDownTime = Time.unscaledTime;
        LastUpTime = -1f;

        _isDown = true;
        _previewShown = false;

        // CHẶN DỨT ĐIỂM: vừa chạm là tắt hết aim canvas ngay (tránh hiện "ngay lập tức")
        if (MenuController.Instance != null)
            MenuController.Instance.HideAllAimCanvases();

        if (_coPreview != null) StopCoroutine(_coPreview);
        _coPreview = StartCoroutine(CoHoldPreview());

#if UNITY_EDITOR
        Debug.Log($"[CAST TAP] DOWN t={LastDownTime:0.000}");
#endif
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        LastUpTime = Time.unscaledTime;

        _isDown = false;

        if (_coPreview != null)
        {
            StopCoroutine(_coPreview);
            _coPreview = null;
        }

        // Nếu đã show preview thì thả tay -> hide (đúng kiểu giữ để ngắm)
        if (_previewShown && MenuController.Instance != null)
            MenuController.Instance.HideAllAimCanvases();

        _previewShown = false;

#if UNITY_EDITOR
        Debug.Log($"[CAST TAP] UP t={LastUpTime:0.000} dur={(LastUpTime - LastDownTime):0.000}");
#endif
    }

    public float GetHoldDurationNow()
    {
        if (LastDownTime < 0f) return -1f;
        // lúc Button.onClick chạy (thường sau PointerUp), ta tính theo thời điểm hiện tại
        return Time.unscaledTime - LastDownTime;
    }

    public bool IsQuickTapNow()
    {
        float d = GetHoldDurationNow();
        return d >= 0f && d <= threshold;
    }

    private IEnumerator CoHoldPreview()
    {
        // đợi 0.5s
        float t = 0f;
        while (t < previewDelay)
        {
            if (!_isDown) yield break; // thả trước 0.5s => auto=1 => không show
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        if (!_isDown) yield break;

        // Sau 0.5s vẫn đang giữ => auto=0 => show theo logic hiện tại
        if (MenuController.Instance != null && skillIndex > 0)
        {
            MenuController.Instance.ShowAimCanvasForSkill(skillIndex);
            _previewShown = true;
        }

        _coPreview = null;
    }
}