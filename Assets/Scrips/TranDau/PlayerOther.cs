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

    // Thêm biến để lưu target cho skill 3
    private Transform target;

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

        if (hasTarget)
        {
            RotateToTarget();
        }

        serverIsAttack = true;

        // dùng skillNormal
        currentSkillCfg = skillNormal;

        // bật animation nếu có
        if (!string.IsNullOrEmpty(currentSkillCfg.animationBool))
        {
            animator.SetBool(currentSkillCfg.animationBool, true);
            Invoke(nameof(EndSkillAnimationWrapper), currentSkillCfg.animationDuration);
        }

        // spawn prefab
        Invoke(nameof(SpawnSkillWrapper), currentSkillCfg.delaySpawn);

        // Reset attack state (nếu vẫn muốn)
        Invoke(nameof(EndNormalAttack), 1.5f);
    }

    private void RotateToTarget()
    {
        if (TranDauControl.Instance.playerMove == null) return;

        Vector3 dir = TranDauControl.Instance.playerMove.transform.position - transform.position;
        dir.y = 0;

        if (dir.sqrMagnitude > 0.01f)
        {
            Quaternion lookRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, 20f * Time.deltaTime);
        }
    }

    private void EndNormalAttack()
    {
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

        // Check walking by target distance
        bool isWalking = Vector3.Distance(transform.position, targetPos) > 0.05f;
        animator.SetBool("isWalking", isWalking);
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
            RotateToTarget();

            // Chỉ skill 3 spawn theo logic đặc biệt
            if (skill == 3)
            {
                Invoke(nameof(SpawnSkillWrapper3), currentSkillCfg.delaySpawn);
            }
            else
            {
                // Skill 1 và 2 spawn bình thường
                Invoke(nameof(SpawnSkillWrapper), currentSkillCfg.delaySpawn);
            }
        }
        else
        {
            // Không có target, spawn theo hướng hiện tại (giống PlayerMove khi không có target)
            target = null;
            if (skill == 3)
            {
                Invoke(nameof(SpawnSkillWrapper3), currentSkillCfg.delaySpawn);
            }
            else
            {
                Invoke(nameof(SpawnSkillWrapper), currentSkillCfg.delaySpawn);
            }
        }

        // Bật animation skill nếu có
        if (!string.IsNullOrEmpty(currentSkillCfg.animationBool))
        {
            animator.SetBool(currentSkillCfg.animationBool, true);
            Invoke(nameof(EndSkillAnimationWrapper), currentSkillCfg.animationDuration);
        }
    }

    private void EndSkillAnimationWrapper()
    {
        if (currentSkillCfg != null && !string.IsNullOrEmpty(currentSkillCfg.animationBool))
            animator.SetBool(currentSkillCfg.animationBool, false);
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

    public void onDeath()
    {
        animator.SetBool("isWalking", false);
        animator.SetBool("isDanhThuong", false);
        animator.SetBool("isTungChieu", false);
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

        // Nếu animation chiến đấu chạy >1.5s → coi như bị kẹt
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
        animator.SetBool("isDanhThuong", false);
        animator.SetBool("isTungChieu", false);
        animator.SetBool("isSkill2", false);
        animator.SetBool("isSkill3", false);

        animator.Play("Idle", 0, 0f);

        stuckTimer = 0f;
    }
    // ======================================================================
}