using TMPro;
using UnityEngine;
using System.Collections;

public class PlayerOther : MonoBehaviour
{
    public Animator animator;
    public int teamId = 0;

    private bool _netMoving = false;
    private float _animSpeedSmoothed = 0f;
    private float _stateHoldUntil = 0f;

    private Vector3 targetPos;
    private Quaternion targetRot;
    public bool isAlive = true;

    private Vector3 lastServerPos;
    private Vector3 serverVelocity;
    private float lastUpdateTime;

    private const float EXTRAPOLATION_LIMIT = 1.2f;
    private const float MAX_EXTRA_DISTANCE = 150f;

    private float catchupTimer = 0f;
    private const float CATCHUP_DURATION = 0.25f;
    private const float CATCHUP_MULT = 6f;

    private const float HARD_SNAP_DISTANCE = 80f;
    private const float HARD_SNAP_TIME_GAP = 0.35f;

    public float camGiacDiChuyen = 22f;
    public float camGiacXoayMat = 15f;
    public float chongRung = 5f;

    public LayerMask enemyLayer;
    public ProgressBar HealthBar;

    [Header("Canvas HP")]
    public Canvas canvasHp; // ✅ Khai báo Canvas HP để kéo vào Inspector

    private bool serverIsAttack;
    public Transform parentSkill;

    [System.Serializable]
    public class SkillConfig
    {
        public GameObject skillObject;
        public string animationBool = "";
        public float animationDuration = 1f;
        public float delaySpawn = 0.3f;
    }

    public SkillConfig skill1 = new SkillConfig();
    public SkillConfig skill2 = new SkillConfig();
    public SkillConfig skill3 = new SkillConfig();
    private SkillConfig currentSkillCfg;

    public float hpMax;
    public float hpCurrent;

    private bool isNormalAttacking, isSkillCasting, isHit;
    private Transform target;
    private Vector3 velocity;

    [System.Serializable]
    public class NormalAttackConfig
    {
        public GameObject attackObject;
        public int attackRange = 300;
        public int damage = 1;
        public float duration = 1.2f;
        public float spawnDelay = 0.2f;
        public string animationBool = "isAttack";
    }

    [Header("Floating Text")]
    public TMP_Text txtMinusHp;

    [Header("Floating Text Tuning")]
    public float floatHeight = 50f;
    public float floatDuration = 0.7f;
    public float floatArcX = 25f;
    public float startScale = 0.85f;
    public float peakScale = 1.15f;
    public float endScale = 0.95f;

    private Coroutine coMinusHpFloat;
    public NormalAttackConfig normalAttackConfig = new NormalAttackConfig();

    private bool hasPendingServerAim;
    private TranDauControl.SkillCastInfo pendingAim;
    private Vector3 serverDirCached;

    [Header("Skill Spawn Parent")]
    public GameObject skillSpawnRoot;

    private Vector2 minusHpOriginPos;
    private bool minusHpOriginCached;

    private bool _hasFirstSnapshot = false;

    private float stuckTimer = 0f;
    private string lastStateName = "";

    public long userId;

    private const float DEFAULT_SKILL_MOVE_SPEED = 10f;

#if UNITY_EDITOR
    private float _debugLastSpeed = -1f;
    private int _debugFlipCount = 0;
    private float _debugLastFlipTime = 0f;
#endif

    // ===== Death/Respawn: disable collision & controllers =====
    private Collider[] _cachedColliders;
    private CharacterController[] _cachedCharControllers;
    private Rigidbody[] _cachedRigidbodies;
    private bool _physicsCached = false;

    private void CacheOriginIfNeeded(TMP_Text txt, ref Vector2 origin, ref bool cached)
    {
        if (!cached && txt != null)
        {
            origin = txt.rectTransform.anchoredPosition;
            cached = true;
        }
    }

    private void ResetFloatTextToOrigin(TMP_Text txt, ref Vector2 origin, ref bool cached)
    {
        if (txt == null) return;
        CacheOriginIfNeeded(txt, ref origin, ref cached);

        var rt = txt.rectTransform;
        rt.anchoredPosition = origin;
        rt.localScale = Vector3.one * startScale;

        var c = txt.color;
        c.a = 1f;
        txt.color = c;
    }

