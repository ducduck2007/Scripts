using UnityEngine;

public class PlayerOther : MonoBehaviour
{
    public Animator animator;

    private Vector3 targetPos;
    private Quaternion targetRot;
    private bool isAlive;

    public float moveSmooth = 15f;
    public float rotateSmooth = 12f;
    public float movingThreshold = 0.01f;

    public LayerMask enemyLayer;

    public ProgressBar HealthBar;
    public Canvas Canvas;
    private bool serverIsAttack;

    public Transform parentSkill;

    // ====================== SKILL CONFIG ==========================
    [System.Serializable]
    public class SkillConfig
    {
        public GameObject prefab;
        public string animationBool = "";
        public float animationDuration = 1f;
        public float delaySpawn = 0.3f;
    }

    public SkillConfig skillNormal = new SkillConfig();   // <=== Thêm dòng này
    public SkillConfig skill1 = new SkillConfig();
    public SkillConfig skill2 = new SkillConfig();
    public SkillConfig skill3 = new SkillConfig();

    private SkillConfig currentSkillCfg; // skill đang cast
    // ===============================================================

    public float hpMax;
    public float hpCurrent;

    // Thêm các biến giống PlayerMove
    private bool isNormalAttacking;
    private bool isSkillCasting;
    private bool isHit;
    private Transform target;
    private Vector3 velocity;

