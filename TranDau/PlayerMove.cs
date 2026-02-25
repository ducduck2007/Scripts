using System.Collections;
using UnityEngine;
using TMPro;

public class PlayerMove : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 300f;
    public float gravity = -400f;
    private float turnSpeed = 10f;
    private float rotateSpeed = 10f;

    [Header("Refs")]
    public Animator animator;
    public CharacterController controller;
    public SpriteRenderer spTamDanhThuong;
    public RectTransform directionArrow;

    [Header("Skill Spawn Parent (optional)")]
    public GameObject skillSpawnRoot;

    [Header("Combat")]
    public LayerMask enemyLayer;
    public ProgressBar HealthBar;

    [Header("State")]
    public bool isAlive = true;
    public float hpMax;
    public float hpCurrent;

    [System.Serializable]
    public class SkillConfig
    {
        public GameObject skillObject;
        public string animationBool = "";
        public float animationDuration = 1f;
        public float delaySpawn = 0.3f;
    }

    [Header("Skills")]
    public SkillConfig skill1 = new SkillConfig();
    public SkillConfig skill2 = new SkillConfig();
    public SkillConfig skill3 = new SkillConfig();
    private SkillConfig currentSkillCfg;
    private int currentSkillIndex;

    [System.Serializable]
    public class NormalAttackConfig
    {
        public GameObject attackObject;
        public int attackRange = 300;
        public int damage = 1;
        public float duration = 1.2f;
        public float spawnDelay = 0.2f;
        public string animationBool = "";
    }

    [Header("Normal Attack")]
    public NormalAttackConfig normalAttackConfig = new NormalAttackConfig();

    [Header("Auto Aim (Click-only Rotate skills)")]
    [SerializeField] private bool enableAutoAimClickOnlyRotate = true;

    [SerializeField] private float autoAimPrimaryRange = 0f;

    [SerializeField] private float autoAimExpandMultiplier = 1.8f;

    private static readonly Collider[] _autoAimHits = new Collider[64];

    [Header("Floating Text")]
    public TMP_Text txtGold;
    public TMP_Text txtExp;
    public TMP_Text txtMinusHp;

    [Header("Floating Text Tuning")]
    public float floatHeight = 50f;
    public float floatDuration = 0.7f;
    public float floatArcX = 25f;
    public float startScale = 0.85f;
    public float peakScale = 1.15f;
    public float endScale = 0.95f;

    private bool hasPendingServerAim;
    private TranDauControl.SkillCastInfo pendingAim;

    private bool hasAimOverride;
    private Vector3 aimOverrideWorldDir;

    [SerializeField] private bool useServerSkillVisuals = true;
    [SerializeField] private float waitServerAimTimeout = 0.8f;

    public void OnServerSkillCast(TranDauControl.SkillCastInfo info)
    {
        Vector3 d = info.dir;
        if (info.hasTarget) d = (info.targetPos - info.origin);
        d.y = 0f;
        if (d.sqrMagnitude > 0.0001f) d.Normalize();

        info.dir = d;
        pendingAim = info;
        hasPendingServerAim = true;

        if (SkillCastProtocol33.UsesDirection(info.typeSkill) && d.sqrMagnitude > 0.0001f)
            ApplyAutoAimToAimCanvases_Rotate(d);

        if (SkillCastProtocol33.UsesTargetPos(info.typeSkill) && info.hasTarget)
            ApplyAutoAimToAimCanvases_AOE(info.targetPos);
    }

    private Coroutine coGoldFloat, coExpFloat, coMinusHpFloat;
    private static readonly Collider[] _findTargetHits = new Collider[16];
    private Coroutine _activeSkillSpawnCo;
    private Coroutine _activeSkillMoveCo;

    private bool isNormalAttacking, isSkillCasting;
    private Transform target;
    private Vector3 velocity;
    private float lastInputTime;
    private const float inputInterval = 0.05f;
    private Vector2 lastInput = Vector2.zero;
    private const float INTERVAL = 1f / 60f;
    private float timer;
    private Coroutine coShowRange;

    [Header("Skill Aim Canvases")]
    public GameObject[] aimCanvasesByType = new GameObject[10];

    private Vector2 minusHpOriginPos;
    private bool minusHpOriginCached;

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

    public void DisableAllAimCanvases()
    {
        if (aimCanvasesByType != null)
        {
            for (int i = 0; i < aimCanvasesByType.Length; i++)
                if (aimCanvasesByType[i] != null) aimCanvasesByType[i].SetActive(false);
        }
    }

    public GameObject GetAimCanvasByType(int type)
    {
        if (aimCanvasesByType == null || aimCanvasesByType.Length == 0) return null;

        int idx = type - 1;
        if (idx >= 0 && idx < aimCanvasesByType.Length && aimCanvasesByType[idx] != null)
            return aimCanvasesByType[idx];

        for (int i = 0; i < aimCanvasesByType.Length; i++)
            if (aimCanvasesByType[i] != null) return aimCanvasesByType[i];

        return null;
    }

    private void Start()
    {
        if (ShouldUseHealthBar())
        {
            SafeSetThanhMau(0);
            SafeSetHealthBarActive(true);
        }
        else
        {
            SafeSetHealthBarActive(false);
        }

        ResetAllAnimatorStates();
        isAlive = true;
        DisableAllAimCanvases();
        CacheOriginIfNeeded(txtMinusHp, ref minusHpOriginPos, ref minusHpOriginCached);
        DisableAllSkillObjects();
    }

    private void OnDisable()
    {
        StopAllCoroutines();

        coGoldFloat = null;
        coExpFloat = null;
        coMinusHpFloat = null;
        coShowRange = null;
        _activeSkillSpawnCo = null;
        _activeSkillMoveCo = null;

        hasPendingServerAim = false;
        pendingAim = default;

        isNormalAttacking = false;
        isSkillCasting = false;
        currentSkillCfg = null;
    }

    private void DisableAllSkillObjects()
    {
        if (normalAttackConfig.attackObject != null)
            normalAttackConfig.attackObject.SetActive(false);

        if (skill1?.skillObject != null)
            skill1.skillObject.SetActive(false);

        if (skill2?.skillObject != null)
            skill2.skillObject.SetActive(false);

        if (skill3?.skillObject != null)
            skill3.skillObject.SetActive(false);
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer < INTERVAL) return;
        timer -= INTERVAL;

        if (!isAlive)
        {
            animator.SetFloat("Speed", 0f);
            return;
        }

        if (controller == null || !controller.enabled || !controller.gameObject.activeInHierarchy)
        {
            if (animator != null) animator.SetFloat("Speed", 0f);
            return;
        }

        animator.applyRootMotion = !(isNormalAttacking || isSkillCasting);

        if ((isNormalAttacking || isSkillCasting) && target != null)
            RotateToTarget();

        if (isNormalAttacking || isSkillCasting)
        {
            animator.SetFloat("Speed", 0f);
            return;
        }

        Move();
    }

    public void ApplyServerData(PlayerOutPutSv data)
    {
        isAlive = data.isAlive;
        SetHp(data.hp, data.maxHp);
    }

    public void SetPotion()
    {
        if (controller == null) return;
        controller.enabled = false;
        controller.transform.position = new Vector3(B.Instance.PosX, 0, B.Instance.PosZ);
        controller.enabled = true;
    }

    public void onDeath()
    {
        isAlive = false;
        ResetAllAnimatorStates();
        animator.SetBool("isDeath", true);
        CancelAllCombatInvokes();
        isNormalAttacking = false;
        isSkillCasting = false;
        SafeSetHealthBarActive(false);
        if (txtMinusHp != null) txtMinusHp.gameObject.SetActive(false);
        DisableAllSkillObjects();
    }

    public void onRespawn(int hp)
    {
        isAlive = true;
        animator.SetBool("isDeath", false);
        ResetAllAnimatorStates();
        SetPotion();

        if (controller != null && controller.enabled && controller.gameObject.activeInHierarchy)
            SendData.SendStop(controller.transform.position);

        lastInput = Vector2.zero;
        SafeSetHealthBarActive(ShouldUseHealthBar());
        SetHp(hp, hp);

        if (txtMinusHp != null)
        {
            ForceShowText(txtMinusHp);
            ResetFloatTextToOrigin(txtMinusHp, ref minusHpOriginPos, ref minusHpOriginCached);
            txtMinusHp.gameObject.SetActive(false);
        }

        DisableAllSkillObjects();
    }

    public void NormalAttack()
    {
        if (!isAlive || IsBusy()) return;

        AudioManager.Instance.PlayHeroSound(GetHeroKey(), AudioManager.HeroSoundType.Effort);
        AudioManager.Instance.AudioNormalAttack();

        ShowNormalAttackRangeOnce();
        SendData.SendAttack(0, 3, controller.transform.position, 0);

        FindTargetInRange(normalAttackConfig.attackRange);
        StartNormalAttack();
    }

    public void OnNormalAttackAnimationEnd()
    {
        EndNormalAttack();
    }

    private void StartNormalAttack()
    {
        if (target != null) RotateToTarget();

        ResetCombatStates();
        isNormalAttacking = true;
        animator.SetBool("isAttack", true);
        Invoke(nameof(ShowNormalAttackObject), normalAttackConfig.spawnDelay);
        Invoke(nameof(AutoResetNormalAttack), normalAttackConfig.duration);
    }

    private void AutoResetNormalAttack()
    {
        if (isNormalAttacking) EndNormalAttack();
    }

    private void EndNormalAttack()
    {
        if (!isNormalAttacking) return;
        animator.SetBool("isAttack", false);
        isNormalAttacking = false;

        if (normalAttackConfig.attackObject != null)
            normalAttackConfig.attackObject.SetActive(false);

        UpdateMovementAnimation();
    }

    private void ShowNormalAttackObject()
    {
        if (normalAttackConfig.attackObject == null || controller == null) return;

        Vector3 spawnPos;
        Quaternion spawnRot;
        GetTargetSpawn(out spawnPos, out spawnRot, normalAttackConfig.attackRange);

        normalAttackConfig.attackObject.transform.position = spawnPos;
        normalAttackConfig.attackObject.transform.rotation = spawnRot;
        normalAttackConfig.attackObject.SetActive(true);
    }

    private void ShowNormalAttackRangeOnce()
    {
        if (coShowRange != null)
        {
            StopCoroutine(coShowRange);
            coShowRange = null;
        }
        coShowRange = StartCoroutine(CoShowNormalAttackRange());
    }

    private IEnumerator CoShowNormalAttackRange()
    {
        if (spTamDanhThuong == null) yield break;

        Vector2 size = spTamDanhThuong.size;
        float d = normalAttackConfig.attackRange * 2f;
        size.x = d;
        size.y = d;
        spTamDanhThuong.size = size;

        spTamDanhThuong.enabled = true;
        yield return new WaitForSeconds(0.2f);
        spTamDanhThuong.enabled = false;
    }

    public void SetSkillAimOverride(Vector3 worldDir)
    {
        worldDir.y = 0f;
        if (worldDir.sqrMagnitude > 0.0001f)
        {
            aimOverrideWorldDir = worldDir.normalized;
            hasAimOverride = true;
        }
        else
        {
            hasAimOverride = false;
            aimOverrideWorldDir = Vector3.zero;
        }
    }

    public void ClearSkillAimOverride()
    {
        hasAimOverride = false;
        aimOverrideWorldDir = Vector3.zero;
    }

    private float GetPrimaryAutoAimRange()
    {
        if (autoAimPrimaryRange > 0.01f) return autoAimPrimaryRange;
        return Mathf.Max(0f, normalAttackConfig != null ? normalAttackConfig.attackRange : 0f);
    }

    private bool TryFindNearestEnemyNonAlloc(Vector3 origin, float range, out Transform nearest)
    {
        nearest = null;
        if (range <= 0.01f) return false;

        int count = Physics.OverlapSphereNonAlloc(origin, range, _autoAimHits, enemyLayer, QueryTriggerInteraction.Ignore);
        if (count <= 0) return false;

        float best = float.MaxValue;
        for (int i = 0; i < count; i++)
        {
            var c = _autoAimHits[i];
            if (c == null) continue;
            var t = c.transform;
            if (t == null || !t.gameObject.activeInHierarchy) continue;

            float sqr = (t.position - origin).sqrMagnitude;
            if (sqr < best)
            {
                best = sqr;
                nearest = t;
            }
            _autoAimHits[i] = null;
        }

        return nearest != null;
    }

    private bool TryAutoAimClickOnlyRotate(out Vector3 chosenDir, out Transform aimTarget, float primaryRange, float expandedRange)
    {
        chosenDir = Vector3.zero;
        aimTarget = null;

        if (!enableAutoAimClickOnlyRotate) return false;
        if (controller == null) return false;

        Vector3 origin = controller.transform.position;

        if (TryFindNearestEnemyNonAlloc(origin, primaryRange, out var t1) && t1 != null)
        {
            Vector3 d = t1.position - origin;
            d.y = 0f;
            if (d.sqrMagnitude > 0.0001f)
            {
                chosenDir = d.normalized;
                aimTarget = t1;
                return true;
            }
        }

        if (expandedRange > primaryRange + 0.01f)
        {
            if (TryFindNearestEnemyNonAlloc(origin, expandedRange, out var t2) && t2 != null)
            {
                Vector3 d = t2.position - origin;
                d.y = 0f;
                if (d.sqrMagnitude > 0.0001f)
                {
                    chosenDir = d.normalized;
                    aimTarget = t2;
                    return true;
                }
            }
        }

        return false;
    }

    public void CastSkill(int skill, int autoFlag = 0)
    {
        if (!isAlive || isSkillCasting) return;

        if (isNormalAttacking)
        {
            CancelInvoke(nameof(AutoResetNormalAttack));
            CancelInvoke(nameof(ShowNormalAttackObject));

            if (normalAttackConfig.attackObject != null)
                normalAttackConfig.attackObject.SetActive(false);

            animator.SetBool("isAttack", false);

            isNormalAttacking = false;
        }

        currentSkillCfg = skill switch
        {
            1 => skill1,
            2 => skill2,
            3 => skill3,
            _ => null
        };
        if (currentSkillCfg == null) return;

        currentSkillIndex = skill;
        bool useServerAuto = (autoFlag == 1);

        if (useServerAuto)
        {
            DisableAllAimCanvases(); // auto=1 => không hiển thị aim canvas cho mọi type
        }

        int skillType = GetSkillTypeForSkillId(skill);
        bool isGroundAOE = (skillType == 3);
        bool isRotateSkill = !isGroundAOE;

        int dirX = 0, dirY = 0;
        Vector3 chosenWorldDir = Vector3.zero;
        bool manualAim = false;
        int targetX = 0, targetY = 0;
        target = null;

        if (useServerAuto && controller != null)
        {
            // Click nhanh: server sẽ tự chọn hướng/target => client không lấy joystick aim
            chosenWorldDir = controller.transform.forward;
            chosenWorldDir.y = 0f;
            if (chosenWorldDir.sqrMagnitude > 0.0001f) chosenWorldDir.Normalize();

            // Giữ dir/target = 0 để server auto hoàn toàn
            dirX = 0; dirY = 0;
            targetX = 0; targetY = 0;

            manualAim = true; // để không chạy các block aim phía dưới
        }

        if (!isGroundAOE && hasAimOverride && aimOverrideWorldDir.sqrMagnitude > 0.0001f)
        {
            Vector3 d = aimOverrideWorldDir;
            d.y = 0f;
            d.Normalize();
            chosenWorldDir = d;

            EncodeDirForServer(d, out dirX, out dirY);
            manualAim = true;

            hasAimOverride = false;
            aimOverrideWorldDir = Vector3.zero;
        }
        else if (!useServerAuto && aimCanvasesByType != null && isGroundAOE)
        {
            bool useGroundTarget = false;
            Vector3 groundTargetWorld = Vector3.zero;

            for (int i = 0; i < aimCanvasesByType.Length; i++)
            {
                var go = aimCanvasesByType[i];
                if (go == null || !go.activeInHierarchy) continue;

                var msa = go.GetComponentInChildren<MobileSkillAim>(true);
                if (msa != null)
                {
                    var ss = msa.skillShotSmall;
                    var cv = (ss != null) ? ss.GetComponentInParent<Canvas>() : null;

                    if (msa.enableGroundMove && ss != null)
                    {
                        bool ok = TryGetGroundPointFromRect(ss, out groundTargetWorld);

                        if (!ok && cv != null && cv.renderMode == RenderMode.WorldSpace)
                        {
                            groundTargetWorld = ss.position;
                            groundTargetWorld.y = 0f;
                            ok = true;
                        }

                        if (ok)
                        {
                            useGroundTarget = true;
                            break;
                        }
                    }
                }
            }

            if (useGroundTarget && controller != null)
            {
                Vector3 origin = controller.transform.position;
                Vector3 d = groundTargetWorld - origin;
                d.y = 0f;
                if (d.sqrMagnitude > 0.0001f) d.Normalize();
                chosenWorldDir = d;

                EncodeDirForServer(d, out dirX, out dirY);
                manualAim = true;

                SkillCastProtocol33.UnityToServerPos(groundTargetWorld, out targetX, out targetY);
            }
        }

        if (!manualAim && isGroundAOE && controller != null)
        {
            float primary = GetPrimaryAutoAimRange();
            float expanded = primary * Mathf.Max(1f, autoAimExpandMultiplier);

            if (TryFindNearestEnemyNonAlloc(controller.transform.position, primary, out var t1) && t1 != null)
            {
                Vector3 enemyPos = t1.position;
                enemyPos.y = 0f;

                if (!useServerAuto) ApplyAutoAimToAimCanvases_AOE(enemyPos);

                SkillCastProtocol33.UnityToServerPos(enemyPos, out targetX, out targetY);

                Vector3 d = enemyPos - controller.transform.position;
                d.y = 0f;
                if (d.sqrMagnitude > 0.0001f)
                {
                    chosenWorldDir = d.normalized;
                    EncodeDirForServer(chosenWorldDir, out dirX, out dirY);
                    manualAim = true;
                }

                target = t1;
            }
            else if (TryFindNearestEnemyNonAlloc(controller.transform.position, expanded, out var t2) && t2 != null)
            {
                Vector3 enemyPos = t2.position;
                enemyPos.y = 0f;

                if (!useServerAuto) ApplyAutoAimToAimCanvases_AOE(enemyPos);

                SkillCastProtocol33.UnityToServerPos(enemyPos, out targetX, out targetY);

                Vector3 d = enemyPos - controller.transform.position;
                d.y = 0f;
                if (d.sqrMagnitude > 0.0001f)
                {
                    chosenWorldDir = d.normalized;
                    EncodeDirForServer(chosenWorldDir, out dirX, out dirY);
                    manualAim = true;
                }

                target = t2;
            }
        }

        if (!manualAim)
        {
            Vector2 input = Vector2.zero;
            try
            {
                if (MenuController.Instance?.joystick != null)
                    input = MenuController.Instance.joystick.inputVector;
            }
            catch { }

            if (input.magnitude > 0.1f)
            {
                Vector2 n = input.normalized;
                chosenWorldDir = new Vector3(n.x, 0f, n.y);
                EncodeDirForServer(chosenWorldDir, out dirX, out dirY);
                manualAim = true;
            }
        }

        if (!manualAim && isRotateSkill && controller != null)
        {
            float primary = GetPrimaryAutoAimRange();
            float expanded = primary * Mathf.Max(1f, autoAimExpandMultiplier);

            if (TryAutoAimClickOnlyRotate(out var autoDir, out var autoTarget, primary, expanded))
            {
                chosenWorldDir = autoDir;
                EncodeDirForServer(chosenWorldDir, out dirX, out dirY);
                manualAim = true;

                if (!useServerAuto) ApplyAutoAimToAimCanvases_Rotate(chosenWorldDir);

                if (autoTarget != null)
                {
                    if (targetX == 0 && targetY == 0)
                        SkillCastProtocol33.UnityToServerPos(autoTarget.position, out targetX, out targetY);

                    target = autoTarget;
                }
            }
        }

        if (!manualAim && controller != null)
        {
            Vector3 f = controller.transform.forward;
            f.y = 0f;
            f = f.sqrMagnitude > 0.0001f ? f.normalized : Vector3.forward;
            chosenWorldDir = f;
            EncodeDirForServer(chosenWorldDir, out dirX, out dirY);
        }

        if (!manualAim && targetX == 0 && targetY == 0)
        {
            FindTargetInRange(normalAttackConfig.attackRange);
            if (target != null)
            {
                SkillCastProtocol33.UnityToServerPos(target.position, out targetX, out targetY);
            }
        }

        if (controller != null)
            SendData.SendCastSkill(skill, dirX, dirY, targetX, targetY, controller.transform.position, autoFlag);

        if (chosenWorldDir.sqrMagnitude > 0.0001f && controller != null)
        {
            controller.transform.rotation = Quaternion.LookRotation(chosenWorldDir);
        }
        else if (!manualAim && target != null)
        {
            RotateToTarget();
        }

        isSkillCasting = true;
        animator.SetFloat("Speed", 0f);

        switch (skill)
        {
            case 1: animator.SetBool("isSkill1", true); break;
            case 2: animator.SetBool("isSkill2", true); break;
            case 3: animator.SetBool("isSkill3", true); break;
        }

        Invoke(nameof(EndSkillAnimationWrapper), Mathf.Max(0.05f, currentSkillCfg.animationDuration));

        if (useServerSkillVisuals)
            StartCoroutine(CoShowSkillWhenServerAimReady(currentSkillCfg, currentSkillIndex, chosenWorldDir));
        else
            StartCoroutine(CoShowSkillFallbackAfterDelay(currentSkillCfg, chosenWorldDir));
    }

    private IEnumerator CoShowSkillWhenServerAimReady(SkillConfig cfg, int skillIndex, Vector3 fallbackDir)
    {
        yield return new WaitForSeconds(cfg.delaySpawn);

        if (cfg.skillObject == null)
        {
            yield break;
        }

        float t = 0f;
        while (t < waitServerAimTimeout)
        {
            if (hasPendingServerAim)
            {
                bool isMatch = true;
                try { isMatch = (pendingAim.skillId == skillIndex); }
                catch { }
                if (isMatch) break;
            }
            t += Time.deltaTime;
            yield return null;
        }

        if (hasPendingServerAim)
        {
            try
            {
                if (controller != null && pendingAim.dir.sqrMagnitude > 0.0001f)
                {
                    Vector3 look = pendingAim.dir;
                    look.y = 0f;
                    controller.transform.rotation = Quaternion.LookRotation(look);

                    // ★ đảm bảo UI aim cũng theo server
                    if (SkillCastProtocol33.UsesDirection(pendingAim.typeSkill) && pendingAim.dir.sqrMagnitude > 0.0001f)
                        ApplyAutoAimToAimCanvases_Rotate(pendingAim.dir);

                    if (SkillCastProtocol33.UsesTargetPos(pendingAim.typeSkill) && pendingAim.hasTarget)
                        ApplyAutoAimToAimCanvases_AOE(pendingAim.targetPos);
                }
            }
            catch { }

            ShowSkillByServerAim(cfg, pendingAim);
        }
        else
        {
            ShowSkillFallback(cfg, fallbackDir);
        }

        hasPendingServerAim = false;
    }

    private IEnumerator CoShowSkillFallbackAfterDelay(SkillConfig cfg, Vector3 fallbackDir)
    {
        yield return new WaitForSeconds(cfg.delaySpawn);

        if (cfg.skillObject == null)
        {
            yield break;
        }

        ShowSkillFallback(cfg, fallbackDir);
    }

    private void ShowSkillByServerAim(SkillConfig cfg, TranDauControl.SkillCastInfo info)
    {
        if (cfg == null) return;

        if (cfg.skillObject == null)
        {
            return;
        }

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
            float spd = info.speed > 0 ? info.speed : 10f;
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

        if (dir.sqrMagnitude > 0.0001f) dir.Normalize();

        rot = (dir.sqrMagnitude > 0.0001f)
            ? Quaternion.LookRotation(dir)
            : (controller != null ? controller.transform.rotation : Quaternion.identity);

        if (usesTarget && info.hasTarget)
            dst = info.targetPos;
        else if (usesDir && dir.sqrMagnitude > 0.0001f && info.maxRange > 0.01f)
            dst = origin + dir * info.maxRange;
        else
            dst = origin;

        needMove = (usesTarget || (usesDir && info.maxRange > 0.01f));
    }

    private void ShowSkillFallback(SkillConfig cfg, Vector3 fallbackDir)
    {
        if (cfg == null) return;

        if (cfg.skillObject == null)
        {
            return;
        }

        if (controller == null) return;

        Vector3 dir = fallbackDir;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.0001f)
        {
            dir = controller.transform.forward;
            dir.y = 0f;
        }

        if (dir.sqrMagnitude > 0.0001f) dir.Normalize();

        Vector3 origin = controller.transform.position;
        Vector3 dst = origin + dir * normalAttackConfig.attackRange;
        Quaternion rot = (dir.sqrMagnitude > 0.0001f) ? Quaternion.LookRotation(dir) : controller.transform.rotation;

        cfg.skillObject.transform.position = origin;
        cfg.skillObject.transform.rotation = rot;
        cfg.skillObject.SetActive(true);

        StartCoroutine(CoMoveSkillObject(cfg.skillObject, dst, 10f));
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

    private void EndSkillAnimationWrapper()
    {
        isSkillCasting = false;
        animator.SetBool("isSkill1", false);
        animator.SetBool("isSkill2", false);
        animator.SetBool("isSkill3", false);

        if (currentSkillCfg?.skillObject != null)
            currentSkillCfg.skillObject.SetActive(false);

        currentSkillCfg = null;
        currentSkillIndex = 0;
        UpdateMovementAnimation();
    }

    private void FindTargetInRange(float range)
    {
        if (controller == null) return;

        int count = Physics.OverlapSphereNonAlloc(
            controller.transform.position,
            range,
            _findTargetHits,
            enemyLayer,
            QueryTriggerInteraction.Ignore
        );

        target = null;
        float minDist = Mathf.Infinity;

        for (int i = 0; i < count; i++)
        {
            var h = _findTargetHits[i];
            if (h == null) continue;

            float d = Vector3.Distance(controller.transform.position, h.transform.position);
            if (d < minDist)
            {
                minDist = d;
                target = h.transform;
            }

            _findTargetHits[i] = null;
        }
    }

    private void RotateToTarget()
    {
        if (target == null || controller == null) return;

        Vector3 dir = target.position - controller.transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        controller.transform.rotation = Quaternion.Slerp(controller.transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
    }

    private void GetTargetSpawn(out Vector3 spawnPos, out Quaternion spawnRot, float fallbackRange)
    {
        if (target != null)
        {
            spawnPos = target.position;
            spawnRot = Quaternion.LookRotation(target.position - controller.transform.position);
        }
        else
        {
            spawnPos = controller.transform.position + controller.transform.forward * fallbackRange;
            spawnRot = controller.transform.rotation;
        }
    }

    private void Move()
    {
        if (!isAlive || controller == null || !controller.enabled || !controller.gameObject.activeInHierarchy)
            return;

        if (IsInCombatState())
        {
            HandleCombatMovement();
            return;
        }

        HandleNormalMovement();
    }

    private void HandleCombatMovement()
    {
        if (controller == null) return;
        if (target != null) RotateToTarget();
        if (controller.isGrounded && velocity.y < 0f) velocity.y = -2f;
        velocity.y += gravity * Time.deltaTime;
        controller.Move(new Vector3(0f, velocity.y, 0f) * Time.deltaTime);
    }

    private void HandleNormalMovement()
    {
        if (controller == null) return;

        Vector2 input = MenuController.Instance.joystick.inputVector;
        Vector3 direction = new Vector3(input.x, 0f, input.y);
        bool hasInput = direction.magnitude > 0.1f;

        SetAnimatorSpeed(hasInput ? 1f : 0f);

        if (hasInput)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            controller.transform.rotation = Quaternion.Lerp(controller.transform.rotation,
                Quaternion.Euler(0f, targetAngle, 0f), Time.deltaTime * turnSpeed);
            controller.Move(controller.transform.forward * moveSpeed * Time.deltaTime);
        }

        if (controller.isGrounded)
        {
            if (velocity.y < 0f) velocity.y = -2f;
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }

        controller.Move(Vector3.up * velocity.y * Time.deltaTime);

        if (!GameStateManager.IsInGame() || Time.time - lastInputTime < inputInterval) return;
        lastInputTime = Time.time;

        if (hasInput)
        {
            Vector2 normalized = input.normalized;
            SendData.SendMovementInput(Mathf.RoundToInt(normalized.x * 100),
                Mathf.RoundToInt(normalized.y * 100), true, controller.transform.position);
            lastInput = input;

            float angle = Mathf.Atan2(normalized.y, normalized.x) * Mathf.Rad2Deg;
            directionArrow.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
            directionArrow.gameObject.SetActive(true);
        }
        else if (lastInput.magnitude > 0.1f)
        {
            SendData.SendStop(controller.transform.position);
            lastInput = Vector2.zero;
            directionArrow.gameObject.SetActive(false);
        }
    }

    public void SetHp(int hp, int maxHp)
    {
        hpMax = maxHp;
        hpCurrent = hp;

        if (!ShouldUseHealthBar() || HealthBar == null) return;

        try
        {
            float ratio = hpMax <= 0f ? 0f : (hpCurrent / hpMax);
            if (hpCurrent < hpMax)
                HealthBar.SetProgress(ratio, 30);
            else
                HealthBar.SetProgress(1f, 100);
        }
        catch { }
    }

    private bool ShouldUseHealthBar() => TranDauControl.Instance != null;

    private void SafeSetThanhMau(int type)
    {
        if (ShouldUseHealthBar() && HealthBar != null)
        {
            try { HealthBar.SetThanhMau(type); }
            catch { }
        }
    }

    private void SafeSetHealthBarActive(bool active)
    {
        if (!ShouldUseHealthBar()) active = false;
        if (HealthBar != null)
        {
            try { HealthBar.gameObject.SetActive(active); }
            catch { }
        }
    }

    private bool IsBusy() => isNormalAttacking || isSkillCasting;
    private bool IsInCombatState() => isNormalAttacking || isSkillCasting;

    private void UpdateMovementAnimation()
    {
        if (IsBusy() || !isAlive) return;
        Vector2 input = MenuController.Instance.joystick.inputVector;
        SetAnimatorSpeed(input.magnitude > 0.1f ? 1f : 0f);
    }

    private void SetAnimatorSpeed(float speed)
    {
        animator.SetFloat("Speed", speed);
    }

    private void ResetAllAnimatorStates()
    {
        animator.SetFloat("Speed", 0f);
        animator.SetBool("isAttack", false);
        animator.SetBool("isSkill1", false);
        animator.SetBool("isSkill2", false);
        animator.SetBool("isSkill3", false);
        animator.SetBool("isDeath", false);
    }

    private void ResetCombatStates()
    {
        CancelInvoke(nameof(AutoResetNormalAttack));
        CancelInvoke(nameof(ShowNormalAttackObject));
        isNormalAttacking = false;
    }

    private void CancelAllCombatInvokes()
    {
        CancelInvoke(nameof(AutoResetNormalAttack));
        CancelInvoke(nameof(ShowNormalAttackObject));
        CancelInvoke(nameof(EndSkillAnimationWrapper));
    }

    public void ShowGold(int value)
    {
        if (txtGold == null) return;
        txtGold.text = $"+{value}";
        if (coGoldFloat != null) StopCoroutine(coGoldFloat);
        coGoldFloat = StartCoroutine(CoFloatText(txtGold));
    }

    public void ShowExp(int value)
    {
        if (txtExp == null) return;
        txtExp.text = $"+{value}";
        if (coExpFloat != null) StopCoroutine(coExpFloat);
        coExpFloat = StartCoroutine(CoFloatText(txtExp));
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

    private bool TryGetGroundPointFromRect(RectTransform rt, out Vector3 world)
    {
        world = Vector3.zero;
        if (rt == null) return false;

        Canvas canvas = rt.GetComponentInParent<Canvas>();

        if (canvas != null && canvas.renderMode == RenderMode.WorldSpace)
        {
            world = rt.position;
            world.y = 0f;
            return true;
        }

        Camera cam = null;

        if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceCamera)
            cam = canvas.worldCamera;

        if (cam == null)
            cam = Camera.main;

        if (cam == null) return false;

        Vector3 screen = RectTransformUtility.WorldToScreenPoint(cam, rt.position);
        Ray ray = cam.ScreenPointToRay(screen);

        if (Physics.Raycast(ray, out RaycastHit hit, 50000f, ~0, QueryTriggerInteraction.Ignore))
        {
            world = hit.point;
            world.y = 0f;
            return true;
        }

        Plane p = new Plane(Vector3.up, Vector3.zero);
        if (p.Raycast(ray, out float enter))
        {
            world = ray.GetPoint(enter);
            world.y = 0f;
            return true;
        }

        return false;
    }

    private void EncodeDirForServer(Vector3 worldDir, out int dirX, out int dirY)
    {
        worldDir.y = 0f;
        if (worldDir.sqrMagnitude < 0.0001f)
        {
            dirX = 0;
            dirY = 0;
            return;
        }

        worldDir.Normalize();
        dirX = Mathf.RoundToInt(worldDir.x * 100f);
        dirY = Mathf.RoundToInt(worldDir.z * 100f);
    }

    private int GetSkillTypeForSkillId(int skillId)
    {
        try
        {
            if (B.Instance == null || UserData.Instance == null) return 0;
            long myId = UserData.Instance.UserID;
            return B.Instance.GetSkillType(myId, skillId);
        }
        catch
        {
            return 0;
        }
    }

    public int GetSkillTypeForSkillId_Public(int skillId)
    {
        return GetSkillTypeForSkillId(skillId);
    }

    public bool TryCastSkill(int skill, int autoFlag = 0)
    {
        bool wasCasting = isSkillCasting;
        CastSkill(skill, autoFlag);
        return (!wasCasting && isSkillCasting);
    }

    private string GetHeroKey()
    {
        switch (MenuController.Instance?.localHeroType)
        {
            case 1: return "Kayn";
            case 2: return "Leona";
            default:
                Debug.LogWarning($"[Audio] Unknown heroType: {MenuController.Instance?.localHeroType}");
                return null;
        }
    }

    private void ApplyAutoAimToAimCanvases_Rotate(Vector3 worldDir)
    {
        if (aimCanvasesByType == null) return;

        for (int i = 0; i < aimCanvasesByType.Length; i++)
        {
            var go = aimCanvasesByType[i];
            if (go == null) continue;

            var msa = go.GetComponentInChildren<MobileSkillAim>(true);
            if (msa == null) continue;

            // Không chặn theo enableRotateVisual, vì có canvas dùng logic rotate khác
            msa.SetAutoRotateWorldDir(worldDir);
        }
    }

    private void ApplyAutoAimToAimCanvases_AOE(Vector3 worldPos)
    {
        if (aimCanvasesByType == null) return;

        for (int i = 0; i < aimCanvasesByType.Length; i++)
        {
            var go = aimCanvasesByType[i];
            if (go == null) continue;

            var msa = go.GetComponentInChildren<MobileSkillAim>(true);
            if (msa == null) continue;

            if (!msa.enableGroundMove) continue;
            if (msa.skillShotSmall == null) continue;

            msa.SetAutoAOETargetWorld(worldPos, snapImmediately: true);
        }
    }

}