    void Start()
    {
        CachePhysicsIfNeeded();
        SetPhysicsEnabled(true);

        SafeSetHealthBarActive(TranDauControl.Instance != null);
        SafeSetCanvasHpActive(TranDauControl.Instance != null); // ✅ Bật canvas HP khi start

        if (TranDauControl.Instance != null)
            SafeSetThanhMau(teamId == B.Instance.teamId ? 1 : 2);

        isAlive = true;

        Vector3 pos = transform.position;
        pos.y = 0f;
        transform.position = pos;

        lastServerPos = transform.position;
        targetPos = transform.position;
        targetRot = transform.rotation;
        serverVelocity = Vector3.zero;
        lastUpdateTime = Time.time;

        _netMoving = false;
        _animSpeedSmoothed = 0f;

        CacheOriginIfNeeded(txtMinusHp, ref minusHpOriginPos, ref minusHpOriginCached);
        DisableAllSkillObjects();
    }

    private void DisableAllSkillObjects()
    {
        if (normalAttackConfig.attackObject != null)
            normalAttackConfig.attackObject.SetActive(false);

        if (skill1 != null && skill1.skillObject != null)
            skill1.skillObject.SetActive(false);

        if (skill2 != null && skill2.skillObject != null)
            skill2.skillObject.SetActive(false);

        if (skill3 != null && skill3.skillObject != null)
            skill3.skillObject.SetActive(false);
    }

    public void SetTeamId(int teamId) => this.teamId = teamId;

    public void SetHp(int hp, int maxHp)
    {
        hpMax = maxHp;
        hpCurrent = hp;

        if (TranDauControl.Instance == null || HealthBar == null) return;

        float ratio = (hpMax <= 0f) ? 0f : Mathf.Clamp01(hpCurrent / hpMax);
        if (hpCurrent < hpMax) HealthBar.SetProgress(ratio, 30);
        else HealthBar.SetProgress(1f, 100);
    }

    public void ResetNetworkState(Vector3 spawnPos, float heading, int newTeamId)
    {
        teamId = newTeamId;
        isAlive = true;
        SetPhysicsEnabled(true);

        isNormalAttacking = false;
        isSkillCasting = false;
        isHit = false;
        serverIsAttack = false;

        transform.position = spawnPos;
        targetPos = spawnPos;
        lastServerPos = spawnPos;
        serverVelocity = Vector3.zero;

        targetRot = Quaternion.Euler(0, heading, 0);
        transform.rotation = targetRot;

        lastUpdateTime = Time.time;
        catchupTimer = CATCHUP_DURATION;

        _netMoving = false;
        _animSpeedSmoothed = 0f;
        velocity = Vector3.zero;

        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
            animator.SetBool("isAttack", false);
            animator.SetBool("isSkill1", false);
            animator.SetBool("isSkill2", false);
            animator.SetBool("isSkill3", false);
            animator.SetBool("isDeath", false);
            animator.SetBool("isHit", false);
        }

        SafeSetHealthBarActive(TranDauControl.Instance != null);
        SafeSetCanvasHpActive(TranDauControl.Instance != null); // ✅ Bật canvas HP khi respawn

        if (TranDauControl.Instance != null)
            SafeSetThanhMau(teamId == B.Instance.teamId ? 1 : 2);

        ForceEnableRenderers();

        hasPendingServerAim = false;
        pendingAim = default;
        serverDirCached = Vector3.zero;

        DisableAllSkillObjects();

