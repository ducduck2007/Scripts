using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    [SerializeField] private Image progressImage;
    [SerializeField] private float defaultSpeed = 30f;

    [Header("Optimize")]
    [Tooltip("Nếu chênh lệch nhỏ hơn epsilon thì bỏ qua update để giảm rebuild UI.")]
    [SerializeField] private float epsilon = 0.0025f; // ~0.25%

    [Tooltip("Giới hạn số lần gọi OnProgress mỗi giây (0 = không giới hạn).")]
    [SerializeField] private float onProgressMaxHz = 10f; // MOBA: 5-10Hz là đủ

    [Tooltip("Nếu tắt, sẽ set fillAmount thẳng (hợp cho minion/đơn vị nhiều).")]
    [SerializeField] private bool animate = true;

    [SerializeField] private UnityEvent<float> OnProgress;
    [SerializeField] private UnityEvent OnCompleted;

    public Sprite[] sprMau;

    Coroutine runner;
    float target;
    float speed;
    bool hasTarget;

    float lastProgressEventTime;

    void Start()
    {
        if (progressImage == null) progressImage = GetComponentInChildren<Image>();

        if (progressImage == null || progressImage.type != Image.Type.Filled)
        {
            enabled = false;
#if UNITY_EDITOR
            EditorGUIUtility.PingObject(gameObject);
#endif
            return;
        }

        target = progressImage.fillAmount;
        speed = defaultSpeed;

        runner = StartCoroutine(Run());
    }

    void OnDisable()
    {
        // optional: dừng coroutine khi disable để tránh chạy nền
        if (runner != null) StopCoroutine(runner);
        runner = null;
        hasTarget = false;
    }

    void OnEnable()
    {
        if (runner == null && isActiveAndEnabled && gameObject.activeInHierarchy)
            runner = StartCoroutine(Run());
    }

    public void SetThanhMau(int team)
    {
        if (sprMau == null || sprMau.Length == 0) return;
        team = Mathf.Clamp(team, 0, sprMau.Length - 1);
        progressImage.sprite = sprMau[team];
    }

    public void SetProgress(float progress) => SetProgress(progress, defaultSpeed);

    public void SetProgress(float progress, float speed)
    {
        if (!isActiveAndEnabled || !gameObject.activeInHierarchy || progressImage == null)
            return;

        progress = Mathf.Clamp01(progress);

        float current = progressImage.fillAmount;

        // Nếu thay đổi quá nhỏ thì bỏ qua (giảm UI rebuild)
        if (Mathf.Abs(progress - current) < epsilon)
            return;

        // Nếu không animate (minion), set thẳng
        if (!animate || speed <= 0f)
        {
            progressImage.fillAmount = progress;
            OnProgress?.Invoke(progress);
            return;
        }

        this.target = progress;
        this.speed = speed;
        hasTarget = true;
        // Không StopCoroutine/StartCoroutine nữa -> 1 runner xử lý tất cả
    }

    IEnumerator Run()
    {
        var waitEndOfFrame = new WaitForEndOfFrame();

        while (true)
        {
            if (hasTarget && progressImage != null)
            {
                float current = progressImage.fillAmount;

                // Tiến dần về target
                float next = Mathf.MoveTowards(current, target, Time.unscaledDeltaTime * (speed * 0.01f));
                // speed cũ kiểu "30" khá lớn, MoveTowards dùng step nên scale lại 0.01f cho dễ tune

                if (Mathf.Abs(next - current) >= epsilon)
                {
                    progressImage.fillAmount = next;
                    TryInvokeOnProgress(next);
                }

                // Đã tới đích
                if (Mathf.Abs(target - progressImage.fillAmount) < epsilon)
                {
                    progressImage.fillAmount = target;
                    TryInvokeOnProgress(target);
                    hasTarget = false;
                    OnCompleted?.Invoke();
                }
            }

            // chạy sau khi UI/layout xong frame (đỡ “đập” layout giữa frame)
            yield return waitEndOfFrame;
        }
    }

    void TryInvokeOnProgress(float value)
    {
        if (OnProgress == null) return;

        if (onProgressMaxHz <= 0f)
        {
            OnProgress.Invoke(value);
            return;
        }

        float now = Time.unscaledTime;
        float minInterval = 1f / onProgressMaxHz;

        if (now - lastProgressEventTime >= minInterval)
        {
            lastProgressEventTime = now;
            OnProgress.Invoke(value);
        }
    }

    // Cho phép set nhanh ngoài code: hero animate, minion không animate
    public void SetAnimate(bool value) => animate = value;
}
