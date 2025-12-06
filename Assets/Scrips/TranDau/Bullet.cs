using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 25f;
    private Transform target;
    private int damage;

    public void Setup(Transform target, int damage)
    {
        this.target = target;
        this.damage = damage;
    }

    private void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
        transform.LookAt(target);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Gọi script HP của Player
            // PlayerHealth hp = other.GetComponent<PlayerHealth>();
            // if (hp != null)
            // {
            //     hp.TakeDamage(damage);
            // }

            Destroy(gameObject); // Hủy đạn khi va chạm
        }
    }

    private void Start()
    {
        Destroy(gameObject, 4f); // Hủy nếu không trúng trong 4 giây
    }
}
