// File: MinionMove.cs
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

    public float moveSmooth = 10f;     // Tốc độ lerp
    public float stopThreshold = 10f;  // ngưỡng coi như đứng yên

    public Animator aniLinhDo, aniLinhXanh;
    private Animator aniLinh;

    [SerializeField] float rotateSpeed = 10f;

    void Update()
    {
        if (!isAlive) return;
        if (aniLinh == null) return; // tránh NullRef nếu prefab thiếu animator ref

        Vector3 currentPos = transform.position;
        Vector3 dir = targetPos - currentPos;
        dir.y = 0f;

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
            if (sqrDist > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
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

        // ✅ IMPORTANT: Không Destroy animator object (dễ destroy nhầm root => minion biến mất)
        if (teamId == 1)
        {
            aniLinh = aniLinhXanh;

            if (aniLinhDo != null) aniLinhDo.gameObject.SetActive(false);
            if (aniLinhXanh != null) aniLinhXanh.gameObject.SetActive(true);

            int layer1 = LayerMask.NameToLayer("player1");
            if (layer1 != -1 && gameObject.layer != layer1)
                gameObject.layer = layer1;
        }
        else
        {
            aniLinh = aniLinhDo;

            if (aniLinhXanh != null) aniLinhXanh.gameObject.SetActive(false);
            if (aniLinhDo != null) aniLinhDo.gameObject.SetActive(true);

            int layer2 = LayerMask.NameToLayer("player2");
            if (layer2 != -1 && gameObject.layer != layer2)
                gameObject.layer = layer2;
        }

        // Nếu prefab thiếu ref animator → log để biết
        if (aniLinh == null)
        {
            // Debug.LogWarning($"[MinionMove] Animator ref missing on minionId={minionId}. Check aniLinhDo/aniLinhXanh assign in prefab.");
        }

        // UI máu
        if (imgFill != null && sprMau != null && sprMau.Length >= 2)
        {
            imgFill.sprite = (teamId == B.Instance.teamId) ? sprMau[0] : sprMau[1];
        }
    }

    float lastHpPercent = -1f;

    public void UpdateFromServer(float x, float y, int hp, int maxHp)
    {
        // Update position từ server
        targetPos = new Vector3(x, 0, y);

        // Nếu server chưa gửi hp/maxHp (0/0) => bỏ qua update HP và bỏ qua chết
        if (maxHp > 0)
        {
            float percent = Mathf.Clamp01((float)hp / maxHp);
            if (Mathf.Abs(lastHpPercent - percent) > 0.01f)
            {
                lastHpPercent = percent;
                imgFill.fillAmount = percent;
            }

            // Chỉ chết nếu maxHp hợp lệ
            if (hp <= 0 && isAlive)
            {
                OnDeath();
            }
        }
    }

    float lastSpeed = -1f;
    bool lastAttack = false;

    void SetAnimatorSpeed(float speed)
    {
        if (aniLinh == null) return;
        if (Mathf.Abs(lastSpeed - speed) < 0.01f) return;
        lastSpeed = speed;
        aniLinh.SetFloat("Speed", speed);
    }

    void SetAnimatorAttack(bool val)
    {
        if (aniLinh == null) return;
        if (lastAttack == val) return;
        lastAttack = val;
        aniLinh.SetBool("isAttack", val);
    }

    public void OnDeath()
    {
        isAlive = false;
        Destroy(gameObject);
    }
}
