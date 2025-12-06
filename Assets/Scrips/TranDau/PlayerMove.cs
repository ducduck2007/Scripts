using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    // ==================== CORE MOVEMENT ====================
    public float moveSpeed = 50f;
    public float gravity = -20f;
    public float turnSpeed = 10f;
    public float rotateSpeed = 10f;

    // Components
    public Animator animator;
    public CharacterController controller;

    // Attack
    public float normalAttackRange = 3f;
    public float skillAttackRange = 6f;
    public float attackCooldown = 1f;
    public Transform attackPoint;
    public LayerMask enemyLayer;
    public int normalAttackDamage = 1;

    // ==================== SKILL CONFIGURATION ====================
    [System.Serializable]
    public class SkillConfig
    {
        public string skillName;
        public float duration = 2f;
        public int damage = 3;
        public GameObject enemyEffectPrefab;
        public Transform playerEffectPrefab;
        public string animationBool;
        public float effectDelay = 0.3f;
        public float damageDelay = 0.5f;
        public bool spawnEffectAtStart = false;
        public bool spawnEffectOnDamage = true;
        public float effectScale = 1f;
        public float effectDuration = 2f;
    }

    // Skill configurations
    public SkillConfig skill1Config = new SkillConfig
    {
        skillName = "Skill1",
        animationBool = "isTungChieu"
    };

    public SkillConfig skill2Config = new SkillConfig
    {
        skillName = "Skill2",
        animationBool = "isSkill2"
    };

    public SkillConfig skill3Config = new SkillConfig
    {
        skillName = "Skill3",
        animationBool = "isSkill3"
    };

    // Normal attack
    private bool isNormalAttacking;
    private float normalAttackEndTime;
    public float normalAttackDuration = 1.2f;

    // ==================== EFFECT SETTINGS ====================
    [Header("Effect Settings")]
    public Transform skillEffectPoint; // Vị trí spawn hiệu ứng trên enemy
    public Transform playerSkillEffectPoint; // Vị trí spawn hiệu ứng player

    [Header("Effect Height Settings")]
    public bool spawnAtFeet = false;
    public bool spawnAtCustomHeight = true;
    public float effectHeight = 0.5f;
    public float groundOffset = 0.1f;
    public LayerMask groundLayer;
    public float raycastDistance = 5f;

    [Header("Effect Position Offset")]
    public Vector3 positionOffset = Vector3.zero;
    public Vector3 rotationOffset = Vector3.zero;

    [Header("Effect Movement")]
    public bool followPlayer = true;
    public bool destroyOnCastEnd = true;

    // ==================== PRIVATE VARIABLES ====================
    private Transform target;
    private float lastAttackTime;
    private Vector3 velocity;

    // Current active states
    private SkillConfig currentSkill;
    private float skillEndTime;
    private bool isCastingSkill;

    // Effect tracking
    private GameObject currentPlayerEffect;
    private bool skillEffectSpawned;
    private bool playerSkillEffectSpawned;

    void Start()
    {
        if (groundLayer.value == 0)
        {
            groundLayer = LayerMask.GetMask("Default", "Ground", "Terrain");
        }

        // Set default skill names if not set
        if (string.IsNullOrEmpty(skill1Config.skillName)) skill1Config.skillName = "Skill1";
        if (string.IsNullOrEmpty(skill2Config.skillName)) skill2Config.skillName = "Skill2";
        if (string.IsNullOrEmpty(skill3Config.skillName)) skill3Config.skillName = "Skill3";
    }

    void Update()
    {
        // Check skill end conditions
        if (isNormalAttacking && Time.time >= normalAttackEndTime)
            OnNormalAttackEnd();

        if (isCastingSkill && Time.time >= skillEndTime)
            OnSkillEnd(currentSkill);

        // Movement
        Move();

        // Update effect position if following player
        UpdateEffectPosition();
    }

    #region ==================== PUBLIC SKILL METHODS ====================
    public void CastSkill3()
    {
        StartSkill(skill3Config);
    }

    public void CastSkill2() => StartSkill(skill2Config);

    public void CastSkill1() => StartSkill(skill1Config);

    public void NormalAttack()
    {
        if (IsBusy()) return;

        Debug.Log($">>> Kiểm tra đánh thường...");
        FindTargetInRange(normalAttackRange);
        StartNormalAttack();
    }
    #endregion

    #region ==================== SKILL MANAGEMENT ====================
    private void StartSkill(SkillConfig config)
    {
        if (IsBusy()) return;

        currentSkill = config;
        isCastingSkill = true;
        skillEndTime = Time.time + config.duration;

        Debug.Log($">>> Bắt đầu {config.skillName}...");

        // Reset movement
        velocity = Vector3.zero;

        // Reset effect flags
        skillEffectSpawned = false;
        playerSkillEffectSpawned = false;

        // Clean up old effect
        CleanupCurrentEffect();

        // Play animation
        SetAnimatorWalking(false);
        animator.SetBool(config.animationBool, true);

        // Find target
        FindTargetInRange(skillAttackRange);

        // Schedule effect and damage
        if (config.spawnEffectAtStart)
        {
            Invoke(nameof(SpawnPlayerSkillEffect), 0f);
        }
        else
        {
            Invoke(nameof(SpawnPlayerSkillEffect), config.effectDelay);
        }

        // Schedule damage
        Invoke(nameof(DealCurrentSkillDamage), config.damageDelay);
    }

    private void DealCurrentSkillDamage()
    {
        if (currentSkill == null) return;

        if (target != null)
        {
            // Spawn enemy effect (chỉ spawn nếu có prefab)
            if (currentSkill.enemyEffectPrefab != null && skillEffectPoint != null)
            {
                var fx = Instantiate(currentSkill.enemyEffectPrefab,
                                   skillEffectPoint.position,
                                   skillEffectPoint.rotation);

                // Scale effect
                fx.transform.localScale *= 3f;

                // Destroy after duration
                Destroy(fx, 2f);
            }

            // Spawn player effect if not already spawned
            if (currentSkill.spawnEffectOnDamage && !playerSkillEffectSpawned)
            {
                SpawnPlayerSkillEffect();
            }

            // Deal damage
            DealDamageInRange(skillAttackRange, currentSkill.damage, currentSkill.skillName);
        }
        else
        {
            Debug.Log($"{currentSkill.skillName} không trúng mục tiêu nào");
        }

        skillEffectSpawned = true;
    }

    private void OnSkillEnd(SkillConfig config)
    {
        if (config == null) return;

        Debug.Log($"{config.skillName} animation kết thúc");

        // Reset animation
        animator.SetBool(config.animationBool, false);
        isCastingSkill = false;

        // Cleanup effect
        if (destroyOnCastEnd && currentPlayerEffect != null)
        {
            Destroy(currentPlayerEffect);
            currentPlayerEffect = null;
        }

        // Reset tracking
        skillEffectSpawned = false;
        playerSkillEffectSpawned = false;
        currentSkill = null;

        // Update walking animation
        UpdateWalkingAnimation();
    }
    #endregion

    #region ==================== NORMAL ATTACK ====================
    private void StartNormalAttack()
    {
        isNormalAttacking = true;
        normalAttackEndTime = Time.time + normalAttackDuration;

        SetAnimatorWalking(false);
        animator.SetBool("isDanhThuong", true);

        lastAttackTime = Time.time;

        Debug.Log(">>> Đang đánh thường (animation chạy)");
        Invoke(nameof(DealNormalAttackDamage), 0.3f);
    }

    private void OnNormalAttackEnd()
    {
        animator.SetBool("isDanhThuong", false);
        isNormalAttacking = false;
        UpdateWalkingAnimation();
        Debug.Log("Đánh thường animation kết thúc");
    }
    #endregion

    #region ==================== EFFECT MANAGEMENT ====================
    private void SpawnPlayerSkillEffect()
    {
        if (currentSkill == null || playerSkillEffectSpawned || !isCastingSkill) return;

        playerSkillEffectSpawned = true;

        if (currentSkill.playerEffectPrefab == null) return;

        Vector3 spawnPosition = CalculateEffectPosition();
        Quaternion spawnRotation = CalculateEffectRotation();

        currentPlayerEffect = Instantiate(currentSkill.playerEffectPrefab.gameObject,
                                         spawnPosition,
                                         spawnRotation);

        // Apply scaling
        currentPlayerEffect.transform.localScale *= currentSkill.effectScale;

        // Parent if following player
        if (followPlayer)
        {
            currentPlayerEffect.transform.parent = transform;
        }

        // Calculate destroy time
        float destroyTime = currentSkill.effectDuration;
        if (destroyOnCastEnd)
        {
            float timeLeft = skillEndTime - Time.time;
            destroyTime = Mathf.Min(destroyTime, timeLeft);
        }

        Destroy(currentPlayerEffect, destroyTime);
    }

    private Vector3 CalculateEffectPosition()
    {
        if (spawnAtFeet)
        {
            return transform.position + Vector3.up * groundOffset;
        }

        if (spawnAtCustomHeight)
        {
            Vector3 basePosition = playerSkillEffectPoint != null ?
                playerSkillEffectPoint.position : transform.position;
            return basePosition + Vector3.up * effectHeight + positionOffset;
        }

        Vector3 pos = playerSkillEffectPoint != null ?
            playerSkillEffectPoint.position : transform.position;
        return pos + positionOffset;
    }

    private Quaternion CalculateEffectRotation()
    {
        Quaternion spawnRotation = Quaternion.identity;
        if (playerSkillEffectPoint != null && !spawnAtFeet)
        {
            spawnRotation = playerSkillEffectPoint.rotation;
        }
        return spawnRotation * Quaternion.Euler(rotationOffset);
    }

    private void UpdateEffectPosition()
    {
        if (currentPlayerEffect == null || !followPlayer) return;

        Vector3 newPosition = CalculateEffectPosition();
        currentPlayerEffect.transform.position = newPosition;
    }

    private void CleanupCurrentEffect()
    {
        if (currentPlayerEffect != null)
        {
            Destroy(currentPlayerEffect);
            currentPlayerEffect = null;
        }
    }
    #endregion

    #region ==================== COMBAT LOGIC ====================
    private void FindTargetInRange(float range)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, range, enemyLayer);

        float minDist = Mathf.Infinity;
        target = null;

        foreach (var h in hits)
        {
            BotController bot = h.GetComponent<BotController>();
            if (bot != null && !bot.isDead)
            {
                float d = Vector3.Distance(transform.position, h.transform.position);
                if (d < minDist)
                {
                    minDist = d;
                    target = h.transform;
                }
            }
        }

        Debug.Log(target != null ?
            $"Tìm thấy target trong tầm {range}: {target.name}" :
            $"Không tìm thấy target trong tầm {range}");
    }

    void RotateToTarget()
    {
        if (target == null) return;

        Vector3 dir = target.position - transform.position;
        dir.y = 0;

        if (dir.sqrMagnitude < 0.1f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
    }

    private void DealNormalAttackDamage()
    {
        if (target == null)
        {
            Debug.Log("Đánh thường không trúng mục tiêu nào");
            return;
        }

        float distance = Vector3.Distance(transform.position, target.position);
        if (distance <= normalAttackRange)
        {
            BotController bot = target.GetComponent<BotController>();
            if (bot != null)
            {
                bot.TakeDamage(normalAttackDamage);
                Debug.Log($"Đánh thường gây {normalAttackDamage} damage cho {target.name}");
            }
        }
        else
        {
            Debug.Log("Mục tiêu đã ra khỏi tầm đánh!");
        }
    }

    private void DealDamageInRange(float range, int damage, string attackType)
    {
        if (target == null) return;

        Collider[] hits = Physics.OverlapSphere(transform.position, range, enemyLayer);

        int hitCount = 0;
        foreach (var h in hits)
        {
            BotController bot = h.GetComponent<BotController>();
            if (bot != null && !bot.isDead)
            {
                bot.TakeDamage(damage);
                hitCount++;
                Debug.Log($"{attackType} gây {damage} sát thương cho {h.name}");
            }
        }

        Debug.Log($"{attackType} trúng {hitCount} mục tiêu");
    }
    #endregion

    #region ==================== MOVEMENT ====================
    private void Move()
    {
        // Handle rotation during combat
        if (IsInCombatState())
        {
            HandleCombatMovement();
            return;
        }

        HandleNormalMovement();
    }

    private void HandleCombatMovement()
    {
        RotateToTarget();

        // Apply gravity
        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;
        controller.Move(new Vector3(0, velocity.y, 0) * Time.deltaTime);
    }

    private void HandleNormalMovement()
    {
        Vector2 input = MenuController.Instance.joystick.inputVector;
        Vector3 direction = new Vector3(input.x, 0, input.y);

        SetAnimatorWalking(direction.magnitude > 0.1f);

        if (direction.magnitude > 0.1f)
        {
            CameraFollow.Instance?.SetFollow(true);
        }

        Vector3 moveDir = Vector3.zero;

        if (direction.magnitude > 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            float angle = Mathf.LerpAngle(transform.eulerAngles.y, targetAngle, Time.deltaTime * turnSpeed);

            transform.rotation = Quaternion.Euler(0, angle, 0);
            moveDir = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward * moveSpeed;
        }

        // Apply gravity
        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;

        Vector3 finalMove = moveDir + velocity;
        controller.Move(finalMove * Time.deltaTime);
    }

    private bool IsInCombatState()
    {
        return isNormalAttacking || isCastingSkill;
    }

    private bool IsBusy()
    {
        return isNormalAttacking || isCastingSkill;
    }
    #endregion

    #region ==================== UTILITY METHODS ====================
    private void SetAnimatorWalking(bool isWalking)
    {
        animator.SetBool("isWalking", isWalking);
    }

    private void UpdateWalkingAnimation()
    {
        Vector2 input = MenuController.Instance.joystick.inputVector;
        SetAnimatorWalking(input.magnitude > 0.1f);
    }
    #endregion

    #region ==================== INTERFACE IMPLEMENTATION ====================
    public void OnSkillAnimationEnd(string skillName)
    {
        Debug.Log($"Skill {skillName} animation ended");

        // Reset animation bools cho tất cả skill
        animator.SetBool("isTungChieu", false);
        animator.SetBool("isSkill2", false);
        animator.SetBool("isSkill3", false);

        // Reset casting state
        isCastingSkill = false;
        currentSkill = null;

        // Cleanup effect
        if (destroyOnCastEnd && currentPlayerEffect != null)
        {
            Destroy(currentPlayerEffect);
            currentPlayerEffect = null;
        }

        // Update walking animation
        UpdateWalkingAnimation();
    }
    #endregion
}