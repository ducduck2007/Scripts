using System.Collections;
using UnityEngine;

public class HeroShowcase2D : MonoBehaviour
{
    [Header("Animator Idle")]
    public string idleBoolParam = "isIdle";
    public bool setIdleTrueWhenDone = true;
    public bool setIdleFalseOnStart = true;

    [Header("Idle Transition")]
    public float idleTransitionDelay = 0.08f;
    public float idleCrossFadeDuration = 0.25f;
    public string idleStateName = "";

    [Header("Debug Pause")]
    [Tooltip("Bật để tự động dừng animation tại pauseAtTime giây")]
    public bool autoPauseEnabled = false;
    [Tooltip("Dừng tại giây này kể từ lúc bắt đầu chuỗi steps")]
    public float pauseAtTime = 5.5f;

    Transform _target;
    HeroShowcaseProfile _profile;
    Coroutine _co;
    bool _paused = false;

    public void PlayFor(Transform target, HeroShowcaseProfile profile)
    {
        if (target == null || profile == null) return;

        if (_co != null) { StopCoroutine(_co); _co = null; }

        if (_target != null && _profile != null)
        {
            // Reset animator hero cũ
            var oldAnim = _target.GetComponentInChildren<Animator>(true);
            if (oldAnim != null) oldAnim.speed = 1f;

            ApplyPose(_target, _profile.defaultPose);
        }

        _target = target;
        _profile = profile;
        _paused = false;

        // Đảm bảo animator hero mới chạy bình thường
        var newAnim = _target.GetComponentInChildren<Animator>(true);
        if (newAnim != null) newAnim.speed = 1f;

        if (setIdleFalseOnStart)
            SetIdle(_target, false);

        ApplyPose(_target, _profile.defaultPose);

        _co = StartCoroutine(CoRunOnce());
    }

    IEnumerator CoRunOnce()
    {
        Debug.Log("[CoRunOnce] Bắt đầu chạy");
        if (_target == null || _profile == null) yield break;

        if (_profile.enterDelay > 0f)
            yield return WaitRealtime(_profile.enterDelay);

        var prev = _profile.defaultPose;

        if (_profile.steps != null && _profile.steps.Length > 0)
        {
            float elapsed = 0f;

            Coroutine autoPauseCo = null;
            if (autoPauseEnabled && pauseAtTime >= 0f)
            {
                Debug.Log($"[CoRunOnce] Khởi động CoAutoPause với pauseAtTime={pauseAtTime}");
                autoPauseCo = StartCoroutine(CoAutoPause(pauseAtTime));
            }
            else
            {
                Debug.Log($"[CoRunOnce] autoPauseEnabled={autoPauseEnabled}, pauseAtTime={pauseAtTime} → KHÔNG chạy CoAutoPause");
            }

            for (int i = 0; i < _profile.steps.Length; i++)
            {
                if (_target == null) yield break;
                var s = _profile.steps[i];
                float waitTime = s.atTime - elapsed;
                if (waitTime > 0f)
                {
                    elapsed += waitTime;
                    yield return WaitRealtime(waitTime);
                }
                Debug.Log($"[CoRunOnce] Step {i} hoàn thành, elapsed={elapsed}, _paused={_paused}");
                yield return LerpPose(prev, s.pose, Mathf.Max(0f, s.lerpTime));
                elapsed += Mathf.Max(0f, s.lerpTime);
                prev = s.pose;
            }

            Debug.Log($"[CoRunOnce] Tất cả steps xong, elapsed={elapsed}, _paused={_paused}");

            float waitToReturn = _profile.returnAtTime - elapsed;
            if (waitToReturn > 0f)
                yield return WaitRealtime(waitToReturn);

            yield return LerpPose(prev, _profile.defaultPose, Mathf.Max(0f, _profile.returnLerpTime));

            if (autoPauseCo != null) StopCoroutine(autoPauseCo);
        }

        Debug.Log("[CoRunOnce] Kết thúc, chuyển sang Idle");

        if (setIdleTrueWhenDone)
            yield return TransitionToIdle(_target);

        _co = null;
    }

    IEnumerator CoAutoPause(float pauseAt)
    {
        Debug.Log($"[AutoPause] Bắt đầu đếm, sẽ pause tại: {pauseAt}s");
        yield return new WaitForSecondsRealtime(pauseAt);

        Debug.Log($"[AutoPause] Đã đến {pauseAt}s → set _paused = true");
        _paused = true;

        if (_target != null)
        {
            var anim = _target.GetComponentInChildren<Animator>(true);
            if (anim != null)
            {
                Debug.Log($"[AutoPause] Tìm thấy Animator: {anim.gameObject.name}, speed hiện tại: {anim.speed} → set về 0");
                anim.speed = 0f;
                Debug.Log($"[AutoPause] Animator speed sau khi set: {anim.speed}");
            }
            else
            {
                Debug.LogWarning("[AutoPause] KHÔNG tìm thấy Animator trong _target!");
            }
        }
        else
        {
            Debug.LogWarning("[AutoPause] _target là NULL!");
        }
    }

    IEnumerator WaitRealtime(float duration)
    {
        float acc = 0f;
        while (acc < duration)
        {
            yield return null;
            if (!_paused)
                acc += Time.unscaledDeltaTime;
        }
    }

    IEnumerator LerpPose(HeroShowcaseProfile.Pose a, HeroShowcaseProfile.Pose b, float duration)
    {
        if (_target == null) yield break;

        if (duration <= 0f) { ApplyPose(_target, b); yield break; }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (_target == null) yield break;

            yield return null;

            if (!_paused)
                elapsed += Time.unscaledDeltaTime;

            float k = Smooth(Mathf.Clamp01(elapsed / duration));
            _target.localPosition = Vector3.LerpUnclamped(a.localPosition, b.localPosition, k);
            _target.localRotation = Quaternion.Euler(Vector3.LerpUnclamped(a.localEuler, b.localEuler, k));
            _target.localScale = Vector3.one * Mathf.LerpUnclamped(a.uniformScale, b.uniformScale, k);
        }

        ApplyPose(_target, b);
    }

    IEnumerator TransitionToIdle(Transform t)
    {
        if (t == null) yield break;

        if (idleTransitionDelay > 0f)
            yield return WaitRealtime(idleTransitionDelay);

        var anim = t.GetComponentInChildren<Animator>(true);
        if (anim != null)
        {
            if (!string.IsNullOrEmpty(idleStateName))
                try { anim.CrossFadeInFixedTime(idleStateName, idleCrossFadeDuration); } catch { }

            yield return null;
            try { anim.SetBool(idleBoolParam, true); } catch { }
        }
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
        try { anim.SetBool(idleBoolParam, val); } catch { }
    }

    static float Smooth(float x) { x = Mathf.Clamp01(x); return x * x * (3f - 2f * x); }
}