using UnityEngine;
using DG.Tweening;

public class AutoPlayPingPong : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] private Animator animator;
    [SerializeField] private int layerIndex = 0;

    [Header("States")]
    [SerializeField] private string stateA = "Appearance";
    [SerializeField] private string idleState = "Idle";

    [Header("Flow")]
    [SerializeField, Range(0.5f, 1.0f)] private float switchAToIdleAt = 0.99f;
    [SerializeField] private float loopBackDelay = 3f;

    [Header("Scale Target")]
    [SerializeField] private Transform scaleTarget;

    [Header("Scale Phases (normalized time 0..1)")]
    [SerializeField, Range(0f, 1f)] private float phase1End = 0.33f;
    [SerializeField, Range(0f, 1f)] private float phase2End = 0.66f;
    [SerializeField] private Vector3 scalePhase1 = Vector3.one;
    [SerializeField] private Vector3 scalePhase2 = Vector3.one * 1.2f;
    [SerializeField] private Vector3 scalePhase3 = Vector3.one * 1.5f;

    [Header("DOTween")]
    [SerializeField] private float phaseTweenDuration = 0.25f;
    [SerializeField] private Ease phaseEase = Ease.OutCubic;
    [SerializeField] private bool tweenUseUnscaledTime = true;

    public enum PlayAudioWhen { OnPlayCalled, OnEnterAppearance, OnEnterIdle }

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip audioClip;
    [SerializeField] private float audioDelay = 0f;
    [SerializeField] private float audioStopAt = 0f;
    [SerializeField] private PlayAudioWhen playAudioWhen = PlayAudioWhen.OnPlayCalled;
    [SerializeField] private bool playOncePerCycle = true;

    private bool switchedToIdle;
    private float idleEnterTime = -1f;

    private int currentPhase = -1;
    private Vector3 currentScaleTarget;

    private Tween scaleTween;
    private Tween audioDelayTween;
    private Tween audioStopTween;

    private bool audioPlayedThisCycle;

    private void Awake()
    {
        if (!animator) animator = GetComponent<Animator>();
        if (!scaleTarget) scaleTarget = transform;
        if (!audioSource) audioSource = GetComponent<AudioSource>();
        currentScaleTarget = scaleTarget.localScale;
    }

    private void OnDisable()
    {
        scaleTween?.Kill(false);
        audioDelayTween?.Kill(false);
        audioStopTween?.Kill(false);

        scaleTween = null;
        audioDelayTween = null;
        audioStopTween = null;
    }

    public void Play()
    {
        ResetCycleState();
        animator.Play(stateA, layerIndex, 0f);

        currentPhase = -1;
        ForceScaleTarget(scalePhase1);

        if (playAudioWhen == PlayAudioWhen.OnPlayCalled)
            TryPlayAudioOnceWithDelay();
    }

    public void Stop()
    {
        scaleTween?.Kill(false);
        audioDelayTween?.Kill(false);
        audioStopTween?.Kill(false);

        scaleTween = null;
        audioDelayTween = null;
        audioStopTween = null;

        enabled = false;
    }

    private void Update()
    {
        if (!animator) return;

        var st = animator.GetCurrentAnimatorStateInfo(layerIndex);

        if (!switchedToIdle)
        {
            if (st.IsName(stateA))
            {
                if (playAudioWhen == PlayAudioWhen.OnEnterAppearance)
                    TryPlayAudioOnceWithDelay();

                UpdateScalePhaseByAppearance(st);

                if (GetNormalized01(st) >= switchAToIdleAt)
                {
                    switchedToIdle = true;
                    idleEnterTime = Time.time;
                    currentPhase = -1;

                    animator.Play(idleState, layerIndex, 0f);

                    if (playAudioWhen == PlayAudioWhen.OnEnterIdle)
                        TryPlayAudioOnceWithDelay();
                }
            }
            return;
        }

        if (loopBackDelay <= 0f) return;

        if (!st.IsName(idleState))
        {
            idleEnterTime = Time.time;
            return;
        }

        if (idleEnterTime < 0f) idleEnterTime = Time.time;

        if (Time.time - idleEnterTime >= loopBackDelay)
        {
            ResetCycleState();
            animator.Play(stateA, layerIndex, 0f);

            currentPhase = -1;
            ForceScaleTarget(scalePhase1);

            if (!playOncePerCycle)
                audioPlayedThisCycle = true;
        }
    }

    private void ResetCycleState()
    {
        switchedToIdle = false;
        idleEnterTime = -1f;
        currentPhase = -1;
        audioPlayedThisCycle = false;
    }

    private static float GetNormalized01(AnimatorStateInfo st)
    {
        float t = st.normalizedTime;
        return t - Mathf.Floor(t);
    }

    private void UpdateScalePhaseByAppearance(AnimatorStateInfo st)
    {
        float t = GetNormalized01(st);
        int phase = (t < phase1End) ? 1 : (t < phase2End) ? 2 : 3;
        if (phase == currentPhase) return;
        currentPhase = phase;

        currentScaleTarget = phase switch
        {
            1 => scalePhase1,
            2 => scalePhase2,
            _ => scalePhase3
        };

        scaleTween?.Kill(false);
        scaleTween = scaleTarget
            .DOScale(currentScaleTarget, phaseTweenDuration)
            .SetEase(phaseEase)
            .SetUpdate(tweenUseUnscaledTime);
    }

    private void ForceScaleTarget(Vector3 s)
    {
        scaleTween?.Kill(false);
        scaleTween = null;
        currentScaleTarget = s;
        scaleTarget.localScale = s;
    }

    private void TryPlayAudioOnceWithDelay()
    {
        if (audioPlayedThisCycle) return;
        if (!audioSource || !audioClip) return;

        audioPlayedThisCycle = true;

        audioDelayTween?.Kill(false);
        audioStopTween?.Kill(false);
        audioDelayTween = null;
        audioStopTween = null;

        if (audioDelay <= 0f)
        {
            PlayAudioNow();
            return;
        }

        audioDelayTween = DOVirtual.DelayedCall(audioDelay, () =>
        {
            if (!this || !isActiveAndEnabled) return;
            if (!audioSource) return;
            PlayAudioNow();
        }).SetUpdate(tweenUseUnscaledTime);
    }

    private void PlayAudioNow()
    {
        audioSource.Stop();
        audioSource.clip = audioClip;
        audioSource.time = 0f;
        audioSource.Play();

        float stopAt = audioStopAt;
        if (stopAt > 0f)
        {
            float dur = audioClip.length;
            if (stopAt > dur) stopAt = dur;

            audioStopTween?.Kill(false);
            audioStopTween = DOVirtual.DelayedCall(stopAt, () =>
            {
                if (!audioSource) return;
                audioSource.Stop();
            }).SetUpdate(tweenUseUnscaledTime);
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (phase2End < phase1End) phase2End = phase1End;
        if (phaseTweenDuration < 0f) phaseTweenDuration = 0f;
        if (switchAToIdleAt < 0.5f) switchAToIdleAt = 0.5f;
        if (audioDelay < 0f) audioDelay = 0f;
        if (audioStopAt < 0f) audioStopAt = 0f;
    }
#endif
}
