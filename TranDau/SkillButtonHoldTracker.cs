using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class SkillButtonHoldTracker : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public float threshold = 0.25f;
    public int skillIndex = 0;
    public float previewDelay = 0.25f;

    private Coroutine _coPreview;
    private bool _aimShown;       // canvas đang hiện
    private bool _isDown;
    private bool _castFired;      // Button.onClick đã fire

    public float LastDownTime { get; private set; } = -1f;
    public float LastUpTime { get; private set; } = -1f;

    public void OnPointerDown(PointerEventData eventData)
    {
        LastDownTime = Time.unscaledTime;
        LastUpTime = -1f;
        _isDown = true;
        _aimShown = false;
        _castFired = false;

        // Đảm bảo canvas tắt khi bắt đầu chạm
        MenuController.Instance?.HideAllAimCanvases();

        if (_coPreview != null) StopCoroutine(_coPreview);
        _coPreview = StartCoroutine(CoHoldPreview());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        LastUpTime = Time.unscaledTime;
        _isDown = false;

        if (_coPreview != null) { StopCoroutine(_coPreview); _coPreview = null; }

        // Nếu canvas đang hiện:
        // - Nếu cast đã fire (click nhanh ko có aim) → không cần làm gì
        // - Nếu chưa fire → đây là trường hợp thả mà sẽ fire ngay sau 
        //   → để CoHideAfterCast xử lý
        if (_aimShown && !_castFired)
            StartCoroutine(CoHideAfterCast());
        else if (!_aimShown)
        { /* canvas chưa hiện, không làm gì */ }
    }

    /// <summary>
    /// Gọi từ MenuController khi Button.onClick fire (TungChieu1/2/3)
    /// </summary>
    public void OnCastFired()
    {
        _castFired = true;
        // Canvas vẫn sáng để CastSkill đọc aim.
        // Sau 1 frame thì tắt (CastSkill chạy cùng frame với onClick)
        StartCoroutine(CoHideNextFrame());
    }

    public float GetHoldDurationNow()
    {
        if (LastDownTime < 0f) return -1f;
        return Time.unscaledTime - LastDownTime;
    }

    public bool IsQuickTapNow()
    {
        float d = GetHoldDurationNow();
        return d >= 0f && d <= threshold;
    }

    // Hiện canvas sau previewDelay giây giữ
    private IEnumerator CoHoldPreview()
    {
        float t = 0f;
        while (t < previewDelay)
        {
            if (!_isDown) yield break;
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        if (!_isDown) yield break;

        if (MenuController.Instance != null && skillIndex > 0)
        {
            MenuController.Instance.ShowAimCanvasForSkill(skillIndex);
            _aimShown = true;
        }

        _coPreview = null;
    }

    // Tắt canvas sau khi cast đã fire (chờ 1 frame để CastSkill đọc xong aim)
    private IEnumerator CoHideNextFrame()
    {
        yield return null; // chờ 1 frame
        MenuController.Instance?.HideAllAimCanvases();
        _aimShown = false;
        _castFired = false;
    }

    // Tắt canvas sau khi OnPointerUp mà cast chưa fire
    // (vd: user thả tay sau khi giữ, Button.onClick sẽ fire ngay)
    private IEnumerator CoHideAfterCast()
    {
        float timeout = 0.1f;
        float t = 0f;
        while (t < timeout)
        {
            if (_castFired)
            {
                // Cast đã fire, CoHideNextFrame đang xử lý
                yield break;
            }
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        // Timeout: cast không fire (pointer exit?) → hide luôn
        MenuController.Instance?.HideAllAimCanvases();
        _aimShown = false;
    }
}