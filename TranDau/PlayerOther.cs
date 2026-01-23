using UnityEngine;
using System.Collections;

public class PlayerOther : MonoBehaviour
{
    public Animator animator;

    public int teamId = 0;

    private Vector3 targetPos;
    private Quaternion targetRot;
    private bool isAlive = true;

    // ==================== NETWORK / PREDICTION ====================
    private Vector3 lastServerPos;
    private Vector3 serverVelocity;
    private float lastUpdateTime;

    // Extrapolate lâu hơn để tránh đứng hình khi mất snapshot
    private const float EXTRAPOLATION_LIMIT = 1.2f;

    // Clamp dự đoán tránh bay quá xa
    private const float MAX_EXTRA_DISTANCE = 150f;

    // Nếu gap quá lớn, bật chế độ "đuổi kịp nhanh" thay vì teleport cứng
    private float catchupTimer = 0f;
    private const float CATCHUP_DURATION = 0.25f;
    private const float CATCHUP_MULT = 6f;

    // ✅ FIX DỨT ĐIỂM: nếu lệch quá lớn hoặc lâu không update => snap thẳng
    private const float HARD_SNAP_DISTANCE = 80f;   // chỉnh theo game/map
    private const float HARD_SNAP_TIME_GAP = 0.35f; // giây

    // SMOOTHING PARAMETERS
    public float moveSmooth = 20f;
    public float rotateSmooth = 15f;
    public float movingThreshold = 5f;

    public LayerMask enemyLayer;

    public ProgressBar HealthBar;
    private bool serverIsAttack;

    public Transform parentSkill;

    [System.Serializable]
    public class SkillConfig
    {
        public GameObject prefab;
        public string animationBool = "";
        public float animationDuration = 1f;
        public float delaySpawn = 0.3f;
        public float damageDelay = 0.5f;
        public float projectileSpeed = 10f;

        public bool spawnAtSelf = false;
        public bool spawnAtTarget = false;
        public bool moveToTarget = false;
    }

    public SkillConfig skill1 = new SkillConfig();
    public SkillConfig skill2 = new SkillConfig();
    public SkillConfig skill3 = new SkillConfig();

    private SkillConfig currentSkillCfg;

    public float hpMax;
    public float hpCurrent;

    private bool isNormalAttacking;
    private bool isSkillCasting;
    private bool isHit;
    private Transform target;
    private Vector3 velocity;

    [System.Serializable]
    public class NormalAttackConfig
    {
        public GameObject prefab;
        public int attackRange = 400;
        public int damage = 1;
        public float duration = 1.2f;
        public float damageDelay = 0.3f;
        public float spawnDelay = 0.2f;
        public string animationBool = "isAttack";

        public bool spawnAtSelf = false;
        public bool spawnAtTarget = false;
        public bool moveToTarget = false;
    }

    public NormalAttackConfig normalAttackConfig = new NormalAttackConfig();

    void Start()
    {
        if (ShouldUseHealthBar())
        {
            SafeSetHealthBarActive(true);
            SafeSetThanhMau(teamId == B.Instance.teamId ? 1 : 2);
        }
        else
        {
            SafeSetHealthBarActive(false);
        }

        isAlive = true;

        lastServerPos = transform.position;
        targetPos = transform.position;
        targetRot = transform.rotation;
        serverVelocity = Vector3.zero;
        lastUpdateTime = Time.time;
    }

    public void SetTeamId(int teamId)
    {
        this.teamId = teamId;
    }

    public void SetHp(int hp, int maxHp)
    {
        hpMax = maxHp;
        hpCurrent = hp;

        if (!ShouldUseHealthBar()) return;
        if (HealthBar == null) return;

        try
        {
            float ratio = (hpMax <= 0f) ? 0f : Mathf.Clamp01(hpCurrent / hpMax);
            if (hpCurrent < hpMax)
                HealthBar.SetProgress(ratio, 30);
            else
                HealthBar.SetProgress(1f, 100);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[PlayerOther] HealthBar SetProgress error: {e.Message}");
        }
    }

