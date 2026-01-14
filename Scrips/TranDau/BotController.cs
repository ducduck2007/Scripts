using UnityEngine;
using System.Collections;

public class BotController : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackRange = 380f;
    public float rotateSpeed = 5f;
    public float attackCooldown = 1f;
    public int attackDamage = 1;

    [Header("Health Settings")]
    public int maxHealth = 5;
    private int currentHealth;
    public bool isDead = false;

    [Header("Animation")]
    public Animator animator;

    [Header("Player Layer")]
    public LayerMask enemyLayer;

    private Transform target;
    private float lastAttackTime = -999f;

    private bool isNormalAttacking;
    private bool isSkillCasting;
    private bool isHit;

    [Header("Hit Effect")]
    public GameObject hitEffectPrefab;
    public Transform hitEffectPoint;

    [Header("Gravity Settings")]
    public float gravity = -20f;
    private Vector3 velocity;
    public CharacterController controller;

    [Header("Health Bar")]
    public ProgressBar HealthBar;

    [Header("Team Settings")]
    public int teamId = 0;

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

    [System.Serializable]
    public class NormalAttackConfig
    {
        public GameObject prefab;
        public int attackRange = 3;
        public int damage = 1;
        public float duration = 1.2f;
        public float damageDelay = 0.3f;
        public float spawnDelay = 0.2f;
        public string animationBool = "isAttack";

        public bool spawnAtSelf = false;
        public bool spawnAtTarget = false;
        public bool moveToTarget = false;
    }

    [Header("Normal Attack Config")]
    public NormalAttackConfig normalAttackConfig = new NormalAttackConfig();

    // ========== OPTIMIZATION: Buffer cho Physics queries ==========
    private Collider[] hitBuffer = new Collider[10];

    // ========== OPTIMIZATION: Giảm tần suất tìm player ==========
    private float findPlayerTimer = 0f;
    private const float FIND_PLAYER_INTERVAL = 0.15f; // Chỉ tìm ~6-7 lần/giây

    void Start()
    {
        currentHealth = maxHealth;

        if (animator == null)
            animator = GetComponent<Animator>();

        if (controller == null)
            controller = GetComponent<CharacterController>();

        if (HealthBar != null)
        {
            if (teamId == B.Instance.teamId)
            {
                HealthBar.SetThanhMau(1);
            }
            else
            {
                HealthBar.SetThanhMau(2);
            }
        }

        ResetAllAnimatorStates();
    }

    const float INTERVAL = 1f / 60f;
    float timer;

    void Update()
    {
        if (isDead) return;

        timer += Time.deltaTime;
        if (timer < INTERVAL) return;

        timer -= INTERVAL;

        ApplyGravity();

        if (IsBusy())
        {
            SetAnimatorSpeed(0f);
            return;
        }

        // ========== OPTIMIZATION: Giảm tần suất tìm player ==========
        findPlayerTimer += Time.deltaTime;
        if (findPlayerTimer >= FIND_PLAYER_INTERVAL)
        {
            findPlayerTimer = 0f;
            FindPlayer();
        }

        if (target != null)
        {
            RotateToTarget();
            AutoAttack();
        }
        else
        {
            animator.SetBool("isAttack", false);
        }

        UpdateMovementAnimation();
    }

    void ApplyGravity()
    {
        if (controller == null) return;

        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }

    void UpdateMovementAnimation()
    {
        if (IsBusy() || isDead) return;
        SetAnimatorSpeed(0f);
    }

    // ========== OPTIMIZATION: Dùng OverlapSphereNonAlloc và sqrMagnitude ==========
    void FindPlayer()
    {
        int hitCount = Physics.OverlapSphereNonAlloc(
            transform.position,
            attackRange,
            hitBuffer,
            enemyLayer
        );

        if (hitCount > 0)
        {
            float minDist = Mathf.Infinity;
            Transform nearestPlayer = null;

            for (int i = 0; i < hitCount; i++)
            {
                var hit = hitBuffer[i];

                PlayerMove playerMove = hit.GetComponent<PlayerMove>();
                PlayerOther playerOther = hit.GetComponent<PlayerOther>();

                if (playerMove != null)
                {
                    if (teamId != 0 && B.Instance.teamId == teamId)
                        continue;
                }
                else if (playerOther != null)
                {
                    if (teamId != 0 && playerOther.teamId == teamId)
                        continue;
                }

                // Dùng sqrMagnitude thay vì Distance
                float sqrDist = (hit.transform.position - transform.position).sqrMagnitude;
                if (sqrDist < minDist)
                {
                    minDist = sqrDist;
                    nearestPlayer = hit.transform;
                }
            }

            target = nearestPlayer;
        }
        else
        {
            target = null;
        }
    }

    void RotateToTarget()
    {
        if (target == null) return;

        Vector3 dir = target.position - transform.position;
        dir.y = 0;

        if (dir.sqrMagnitude < 0.01f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime * rotateSpeed);
    }

    private void RotateToTargetInstant()
    {
        if (target == null) return;

        Vector3 dir = target.position - transform.position;
        dir.y = 0;

        if (dir.sqrMagnitude < 0.01f) return;

        transform.rotation = Quaternion.LookRotation(dir);
    }

    void AutoAttack()
    {
        if (target == null || isDead) return;

        // Dùng sqrMagnitude
        float sqrDist = (target.position - transform.position).sqrMagnitude;
        if (sqrDist > attackRange * attackRange)
        {
            target = null;
            return;
        }

        if (Time.time - lastAttackTime >= attackCooldown)
        {
            lastAttackTime = Time.time;
            StartNormalAttack();
        }
    }

    public void SetAttackState(bool isAttack, bool hasTarget)
    {
        if (!isAttack || isDead) return;

        FindTargetInRange(normalAttackConfig.attackRange);

        if (target != null)
        {
            RotateToTargetInstant();
        }

        isNormalAttacking = true;

        animator.SetBool("isAttack", true);

        Invoke(nameof(SpawnNormalAttackPrefab), normalAttackConfig.spawnDelay);

        Invoke(nameof(AutoResetNormalAttack), normalAttackConfig.duration);
    }

    public void CastSkillFromServer(int skill, bool hasTarget)
    {
        if (isDead) return;

        if (skill == 1) currentSkillCfg = skill1;
        else if (skill == 2) currentSkillCfg = skill2;
        else if (skill == 3) currentSkillCfg = skill3;
        else return;

        float skillRange = normalAttackConfig.attackRange;
        FindTargetInRange(skillRange);

        if (target != null)
        {
            RotateToTargetInstant();
        }

        isSkillCasting = true;

        if (!string.IsNullOrEmpty(currentSkillCfg.animationBool))
        {
            SetAnimatorSpeed(0f);

            switch (skill)
            {
                case 1:
                    animator.SetBool("isSkill1", true);
                    break;
                case 2:
                    animator.SetBool("isSkill2", true);
                    break;
                case 3:
                    animator.SetBool("isSkill3", true);
                    break;
            }

            Invoke(nameof(EndSkillAnimationWrapper), currentSkillCfg.animationDuration);
        }

        Invoke(nameof(SpawnSkillWithDamageDelay), currentSkillCfg.delaySpawn);
    }

    public void ApplyServerData(PlayerOutPutSv data)
    {
        transform.position = new Vector3(data.x / 2, transform.position.y, data.y / 2);
        transform.rotation = Quaternion.Euler(0, data.heading, 0);

        SetHp(data.hp, data.maxHp);

        if (!data.isAlive && !isDead)
            onDeath();
        else if (data.isAlive && isDead)
            onRespawn(data.x, data.y, data.hp);
    }

    public void SetHp(int hp, int maxHp)
    {
        maxHealth = maxHp;
        currentHealth = hp;

        if (HealthBar != null)
        {
            if (currentHealth < maxHealth)
            {
                HealthBar.SetProgress((float)currentHealth / maxHealth, 30);
            }
            else
            {
                HealthBar.SetProgress(1f, 100);
            }
        }
    }

    public void SetTeamId(int teamId)
    {
        this.teamId = teamId;

        if (HealthBar != null)
        {
            if (teamId == B.Instance.teamId)
            {
                HealthBar.SetThanhMau(1);
            }
            else
            {
                HealthBar.SetThanhMau(2);
            }
        }
    }

    // ========== OPTIMIZATION: Dùng cache từ TranDauControl ==========
    private void FindTargetInRange(float range)
    {
        // Thử dùng cache từ TranDauControl trước
        if (TranDauControl.Instance != null)
        {
            target = TranDauControl.Instance.FindNearestEnemy(transform.position, range, teamId);
            if (target != null) return;
        }

        // Fallback: dùng Physics nếu cache không có kết quả
        int hitCount = Physics.OverlapSphereNonAlloc(
            transform.position,
            range,
            hitBuffer,
            enemyLayer
        );

        target = null;
        float minDist = range * range;

        for (int i = 0; i < hitCount; i++)
        {
            if (hitBuffer[i].transform == this.transform) continue;

            float sqrDist = (hitBuffer[i].transform.position - transform.position).sqrMagnitude;
            if (sqrDist < minDist)
            {
                minDist = sqrDist;
                target = hitBuffer[i].transform;
            }
        }
    }

    #region NORMAL ATTACK
    private void StartNormalAttack()
    {
        if (target != null) RotateToTarget();

        ResetCombatStates();
        isNormalAttacking = true;
        animator.SetBool("isAttack", true);

        Invoke(nameof(SpawnNormalAttackPrefab), normalAttackConfig.spawnDelay);

        PlayerMove player = target.GetComponent<PlayerMove>();
        PlayerOther playerOther = target.GetComponent<PlayerOther>();

        if (player != null)
        {
            // Debug.Log("Bot đánh Player!");
        }
        else if (playerOther != null)
        {
            // Debug.Log("Bot đánh PlayerOther!");
        }

        Invoke(nameof(AutoResetNormalAttack), normalAttackConfig.duration);
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
                SkillMovementBot skillMovement = attackObj.AddComponent<SkillMovementBot>();
                skillMovement.targetPosition = target.position;
                skillMovement.moveSpeed = 10f;
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

        // Debug.Log("Bot normal attack prefab spawned at: " + spawnPos);
    }

    public void OnNormalAttackAnimationEnd()
    {
        EndNormalAttack();
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
        UpdateMovementAnimation();
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
                SkillMovementBot skillMovement = skillObj.AddComponent<SkillMovementBot>();
                if (target != null)
                {
                    skillMovement.targetPosition = target.position;
                }
                else
                {
                    skillMovement.targetPosition = transform.position +
                        transform.forward * normalAttackConfig.attackRange;
                }
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

                SkillMovementBot skillMovement = skillObj.AddComponent<SkillMovementBot>();
                if (target != null)
                {
                    skillMovement.targetPosition = target.position;
                }
                else
                {
                    skillMovement.targetPosition = transform.position +
                        transform.forward * normalAttackConfig.attackRange;
                }
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

        // Debug.Log("Bot skill spawned at: " + spawnPos);
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

    private class SkillMovementBot : MonoBehaviour
    {
        public Vector3 targetPosition;
        public float moveSpeed = 10f;

        void Update()
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                Destroy(gameObject);
            }
        }
    }
    #endregion

    #region HIT LOGIC
    public void TakeDamage(int damage = 1)
    {
        if (isDead || isHit) return;

        currentHealth -= damage;
        // Debug.Log($"Bot bị đánh! Máu còn {currentHealth}");

        SetHp(currentHealth, maxHealth);

        TakeHit();

        if (hitEffectPrefab != null)
        {
            GameObject effect = Instantiate(
                hitEffectPrefab,
                hitEffectPoint != null ? hitEffectPoint.position : transform.position,
                Quaternion.identity
            );

            Destroy(effect, 1.5f);
        }

        if (currentHealth <= 0)
            onDeath();
    }

    public void TakeHit()
    {
        if (isHit || isDead) return;

        isHit = true;
        animator.SetBool("isHit", true);

        Invoke(nameof(ResetHitState), 0.5f);
    }

    private void ResetHitState()
    {
        isHit = false;
        animator.SetBool("isHit", false);
        UpdateMovementAnimation();
    }
    #endregion

    #region STATE MANAGEMENT
    private bool IsBusy() => isNormalAttacking || isSkillCasting || isHit;

    private void ResetAllAnimatorStates()
    {
        SetAnimatorSpeed(0f);
        animator.SetBool("isAttack", false);
        animator.SetBool("isSkill1", false);
        animator.SetBool("isSkill2", false);
        animator.SetBool("isSkill3", false);
        animator.SetBool("isHit", false);
        animator.SetBool("isDeath", false);
    }

    private void SetAnimatorSpeed(float speed)
    {
        animator.SetFloat("Speed", speed);
    }

    private void ResetCombatStates()
    {
        CancelInvoke(nameof(AutoResetNormalAttack));
        CancelInvoke(nameof(SpawnNormalAttackPrefab));
        CancelInvoke(nameof(SpawnSkillWithDamageDelay));
        CancelInvoke(nameof(EndSkillAnimationWrapper));
        CancelInvoke(nameof(ResetHitState));
        isNormalAttacking = false;
        isSkillCasting = false;
        isHit = false;
    }
    #endregion

    public void onDeath()
    {
        isDead = true;

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
        CancelInvoke(nameof(ResetHitState));

        isNormalAttacking = false;
        isSkillCasting = false;
        isHit = false;

        if (HealthBar != null)
        {
            HealthBar.gameObject.SetActive(false);
        }

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Destroy(gameObject, 3f);
    }

    public void onRespawn(float x, float y, int hp)
    {
        isDead = false;

        animator.SetBool("isDeath", false);

        transform.position = new Vector3(x, 0, y);

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = true;

        if (HealthBar != null)
        {
            HealthBar.gameObject.SetActive(true);
        }

        SetHp(hp, maxHealth);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}