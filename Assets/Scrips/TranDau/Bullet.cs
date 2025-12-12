using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 20f;
    public float lifeTime = 3f;

    private Transform target;
    private int damage;
    // private Vector3 damageSourcePosition;

    // Thêm reference đến PlayerMove
    private PlayerMove playerMove;

    public void Setup(Transform target, int damage, Vector3 sourcePosition = default(Vector3))
    {
        this.target = target;
        this.damage = damage;
        // this.damageSourcePosition = sourcePosition;

        // Tìm PlayerMove
        if (target != null)
        {
            playerMove = target.GetComponent<PlayerMove>();
        }

        // Hủy sau lifetime
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        if (target == null || playerMove == null)
        {
            Destroy(gameObject);
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
        // if (playerMove != null && !playerMove.isDead)
        // {
        //     playerMove.TakeDamage(damage, Vector3.zero);
        // }

        // Hủy bullet
        Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        // Kiểm tra nếu chạm vào player
        if (other.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }
}