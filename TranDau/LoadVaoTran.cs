using System.Collections;
using TMPro;
using UnityEngine;

public class LoadVaoTran : ScaleScreen
{
    public TextMeshProUGUI loadingText;

    private Coroutine routine;
    private CanvasGroup _cg;

    protected override void OnEnable()
    {
        base.OnEnable();

        // Tạo CanvasGroup một lần
        if (_cg == null)
        {
            _cg = GetComponent<CanvasGroup>();
            if (_cg == null) _cg = gameObject.AddComponent<CanvasGroup>();
            SetVisible(false); // bắt đầu ẩn
        }
    }

    /// <summary>
    /// Ẩn/hiện bằng CanvasGroup — không SetActive, không spike
    /// </summary>
    private void SetVisible(bool val)
    {
        if (_cg == null) return;
        _cg.alpha = val ? 1f : 0f;
        _cg.interactable = val;
        _cg.blocksRaycasts = val;
    }

    /// <summary>
    /// Prewarm: object luôn active, chỉ ẩn bằng CanvasGroup
    /// </summary>
    public void Prewarm()
    {
        SetVisible(false);
    }

    /// <summary>
    /// Show thật
    /// </summary>
    public void Show(bool val = true)
    {
        if (val)
        {
            SetVisible(true);
            PlayLoadGate.Reset();

            if (PopupController.Instance != null)
                PopupController.Instance.ChonTuong.Show(false);

            if (routine != null) StopCoroutine(routine);
            routine = StartCoroutine(AnimateDots());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator AnimateDots()
    {
        int dotCount = 0;
        while (true)
        {
            dotCount = (dotCount + 1) % 4;
            if (loadingText != null)
                loadingText.text = "Đang vào trận" + new string('.', dotCount);
            yield return new WaitForSeconds(0.4f);
        }
    }
}