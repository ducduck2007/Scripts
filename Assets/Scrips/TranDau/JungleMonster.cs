using UnityEngine;

public class JungleMonster : MonoBehaviour
{
    public int id;
    public Animator animator;

    public float moveSmooth = 10f;
    public float rotateSmooth = 12f;
    public float deadRespawnTime = 60f;

    private int currentHP;
    private int maxHP;
    private bool isDead = false;

    private Vector3 targetPos;
    private Vector3 lastPos;

    private float respawnTimer = 0f;

    private void Start()
    {
        targetPos = transform.position;
        lastPos = transform.position;
        animator = gameObject.GetComponent<Animator>();
    }

    private void Update()
    {
        if (isDead)
        {
            HandleRespawn();
            return;
        }

        // Move
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * moveSmooth);

        // Rotate
        RotateToMoveDirection();

        // Update animation
        float speed = (transform.position - lastPos).magnitude / Time.deltaTime;
        animator.SetBool("isWalking", speed > 0.05f);

        lastPos = transform.position;
    }

    // ===============================
    // SERVER UPDATE
    // ===============================
    public void UpdateFromServer(float x, float y, int hp, int hpMax)
    {
        maxHP = hpMax;
        currentHP = hp;

        targetPos = new Vector3(x, transform.position.y, y);

        

        if (isDead && currentHP > 0)
            ForceRespawn();
    }

    // ===============================
    // ROTATION
    // ===============================
    void RotateToMoveDirection()
    {
        Vector3 dir = targetPos - transform.position;
        dir.y = 0;

        if (dir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime * rotateSmooth);
        }
    }

    // ===============================
    // DEATH
    // ===============================
    public void Die()
    {
        isDead = true;
        animator.SetBool("isWalking", false);
        animator.SetTrigger("Die");
        respawnTimer = deadRespawnTime;

        GetComponent<Collider>().enabled = false;
    }

    void HandleRespawn()
    {
        respawnTimer -= Time.deltaTime;

        if (respawnTimer <= 0)
            Respawn();
    }

    void Respawn()
    {
        isDead = false;

        animator.ResetTrigger("Die");
        animator.Play("Idle", 0);

        GetComponent<Collider>().enabled = true;

        // HP sẽ được server gửi sau
    }

    // Trường hợp server gửi HP > 0 trong khi client đang Dead (sync sai)
    void ForceRespawn()
    {
        isDead = false;

        animator.ResetTrigger("Die");
        animator.Play("Idle", 0);

        GetComponent<Collider>().enabled = true;
    }
}
