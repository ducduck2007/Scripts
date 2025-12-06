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
    public LayerMask playerLayer;

    private Transform target;
    private float lastAttackTime = -999f;

    // ======= Bị đánh =======
    private bool isHit = false;
    public float hitDuration = 0.5f;
    private float hitEndTime;

    [Header("Hit Effect")]
    public GameObject hitEffectPrefab;
    public Transform hitEffectPoint;

    // ======= Gravity =======
    [Header("Gravity Settings")]
    public float gravity = -20f;
    private Vector3 velocity;
    public CharacterController controller;

    void Start()
    {
        currentHealth = maxHealth;

        if (animator == null)
            animator = GetComponent<Animator>();

        if (controller == null)
            controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (isDead) return;

        // Gravity luôn chạy
        ApplyGravity();

        // Check end hit
        if (isHit && Time.time >= hitEndTime)
            EndHit();

        if (isHit) return; // bị đánh thì không tấn công

        FindPlayer();

        if (target != null)
        {
            RotateToTarget();
            AutoAttack();
        }
        else
        {
            animator.SetBool("isAttack", false);
        }
    }

    void ApplyGravity()
    {
        if (controller == null) return;

        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }

    void FindPlayer()
    {
        // Đảm bảo tầm tìm player đúng
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange, playerLayer);

        if (hits.Length > 0)
        {
            // Tìm player gần nhất
            float minDist = Mathf.Infinity;
            Transform nearestPlayer = null;

            foreach (var hit in hits)
            {
                float dist = Vector3.Distance(transform.position, hit.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
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

    void AutoAttack()
    {
        if (target == null || isDead) return;

        // Kiểm tra khoảng cách từ bot đến player
        float distance = Vector3.Distance(transform.position, target.position);
        if (distance > attackRange)
        {
            target = null;
            return;
        }

        if (Time.time - lastAttackTime >= attackCooldown)
        {
            lastAttackTime = Time.time;

            animator.SetBool("isAttack", true);
            StartCoroutine(StopAttackAnim());

            // Gây damage cho player
            PlayerMove player = target.GetComponent<PlayerMove>();
            if (player != null)
            {
                // Ở đây bạn cần có method TakeDamage() trong PlayerMove
                // player.TakeDamage(attackDamage);
                Debug.Log("Bot đánh Player!");
            }
        }
    }

    IEnumerator StopAttackAnim()
    {
        yield return new WaitForSeconds(0.5f);
        animator.SetBool("isAttack", false);
    }

    public void TakeDamage(int damage = 1)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log($"Bot bị đánh! Máu còn {currentHealth}");

        if (!isHit)
        {
            animator.SetTrigger("isBiDanh");

            if (hitEffectPrefab != null)
            {
                GameObject effect = Instantiate(
                    hitEffectPrefab,
                    hitEffectPoint != null ? hitEffectPoint.position : transform.position,
                    Quaternion.identity
                );

                Destroy(effect, 1.5f);
            }

            isHit = true;
            hitEndTime = Time.time + hitDuration;
        }

        if (currentHealth <= 0)
            Die();
    }

    void EndHit()
    {
        isHit = false;
        animator.ResetTrigger("isBiDanh");
    }

    void Die()
    {
        isDead = true;
        animator.SetBool("isDeath", true);

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Destroy(gameObject, 3f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