    void Start()
    {
        if (HealthBar != null)
        {
            HealthBar.transform.SetParent(Canvas.transform);
            HealthBar.color = Color.red;
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

    public void ApplyServerData(PlayerOutPutSv data)
    {
        targetPos = new Vector3(data.x, transform.position.y, data.y);
        targetRot = Quaternion.Euler(0, data.heading, 0);
        isAlive = data.isAlive;
        SetHp(data.hp, data.maxHp);
    }

    public void SetAttackState(bool isAttack, bool hasTarget)
    {
        if (!isAttack) return;

        // Tìm target nếu có
        if (hasTarget && TranDauControl.Instance.playerMove != null)
        {
            target = TranDauControl.Instance.playerMove.transform;
        }
        else
        {
            target = null;
        }

        // Xoay về target nếu có
        if (target != null)
        {
            RotateToTarget();
        }

        // Dùng skillNormal
        currentSkillCfg = skillNormal;

        // Set trạng thái tấn công
        isNormalAttacking = true;
        serverIsAttack = true;

        // Bật animation nếu có
        if (!string.IsNullOrEmpty(currentSkillCfg.animationBool))
        {
            animator.SetBool("isAttack", true); // Giống PlayerMove
            Invoke(nameof(EndSkillAnimationWrapper), currentSkillCfg.animationDuration);
        }

        // Spawn prefab
        Invoke(nameof(SpawnSkillWrapper), currentSkillCfg.delaySpawn);

        // Auto reset sau duration
        Invoke(nameof(AutoResetNormalAttack), 1.2f); // Dùng duration giống PlayerMove
    }

    private void RotateToTarget()
    {
        if (target == null) return;

        Vector3 dir = target.position - transform.position;
        dir.y = 0;

        if (dir.sqrMagnitude > 0.01f)
        {
            Quaternion lookRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, rotateSmooth * Time.deltaTime);
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
        serverIsAttack = false;
    }

    public void SetPotion(Vector3 pos)
    {
        transform.position = pos;
    }

    private void Update()
    {
        // Smooth movement
        DetectAnimatorStuck();
        transform.position = Vector3.Lerp(transform.position, targetPos, moveSmooth * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotateSmooth * Time.deltaTime);

        // Check movement speed giống PlayerMove
        Vector3 moveVector = (targetPos - transform.position);
        moveVector.y = 0;
        float speedValue = moveVector.magnitude > 0.05f ? 1f : 0f;

        // Chỉ set speed khi không busy (giống PlayerMove)
        if (!IsBusy())
        {
            SetAnimatorSpeed(speedValue);
        }
        else
        {
            SetAnimatorSpeed(0f); // Dừng di chuyển khi đang trong trạng thái bận
        }
    }

    public void CastSkillFromServer(int skill, bool hasTarget)
    {
        if (skill == 1) currentSkillCfg = skill1;
        else if (skill == 2) currentSkillCfg = skill2;
        else if (skill == 3) currentSkillCfg = skill3;
        else return;

        // Tìm target nếu có (giống PlayerMove)
        if (hasTarget && TranDauControl.Instance.playerMove != null)
        {
            target = TranDauControl.Instance.playerMove.transform;
        }
        else
        {
            target = null;
        }

        // Xoay về target nếu có
        if (target != null)
        {
            RotateToTarget();
        }

        // Set trạng thái skill
        isSkillCasting = true;

        // Bật animation skill nếu có
        if (!string.IsNullOrEmpty(currentSkillCfg.animationBool))
        {
            // Tắt trạng thái di chuyển
            SetAnimatorSpeed(0f);

            // Set bool cho skill tương ứng giống PlayerMove
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

        // Spawn skill
        if (skill == 3)
        {
            Invoke(nameof(SpawnSkillWrapper3), currentSkillCfg.delaySpawn);
        }
        else
        {
            Invoke(nameof(SpawnSkillWrapper), currentSkillCfg.delaySpawn);
        }
    }

    private void EndSkillAnimationWrapper()
    {
        // Reset trạng thái skill
        isSkillCasting = false;

        if (currentSkillCfg != null && !string.IsNullOrEmpty(currentSkillCfg.animationBool))
        {
            // Reset tất cả bool skill giống PlayerMove
            animator.SetBool("isAttack", false);
            animator.SetBool("isSkill1", false);
            animator.SetBool("isSkill2", false);
            animator.SetBool("isSkill3", false);
        }
    }

    private void SpawnSkillWrapper()
    {
        if (currentSkillCfg != null && currentSkillCfg.prefab != null)
        {
            Instantiate(currentSkillCfg.prefab, parentSkill.position, parentSkill.rotation);
        }
    }

    // Thêm hàm mới cho skill 3 với logic giống PlayerMove
    private void SpawnSkillWrapper3()
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
            // Spawn vào vị trí mục tiêu (player chính)
            spawnPos = target.position;
            spawnRot = Quaternion.LookRotation(target.position - transform.position);
        }
        else
        {
            // Không có target → spawn về phía trước mặt 1 khoảng
            // Giả sử tầm đánh là 3 (giống PlayerMove)
            int attackRange = 3;
            spawnPos = transform.position + transform.forward * attackRange;
            spawnRot = transform.rotation;
        }

        Instantiate(currentSkillCfg.prefab, spawnPos, spawnRot);

        Debug.Log("Skill 3 spawned at: " + spawnPos);
    }

    // Hàm Set Animator Speed giống PlayerMove
    private void SetAnimatorSpeed(float speed)
    {
        animator.SetFloat("Speed", speed);
    }

    // Hàm kiểm tra trạng thái bận giống PlayerMove
    private bool IsBusy() => isNormalAttacking || isSkillCasting || isHit;

    public void onDeath()
    {
        // Reset tất cả trạng thái giống PlayerMove
        SetAnimatorSpeed(0f);
        animator.SetBool("isAttack", false);
        animator.SetBool("isSkill1", false);
        animator.SetBool("isSkill2", false);
        animator.SetBool("isSkill3", false);
        animator.SetBool("isDeath", true);

        if (HealthBar != null)
        {
            HealthBar.gameObject.SetActive(false);
        }
    }

    public void onRespawn(float x, float y, int hp)
    {
        animator.SetBool("isDeath", false);
        SetPotion(new Vector3(x, 200, y));
        if (HealthBar != null)
        {
            HealthBar.gameObject.SetActive(true);
        }
        SetHp(hp, hp);
    }

    private float stuckTimer = 0f;
    private string lastStateName = "";

    void DetectAnimatorStuck()
    {
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        string stateName = GetCurrentStateName(info);

        // Nếu đổi trạng thái → reset timer
        if (stateName != lastStateName)
        {
            lastStateName = stateName;
            stuckTimer = 0f;
            return;
        }

        stuckTimer += Time.deltaTime;

        // Nếu đang idle hoặc walking → không bao giờ reset
        if (stateName == "Idle" || stateName == "Walking")
            return;

        // Nếu animation chiến đấu chạy >5s → coi như bị kẹt
        if (stuckTimer > 5f)
        {
            ForceResetAnimator();
        }
    }

    string GetCurrentStateName(AnimatorStateInfo info)
    {
        int hash = info.shortNameHash;

        return hash.ToString(); // fallback
    }

    void ForceResetAnimator()
    {
        // Reset tất cả trạng thái giống PlayerMove
        SetAnimatorSpeed(0f);
        animator.SetBool("isAttack", false);
        animator.SetBool("isSkill1", false);
        animator.SetBool("isSkill2", false);
        animator.SetBool("isSkill3", false);

        animator.Play("Idle", 0, 0f);

        // Reset các biến trạng thái
        isNormalAttacking = false;
        isSkillCasting = false;
        isHit = false;

        stuckTimer = 0f;
    }
    // ======================================================================
}