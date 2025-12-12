using UnityEngine;

public class PlayerAnimationTester : MonoBehaviour
{
    public Animator animator;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotateSpeed = 360f;

    [Header("Normal Attack")]
    public GameObject normalAttackPrefab;
    public float normalAttackDuration = 0.8f;
    public float normalAttackDelay = 0.2f;

    [Header("Skill 1")]
    public GameObject skill1Prefab;
    public float skill1Duration = 1.2f;
    public float skill1Delay = 0.2f;

    [Header("Skill 2")]
    public GameObject skill2Prefab;
    public float skill2Duration = 1.2f;
    public float skill2Delay = 0.2f;

    [Header("Skill 3")]
    public GameObject skill3Prefab;
    public float skill3Duration = 1.2f;
    public float skill3Delay = 0.2f;

    [Header("Spawn point")]
    public Transform spawnPoint;

    private void Update()
    {
        HandleMovement();
        HandleAnimationInput();
    }

    private void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 move = new Vector3(h, 0, v);

        bool isWalking = move.sqrMagnitude > 0.01f;
        animator.SetBool("isWalking", isWalking);

        if (isWalking)
        {
            transform.position += move.normalized * moveSpeed * Time.deltaTime;

            Quaternion targetRot = Quaternion.LookRotation(move.normalized);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
        }
    }

    private void HandleAnimationInput()
    {
        // Normal Attack
        if (Input.GetKeyDown(KeyCode.J))
        {
            PlayTimedAnimation("isDanhThuong", normalAttackDuration);

            if (normalAttackPrefab)
                Invoke(nameof(SpawnNormalAttack), normalAttackDelay);
        }

        // Skill 1
        if (Input.GetKeyDown(KeyCode.K))
        {
            PlayTimedAnimation("isTungChieu", skill1Duration);

            if (skill1Prefab)
                Invoke(nameof(SpawnSkill1), skill1Delay);
        }

        // Skill 2
        if (Input.GetKeyDown(KeyCode.L))
        {
            PlayTimedAnimation("isSkill2", skill2Duration);

            if (skill2Prefab)
                Invoke(nameof(SpawnSkill2), skill2Delay);
        }

        // Skill 3
        if (Input.GetKeyDown(KeyCode.Semicolon))
        {
            PlayTimedAnimation("isSkill3", skill3Duration);

            if (skill3Prefab)
                Invoke(nameof(SpawnSkill3), skill3Delay);
        }

        // Death test
        if (Input.GetKeyDown(KeyCode.Y))
            animator.SetBool("isDeath", true);

        if (Input.GetKeyDown(KeyCode.U))
            animator.SetBool("isDeath", false);
    }

    private void PlayTimedAnimation(string animBool, float time)
    {
        animator.SetBool(animBool, true);
        animToEnd = animBool;
        Invoke(nameof(EndAnimWrapper), time);
    }

    private string animToEnd;
    private void EndAnimWrapper()
    {
        if (!string.IsNullOrEmpty(animToEnd))
            animator.SetBool(animToEnd, false);
    }

    private void SpawnNormalAttack()
    {
        Instantiate(normalAttackPrefab, spawnPoint.position, spawnPoint.rotation);
    }

    private void SpawnSkill1()
    {
        Instantiate(skill1Prefab, spawnPoint.position, spawnPoint.rotation);
    }

    private void SpawnSkill2()
    {
        Instantiate(skill2Prefab, spawnPoint.position, spawnPoint.rotation);
    }

    private void SpawnSkill3()
    {
        Instantiate(skill3Prefab, spawnPoint.position, spawnPoint.rotation);
    }
}
