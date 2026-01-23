using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 20f;
    public float lifeTime = 3f;

    private Transform target;
    private int damage;
    // private Vector3 damageSourcePosition;

    // Thêm reference đến PlayerMove

    public void Setup(Transform target, int damage, Vector3 sourcePosition = default(Vector3))
    {
        this.target = target;
        this.damage = damage;
        // this.damageSourcePosition = sourcePosition;

        // Hủy sau lifetime
        // Invoke(nameof(SendDame), 0.7f);
        Destroy(gameObject, lifeTime);
    }

    public void SendDame()
    {
        if (!target) return;
        PlayerMove pm = target.GetComponent<PlayerMove>();
        if (pm != null)
        {
            SendData.OnTruBanMinh();
        }
        MinionMove minion = target.GetComponent<MinionMove>();
        if (minion != null && minion.teamId == B.Instance.teamId)
        {
            SendData.OnTruBanLinh(minion.teamId, minion.minionId);
        }
    }

    void Update()
    {
        if (target == null)
        {
            SetDestroy();
            return;
        }

        // Di chuyển về phía target
        Vector3 dir = target.position - transform.position;
        float distanceThisFrame = speed * Time.deltaTime;

        // Nếu đã đến gần target
        if (dir.magnitude <= distanceThisFrame)
        {
            HitTarget();
            return;
        }

        // Di chuyển
        transform.Translate(dir.normalized * distanceThisFrame, Space.World);
        transform.LookAt(target);
    }

    void HitTarget()
    {
        // Gây damage cho player (đã comment out trong phiên bản đơn giản)
        SetDestroy();
    }

    void OnTriggerEnter(Collider other)
    {
        // Kiểm tra nếu chạm vào player
        if (other.CompareTag("Player"))
        {
            SetDestroy();
        }
    }

    public void SetDestroy()
    {
        SendDame();
        Destroy(gameObject);
    }
}