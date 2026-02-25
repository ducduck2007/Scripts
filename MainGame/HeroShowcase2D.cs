using System.Collections;
using UnityEngine;

public class HeroShowcase2D : MonoBehaviour
{
    [Header("Animator Idle")]
    public string idleBoolParam = "isIdle";
    public bool setIdleTrueWhenDone = true;
    public bool setIdleFalseOnStart = true;

    [Header("Idle Transition")]
    [Tooltip("Delay sau khi lerp về default pose xong, trước khi bật idle")]
    public float idleTransitionDelay = 0.08f;

    [Tooltip("Thời gian crossfade animator sang idle (giây)")]
    public float idleCrossFadeDuration = 0.25f;

    [Tooltip("Tên state idle để crossfade (để trống nếu chỉ dùng bool)")]
    public string idleStateName = "";

    Transform _target;
    HeroShowcaseProfile _profile;

    Coroutine _co;

    public void PlayFor(Transform target, HeroShowcaseProfile profile)
    {
        if (target == null || profile == null) return;

        if (_co != null)
        {
            StopCoroutine(_co);
            _co = null;
        }

        // Reset pose của hero trước đó về default
        if (_target != null && _profile != null)
            ApplyPose(_target, _profile.defaultPose);

        _target = target;
        _profile = profile;

        if (setIdleFalseOnStart)
            SetIdle(_target, false);

        ApplyPose(_target, _profile.defaultPose);

        _co = StartCoroutine(CoRunOnce());
    }

    IEnumerator CoRunOnce()
    {
        if (_target == null || _profile == null) yield break;

        // --- Enter delay ---
        if (_profile.enterDelay > 0f)
            yield return new WaitForSecondsRealtime(_profile.enterDelay);

        var prev = _profile.defaultPose;

        if (_profile.steps != null && _profile.steps.Length > 0)
        {
            // Dùng elapsed tích lũy thay vì snapshot Time.unscaledTime
            // để tránh sai lệch trên thiết bị yếu / frame drop
            float elapsed = 0f;

            for (int i = 0; i < _profile.steps.Length; i++)
            {
                if (_target == null) yield break;

                var s = _profile.steps[i];

                // Chờ đến đúng atTime tính từ lúc bắt đầu chuỗi steps
                float waitTime = s.atTime - elapsed;
                if (waitTime > 0f)
                {
                    elapsed += waitTime;
                    yield return new WaitForSecondsRealtime(waitTime);
                }

                // Lerp đến pose mục tiêu, thời gian lerp tính riêng
                yield return LerpPose(prev, s.pose, Mathf.Max(0f, s.lerpTime));

                // Cộng thêm lerpTime vào elapsed sau khi lerp xong
                elapsed += Mathf.Max(0f, s.lerpTime);

                prev = s.pose;
            }

            // Chờ rồi lerp về default pose
            float waitToReturn = _profile.returnAtTime - elapsed;
            if (waitToReturn > 0f)
                yield return new WaitForSecondsRealtime(waitToReturn);

            yield return LerpPose(prev, _profile.defaultPose, Mathf.Max(0f, _profile.returnLerpTime));
        }
        else
        {
            if (_profile.returnAtTime > 0f)
                yield return new WaitForSecondsRealtime(_profile.returnAtTime);
        }

        if (setIdleTrueWhenDone)
            yield return TransitionToIdle(_target);

        _co = null;
    }

    IEnumerator TransitionToIdle(Transform t)
    {
        if (t == null) yield break;

        if (idleTransitionDelay > 0f)
            yield return new WaitForSecondsRealtime(idleTransitionDelay);

        var anim = t.GetComponentInChildren<Animator>(true);
        if (anim != null)
        {
            if (!string.IsNullOrEmpty(idleStateName))
            {
                try { anim.CrossFadeInFixedTime(idleStateName, idleCrossFadeDuration); }
                catch { /* state không tồn tại, fallback */ }
            }

            yield return null;

            try { anim.SetBool(idleBoolParam, true); }
            catch { }
        }
    }

    /// <summary>
    /// Lerp pose từ a → b trong khoảng thời gian duration.
    /// Dùng Time.unscaledDeltaTime thay vì snapshot Time.unscaledTime
    /// để animation chạy đúng tốc độ trên mọi thiết bị, kể cả thiết bị yếu.
    /// </summary>
    IEnumerator LerpPose(HeroShowcaseProfile.Pose a, HeroShowcaseProfile.Pose b, float duration)
    {
        if (_target == null) yield break;

        if (duration <= 0f)
        {
            ApplyPose(_target, b);
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (_target == null) yield break;

            // Tích lũy delta mỗi frame, không phụ thuộc vào thời điểm bắt đầu
            elapsed += Time.unscaledDeltaTime;

            float k = Smooth(Mathf.Clamp01(elapsed / duration));

            _target.localPosition = Vector3.LerpUnclamped(a.localPosition, b.localPosition, k);
            _target.localRotation = Quaternion.Euler(Vector3.LerpUnclamped(a.localEuler, b.localEuler, k));

            float sc = Mathf.LerpUnclamped(a.uniformScale, b.uniformScale, k);
            _target.localScale = Vector3.one * sc;

            yield return null;
        }

        // Snap chính xác về pose đích sau khi vòng lặp kết thúc
        ApplyPose(_target, b);
    }

    static void ApplyPose(Transform t, HeroShowcaseProfile.Pose p)
    {
        if (t == null) return;
        t.localPosition = p.localPosition;
        t.localRotation = Quaternion.Euler(p.localEuler);
        t.localScale = Vector3.one * p.uniformScale;
    }

    void SetIdle(Transform t, bool val)
    {
        if (t == null) return;
        var anim = t.GetComponentInChildren<Animator>(true);
        if (anim == null) return;
        try { anim.SetBool(idleBoolParam, val); }
        catch { }
    }

    /// <summary>Smoothstep: easing in-out mượt</summary>
    static float Smooth(float x)
    {
        x = Mathf.Clamp01(x);
        return x * x * (3f - 2f * x);
    }
}