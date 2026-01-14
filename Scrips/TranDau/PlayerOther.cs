using UnityEngine;
using System.Collections;

public class PlayerOther : MonoBehaviour
{
    public Animator animator;

    public int teamId = 0; // Mặc định là 0

    private Vector3 targetPos;
    private Quaternion targetRot;
    private bool isAlive = true; // Thêm khởi tạo mặc định

    public float moveSmooth = 15f;
    public float rotateSmooth = 12f;
    public float movingThreshold = 0.01f;

    public LayerMask enemyLayer;

    public ProgressBar HealthBar;
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
        public float damageDelay = 0.5f;
        public float projectileSpeed = 10f;
        
        // Thêm các checkbox để phân biệt vị trí spawn
        public bool spawnAtSelf = false;        // Sinh ở vị trí nhân vật mình
        public bool spawnAtTarget = false;      // Sinh ở vị trí mục tiêu trong tầm đánh
        public bool moveToTarget = false;       // Prefab sinh ở nhân vật mình rồi di chuyển qua mục tiêu
    }

    public SkillConfig skill1 = new SkillConfig();
    public SkillConfig skill2 = new SkillConfig();
    public SkillConfig skill3 = new SkillConfig();

    private SkillConfig currentSkillCfg;
    // ===============================================================

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
        public float spawnDelay = 0.2f; // Delay sinh prefab
        public string animationBool = "isAttack";
        
        // Thêm các checkbox cho đánh thường
        public bool spawnAtSelf = false;        // Sinh ở vị trí nhân vật mình
        public bool spawnAtTarget = false;      // Sinh ở vị trí mục tiêu trong tầm đánh
        public bool moveToTarget = false;       // Prefab sinh ở nhân vật mình rồi di chuyển qua mục tiêu
    }

    public NormalAttackConfig normalAttackConfig = new NormalAttackConfig();

    void Start()
    {
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
        
        // Khởi tạo trạng thái sống
        isAlive = true;
    }
    
    public void SetTeamId(int teamId)
    {
        this.teamId = teamId;
    }

    public void SetHp(int hp, int maxHp)
    {
        hpMax = maxHp;
        hpCurrent = hp;
        if (HealthBar != null)
        {
            if (hpCurrent < hpMax)
            {
                HealthBar.SetProgress((float)(hpCurrent / hpMax), 30);
            }
            else
            {
                HealthBar.SetProgress(1f, 100);
            }
        }
    }

    public void ApplyServerData(PlayerOutPutSv data)
    {
        targetPos = new Vector3(data.x/2, transform.position.y, data.y/2);
        targetRot = Quaternion.Euler(0, data.heading, 0);
        isAlive = data.isAlive;
        SetHp(data.hp, data.maxHp);

        // THÊM SET TEAM ID TỪ SERVER NẾU CÓ
        // if (data.teamId != 0) SetTeamId(data.teamId);
    }

    public void SetAttackState(bool isAttack, bool hasTarget)
    {
        // Kiểm tra nếu nhân vật đã chết thì không thực hiện tấn công
        if (!isAlive) return;
        
        if (!isAttack) return;

        // Tìm target trong tầm đánh
        FindTargetInRange(normalAttackConfig.attackRange);

        // Xoay về target nếu có
        if (target != null)
        {
            RotateToTargetInstant(); // Xoay tức thời khi bắt đầu đánh
        }
        else
        {
            // Nếu không có target trong tầm, vẫn quay về hướng server gửi
            transform.rotation = targetRot;
        }

        // Set trạng thái tấn công
        isNormalAttacking = true;
        serverIsAttack = true;

        // Bật animation đánh thường
        animator.SetBool("isAttack", true);

        // Sinh prefab cho đánh thường
        Invoke(nameof(SpawnNormalAttackPrefab), normalAttackConfig.spawnDelay);

        // Auto reset sau duration
        Invoke(nameof(AutoResetNormalAttack), normalAttackConfig.duration);
    }

    // Xoay tức thời khi bắt đầu đánh/skill
    private void RotateToTargetInstant()
    {
        if (target == null) return;

        Vector3 dir = target.position - transform.position;
        dir.y = 0;

        if (dir.sqrMagnitude < 0.01f) return;

        // QUAY TỨC THỜI VỀ HƯỚNG TARGET khi bắt đầu đánh/skill
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
    }

    void Update()
    {
        // Kiểm tra nếu nhân vật đã chết thì không thực hiện bất kỳ hành động nào
        if (!isAlive)
        {
            SetAnimatorSpeed(0f);
            return;
        }
        
        // Smooth movement
        DetectAnimatorStuck();
        transform.position = Vector3.Lerp(transform.position, targetPos, moveSmooth * Time.deltaTime);

        // Chỉ smooth rotation theo server khi không busy
        if (!IsBusy())
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotateSmooth * Time.deltaTime);
        }

        // Check movement speed
        Vector3 moveVector = (targetPos - transform.position);
        moveVector.y = 0;
        float speedValue = moveVector.magnitude > 0.05f ? 1f : 0f;

        // Chỉ set speed khi không busy
        if (!IsBusy())
        {
            SetAnimatorSpeed(speedValue);
        }
        else
        {
            SetAnimatorSpeed(0f); // Dừng di chuyển khi đang trong trạng thái bận
        }
    }

    // SỬA LẠI HÀM FindTargetInRange ĐỂ TÌM ĐÚNG MỤC TIÊU
    private void FindTargetInRange(float range)
    {
        // Tìm tất cả PlayerMove và PlayerOther trong scene
        PlayerMove[] allPlayerMoves = FindObjectsOfType<PlayerMove>();
        PlayerOther[] allPlayerOthers = FindObjectsOfType<PlayerOther>();

        target = null;
        float minDist = Mathf.Infinity;

        // Kiểm tra PlayerMove trước
        foreach (var playerMove in allPlayerMoves)
        {
            // Nếu teamId đã được set, kiểm tra team khác nhau
            if (teamId != 0 && B.Instance.teamId == teamId)
                continue; // Bỏ qua đồng đội

            float distance = Vector3.Distance(transform.position, playerMove.transform.position);
            if (distance <= range && distance < minDist)
            {
                minDist = distance;
                target = playerMove.transform;
            }
        }

        // Kiểm tra PlayerOther
        foreach (var playerOther in allPlayerOthers)
        {
            // Bỏ qua chính mình
            if (playerOther == this) continue;

            // Kiểm tra team khác nhau nếu có teamId
            if (teamId != 0 && playerOther.teamId == teamId)
                continue; // Bỏ qua đồng đội

            float distance = Vector3.Distance(transform.position, playerOther.transform.position);
            if (distance <= range && distance < minDist)
            {
                minDist = distance;
                target = playerOther.transform;
            }
        }

        // Fallback: nếu không tìm thấy, thử dùng Physics.OverlapSphere
        if (target == null)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, range, enemyLayer);

            foreach (var hit in hits)
            {
                // Bỏ qua chính mình
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

    // HÀM LẤY TEAM ID TỪ PlayerMove (nếu có)

    public void CastSkillFromServer(int skill, bool hasTarget)
    {
        // Kiểm tra nếu nhân vật đã chết thì không thực hiện skill
        if (!isAlive) return;
        
        if (skill == 1) currentSkillCfg = skill1;
        else if (skill == 2) currentSkillCfg = skill2;
        else if (skill == 3) currentSkillCfg = skill3;
        else return;

        // Tìm target trong tầm skill
        float skillRange = normalAttackConfig.attackRange;
        FindTargetInRange(skillRange);

        // Xoay về target nếu có - XOAY TỨC THỜI khi bắt đầu skill
        if (target != null)
        {
            RotateToTargetInstant();
        }
        else
        {
            // Nếu không có target, quay về hướng server gửi
            transform.rotation = targetRot;
        }

        // Set trạng thái skill
        isSkillCasting = true;

        // Bật animation skill nếu có
        if (!string.IsNullOrEmpty(currentSkillCfg.animationBool))
        {
            // Tắt trạng thái di chuyển
            SetAnimatorSpeed(0f);

            // Set bool cho skill tương ứng
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

        // Spawn skill với damage delay
        Invoke(nameof(SpawnSkillWithDamageDelay), currentSkillCfg.delaySpawn);
    }

    private void EndSkillAnimationWrapper()
    {
        // Reset trạng thái skill
        isSkillCasting = false;

        if (currentSkillCfg != null && !string.IsNullOrEmpty(currentSkillCfg.animationBool))
        {
            // Reset tất cả bool skill
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

        // Kiểm tra các checkbox để xác định vị trí spawn
        if (normalAttackConfig.spawnAtSelf)
        {
            // Sinh ở vị trí nhân vật mình
            spawnPos = transform.position;
            spawnRot = transform.rotation;
            
            GameObject attackObj = Instantiate(normalAttackConfig.prefab, spawnPos, spawnRot);
            
            // Nếu cần di chuyển đến mục tiêu
            if (normalAttackConfig.moveToTarget && target != null)
            {
                PlayerOtherSkillMovement skillMovement = attackObj.AddComponent<PlayerOtherSkillMovement>();
                skillMovement.targetPosition = target.position;
                skillMovement.moveSpeed = 1200f; // Tốc độ cho đánh thường
            }
        }
        else if (normalAttackConfig.spawnAtTarget)
        {
            // Sinh ở vị trí mục tiêu
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
            // Mặc định: spawn ở vị trí mục tiêu (giữ nguyên logic cũ)
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
        
        Debug.Log("Normal attack prefab spawned at: " + spawnPos);
    }

    private void SpawnSkillWithDamageDelay()
    {
        if (currentSkillCfg == null || currentSkillCfg.prefab == null) return;

        Vector3 spawnPos;
        Quaternion spawnRot;

        // Kiểm tra các checkbox để xác định vị trí spawn
        if (currentSkillCfg.spawnAtSelf)
        {
            // Sinh ở vị trí nhân vật mình
            spawnPos = transform.position;
            spawnRot = transform.rotation;
            
            GameObject skillObj = Instantiate(currentSkillCfg.prefab, spawnPos, spawnRot);
            
            // Nếu cần di chuyển đến mục tiêu
            if (currentSkillCfg.moveToTarget)
            {
                PlayerOtherSkillMovement skillMovement = skillObj.AddComponent<PlayerOtherSkillMovement>();
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
            // Sinh ở vị trí mục tiêu
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
            // Mặc định: giữ nguyên logic cũ cho skill1
            if (currentSkillCfg == skill1) // Skill 1 đặc biệt
            {
                spawnPos = transform.position;
                spawnRot = transform.rotation;

                GameObject skillObj = Instantiate(currentSkillCfg.prefab, spawnPos, spawnRot);

                PlayerOtherSkillMovement skillMovement = skillObj.AddComponent<PlayerOtherSkillMovement>();
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
            else // Skill 2, 3 và normal attack
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

        Debug.Log("Skill spawned at: " + spawnPos);
    }

    // Class hỗ trợ di chuyển cho skill (dành riêng cho PlayerOther)
    private class PlayerOtherSkillMovement : MonoBehaviour
    {
        public Vector3 targetPosition;
        public float moveSpeed = 10f;

        void Update()
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            // Hủy khi đến đích
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                Destroy(gameObject);
            }
        }
    }

    // Hàm Set Animator Speed
    private void SetAnimatorSpeed(float speed)
    {
        animator.SetFloat("Speed", speed);
    }

    // Hàm kiểm tra trạng thái bận
    private bool IsBusy() => isNormalAttacking || isSkillCasting || isHit;

    public void onDeath()
    {
        // Đánh dấu nhân vật đã chết
        isAlive = false;
        
        // Reset tất cả trạng thái
        SetAnimatorSpeed(0f);
        animator.SetBool("isAttack", false);
        animator.SetBool("isSkill1", false);
        animator.SetBool("isSkill2", false);
        animator.SetBool("isSkill3", false);
        animator.SetBool("isDeath", true);
        
        // Hủy bỏ tất cả các Invoke đang chạy
        CancelInvoke(nameof(AutoResetNormalAttack));
        CancelInvoke(nameof(SpawnNormalAttackPrefab));
        CancelInvoke(nameof(EndSkillAnimationWrapper));
        CancelInvoke(nameof(SpawnSkillWithDamageDelay));
        
        // Reset các trạng thái tấn công
        isNormalAttacking = false;
        isSkillCasting = false;
        isHit = false;
        serverIsAttack = false;

        if (HealthBar != null)
        {
            HealthBar.gameObject.SetActive(false);
        }
    }

    public void onRespawn(float x, float y, int hp)
    {
        // Đánh dấu nhân vật sống lại
        isAlive = true;
        
        animator.SetBool("isDeath", false);
        SetPotion(new Vector3(x, 0, y));
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
        // Kiểm tra nếu nhân vật đã chết thì không kiểm tra stuck
        if (!isAlive) return;
        
        // Kiểm tra nếu animator không hợp lệ
        if (animator == null || !animator.isInitialized) return;

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
        // Sử dụng hash thay vì tên trực tiếp để tránh lỗi
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
        // Reset tất cả trạng thái
        SetAnimatorSpeed(0f);
        animator.SetBool("isAttack", false);
        animator.SetBool("isSkill1", false);
        animator.SetBool("isSkill2", false);
        animator.SetBool("isSkill3", false);
        animator.SetBool("isDeath", false);
        animator.SetBool("isHit", false);

        // Reset các biến trạng thái
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
    }
}