        if (txtMinusHp != null)
        {
            ForceShowText(txtMinusHp);
            ResetFloatTextToOrigin(txtMinusHp, ref minusHpOriginPos, ref minusHpOriginCached);
            txtMinusHp.gameObject.SetActive(false);
        }
    }

    private void ReviveVisuals()
    {
        isAlive = true;
        SetPhysicsEnabled(true);

        if (animator != null && animator.isInitialized)
        {
            animator.SetBool("isDeath", false);
            animator.SetBool("isHit", false);
            animator.SetBool("isAttack", false);
            animator.SetBool("isSkill1", false);
            animator.SetBool("isSkill2", false);
            animator.SetBool("isSkill3", false);
            animator.SetFloat("Speed", 0f);
        }

        _netMoving = false;
        _animSpeedSmoothed = 0f;

        SafeSetHealthBarActive(TranDauControl.Instance != null);
        SafeSetCanvasHpActive(TranDauControl.Instance != null); // ✅ Bật canvas HP khi revive

        if (TranDauControl.Instance != null)
            SafeSetThanhMau(teamId == B.Instance.teamId ? 1 : 2);

        ForceEnableRenderers();
        stuckTimer = 0f;
        lastStateName = "";

        DisableAllSkillObjects();

        if (txtMinusHp != null)
        {
            ForceShowText(txtMinusHp);
            ResetFloatTextToOrigin(txtMinusHp, ref minusHpOriginPos, ref minusHpOriginCached);
            txtMinusHp.gameObject.SetActive(false);
        }
    }

    private void ForceEnableRenderers()
    {
        var renderers = GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++) renderers[i].enabled = true;

        var sprites = GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < sprites.Length; i++) sprites[i].enabled = true;
    }

    public void ApplyServerData(PlayerOutPutSv data)
    {
        userId = data.userId;

        if (!data.isAlive)
        {
            if (isAlive) onDeath();
            return;
        }
        else
        {
            if (!isAlive) ReviveVisuals();
        }

        Vector3 newServerPos = new Vector3(data.x, 0f, data.y);
        float now = Time.time;

        if (!_hasFirstSnapshot || lastUpdateTime <= 0.0001f)
        {
            _hasFirstSnapshot = true;

            transform.position = newServerPos;
            targetPos = newServerPos;
            lastServerPos = newServerPos;

            serverVelocity = Vector3.zero;
            lastUpdateTime = now;

            targetRot = Quaternion.Euler(0, data.heading, 0);
            transform.rotation = targetRot;

            catchupTimer = CATCHUP_DURATION;

            _netMoving = false;
            _animSpeedSmoothed = 0f;
            velocity = Vector3.zero;

            SetHp(data.hp, data.maxHp);
            return;
        }

        float dt = now - lastUpdateTime;
        if (dt > 0.7f) catchupTimer = CATCHUP_DURATION;

        float dtForVel = Mathf.Max(dt, 1f / 60f);

        if (dt > 0.0001f)
        {
            serverVelocity = (newServerPos - lastServerPos) / dtForVel;

            float maxSpeed = 500f;
            float m = serverVelocity.magnitude;
            if (m > maxSpeed) serverVelocity = serverVelocity / m * maxSpeed;
        }
        else
        {
            serverVelocity = Vector3.zero;
        }

        targetPos = newServerPos;
        lastServerPos = newServerPos;

        targetRot = Quaternion.Euler(0, data.heading, 0);

        SetHp(data.hp, data.maxHp);
        lastUpdateTime = now;

#if UNITY_EDITOR
        DebugLogSnapshot(newServerPos, serverVelocity);
#endif
    }

    public void SetAttackState(bool isAttack, bool hasTarget)
    {
        if (!isAlive || !isAttack) return;

        FindTargetInRange(normalAttackConfig.attackRange);

        if (target != null) RotateToTargetInstant();
        else transform.rotation = targetRot;

        isNormalAttacking = true;
        serverIsAttack = true;

        if (animator != null) animator.SetBool("isAttack", true);

        Invoke(nameof(ShowNormalAttackObject), normalAttackConfig.spawnDelay);
        Invoke(nameof(AutoResetNormalAttack), normalAttackConfig.duration);
    }

    private void RotateToTargetInstant()
    {
        if (target == null) return;
        Vector3 dir = target.position - transform.position;
        dir.y = 0;
        if (dir.sqrMagnitude < 0.01f) return;
        transform.rotation = Quaternion.LookRotation(dir);
    }

    private void AutoResetNormalAttack()
    {
        if (isNormalAttacking) EndNormalAttack();
    }

    private void EndNormalAttack()
    {
        if (!isNormalAttacking) return;

        if (animator != null) animator.SetBool("isAttack", false);

        isNormalAttacking = false;
        serverIsAttack = false;

        if (normalAttackConfig.attackObject != null)
            normalAttackConfig.attackObject.SetActive(false);
    }

    public void SetPotion(Vector3 pos)
    {
        transform.position = pos;
        targetPos = pos;
        lastServerPos = pos;
        serverVelocity = Vector3.zero;
        lastUpdateTime = Time.time;
        catchupTimer = CATCHUP_DURATION;

        _netMoving = false;
        _animSpeedSmoothed = 0f;
        velocity = Vector3.zero;
    }

    void Update()
    {
        if (!isAlive)
        {
            SetAnimatorSpeed(0f);
            return;
        }

        DetectAnimatorStuck();
        UpdateMovementWithExtrapolation();

        if (!IsBusy())
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, camGiacXoayMat * Time.deltaTime);

        UpdateAnimation();
    }

    void LateUpdate()
    {
#if UNITY_EDITOR
        DebugDetectFlip();
#endif
    }

    private void UpdateMovementWithExtrapolation()
    {
        if (isSkillCasting) return;

        Vector3 currentPos = transform.position;
        float timeSinceLastUpdate = Time.time - lastUpdateTime;

        Vector3 extrapolatedTarget = targetPos;

        if (serverVelocity.sqrMagnitude > 0.01f)
        {
            float t = Mathf.Min(timeSinceLastUpdate, EXTRAPOLATION_LIMIT);

            Vector3 predicted = targetPos + serverVelocity * t;
            predicted.y = 0f;

            Vector3 drift = predicted - targetPos;
            drift.y = 0f;
            float driftMag = drift.magnitude;
            if (driftMag > MAX_EXTRA_DISTANCE)
                predicted = targetPos + drift.normalized * MAX_EXTRA_DISTANCE;

            extrapolatedTarget = predicted;
        }

        if (float.IsNaN(extrapolatedTarget.x) || float.IsNaN(extrapolatedTarget.z))
            return;

        Vector3 toTarget = extrapolatedTarget - currentPos;
        toTarget.y = 0f;
        float dist = toTarget.magnitude;

        if (dist >= HARD_SNAP_DISTANCE)
        {
            transform.position = new Vector3(extrapolatedTarget.x, 0f, extrapolatedTarget.z);
            catchupTimer = CATCHUP_DURATION;
            velocity = Vector3.zero;
            return;
        }

        if (dist < chongRung)
            return;

        float smooth = camGiacDiChuyen;

        if (catchupTimer > 0f)
        {
            smooth *= CATCHUP_MULT;
            catchupTimer -= Time.deltaTime;
        }

        float maxSpeed = Mathf.Max(50f, serverVelocity.magnitude * 1.25f);

        Vector3 vel = velocity;
        Vector3 newPos = Vector3.SmoothDamp(
            currentPos,
            extrapolatedTarget,
            ref vel,
            1f / Mathf.Max(1f, smooth),
            maxSpeed,
            Time.deltaTime
        );
        newPos.y = 0f;

        velocity = vel;
        transform.position = newPos;
    }

    private void UpdateAnimation()
    {
        if (animator == null) return;

        Vector3 pos = transform.position;
        Vector3 toTarget = targetPos - pos;
        toTarget.y = 0f;

        float dist = toTarget.magnitude;
        float v = serverVelocity.magnitude;

        const float V_ON = 35f;
        const float V_OFF = 15f;
        float D_ON = Mathf.Max(chongRung, 3.5f);
        float D_OFF = Mathf.Max(0.8f, chongRung * 0.55f);

        bool wantMove = (v >= V_ON && dist >= D_ON);
        bool wantStop = (v <= V_OFF || dist <= D_OFF);

        const float MIN_STATE_HOLD = 0.18f;
        float now = Time.time;

        if (now >= _stateHoldUntil)
        {
            if (!_netMoving)
            {
                if (wantMove)
                {
                    _netMoving = true;
                    _stateHoldUntil = now + MIN_STATE_HOLD;
                }
            }
            else
            {
                if (wantStop)
                {
                    _netMoving = false;
                    _stateHoldUntil = now + MIN_STATE_HOLD;
                }
            }
        }

        float targetSpeed = (!IsBusy() && _netMoving) ? 1f : 0f;

        float step = 10f * Time.deltaTime;
        _animSpeedSmoothed = Mathf.MoveTowards(_animSpeedSmoothed, targetSpeed, step);
        animator.SetFloat("Speed", _animSpeedSmoothed);
    }

    private void FindTargetInRange(float range)
    {
        if (TranDauControl.Instance != null)
        {
            target = TranDauControl.Instance.FindNearestEnemy(transform.position, range, teamId);
            if (target != null) return;
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, range, enemyLayer);
        target = null;
        float minDist = Mathf.Infinity;

        for (int i = 0; i < hits.Length; i++)
        {
            var hit = hits[i];
            if (hit.transform == this.transform) continue;

            float distance = Vector3.Distance(transform.position, hit.transform.position);
            if (distance < minDist)
            {
                minDist = distance;
                target = hit.transform;
            }
        }
    }

    public void CastSkillFromServer(int skill, bool hasTarget)
    {
        if (!isAlive) return;

        if (isNormalAttacking)
        {
            CancelInvoke(nameof(AutoResetNormalAttack));
            CancelInvoke(nameof(ShowNormalAttackObject));
            EndNormalAttack();
        }

        if (skill == 1) currentSkillCfg = skill1;
        else if (skill == 2) currentSkillCfg = skill2;
        else if (skill == 3) currentSkillCfg = skill3;
        else return;

        if (hasPendingServerAim && serverDirCached.sqrMagnitude > 0.0001f)
        {
            Vector3 look = serverDirCached;
            look.y = 0f;
            if (look.sqrMagnitude > 0.0001f) transform.rotation = Quaternion.LookRotation(look);
            else transform.rotation = targetRot;
        }
        else
        {
            FindTargetInRange(normalAttackConfig.attackRange);
            if (target != null) RotateToTargetInstant();
            else transform.rotation = targetRot;
        }

        isSkillCasting = true;
        SetAnimatorSpeed(0f);

        if (animator != null)
        {
            if (skill == 1) animator.SetBool("isSkill1", true);
            else if (skill == 2) animator.SetBool("isSkill2", true);
            else animator.SetBool("isSkill3", true);
        }

        Invoke(nameof(EndSkillAnimationWrapper), Mathf.Max(0.05f, currentSkillCfg.animationDuration));
        Invoke(nameof(ShowSkillWithDelay), Mathf.Max(0f, currentSkillCfg.delaySpawn));
    }

    private void EndSkillAnimationWrapper()
    {
        isSkillCasting = false;

        if (animator != null)
        {
            animator.SetBool("isAttack", false);
            animator.SetBool("isSkill1", false);
            animator.SetBool("isSkill2", false);
            animator.SetBool("isSkill3", false);
        }

        if (currentSkillCfg != null && currentSkillCfg.skillObject != null)
            currentSkillCfg.skillObject.SetActive(false);

        currentSkillCfg = null;
    }

    private void ShowNormalAttackObject()
    {
        if (normalAttackConfig.attackObject == null) return;

        Vector3 spawnPos;
        Quaternion spawnRot;

        if (target != null)
        {
            spawnPos = target.position;
            Vector3 d = (target.position - transform.position);
            d.y = 0f;
            spawnRot = (d.sqrMagnitude > 0.0001f) ? Quaternion.LookRotation(d) : transform.rotation;
        }
        else
        {
            spawnPos = transform.position + transform.forward * normalAttackConfig.attackRange;
            spawnRot = transform.rotation;
        }

        normalAttackConfig.attackObject.transform.position = spawnPos;
        normalAttackConfig.attackObject.transform.rotation = spawnRot;
        normalAttackConfig.attackObject.SetActive(true);
    }

    private void ShowSkillWithDelay()
    {
        if (currentSkillCfg == null) return;

        if (hasPendingServerAim)
        {
            if (currentSkillCfg.skillObject != null)
                ShowSkillByServerAim(currentSkillCfg, pendingAim);

            hasPendingServerAim = false;
            pendingAim = default;
            serverDirCached = Vector3.zero;
            return;
        }

        if (currentSkillCfg.skillObject == null)
            return;

        Vector3 spawnPos;
        Quaternion spawnRot;

        if (target != null)
        {
            spawnPos = target.position;
            Vector3 d = (target.position - transform.position);
            d.y = 0f;
            spawnRot = (d.sqrMagnitude > 0.0001f) ? Quaternion.LookRotation(d) : transform.rotation;
        }
        else
        {
            spawnPos = transform.position + transform.forward * normalAttackConfig.attackRange;
            spawnRot = transform.rotation;
        }

        currentSkillCfg.skillObject.transform.position = spawnPos;
        currentSkillCfg.skillObject.transform.rotation = spawnRot;
        currentSkillCfg.skillObject.SetActive(true);
    }

    private void ShowSkillByServerAim(SkillConfig cfg, TranDauControl.SkillCastInfo info)
    {
        if (cfg == null) return;

        if (cfg.skillObject == null)
            return;

        ComputeServerVisual(info, out Vector3 origin, out Vector3 dst, out Vector3 dir, out Quaternion rot, out bool needMove);

        bool usesTarget = SkillCastProtocol33.UsesTargetPos(info.typeSkill);
        bool usesDir = SkillCastProtocol33.UsesDirection(info.typeSkill);

        if (usesTarget && !usesDir)
        {
            cfg.skillObject.transform.position = dst;
            cfg.skillObject.transform.rotation = rot;
            cfg.skillObject.SetActive(true);
            return;
        }

        cfg.skillObject.transform.position = origin;
        cfg.skillObject.transform.rotation = rot;
        cfg.skillObject.SetActive(true);

        if (needMove && (dst - origin).sqrMagnitude > 0.01f)
        {
            float spd = (info.speed > 0) ? info.speed : DEFAULT_SKILL_MOVE_SPEED;
            StartCoroutine(CoMoveSkillObject(cfg.skillObject, dst, spd));
        }
    }

    private void ComputeServerVisual(TranDauControl.SkillCastInfo info, out Vector3 origin, out Vector3 dst,
        out Vector3 dir, out Quaternion rot, out bool needMove)
    {
        origin = info.origin;

        bool usesDir = SkillCastProtocol33.UsesDirection(info.typeSkill);
        bool usesTarget = SkillCastProtocol33.UsesTargetPos(info.typeSkill);

        dir = info.dir;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.0001f && info.hasTarget)
        {
            Vector3 d = info.targetPos - origin;
            d.y = 0f;
            if (d.sqrMagnitude > 0.0001f) dir = d.normalized;
        }

        if (dir.sqrMagnitude > 0.0001f)
            dir.Normalize();

        rot = (dir.sqrMagnitude > 0.0001f) ? Quaternion.LookRotation(dir) : transform.rotation;

        if (usesTarget && info.hasTarget) dst = info.targetPos;
        else if (usesDir && dir.sqrMagnitude > 0.0001f && info.maxRange > 0.01f) dst = origin + dir * info.maxRange;
        else dst = origin;

        needMove = (usesTarget || (usesDir && info.maxRange > 0.01f));
    }

    private IEnumerator CoMoveSkillObject(GameObject obj, Vector3 targetPosition, float moveSpeed)
    {
        if (obj == null) yield break;

        while (obj != null && obj.activeSelf && Vector3.Distance(obj.transform.position, targetPosition) > 0.1f)
        {
            obj.transform.position = Vector3.MoveTowards(obj.transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }

        if (obj != null)
            obj.SetActive(false);
    }

    private void SetAnimatorSpeed(float speed)
    {
        if (animator != null)
            animator.SetFloat("Speed", speed);
    }

    private bool IsBusy() => isNormalAttacking || isSkillCasting || isHit;

    public void onDeath()
    {
        isAlive = false;

        // ✅ tắt va chạm + controller để không block map
        SetPhysicsEnabled(false);

        SetAnimatorSpeed(0f);

        if (animator != null)
        {
            animator.SetBool("isAttack", false);
            animator.SetBool("isSkill1", false);
            animator.SetBool("isSkill2", false);
            animator.SetBool("isSkill3", false);
            animator.SetBool("isDeath", true);
        }

        CancelInvoke(nameof(AutoResetNormalAttack));
        CancelInvoke(nameof(ShowNormalAttackObject));
        CancelInvoke(nameof(EndSkillAnimationWrapper));
        CancelInvoke(nameof(ShowSkillWithDelay));

        isNormalAttacking = false;
        isSkillCasting = false;
        isHit = false;
        serverIsAttack = false;

        hasPendingServerAim = false;
        pendingAim = default;
        serverDirCached = Vector3.zero;

        _netMoving = false;
        _animSpeedSmoothed = 0f;
        velocity = Vector3.zero;

        DisableAllSkillObjects();

        // ✅ tắt HP bar (ProgressBar)
        SafeSetHealthBarActive(false);

        // ✅ tắt Canvas HP
        SafeSetCanvasHpActive(false);

        if (txtMinusHp != null) txtMinusHp.gameObject.SetActive(false);
    }

    public void onRespawn(float x, float y, int hp)
    {
        Vector3 respawnPos = new Vector3(x, 0, y);
        float heading = transform.eulerAngles.y;
        ResetNetworkState(respawnPos, heading, teamId);
        SetHp(hp, hp);
    }

    void DetectAnimatorStuck()
    {
        if (!isAlive || animator == null || !animator.isInitialized) return;

        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        string stateName = GetCurrentStateName(info);

        if (stateName != lastStateName)
        {
            lastStateName = stateName;
            stuckTimer = 0f;
            return;
        }

        stuckTimer += Time.deltaTime;

        if (stateName == "Idle" || stateName == "Walking") return;

        if (stuckTimer > 5f) ForceResetAnimator();
    }

    string GetCurrentStateName(AnimatorStateInfo info)
    {
        if (info.IsName("Idle")) return "Idle";
        if (info.IsName("Walking")) return "Walking";
        if (info.IsName("Attack")) return "Attack";
        if (info.IsName("Skill1")) return "Skill1";
        if (info.IsName("Skill2")) return "Skill2";
        if (info.IsName("Skill3")) return "Skill3";
        if (info.IsName("Death")) return "Death";
        if (info.IsName("Hit")) return "Hit";

        return info.shortNameHash.ToString();
    }

    void ForceResetAnimator()
    {
        SetAnimatorSpeed(0f);

        if (animator != null)
        {
            animator.SetBool("isAttack", false);
            animator.SetBool("isSkill1", false);
            animator.SetBool("isSkill2", false);
            animator.SetBool("isSkill3", false);
            animator.SetBool("isDeath", false);
            animator.SetBool("isHit", false);
        }

        isNormalAttacking = false;
        isSkillCasting = false;
        isHit = false;

        _netMoving = false;
        _animSpeedSmoothed = 0f;

        stuckTimer = 0f;
        lastStateName = "";
    }

    private void SafeSetThanhMau(int type)
    {
        if (HealthBar != null) HealthBar.SetThanhMau(type);
    }

    private void SafeSetHealthBarActive(bool active)
    {
        if (HealthBar != null) HealthBar.gameObject.SetActive(active);
    }

    // ✅ Hàm mới để tắt/bật Canvas HP
    private void SafeSetCanvasHpActive(bool active)
    {
        if (canvasHp != null) canvasHp.gameObject.SetActive(active);
    }

    public void OnServerSkillCast(TranDauControl.SkillCastInfo info)
    {
        Vector3 d = info.dir;
        if (info.hasTarget)
            d = (info.targetPos - info.origin);

        d.y = 0f;
        if (d.sqrMagnitude > 0.0001f) d.Normalize();

        info.dir = d;
        pendingAim = info;
        hasPendingServerAim = true;
        serverDirCached = d;
    }

    public void ShowMinusHp(int value)
    {
        if (txtMinusHp == null) return;

        ForceShowText(txtMinusHp);
        ResetFloatTextToOrigin(txtMinusHp, ref minusHpOriginPos, ref minusHpOriginCached);

        int v = Mathf.Abs(value);
        txtMinusHp.text = $"-{v}";

        var c = txtMinusHp.color;
        c.a = 1f;
        txtMinusHp.color = c;
        txtMinusHp.ForceMeshUpdate();

        if (coMinusHpFloat != null)
        {
            StopCoroutine(coMinusHpFloat);
            coMinusHpFloat = null;
            ResetFloatTextToOrigin(txtMinusHp, ref minusHpOriginPos, ref minusHpOriginCached);
        }

        coMinusHpFloat = StartCoroutine(CoFloatText(txtMinusHp));
    }

    private void ForceShowText(TMP_Text txt)
    {
        if (txt == null) return;

        Transform p = txt.transform;
        while (p != null)
        {
            if (!p.gameObject.activeSelf) p.gameObject.SetActive(true);

            var cg = p.GetComponent<CanvasGroup>();
            if (cg != null && cg.alpha <= 0.001f) cg.alpha = 1f;

            if (p.localScale.sqrMagnitude < 0.0001f) p.localScale = Vector3.one;
            p = p.parent;
        }

        if (!txt.gameObject.activeSelf) txt.gameObject.SetActive(true);
        txt.enabled = true;
    }

    private IEnumerator CoFloatText(TMP_Text txt)
    {
        if (txt == null) yield break;

        txt.gameObject.SetActive(true);
        RectTransform rt = txt.rectTransform;
        Vector2 startPos = rt.anchoredPosition;
        Vector2 endPos = startPos + Vector2.up * floatHeight;

        float dir = UnityEngine.Random.value < 0.5f ? -1f : 1f;
        float arc = floatArcX * dir;
        float t = 0f;

        Color c = txt.color;
        c.a = 0f;
        txt.color = c;
        rt.localScale = Vector3.one * startScale;

        float dur = Mathf.Max(0.05f, floatDuration);

        while (t < 1f)
        {
            t += Time.deltaTime / dur;
            float u = Mathf.Clamp01(t);
            float easeOut = 1f - Mathf.Pow(1f - u, 3f);

            float xOffset = Mathf.Sin(u * Mathf.PI) * arc;
            Vector2 pos = Vector2.Lerp(startPos, endPos, easeOut);
            pos.x += xOffset;
            rt.anchoredPosition = pos;

            float s;
            if (u < 0.35f)
            {
                float k = u / 0.35f;
                s = Mathf.Lerp(startScale, peakScale, 1f - Mathf.Pow(1f - k, 3f));
            }
            else
            {
                float k = (u - 0.35f) / 0.65f;
                s = Mathf.Lerp(peakScale, endScale, k);
            }
            rt.localScale = Vector3.one * s;

            float alphaIn = Mathf.Clamp01(u / 0.12f);
            float alphaOut = 1f - Mathf.Clamp01((u - 0.55f) / 0.45f);
            c.a = Mathf.Min(alphaIn, alphaOut);
            txt.color = c;

            yield return null;
        }

        rt.anchoredPosition = startPos;
        rt.localScale = Vector3.one * startScale;
        c.a = 0f;
        txt.color = c;
        txt.gameObject.SetActive(false);
    }

#if UNITY_EDITOR
    private void DebugLogSnapshot(Vector3 newTargetPos, Vector3 newServerVel)
    {
        Vector3 pos = transform.position;
        float dist = Vector3.Distance(pos, newTargetPos);

        // Debug.Log(
        //     $"[ANIM-DEBUG] SNAPSHOT " +
        //     $"uid={userId} " +
        //     $"newTarget=({newTargetPos.x:F1},{newTargetPos.z:F1}) " +
        //     $"currentPos=({pos.x:F1},{pos.z:F1}) " +
        //     $"distJump={dist:F1} " +
        //     $"newVel=({newServerVel.x:F1},{newServerVel.z:F1}) " +
        //     $"velMag={newServerVel.magnitude:F1}"
        // );
    }

    private void DebugDetectFlip()
    {
        if (animator == null || !animator.isInitialized) return;

        float currentSpeed = animator.GetFloat("Speed");

        if (Mathf.Abs(currentSpeed - _debugLastSpeed) > 0.5f)
        {
            float now = Time.time;
            if (now - _debugLastFlipTime < 0.2f)
            {
                _debugFlipCount++;

                Vector3 pos = transform.position;
                Vector3 moveVec = targetPos - pos;
                moveVec.y = 0;

                float distToTarget = moveVec.magnitude;
                float velMag = serverVelocity.magnitude;

                // Debug.LogWarning(
                //     $"[ANIM-DEBUG] FLIP#{_debugFlipCount} " +
                //     $"uid={userId} " +
                //     $"speed {_debugLastSpeed:F2}→{currentSpeed:F2} " +
                //     $"distToTarget={distToTarget:F1} " +
                //     $"chongRung={chongRung:F1} " +
                //     $"velMag={velMag:F1} " +
                //     $"timeSinceSnap={Time.time - lastUpdateTime:F3}s " +
                //     $"busy={IsBusy()}"
                // );
            }
            else
            {
                _debugFlipCount = 0;
            }
            _debugLastFlipTime = now;
        }

        _debugLastSpeed = currentSpeed;
    }
#endif

    private void CachePhysicsIfNeeded()
    {
        if (_physicsCached) return;
        _physicsCached = true;

        // Lấy toàn bộ collider/cc trong hierarchy (kể cả object con đang inactive)
        _cachedColliders = GetComponentsInChildren<Collider>(true);
        _cachedCharControllers = GetComponentsInChildren<CharacterController>(true);
        _cachedRigidbodies = GetComponentsInChildren<Rigidbody>(true);
    }

    private void SetPhysicsEnabled(bool enabled)
    {
        CachePhysicsIfNeeded();

        if (_cachedCharControllers != null)
        {
            for (int i = 0; i < _cachedCharControllers.Length; i++)
            {
                var cc = _cachedCharControllers[i];
                if (cc != null) cc.enabled = enabled;
            }
        }

        if (_cachedColliders != null)
        {
            for (int i = 0; i < _cachedColliders.Length; i++)
            {
                var col = _cachedColliders[i];
                if (col != null) col.enabled = enabled;
            }
        }

        // Nếu prefab có rigidbody, disable physics để khỏi đẩy/lắc
        if (_cachedRigidbodies != null)
        {
            for (int i = 0; i < _cachedRigidbodies.Length; i++)
            {
                var rb = _cachedRigidbodies[i];
                if (rb == null) continue;

                rb.isKinematic = !enabled;
                rb.detectCollisions = enabled;
            }
        }
    }
}