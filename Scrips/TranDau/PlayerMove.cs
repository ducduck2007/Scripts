using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerMove : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 50f;
    public float turnSpeed = 10f;
    public float rotateSpeed = 10f;
    public float gravity = -20f;

    [Header("Components")]
    public Animator animator;
    public CharacterController controller;
    public ProgressBar HealthBar;
    public SpriteRenderer spTamDanhThuong;
    public Transform parentSkill;
    public RectTransform directionArrow;

    [Header("Attack Settings")]
    public LayerMask enemyLayer;
    public AttackConfig normalAttackConfig = new AttackConfig();

    [Header("Skills")]
    public AttackConfig skill1 = new AttackConfig();
    public AttackConfig skill2 = new AttackConfig();
    public AttackConfig skill3 = new AttackConfig();

    private bool isAlive = true;
    private bool isAttacking;
    private Transform target;
    private Vector3 velocity;
    private AttackConfig currentAttack;

    private float lastInputTime;
    private Vector2 lastInput;
    private IEnumerator _HienTamDanhThuong;

    private const float INTERVAL = 1f / 60f;
    private const float INPUT_INTERVAL = 0.05f;
    private float timer;

    public float hpMax;
    public float hpCurrent;

    // ========== OPTIMIZATION: Buffer cho Physics queries ==========
    private Collider[] hitBuffer = new Collider[10];

    [System.Serializable]
    public class AttackConfig
    {
        [Header("Basic Settings")]
        public GameObject prefab;
        public string animationBool = "isAttack";
        public int attackRange = 300;
        public int damage = 1;

        [Header("Timing")]
        public float duration = 1.2f;
        public float damageDelay = 0.3f;
        public float spawnDelay = 0.2f;
        public float projectileSpeed = 10f;

        [Header("Spawn Behavior")]
        public SpawnType spawnType = SpawnType.AtTarget;

        [System.NonSerialized]
        public GameObject cachedInstance;
    }

    public enum SpawnType
    {
        AtSelf,
        AtTarget,
        MoveToTarget
    }

    void Start()
    {
        // Debug.Log("PlayerMove initialized.");
        InitializeHealthBar();
        ResetAnimatorStates();
        InitializeAllPrefabs();
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer < INTERVAL) return;
        timer -= INTERVAL;

        if (!isAlive)
        {
            SetAnimatorSpeed(0f);
            return;
        }

        animator.applyRootMotion = !isAttacking;

        if (isAttacking)
        {
            HandleCombatUpdate();
        }
        else
        {
            Move();
        }
    }

    #region INITIALIZATION
    private void InitializeHealthBar()
    {
        if (HealthBar != null)
            HealthBar.SetThanhMau(0);
    }

    private void InitializeAllPrefabs()
    {
        InitializePrefab(normalAttackConfig);
        InitializePrefab(skill1);
        InitializePrefab(skill2);
        InitializePrefab(skill3);
        // Debug.Log("All prefabs initialized.");
    }

    private void InitializePrefab(AttackConfig config)
    {
        if (config.prefab == null) return;

        config.cachedInstance = Instantiate(config.prefab, Vector3.zero, Quaternion.identity);
        config.cachedInstance.SetActive(false);
    }
    #endregion

    #region ATTACK SYSTEM
    public void NormalAttack()
    {
        if (!CanPerformAction()) return;

        ShowAttackRange(normalAttackConfig.attackRange);
        ExecuteAttack(normalAttackConfig);
    }

    public void CastSkill(int skillIndex)
    {
        if (!CanPerformAction()) return;

        AttackConfig skill = GetSkillConfig(skillIndex);
        if (skill == null) return;

        ExecuteAttack(skill);
    }

    private void ExecuteAttack(AttackConfig config)
    {
        FindTargetInRange(config.attackRange);
        if (target != null) RotateToTarget();

        currentAttack = config;
        isAttacking = true;

        SetAnimatorBool(config.animationBool, true);
        SetAnimatorSpeed(0f);

        Invoke(nameof(ActivateAttackPrefab), config.spawnDelay);
        Invoke(nameof(EndAttack), config.duration);
    }

    private void ActivateAttackPrefab()
    {
        if (currentAttack?.cachedInstance == null) return;

        var (spawnPos, spawnRot) = GetSpawnTransform(currentAttack);

        currentAttack.cachedInstance.transform.SetPositionAndRotation(spawnPos, spawnRot);
        currentAttack.cachedInstance.SetActive(true);

        if (currentAttack.spawnType == SpawnType.MoveToTarget)
        {
            SetupProjectileMovement(currentAttack.cachedInstance, currentAttack);
        }
        else
        {
            float deactivateDelay = currentAttack.duration - currentAttack.spawnDelay;
            Invoke(nameof(DeactivateCurrentPrefab), deactivateDelay);
        }

        // Debug.Log($"Attack activated at: {spawnPos}");
    }

    private (Vector3 pos, Quaternion rot) GetSpawnTransform(AttackConfig config)
    {
        Vector3 pos;
        Quaternion rot;

        switch (config.spawnType)
        {
            case SpawnType.AtSelf:
            case SpawnType.MoveToTarget:
                pos = transform.position;
                rot = transform.rotation;
                break;

            case SpawnType.AtTarget:
            default:
                if (target != null)
                {
                    pos = target.position;
                    rot = Quaternion.LookRotation(target.position - transform.position);
                }
                else
                {
                    pos = transform.position + transform.forward * config.attackRange;
                    rot = transform.rotation;
                }
                break;
        }

        return (pos, rot);
    }

    private void SetupProjectileMovement(GameObject projectile, AttackConfig config)
    {
        var movement = projectile.GetComponent<SkillMovement>() ?? projectile.AddComponent<SkillMovement>();

        movement.targetPosition = target != null
            ? target.position
            : transform.position + transform.forward * config.attackRange;

        movement.moveSpeed = config.projectileSpeed;
        movement.onReachedTarget = () => projectile.SetActive(false);
    }

    private void DeactivateCurrentPrefab()
    {
        if (currentAttack?.cachedInstance != null)
            currentAttack.cachedInstance.SetActive(false);
    }

    private void EndAttack()
    {
        if (!isAttacking) return;

        SetAnimatorBool(currentAttack?.animationBool, false);
        isAttacking = false;
        UpdateMovementAnimation();
    }

    public void OnNormalAttackAnimationEnd() => EndAttack();
    #endregion

    #region COMBAT LOGIC
    private void HandleCombatUpdate()
    {
        if (target != null) RotateToTarget();
        ApplyGravity();
        SetAnimatorSpeed(0f);
    }

    // ========== OPTIMIZATION: Dùng OverlapSphereNonAlloc và sqrMagnitude ==========
    private void FindTargetInRange(float range)
    {
        int hitCount = Physics.OverlapSphereNonAlloc(
            transform.position,
            range,
            hitBuffer,
            enemyLayer
        );

        target = null;
        float minDist = range * range; // Dùng sqrMagnitude

        for (int i = 0; i < hitCount; i++)
        {
            float sqrDist = (hitBuffer[i].transform.position - transform.position).sqrMagnitude;
            if (sqrDist < minDist)
            {
                minDist = sqrDist;
                target = hitBuffer[i].transform;
            }
        }
    }

    private void RotateToTarget()
    {
        if (target == null) return;

        Vector3 direction = target.position - transform.position;
        direction.y = 0;

        if (direction.sqrMagnitude < 0.01f) return;

        Quaternion targetRot = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
    }
    #endregion

    #region MOVEMENT
    private void Move()
    {
        if (!isAlive) return;

        Vector2 input = MenuController.Instance.joystick.inputVector;
        Vector3 direction = new Vector3(input.x, 0, input.y);
        bool hasInput = direction.magnitude > 0.1f;

        SetAnimatorSpeed(hasInput ? 1f : 0f);

        if (hasInput)
        {
            RotateToDirection(direction);
            controller.Move(transform.forward * moveSpeed * Time.deltaTime);
        }

        ApplyGravity();
        HandleNetworkInput(input, hasInput);
    }

    private void RotateToDirection(Vector3 direction)
    {
        float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            Quaternion.Euler(0, targetAngle, 0),
            Time.deltaTime * turnSpeed
        );
    }

    private void ApplyGravity()
    {
        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;
        else
            velocity.y += gravity * Time.deltaTime;

        controller.Move(Vector3.up * velocity.y * Time.deltaTime);
    }

    private void HandleNetworkInput(Vector2 input, bool hasInput)
    {
        if (!GameStateManager.IsInGame() || Time.time - lastInputTime < INPUT_INTERVAL)
            return;

        lastInputTime = Time.time;

        if (hasInput)
        {
            Vector2 normalized = input.normalized;
            lastInput = input;

            float angle = Mathf.Atan2(normalized.y, normalized.x) * Mathf.Rad2Deg;
            directionArrow.rotation = Quaternion.Euler(0, 0, angle - 90);
            directionArrow.gameObject.SetActive(true);
        }
        else if (lastInput.magnitude > 0.1f)
        {
            lastInput = Vector2.zero;
            directionArrow.gameObject.SetActive(false);
        }
    }

    public void SetPotion()
    {
        controller.enabled = false;
        transform.position = new Vector3(B.Instance.PosX, 200, B.Instance.PosZ);
        controller.enabled = true;
    }
    #endregion

    #region HELPERS
    private bool CanPerformAction() => isAlive && !isAttacking;

    private AttackConfig GetSkillConfig(int index)
    {
        return index switch
        {
            1 => skill1,
            2 => skill2,
            3 => skill3,
            _ => null
        };
    }

    private void SetAnimatorBool(string boolName, bool value)
    {
        if (!string.IsNullOrEmpty(boolName))
            animator.SetBool(boolName, value);
    }

    private void SetAnimatorSpeed(float speed)
    {
        animator.SetFloat("Speed", speed);
    }

    private void ResetAnimatorStates()
    {
        SetAnimatorSpeed(0f);
        animator.SetBool("isAttack", false);
        animator.SetBool("isSkill1", false);
        animator.SetBool("isSkill2", false);
        animator.SetBool("isSkill3", false);
        animator.SetBool("isDeath", false);
    }

    private void UpdateMovementAnimation()
    {
        if (isAttacking || !isAlive) return;

        Vector2 input = MenuController.Instance.joystick.inputVector;
        SetAnimatorSpeed(input.magnitude > 0.1f ? 1f : 0f);
    }

    private void ShowAttackRange(int range)
    {
        if (_HienTamDanhThuong != null)
            StopCoroutine(_HienTamDanhThuong);

        _HienTamDanhThuong = ShowRangeCoroutine(range);
        StartCoroutine(_HienTamDanhThuong);
    }

    private IEnumerator ShowRangeCoroutine(int range)
    {
        spTamDanhThuong.size = new Vector2(range * 2, range * 2);
        spTamDanhThuong.enabled = true;
        yield return new WaitForSeconds(0.2f);
        spTamDanhThuong.enabled = false;
    }
    #endregion

    #region STATE MANAGEMENT
    public void ApplyServerData(PlayerOutPutSv data)
    {
        isAlive = data.isAlive;
        SetHp(data.hp, data.maxHp);
    }

    public void SetHp(int hp, int maxHp)
    {
        hpMax = maxHp;
        hpCurrent = hp;

        if (HealthBar != null)
        {
            float progress = hpCurrent < hpMax ? (float)(hpCurrent / hpMax) : 1f;
            int speed = hpCurrent < hpMax ? 30 : 100;
            HealthBar.SetProgress(progress, speed);
        }
    }

    public void onDeath()
    {
        isAlive = false;
        ResetAnimatorStates();
        animator.SetBool("isDeath", true);

        CancelAllInvokes();
        isAttacking = false;

        if (HealthBar != null)
            HealthBar.gameObject.SetActive(false);
    }

    public void onRespawn(int hp)
    {
        isAlive = true;
        animator.SetBool("isDeath", false);
        ResetAnimatorStates();
        SetPotion();

        if (HealthBar != null)
            HealthBar.gameObject.SetActive(true);

        SetHp(hp, hp);
    }

    private void CancelAllInvokes()
    {
        CancelInvoke(nameof(ActivateAttackPrefab));
        CancelInvoke(nameof(DeactivateCurrentPrefab));
        CancelInvoke(nameof(EndAttack));
    }
    #endregion

    #region CLEANUP
    void OnDestroy()
    {
        DestroyPrefab(normalAttackConfig);
        DestroyPrefab(skill1);
        DestroyPrefab(skill2);
        DestroyPrefab(skill3);
    }

    private void DestroyPrefab(AttackConfig config)
    {
        if (config?.cachedInstance != null)
            Destroy(config.cachedInstance);
    }
    #endregion

    #region SKILL MOVEMENT
    private class SkillMovement : MonoBehaviour
    {
        public Vector3 targetPosition;
        public float moveSpeed = 10f;
        public System.Action onReachedTarget;

        void Update()
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                onReachedTarget?.Invoke();
                Destroy(this);
            }
        }
    }
    #endregion
}