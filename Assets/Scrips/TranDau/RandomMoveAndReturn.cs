using UnityEngine;

public class RandomMoveWithAnimator : MonoBehaviour
{
    public Animator animator;

    public float moveRadius = 5f;
    public float moveSpeed = 2f;
    public float waitTime = 2f;

    private Vector3 startPos;
    private Vector3 randomPos;
    private float timer;
    private bool isMovingToRandom = false;
    private bool isReturning = false;

    private void Start()
    {
        startPos = transform.position;
        SetIdle();
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (!isMovingToRandom && !isReturning)
        {
            if (timer >= waitTime)
                StartMoveRandom();
        }

        if (isMovingToRandom)
        {
            SetWalking();
            MoveTo(randomPos);

            if (Vector3.Distance(transform.position, randomPos) < 0.1f)
                StartReturn();
        }

        if (isReturning)
        {
            SetWalking();
            MoveTo(startPos);

            if (Vector3.Distance(transform.position, startPos) < 0.1f)
                SetIdle();
        }
    }

    void SetIdle()
    {
        timer = 0f;
        isMovingToRandom = false;
        isReturning = false;

        animator.SetBool("isWalking", false);
    }

    void StartMoveRandom()
    {
        timer = 0f;
        isMovingToRandom = true;

        Vector2 rand = Random.insideUnitCircle * moveRadius;
        randomPos = startPos + new Vector3(rand.x, 0, rand.y);
    }

    void StartReturn()
    {
        isMovingToRandom = false;
        isReturning = true;
    }

    void SetWalking()
    {
        animator.SetBool("isWalking", true);
    }

    void MoveTo(Vector3 target)
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            moveSpeed * Time.deltaTime
        );

        // xoay mặt theo hướng di chuyển
        Vector3 dir = target - transform.position;
        if (dir != Vector3.zero)
        {
            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                Quaternion.LookRotation(dir),
                Time.deltaTime * 8f
            );
        }
    }
}
