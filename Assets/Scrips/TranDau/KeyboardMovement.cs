using UnityEngine;
using System.Collections;

public class KeyboardMovement : MonoBehaviour
{
    public float moveSpeed = 50f;
    public float gravity = -20f;
    public float turnSpeed = 10f;
    public float rotateSpeed = 10f;
    public SpriteRenderer spTamDanhThuong;
    public Transform parentSkill;

    private bool isAlive;
    public float hpMax;
    public float hpCurrent;

    public ProgressBar HealthBar;

    [System.Serializable]
    public class SkillConfig
    {
        public GameObject prefab;
        public string animationTrigger = ""; // Đổi từ animationBool thành animationTrigger
        public float animationDuration = 1f;
        public float delaySpawn = 0.3f;
    }

    public SkillConfig skill1 = new SkillConfig();
    public SkillConfig skill2 = new SkillConfig();
    public SkillConfig skill3 = new SkillConfig();

    private SkillConfig currentSkillCfg;

    public Animator animator;
    public CharacterController controller;

    public Transform attackPoint;
    public LayerMask enemyLayer;
    public int normalAttackDamage = 1;
    public RectTransform directionArrow;

    [System.Serializable]
    public class NormalAttackConfig
    {
        public int attackRange = 3;
        public int damage = 1;
        public float duration = 1.2f;
        public float damageDelay = 0.3f;
        public string animationTrigger = "Attack"; // Giữ nguyên
    }

    public NormalAttackConfig normalAttackConfig = new NormalAttackConfig();

    private bool isNormalAttacking;
    private bool isSkillCasting;
    public Canvas Canvas;

    private Transform target;
    private Vector3 velocity;

    // Biến mới để kiểm soát animation
    private bool isDead = false;

    void Start()
    {
        Debug.Log("PlayerMove initialized. Using Animation Events for attack completion.");
        if (HealthBar != null)
        {
            HealthBar.transform.SetParent(Canvas.transform);
            HealthBar.color = Color.green;
        }
        
        // Khởi tạo animator
        if (animator != null)
        {
            animator.SetBool("IsDead", false);
            animator.SetBool("IsMoving", false);
        }
    }

    void Update()
    {
        if (isDead) return;

        // Tắt root motion khi đánh để code tự kiểm soát hướng
        animator.applyRootMotion = !(isNormalAttacking || isSkillCasting);

        if ((isNormalAttacking || isSkillCasting) && target != null)
        {
            RotateToTarget(); // luôn xoay trong lúc đánh hoặc ra chiêu
        }

        if (!IsBusy())
            Move();
        else
            SetAnimatorWalking(false);
    }

    #region NORMAL ATTACK
    public void NormalAttack()
    {
        if (isDead || IsBusy()) return;

        if (_HienTamDanhThuong != null)
        {
            StopCoroutine(_HienTamDanhThuong);
            _HienTamDanhThuong = null;
        }
        _HienTamDanhThuong = HienTamDanhThuong();
        StartCoroutine(_HienTamDanhThuong);
        
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
        
        // Sử dụng trigger thay vì bool
        animator.ResetTrigger("Attack"); // Reset trước để tránh lỗi
        animator.SetTrigger("Attack");
        
        Invoke(nameof(AutoResetNormalAttack), normalAttackConfig.duration);
    }

    private IEnumerator _HienTamDanhThuong;

    private IEnumerator HienTamDanhThuong()
    {
        Vector2 newSize = spTamDanhThuong.size;
        newSize.x = normalAttackConfig.attackRange * 2;
        newSize.y = normalAttackConfig.attackRange * 2;
        spTamDanhThuong.size = newSize;
        spTamDanhThuong.enabled = true;
        yield return new WaitForSeconds(0.2f);
        spTamDanhThuong.enabled = false;
    }

    private void AutoResetNormalAttack()
    {
        if (isNormalAttacking)
            EndNormalAttack();
    }

    private void EndNormalAttack()
    {
        if (!isNormalAttacking) return;

        isNormalAttacking = false;
        UpdateWalkingAnimation();
    }
    #endregion

