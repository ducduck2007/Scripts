using UnityEngine;
using UnityEngine.UI;

public class MinionMove : MonoBehaviour
{
    public long minionId;
    public int teamId;
    public int laneId;
    public Image imgFill;
    public Sprite[] sprMau;
    private Vector3 targetPos;
    private bool isAlive = true;

    public float moveSmooth = 10f;  // Tốc độ lerp
    public float stopThreshold = 10f; // ngưỡng coi như đứng yên

    public Animator aniLinhDo, aniLinhXanh;

    private Animator aniLinh;

    [SerializeField] float rotateSpeed = 10f;
    void Update()
    {
        if (!isAlive) return;

        // if (Time.frameCount % 2 != 0) return; // 30 FPS logic

        Vector3 currentPos = transform.position;
        Vector3 dir = targetPos - currentPos;
        dir.y = 0f; // tránh nghiêng lên/xuống

        // Di chuyển mượt
        transform.position = Vector3.Lerp(currentPos, targetPos, moveSmooth * Time.deltaTime);

        float sqrDist = dir.sqrMagnitude;
        float stopSqr = stopThreshold * stopThreshold;

        if (sqrDist > stopSqr)
        {
            // ĐANG CHẠY
            SetAnimatorSpeed(1f);
            SetAnimatorAttack(false);

            // XOAY THEO HƯỚNG DI CHUYỂN
            if (dir.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRot,
                    rotateSpeed * Time.deltaTime
                );
                // transform.rotation = Quaternion.MoveTowards(
                //     transform.rotation,
                //     targetRot,
                //     rotateSpeed * Time.deltaTime
                // );
            }
        }
        else
        {
            // ĐỨNG → ATTACK
            SetAnimatorSpeed(0f);
            SetAnimatorAttack(true);
        }
    }

    public void SetData(long id, int team, int lane)
    {
        minionId = id;
        teamId = team;
        laneId = lane;
        if (teamId == 1)
        {
            aniLinh = aniLinhXanh;
            Destroy(aniLinhDo.gameObject);

            int layer1 = LayerMask.NameToLayer("player1");
            if (gameObject.layer != layer1)
            {
                gameObject.layer = layer1;
            }
        }
        else
        {
            aniLinh = aniLinhDo;
            Destroy(aniLinhXanh.gameObject);

            int layer2 = LayerMask.NameToLayer("player2");
            if (gameObject.layer != layer2)
            {
                gameObject.layer = layer2;
            }
        }
        if (teamId == B.Instance.teamId)
        {
            imgFill.sprite = sprMau[0];
        }
        else
        {
            imgFill.sprite = sprMau[1];
        }
    }
    float lastHpPercent = -1f;

    public void UpdateFromServer(float x, float y, int hp, int maxHp)
    {
        // Update position từ server
        targetPos = new Vector3(x, 0, y);
        float percent = Mathf.Clamp01((float)hp / maxHp);
        if (Mathf.Abs(lastHpPercent - percent) > 0.01f)
        {
            lastHpPercent = percent;
            imgFill.fillAmount = percent;
        }
        // Check alive
        if (hp <= 0 && isAlive)
        {
            OnDeath();
        }
    }

    float lastSpeed = -1f;
    bool lastAttack = false;

    void SetAnimatorSpeed(float speed)
    {
        if (Mathf.Abs(lastSpeed - speed) < 0.01f) return;
        lastSpeed = speed;
        aniLinh.SetFloat("Speed", speed);
    }

    void SetAnimatorAttack(bool val)
    {
        if (lastAttack == val) return;
        lastAttack = val;
        aniLinh.SetBool("isAttack", val);
    }

    public void OnDeath()
    {
        isAlive = false;

        // Ẩn minion khi chết
        Destroy(gameObject);
    }
}