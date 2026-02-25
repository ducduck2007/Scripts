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

        if (_profile.enterDelay > 0f)
            yield return new WaitForSecondsRealtime(_profile.enterDelay);

        var prev = _profile.defaultPose;

        if (_profile.steps != null)
        {
            float startT = Time.unscaledTime;

            for (int i = 0; i < _profile.steps.Length; i++)
            {
                if (_target == null) yield break;

                var s = _profile.steps[i];

                float wait = s.atTime - (Time.unscaledTime - startT);
                if (wait > 0f)
                    yield return new WaitForSecondsRealtime(wait);

                yield return LerpPose(prev, s.pose, Mathf.Max(0f, s.lerpTime));
                prev = s.pose;
            }

            float waitToReturn = _profile.returnAtTime - (Time.unscaledTime - startT);
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

    IEnumerator LerpPose(HeroShowcaseProfile.Pose a, HeroShowcaseProfile.Pose b, float duration)
    {
        if (_target == null) yield break;

        if (duration <= 0f)
        {
            ApplyPose(_target, b);
            yield break;
        }

        float t0 = Time.unscaledTime;

        while (true)
        {
            if (_target == null) yield break;

            float t = (Time.unscaledTime - t0) / duration;
            if (t >= 1f) break;

            float k = Smooth(t);

            _target.localPosition = Vector3.LerpUnclamped(a.localPosition, b.localPosition, k);
            _target.localRotation = Quaternion.Euler(Vector3.LerpUnclamped(a.localEuler, b.localEuler, k));

            float sc = Mathf.LerpUnclamped(a.uniformScale, b.uniformScale, k);
            _target.localScale = Vector3.one * sc;

            yield return null;
        }

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

    static float Smooth(float x)
    {
        x = Mathf.Clamp01(x);
        return x * x * (3f - 2f * x);
    }
}