    #region SKILLS
    public void CastSkill(int skill)
    {
        if (isDead) return;
        
        FindTargetInRange(normalAttackConfig.attackRange);
        if (IsBusy()) return;

        // chọn config trước rồi mới xử lý
        if (skill == 1) currentSkillCfg = skill1;
        else if (skill == 2) currentSkillCfg = skill2;
        else if (skill == 3) currentSkillCfg = skill3;
        else return;

        if (target != null)
            RotateToTarget();

        isSkillCasting = true;

        if (!string.IsNullOrEmpty(currentSkillCfg.animationTrigger))
        {
            // Reset tất cả trigger trước
            animator.ResetTrigger("Skill1");
            animator.ResetTrigger("Skill2");
            animator.ResetTrigger("Skill3");
            
            // Set trigger tương ứng
            animator.SetTrigger("Skill" + skill);
            
            Invoke(nameof(EndSkillAnimationWrapper), currentSkillCfg.animationDuration);
        }

        // chỉ skill 1, 2 spawn bình thường
        if (skill != 3)
            Invoke(nameof(SpawnSkillWrapper), currentSkillCfg.delaySpawn);
        else
            Invoke(nameof(SpawnSkillWrapper2), currentSkillCfg.delaySpawn);
    }

    private void EndSkillAnimationWrapper()
    {
        isSkillCasting = false;
        UpdateWalkingAnimation();
    }

    private void SpawnSkillWrapper()
    {
        if (currentSkillCfg != null && currentSkillCfg.prefab != null)
        {
            Instantiate(currentSkillCfg.prefab, parentSkill.position, parentSkill.rotation);
        }
    }

    private void SpawnSkillWrapper2()
    {
        if (currentSkillCfg == null || currentSkillCfg.prefab == null)
        {
            Debug.LogWarning("Skill 3 spawn failed: prefab null");
            return;
        }

        Vector3 spawnPos;
        Quaternion spawnRot;

        if (target != null)
        {
            // Spawn vào vị trí mục tiêu
            spawnPos = target.position;
            spawnRot = Quaternion.LookRotation(target.position - controller.transform.position);
        }
        else
        {
            // Không có target → spawn về phía trước mặt 1 khoảng tầm đánh
            spawnPos = controller.transform.position + controller.transform.forward * normalAttackConfig.attackRange;
            spawnRot = controller.transform.rotation;
        }

        Instantiate(currentSkillCfg.prefab, spawnPos, spawnRot);

        Debug.Log("Skill 3 spawned at: " + spawnPos);
    }
    #endregion

    #region COMBAT LOGIC
    private void FindTargetInRange(float range)
    {
        Collider[] hits = Physics.OverlapSphere(controller.transform.position, range, enemyLayer);

        target = null;
        float minDist = Mathf.Infinity;

        foreach (var h in hits)
        {
            float d = Vector3.Distance(controller.transform.position, h.transform.position);
            if (d < minDist)
            {
                minDist = d;
                target = h.transform;
            }
        }
    }

