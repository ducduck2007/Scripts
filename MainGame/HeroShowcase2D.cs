using System.Collections;
using UnityEngine;

public class HeroShowcase2D : MonoBehaviour
{
    const string HERO_CAMERA_NAME = "HeroCamera";

    Transform heroCamera;
    Coroutine introCo;

    // ==============================
    // CINEMATIC CONFIG
    // ==============================

    const float INTRO_DURATION = 3.5f;
    const float ORBIT_RADIUS_MULTIPLIER = 1.25f;
    const float LOW_HEIGHT_RATIO = -0.35f;
    const float HIGH_HEIGHT_RATIO = 0.25f;
    const float RESET_DURATION = 0.6f;

    // ==============================

    [Header("Animator Idle")]
    public string idleBoolParam = "isIdle";
    public float idleTransitionDelay = 0.08f;
    public float idleCrossFadeDuration = 0.25f;
    public string idleStateName = "";

    // ==============================

    void EnsureCamera()
    {
        if (heroCamera != null) return;

        var camObj = GameObject.Find(HERO_CAMERA_NAME);
        if (camObj != null)
            heroCamera = camObj.transform;
        else
            Debug.LogWarning("Không tìm thấy HeroCamera");
    }

    public void PlayFor(Transform hero)
    {
        if (hero == null) return;

        EnsureCamera();
        if (heroCamera == null) return;

        if (introCo != null)
            StopCoroutine(introCo);

        introCo = StartCoroutine(CoCinematicIntro(hero));
    }

    IEnumerator CoCinematicIntro(Transform hero)
    {
        Renderer r = hero.GetComponentInChildren<Renderer>();
        if (r == null) yield break;

        Bounds b = r.bounds;
        Vector3 center = b.center;
        float height = b.size.y;

        float distance = height * ORBIT_RADIUS_MULTIPLIER;

        Vector3 heroForward = hero.forward;
        Vector3 heroRight = hero.right;

        float elapsed = 0f;

        while (elapsed < INTRO_DURATION)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / INTRO_DURATION);

            float eased = t * t * (3f - 2f * t);

            float angle = Mathf.Lerp(-140f, 130f, eased);
            float rad = angle * Mathf.Deg2Rad;

            float heightOffset = Mathf.Lerp(
                height * LOW_HEIGHT_RATIO,
                height * HIGH_HEIGHT_RATIO,
                eased
            );

            Vector3 offset =
                heroRight * Mathf.Sin(rad) * distance +
                heroForward * Mathf.Cos(rad) * distance;

            Vector3 camPos = center + offset + Vector3.up * heightOffset;

            heroCamera.position = camPos;

            Quaternion lookRot = Quaternion.LookRotation(center - camPos);
            heroCamera.rotation = Quaternion.Slerp(
                heroCamera.rotation,
                lookRot,
                8f * Time.unscaledDeltaTime
            );

            yield return null;
        }

        yield return StartCoroutine(CoSmoothReset());

        // Set isIdle sau khi camera về xong
        yield return TransitionToIdle(hero);

        introCo = null;
    }

    IEnumerator CoSmoothReset()
    {
        if (heroCamera == null) yield break;

        Vector3 startPos = heroCamera.position;
        Quaternion startRot = heroCamera.rotation;

        float t = 0f;
        while (t < RESET_DURATION)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.SmoothStep(0f, 1f, t / RESET_DURATION);
            heroCamera.position = Vector3.Lerp(startPos, Vector3.zero, k);
            heroCamera.rotation = Quaternion.Slerp(startRot, Quaternion.identity, k);
            yield return null;
        }

        heroCamera.position = Vector3.zero;
        heroCamera.rotation = Quaternion.identity;
        heroCamera.localScale = Vector3.one;
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
                try { anim.CrossFadeInFixedTime(idleStateName, idleCrossFadeDuration); } catch { }

            yield return null;
            try { anim.SetBool(idleBoolParam, true); } catch { }
        }
    }

    public void ForceReset()
    {
        EnsureCamera();

        if (introCo != null)
            StopCoroutine(introCo);

        introCo = StartCoroutine(CoSmoothReset());
    }
}