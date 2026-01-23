using UnityEngine;
using System.Collections;

public class PlayerMove : MonoBehaviour
{
    public float moveSpeed = 50f;
    public float gravity = -20f;
    public float turnSpeed = 10f;
    public float rotateSpeed = 10f;
    public SpriteRenderer spTamDanhThuong;
    public Transform parentSkill;

    public bool isAlive = true;
    public float hpMax;
    public float hpCurrent;

    public ProgressBar HealthBar;

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

    public Animator animator;
    public CharacterController controller;

    public Transform attackPoint;
    public LayerMask enemyLayer;
    public int normalAttackDamage = 300;
    public RectTransform directionArrow;

    [System.Serializable]
    public class NormalAttackConfig
    {
        public GameObject prefab;
        public int attackRange = 300;
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

    private bool isNormalAttacking;
    private bool isSkillCasting;

    private Transform target;
    private Vector3 velocity;

    void Start()
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
    }

    const float INTERVAL = 1f / 60f;
    float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer < INTERVAL) return;
        timer -= INTERVAL;

        if (!isAlive)
        {
            // IMPORTANT: Không gọi SetAnimatorSpeed(0) (vì SetAnimatorSpeed giờ không gửi net nhưng vẫn OK).
            // Tuy nhiên giữ rõ ràng: chết thì chỉ set animator.
            animator.SetFloat("Speed", 0f);
            return;
        }

        animator.applyRootMotion = !(isNormalAttacking || isSkillCasting);

        if ((isNormalAttacking || isSkillCasting) && target != null)
        {
            RotateToTarget();
        }

        if (isNormalAttacking || isSkillCasting)
        {
            // Không gửi Stop ở đây nữa
            animator.SetFloat("Speed", 0f);
        }
        else
        {
            Move();
        }
    }

    #region NORMAL ATTACK
    public void NormalAttack()
    {
        if (!isAlive) return;

        if (_HienTamDanhThuong != null)
        {
            StopCoroutine(_HienTamDanhThuong);
            _HienTamDanhThuong = null;
        }
        _HienTamDanhThuong = HienTamDanhThuong();
        StartCoroutine(_HienTamDanhThuong);

        if (IsBusy()) return;

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

        Invoke(nameof(SpawnNormalAttackPrefab), normalAttackConfig.spawnDelay);
        Invoke(nameof(AutoResetNormalAttack), normalAttackConfig.duration);
    }

    private void SpawnNormalAttackPrefab()
    {
        if (normalAttackConfig.prefab == null) return;

        Vector3 spawnPos;
        Quaternion spawnRot;

        if (normalAttackConfig.spawnAtSelf)
        {
            spawnPos = controller.transform.position;
            spawnRot = controller.transform.rotation;

            GameObject attackObj = Instantiate(normalAttackConfig.prefab, spawnPos, spawnRot);

            if (normalAttackConfig.moveToTarget && target != null)
            {
                SkillMovement skillMovement = attackObj.AddComponent<SkillMovement>();
                skillMovement.targetPosition = target.position;
                skillMovement.moveSpeed = 10f;
            }
        }
        else if (normalAttackConfig.spawnAtTarget)
        {
            if (target != null)
            {
                spawnPos = target.position;
                spawnRot = Quaternion.LookRotation(target.position - controller.transform.position);
            }
            else
            {
                spawnPos = controller.transform.position + controller.transform.forward * normalAttackConfig.attackRange;
                spawnRot = controller.transform.rotation;
            }

            Instantiate(normalAttackConfig.prefab, spawnPos, spawnRot);
        }
        else
        {
            if (target != null)
            {
                spawnPos = target.position;
                spawnRot = Quaternion.LookRotation(target.position - controller.transform.position);
            }
            else
            {
                spawnPos = controller.transform.position + controller.transform.forward * normalAttackConfig.attackRange;
                spawnRot = controller.transform.rotation;
            }

            Instantiate(normalAttackConfig.prefab, spawnPos, spawnRot);
        }
    }

    private IEnumerator _HienTamDanhThuong;

    private IEnumerator HienTamDanhThuong()
    {
        if (spTamDanhThuong != null)
        {
            Vector2 newSize = spTamDanhThuong.size;
            newSize.x = normalAttackConfig.attackRange * 2;
            newSize.y = normalAttackConfig.attackRange * 2;
            spTamDanhThuong.size = newSize;
            spTamDanhThuong.enabled = true;
            yield return new WaitForSeconds(0.2f);
            spTamDanhThuong.enabled = false;
        }
        else
        {
            yield return null;
        }
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
    #endregion

    #region SKILLS
    public void CastSkill(int skill)
    {
        if (!isAlive) return;

        FindTargetInRange(normalAttackConfig.attackRange);
        if (IsBusy()) return;

        if (skill == 1) currentSkillCfg = skill1;
        else if (skill == 2) currentSkillCfg = skill2;
        else if (skill == 3) currentSkillCfg = skill3;
        else return;

        if (target != null)
            RotateToTarget();

        isSkillCasting = true;

        if (!string.IsNullOrEmpty(currentSkillCfg.animationBool))
        {
            // Không gửi Stop ở đây nữa
            animator.SetFloat("Speed", 0f);

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

    private void EndSkillAnimationWrapper()
    {
        isSkillCasting = false;

        if (currentSkillCfg != null && !string.IsNullOrEmpty(currentSkillCfg.animationBool))
        {
            animator.SetBool("isSkill1", false);
            animator.SetBool("isSkill2", false);
            animator.SetBool("isSkill3", false);
        }

        UpdateMovementAnimation();
    }

    private void SpawnSkillWithDamageDelay()
    {
        if (currentSkillCfg == null || currentSkillCfg.prefab == null) return;

        Vector3 spawnPos;
        Quaternion spawnRot;

        if (currentSkillCfg.spawnAtSelf)
        {
            spawnPos = controller.transform.position;
            spawnRot = controller.transform.rotation;

            GameObject skillObj = Instantiate(currentSkillCfg.prefab, spawnPos, spawnRot);

            if (currentSkillCfg.moveToTarget)
            {
                SkillMovement skillMovement = skillObj.AddComponent<SkillMovement>();
                skillMovement.targetPosition = target != null
                    ? target.position
                    : controller.transform.position + controller.transform.forward * normalAttackConfig.attackRange;
                skillMovement.moveSpeed = currentSkillCfg.projectileSpeed;
            }
        }
        else if (currentSkillCfg.spawnAtTarget)
        {
            if (target != null)
            {
                spawnPos = target.position;
                spawnRot = Quaternion.LookRotation(target.position - controller.transform.position);
            }
            else
            {
                spawnPos = controller.transform.position + controller.transform.forward * normalAttackConfig.attackRange;
                spawnRot = controller.transform.rotation;
            }

            Instantiate(currentSkillCfg.prefab, spawnPos, spawnRot);
        }
        else
        {
            if (currentSkillCfg == skill1)
            {
                spawnPos = controller.transform.position;
                spawnRot = controller.transform.rotation;

                GameObject skillObj = Instantiate(currentSkillCfg.prefab, spawnPos, spawnRot);

                SkillMovement skillMovement = skillObj.AddComponent<SkillMovement>();
                skillMovement.targetPosition = target != null
                    ? target.position
                    : controller.transform.position + controller.transform.forward * normalAttackConfig.attackRange;
                skillMovement.moveSpeed = currentSkillCfg.projectileSpeed;
            }
            else
            {
                if (target != null)
                {
                    spawnPos = target.position;
                    spawnRot = Quaternion.LookRotation(target.position - controller.transform.position);
                }
                else
                {
                    spawnPos = controller.transform.position + controller.transform.forward * normalAttackConfig.attackRange;
                    spawnRot = controller.transform.rotation;
                }

                Instantiate(currentSkillCfg.prefab, spawnPos, spawnRot);
            }
        }
    }

    private class SkillMovement : MonoBehaviour
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

    #region COMBAT LOGIC
    private void FindTargetInRange(float range)
    {
        if (controller == null) return;

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
        if (target == null || controller == null) return;

        Vector3 dir = target.position - controller.transform.position;
        dir.y = 0;

        if (dir.sqrMagnitude < 0.01f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        controller.transform.rotation = Quaternion.Slerp(controller.transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
    }
    #endregion

    #region MOVEMENT
    private void Move()
    {
        if (!isAlive) return;

        if (IsInCombatState())
        {
            HandleCombatMovement();
            return;
        }
        HandleNormalMovement();
    }

    public void SetPotion()
    {
        if (controller == null) return;

        controller.enabled = false;
        controller.transform.position = new Vector3(B.Instance.PosX, 0, B.Instance.PosZ);
        controller.enabled = true;
    }

    private void HandleCombatMovement()
    {
        if (controller == null) return;

        if (target != null)
            RotateToTarget();

        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;
        controller.Move(new Vector3(0, velocity.y, 0) * Time.deltaTime);
    }

    private void HandleNormalMovement()
    {
        if (controller == null) return;

        Vector2 input = MenuController.Instance.joystick.inputVector;
        Vector3 direction = new Vector3(input.x, 0, input.y);

        bool hasInput = direction.magnitude > 0.1f;

        SetAnimatorSpeed(hasInput ? 1f : 0f);

        if (hasInput)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            controller.transform.rotation = Quaternion.Lerp(controller.transform.rotation, Quaternion.Euler(0, targetAngle, 0), Time.deltaTime * turnSpeed);

            Vector3 moveDir = controller.transform.forward;
            controller.Move(moveDir * moveSpeed * Time.deltaTime);
        }

        if (controller.isGrounded)
        {
            if (velocity.y < 0)
                velocity.y = -2f;
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }

        controller.Move(Vector3.up * velocity.y * Time.deltaTime);

        if (GameStateManager.IsInGame())
        {
            if (Time.time - lastInputTime >= inputInterval)
            {
                lastInputTime = Time.time;

                if (hasInput)
                {
                    Vector2 normalized = input.normalized;
                    SendData.SendMovementInput(
                        Mathf.RoundToInt(normalized.x * 100),
                        Mathf.RoundToInt(normalized.y * 100),
                        true,
                        controller.transform.position
                    );

                    lastInput = input;

                    float angle = Mathf.Atan2(normalized.y, normalized.x) * Mathf.Rad2Deg;
                    directionArrow.rotation = Quaternion.Euler(0, 0, angle - 90);
                    directionArrow.gameObject.SetActive(true);
                }
                else if (lastInput.magnitude > 0.1f)
                {
                    // CHỈ gửi STOP 1 lần khi vừa thả joystick
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
    private bool IsBusy() => isNormalAttacking || isSkillCasting;
    private bool IsInCombatState() => isNormalAttacking || isSkillCasting;

    private void UpdateMovementAnimation()
    {
        if (IsBusy() || !isAlive) return;

        Vector2 input = MenuController.Instance.joystick.inputVector;
        float speedValue = input.magnitude > 0.1f ? 1f : 0f;
        SetAnimatorSpeed(speedValue);
    }

    private void SetAnimatorSpeed(float speed)
    {
        // IMPORTANT: Animator ONLY. Không gửi network ở đây.
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

    public void ApplyServerData(PlayerOutPutSv data)
    {
        isAlive = data.isAlive;

        // ✅ CMD50 sẽ đưa hp/maxHp = 0 => bỏ qua, KHÔNG overwrite HP
        if (data != null && data.maxHp > 0)
        {
            SetHp(data.hp, data.maxHp);
        }
    }

    public void SetHp(int hp, int maxHp)
    {
        hpMax = maxHp;
        hpCurrent = hp;

        if (!ShouldUseHealthBar())
            return;

        if (HealthBar == null)
            return;

        try
        {
            float ratio = hpMax <= 0 ? 0f : (hpCurrent / hpMax);
            if (hpCurrent < hpMax)
                HealthBar.SetProgress(ratio, 30);
            else
                HealthBar.SetProgress(1f, 100);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[PlayerMove] HealthBar SetProgress error: {e.Message}");
        }
    }

    private void ResetCombatStates()
    {
        CancelInvoke(nameof(AutoResetNormalAttack));
        CancelInvoke(nameof(SpawnNormalAttackPrefab));
        isNormalAttacking = false;
    }

    public void onDeath()
    {
        isAlive = false;
        ResetAllAnimatorStates();
        animator.SetBool("isDeath", true);

        CancelInvoke(nameof(AutoResetNormalAttack));
        CancelInvoke(nameof(SpawnNormalAttackPrefab));
        CancelInvoke(nameof(EndSkillAnimationWrapper));
        CancelInvoke(nameof(SpawnSkillWithDamageDelay));

        isNormalAttacking = false;
        isSkillCasting = false;

        SafeSetHealthBarActive(false);
    }

    public void onRespawn(int hp)
    {
        isAlive = true;
        animator.SetBool("isDeath", false);
        ResetAllAnimatorStates();
        SetPotion();

        // OPTIONAL (NÊN CÓ): sync vị trí respawn lên server 1 lần
        if (controller != null)
        {
            SendData.SendStop(controller.transform.position);
            lastInput = Vector2.zero;
        }

        SafeSetHealthBarActive(ShouldUseHealthBar());
        SetHp(hp, hp);
    }
    #endregion

    private bool ShouldUseHealthBar()
    {
        if (TranDauControl.Instance == null) return true;
        return TranDauControl.Instance;
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
            Debug.LogError($"[PlayerMove] HealthBar SetThanhMau error: {e.Message}");
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
            Debug.LogError($"[PlayerMove] HealthBar SetActive error: {e.Message}");
        }
    }
}
