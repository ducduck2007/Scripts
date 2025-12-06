using UnityEngine;

public class TruLinh : MonoBehaviour
{
    public float attackRange = 800f;
    public float fireRate = 1f;
    public GameObject bulletPrefab;
    public GameObject phamVi;
    public Transform firePoint;
    public int damage = 5;

    private float nextFireTime;
    public LayerMask playerLayer;

    private void Update()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange, playerLayer);

        if (hits.Length > 0)
        {
            Transform target = hits[0].transform;

            phamVi.SetActive(true);
            if (Time.time >= nextFireTime)
            {
                Shoot(target);
                nextFireTime = Time.time + 1f / fireRate;
            }
        }
        else
        {
            phamVi.SetActive(false);
        }
    }

    private void Shoot(Transform target)
    {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        bullet.transform.SetParent(null);
        AgentUnity.Log("TruLinh bắn đạn về phía Player");
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        bulletScript.Setup(target, damage);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