    // ==================== NETWORK RESET ====================
    public void ResetNetworkState(Vector3 spawnPos, float heading, int newTeamId)
    {
        teamId = newTeamId;

        isAlive = true;

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

        SafeSetHealthBarActive(ShouldUseHealthBar());
        if (ShouldUseHealthBar())
            SafeSetThanhMau(teamId == B.Instance.teamId ? 1 : 2);

        ForceEnableRenderers();
    }

    private void ReviveVisuals()
    {
        isAlive = true;

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

        SafeSetHealthBarActive(ShouldUseHealthBar());
        if (ShouldUseHealthBar())
            SafeSetThanhMau(teamId == B.Instance.teamId ? 1 : 2);

        ForceEnableRenderers();

        stuckTimer = 0f;
        lastStateName = "";
    }

    private void ForceEnableRenderers()
    {
        var renderers = GetComponentsInChildren<Renderer>(true);
        foreach (var r in renderers) r.enabled = true;

        var sprites = GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var s in sprites) s.enabled = true;
    }

    public void ApplyServerData(PlayerOutPutSv data)
    {
        // ===== 1) ALIVE TRANSITIONS =====
        if (!data.isAlive)
        {
            if (isAlive) onDeath();
            return; // chết thì khỏi movement
        }
        else
        {
            if (!isAlive)
                ReviveVisuals();
        }

        // ===== 2) MOVEMENT SNAPSHOT =====
        Vector3 newServerPos = new Vector3(data.x, transform.position.y, data.y);

        float dt = Time.time - lastUpdateTime;

        // Nếu gap snapshot lớn => bật catch-up để giảm teleport cảm giác
        if (dt > 0.7f)
            catchupTimer = CATCHUP_DURATION;

        if (dt > 0.001f)
        {
            serverVelocity = (newServerPos - lastServerPos) / dt;

            float maxSpeed = 500f;
            if (serverVelocity.magnitude > maxSpeed)
                serverVelocity = serverVelocity.normalized * maxSpeed;
        }

        targetPos = newServerPos;
        lastServerPos = newServerPos;

        targetRot = Quaternion.Euler(0, data.heading, 0);

        // ✅ CMD50 sẽ đưa hp/maxHp = 0 => bỏ qua
        if (data != null && data.maxHp > 0)
        {
            SetHp(data.hp, data.maxHp);
        }

        lastUpdateTime = Time.time;
    }

    public void SetAttackState(bool isAttack, bool hasTarget)
    {
        if (!isAlive) return;
        if (!isAttack) return;

        FindTargetInRange(normalAttackConfig.attackRange);

        if (target != null) RotateToTargetInstant();
        else transform.rotation = targetRot;

        isNormalAttacking = true;
        serverIsAttack = true;

        animator.SetBool("isAttack", true);

        Invoke(nameof(SpawnNormalAttackPrefab), normalAttackConfig.spawnDelay);
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
        if (isNormalAttacking)
            EndNormalAttack();
    }

    private void EndNormalAttack()
    {
        if (!isNormalAttacking) return;

        animator.SetBool("isAttack", false);
        isNormalAttacking = false;
        serverIsAttack = false;
    }

    public void SetPotion(Vector3 pos)
    {
        transform.position = pos;
        targetPos = pos;
        lastServerPos = pos;
        serverVelocity = Vector3.zero;
        lastUpdateTime = Time.time;
        catchupTimer = CATCHUP_DURATION;
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
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotateSmooth * Time.deltaTime);
        }

        UpdateAnimation();
    }

    private void UpdateMovementWithExtrapolation()
    {
        Vector3 currentPos = transform.position;

        float timeSinceLastUpdate = Time.time - lastUpdateTime;

        Vector3 extrapolatedTarget = targetPos;

        // dự đoán theo velocity (có clamp)
        if (serverVelocity.sqrMagnitude > 0.1f)
        {
            float t = Mathf.Min(timeSinceLastUpdate, EXTRAPOLATION_LIMIT);
            Vector3 predicted = targetPos + serverVelocity * t;

            Vector3 delta = predicted - targetPos;
            delta.y = 0;
            if (delta.magnitude > MAX_EXTRA_DISTANCE)
                predicted = targetPos + delta.normalized * MAX_EXTRA_DISTANCE;

            extrapolatedTarget = predicted;
        }

        Vector3 toTarget = extrapolatedTarget - currentPos;
        toTarget.y = 0;
        float distance = toTarget.magnitude;

        if (distance < movingThreshold) return;

        // ✅ FIX DỨT ĐIỂM: snap nếu lệch lớn hoặc lâu không nhận update
        if (distance >= HARD_SNAP_DISTANCE || timeSinceLastUpdate >= HARD_SNAP_TIME_GAP)
        {
            transform.position = new Vector3(extrapolatedTarget.x, currentPos.y, extrapolatedTarget.z);
            catchupTimer = CATCHUP_DURATION;
            return;
        }

        float smooth = moveSmooth;
        if (catchupTimer > 0f)
        {
            smooth *= CATCHUP_MULT;
            catchupTimer -= Time.deltaTime;
        }

        transform.position = Vector3.Lerp(currentPos, extrapolatedTarget, smooth * Time.deltaTime);
    }

    private void UpdateAnimation()
    {
        Vector3 moveVector = (targetPos - transform.position);
        moveVector.y = 0;

        float speedValue = 0f;

        if (!IsBusy())
        {
            if (moveVector.magnitude > movingThreshold || serverVelocity.sqrMagnitude > 0.1f)
                speedValue = 1f;
            else
                speedValue = 0f;

            SetAnimatorSpeed(speedValue);
        }
        else
        {
            SetAnimatorSpeed(0f);
        }
    }

    private void FindTargetInRange(float range)
    {
        // ✅ ưu tiên dùng cache để giảm hitch (nguyên nhân hay gây “trễ vài giây”)
        if (TranDauControl.Instance != null)
        {
            target = TranDauControl.Instance.FindNearestEnemy(transform.position, range, teamId);
            if (target != null) return;
        }

        // fallback giữ y như cũ
        PlayerMove[] allPlayerMoves = FindObjectsOfType<PlayerMove>();
        PlayerOther[] allPlayerOthers = FindObjectsOfType<PlayerOther>();

        target = null;
        float minDist = Mathf.Infinity;

        foreach (var playerMove in allPlayerMoves)
        {
            if (teamId != 0 && B.Instance.teamId == teamId)
                continue;

            float distance = Vector3.Distance(transform.position, playerMove.transform.position);
            if (distance <= range && distance < minDist)
            {
                minDist = distance;
                target = playerMove.transform;
            }
        }

        foreach (var playerOther in allPlayerOthers)
        {
            if (playerOther == this) continue;

            if (teamId != 0 && playerOther.teamId == teamId)
                continue;

            float distance = Vector3.Distance(transform.position, playerOther.transform.position);
            if (distance <= range && distance < minDist)
            {
                minDist = distance;
                target = playerOther.transform;
            }
        }

        if (target == null)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, range, enemyLayer);

            foreach (var hit in hits)
            {
                if (hit.transform == this.transform) continue;

                float distance = Vector3.Distance(transform.position, hit.transform.position);
                if (distance < minDist)
                {
                    minDist = distance;
                    target = hit.transform;
                }
            }
        }
    }

    public void CastSkillFromServer(int skill, bool hasTarget)
    {
        if (!isAlive) return;

        if (skill == 1) currentSkillCfg = skill1;
        else if (skill == 2) currentSkillCfg = skill2;
        else if (skill == 3) currentSkillCfg = skill3;
        else return;

        float skillRange = normalAttackConfig.attackRange;
        FindTargetInRange(skillRange);

        if (target != null) RotateToTargetInstant();
        else transform.rotation = targetRot;

        isSkillCasting = true;

        if (!string.IsNullOrEmpty(currentSkillCfg.animationBool))
        {
            SetAnimatorSpeed(0f);

            switch (skill)
            {
                case 1: animator.SetBool("isSkill1", true); break;
                case 2: animator.SetBool("isSkill2", true); break;
                case 3: animator.SetBool("isSkill3", true); break;
            }

            Invoke(nameof(EndSkillAnimationWrapper), currentSkillCfg.animationDuration);
        }

        Invoke(nameof(SpawnSkillWithDamageDelay), currentSkillCfg.delaySpawn);
    }

    private void EndSkillAnimationWrapper()
    {
        isSkillCasting = false;

        if (currentSkillCfg != null && !string.IsNullOrEmpty(currentSkillCfg.animationBool))
        {
            animator.SetBool("isAttack", false);
            animator.SetBool("isSkill1", false);
            animator.SetBool("isSkill2", false);
            animator.SetBool("isSkill3", false);
        }
    }

    private void SpawnNormalAttackPrefab()
    {
        if (normalAttackConfig.prefab == null) return;

        Vector3 spawnPos;
        Quaternion spawnRot;

        if (normalAttackConfig.spawnAtSelf)
        {
            spawnPos = transform.position;
            spawnRot = transform.rotation;

            GameObject attackObj = Instantiate(normalAttackConfig.prefab, spawnPos, spawnRot);

            if (normalAttackConfig.moveToTarget && target != null)
            {
                PlayerOtherSkillMovement skillMovement = attackObj.AddComponent<PlayerOtherSkillMovement>();
                skillMovement.targetPosition = target.position;
                skillMovement.moveSpeed = 1200f;
            }
        }
        else if (normalAttackConfig.spawnAtTarget)
        {
            if (target != null)
            {
                spawnPos = target.position;
                spawnRot = Quaternion.LookRotation(target.position - transform.position);
            }
            else
            {
                spawnPos = transform.position + transform.forward * normalAttackConfig.attackRange;
                spawnRot = transform.rotation;
            }

            Instantiate(normalAttackConfig.prefab, spawnPos, spawnRot);
        }
        else
        {
            if (target != null)
            {
                spawnPos = target.position;
                spawnRot = Quaternion.LookRotation(target.position - transform.position);
            }
            else
            {
                spawnPos = transform.position + transform.forward * normalAttackConfig.attackRange;
                spawnRot = transform.rotation;
            }

            Instantiate(normalAttackConfig.prefab, spawnPos, spawnRot);
        }
    }

    private void SpawnSkillWithDamageDelay()
    {
        if (currentSkillCfg == null || currentSkillCfg.prefab == null) return;

        Vector3 spawnPos;
        Quaternion spawnRot;

        if (currentSkillCfg.spawnAtSelf)
        {
            spawnPos = transform.position;
            spawnRot = transform.rotation;

            GameObject skillObj = Instantiate(currentSkillCfg.prefab, spawnPos, spawnRot);

            if (currentSkillCfg.moveToTarget)
            {
                PlayerOtherSkillMovement skillMovement = skillObj.AddComponent<PlayerOtherSkillMovement>();
                skillMovement.targetPosition = target != null
                    ? target.position
                    : transform.position + transform.forward * normalAttackConfig.attackRange;
                skillMovement.moveSpeed = currentSkillCfg.projectileSpeed;
            }
        }
        else if (currentSkillCfg.spawnAtTarget)
        {
            if (target != null)
            {
                spawnPos = target.position;
                spawnRot = Quaternion.LookRotation(target.position - transform.position);
            }
            else
            {
                spawnPos = transform.position + transform.forward * normalAttackConfig.attackRange;
                spawnRot = transform.rotation;
            }

            Instantiate(currentSkillCfg.prefab, spawnPos, spawnRot);
        }
        else
        {
            if (currentSkillCfg == skill1)
            {
                spawnPos = transform.position;
                spawnRot = transform.rotation;

                GameObject skillObj = Instantiate(currentSkillCfg.prefab, spawnPos, spawnRot);

                PlayerOtherSkillMovement skillMovement = skillObj.AddComponent<PlayerOtherSkillMovement>();
                skillMovement.targetPosition = target != null
                    ? target.position
                    : transform.position + transform.forward * normalAttackConfig.attackRange;
                skillMovement.moveSpeed = currentSkillCfg.projectileSpeed;
            }
            else
            {
                if (target != null)
                {
                    spawnPos = target.position;
                    spawnRot = Quaternion.LookRotation(target.position - transform.position);
                }
                else
                {
                    spawnPos = transform.position + transform.forward * normalAttackConfig.attackRange;
                    spawnRot = transform.rotation;
                }

                Instantiate(currentSkillCfg.prefab, spawnPos, spawnRot);
            }
        }
    }

    private class PlayerOtherSkillMovement : MonoBehaviour
    {
        public Vector3 targetPosition;
        public float moveSpeed = 10f;

        void Update()
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
                Destroy(gameObject);
        }
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

        SetAnimatorSpeed(0f);
        animator.SetBool("isAttack", false);
        animator.SetBool("isSkill1", false);
        animator.SetBool("isSkill2", false);
        animator.SetBool("isSkill3", false);
        animator.SetBool("isDeath", true);

        CancelInvoke(nameof(AutoResetNormalAttack));
        CancelInvoke(nameof(SpawnNormalAttackPrefab));
        CancelInvoke(nameof(EndSkillAnimationWrapper));
        CancelInvoke(nameof(SpawnSkillWithDamageDelay));

        isNormalAttacking = false;
        isSkillCasting = false;
        isHit = false;
        serverIsAttack = false;

        SafeSetHealthBarActive(false);
    }

    public void onRespawn(float x, float y, int hp)
    {
        Vector3 respawnPos = new Vector3(x, 0, y);
        float heading = transform.eulerAngles.y;

        ResetNetworkState(respawnPos, heading, teamId);
        SetHp(hp, hp);
    }

    // ==================== ANIMATOR STUCK DETECTOR (GIỮ NGUYÊN) ====================
    private float stuckTimer = 0f;
    private string lastStateName = "";

    void DetectAnimatorStuck()
    {
        if (!isAlive) return;
        if (animator == null || !animator.isInitialized) return;

        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        string stateName = GetCurrentStateName(info);

        if (stateName != lastStateName)
        {
            lastStateName = stateName;
            stuckTimer = 0f;
            return;
        }

        stuckTimer += Time.deltaTime;

        if (stateName == "Idle" || stateName == "Walking")
            return;

        if (stuckTimer > 5f)
        {
            ForceResetAnimator();
        }
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
        animator.SetBool("isAttack", false);
        animator.SetBool("isSkill1", false);
        animator.SetBool("isSkill2", false);
        animator.SetBool("isSkill3", false);
        animator.SetBool("isDeath", false);
        animator.SetBool("isHit", false);

        isNormalAttacking = false;
        isSkillCasting = false;
        isHit = false;

        stuckTimer = 0f;
        lastStateName = "";
    }

    void OnDrawGizmosSelected()
    {
        if (transform == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, normalAttackConfig.attackRange);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, targetPos);
        Gizmos.DrawSphere(targetPos, 10f);

        Gizmos.color = Color.blue;
        if (serverVelocity.sqrMagnitude > 0.01f)
            Gizmos.DrawRay(transform.position, serverVelocity.normalized * 50f);
    }

    private bool ShouldUseHealthBar()
    {
        // ✅ FIX: trả về bool đúng
        return TranDauControl.Instance != null;
    }

    private void SafeSetThanhMau(int type)
    {
        if (!ShouldUseHealthBar()) return;
        if (HealthBar == null) return;

        try
        {
            HealthBar.SetThanhMau(type);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[PlayerOther] HealthBar SetThanhMau error: {e.Message}");
        }
    }

    private void SafeSetHealthBarActive(bool active)
    {
        if (!ShouldUseHealthBar()) active = false;
        if (HealthBar == null) return;

        try
        {
            HealthBar.gameObject.SetActive(active);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[PlayerOther] HealthBar SetActive error: {e.Message}");
        }
    }
}