    private void RotateToTarget()
    {
        if (target == null) return;

        Debug.Log(
            "Controller: " + controller.transform.position +
            " | Target: " + target.position +
            " | Dir: " + (target.position - controller.transform.position)
        );

        Vector3 dir = target.position - controller.transform.position;
        dir.y = 0;

        if (dir.sqrMagnitude < 0.01f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        controller.transform.rotation = Quaternion.Slerp(
            controller.transform.rotation,
            targetRot,
            rotateSpeed * Time.deltaTime
        );
    }
    #endregion

    #region MOVEMENT
    private void Move()
    {
        if (IsInCombatState())
        {
            HandleCombatMovement();
            return;
        }
        HandleNormalMovement();
    }

    public void SetPotion()
    {
        controller.enabled = false; // tắt trước khi set vị trí
        controller.transform.position = new Vector3(B.Instance.PosX, 200, B.Instance.PosZ);
        controller.enabled = true;
    }

    private void HandleCombatMovement()
    {
        if (target != null)
            RotateToTarget();

        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;
        controller.Move(new Vector3(0, velocity.y, 0) * Time.deltaTime);
    }

    private void HandleNormalMovement()
    {
        Vector2 input = MenuController.Instance.joystick.inputVector;
        Vector3 direction = new Vector3(input.x, 0, input.y);

        bool hasInput = direction.magnitude > 0.1f;
        
        // Sử dụng parameter IsMoving thay vì isWalking
        animator.SetBool("IsMoving", hasInput);

        if (hasInput)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            controller.transform.rotation = Quaternion.Lerp(controller.transform.rotation,
                                                      Quaternion.Euler(0, targetAngle, 0),
                                                      Time.deltaTime * turnSpeed);

            Vector3 moveDir = controller.transform.forward;
            controller.Move(moveDir * moveSpeed * Time.deltaTime);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
        if (GameStateManager.IsInGame())
        {
            if (Time.time - lastInputTime >= inputInterval)
            {
                lastInputTime = Time.time;
                if (hasInput)
                {
                    Vector2 normalized = input.normalized;
                    int dirX = Mathf.RoundToInt(normalized.x * 100);
                    int dirY = Mathf.RoundToInt(normalized.y * 100);
                    SendData.SendMovementInput(dirX, dirY, true, controller.transform.position);
                    lastInput = input;

                    float angle = Mathf.Atan2(normalized.y, normalized.x) * Mathf.Rad2Deg;
                    directionArrow.rotation = Quaternion.Euler(0, 0, angle - 90);
                    directionArrow.gameObject.SetActive(true);
                }
                else if (lastInput.magnitude > 0.1f)
                {
                    SendData.SendStop(controller.transform.position);
                    lastInput = Vector2.zero;

                    directionArrow.gameObject.SetActive(false);
                }
            }
        }
    }
    private float lastInputTime = 0f;
    private float inputInterval = 0.05f;
    private Vector2 lastInput = Vector2.zero;

    #endregion

    #region STATE + GIZMOS
    private bool IsBusy() => isNormalAttacking || isSkillCasting || isDead;
    private bool IsInCombatState() => isNormalAttacking || isSkillCasting;

    private void UpdateWalkingAnimation()
    {
        Vector2 input = MenuController.Instance.joystick.inputVector;
        SetAnimatorWalking(input.magnitude > 0.1f);
    }
    
    public void ApplyServerData(PlayerOutPutSv data)
    {
        isAlive = data.isAlive;
        SetHp(data.hp, data.maxHp);
        
        // Cập nhật trạng thái chết
        if (!isAlive && !isDead)
        {
            onDeath();
        }
        else if (isAlive && isDead)
        {
            onRespawn(data.hp);
        }
    }

    public void SetHp(int hp, int maxHp)
    {
        hpMax = maxHp;
        hpCurrent = hp;
        if (HealthBar != null)
        {
            if (hpCurrent < hpMax)
            {
                HealthBar.SetProgress((float)(hpCurrent / hpMax), 3);
            }
            else
            {
                HealthBar.SetProgress(1f, 100);
            }
        }
    }

    private void SetAnimatorWalking(bool isWalking)
    {
        if (isDead) return;
        animator.SetBool("IsMoving", isWalking);
    }

    private void ResetCombatStates()
    {
        CancelInvoke(nameof(AutoResetNormalAttack));
        isNormalAttacking = false;
        isSkillCasting = false;
    }

    // Hàm khi nhân vật bị đánh
    public void OnHit()
    {
        if (isDead) return;
        
        // Kích hoạt animation Hit
        animator.SetTrigger("Hit");
    }

    public void onDeath()
    {
        isDead = true;
        isNormalAttacking = false;
        isSkillCasting = false;
        
        // Hủy tất cả invoke
        CancelInvoke(nameof(AutoResetNormalAttack));
        CancelInvoke(nameof(EndSkillAnimationWrapper));
        CancelInvoke(nameof(SpawnSkillWrapper));
        CancelInvoke(nameof(SpawnSkillWrapper2));
        
        // Set animator parameters
        animator.SetBool("IsMoving", false);
        animator.SetBool("IsDead", true);
        
        // Reset tất cả trigger
        animator.ResetTrigger("Attack");
        animator.ResetTrigger("Skill1");
        animator.ResetTrigger("Skill2");
        animator.ResetTrigger("Skill3");
        animator.ResetTrigger("Hit");
        
        if (HealthBar != null)
        {
            HealthBar.gameObject.SetActive(false);
        }
    }

    public void onRespawn(int hp)
    {
        isDead = false;
        
        // Reset animator
        animator.SetBool("IsDead", false);
        animator.Play("Idle"); // Chắc chắn về state Idle
        
        SetPotion();
        if (HealthBar != null)
        {
            HealthBar.gameObject.SetActive(true);
        }
        SetHp(hp, hp);
    }

    void OnDrawGizmosSelected()
    {
        if (controller.transform == null) return;
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(controller.transform.position, normalAttackConfig.attackRange);
    }
    #endregion

    // private ActorVisibility visibility;
}