using UnityEngine;

public class TruLinh : MonoBehaviour
{
    public float attackRange = 800f;
    public float fireRate = 1f;
    public GameObject bulletPrefab;
    public GameObject phamVi;
    public Transform firePoint;
    public int damage = 5;

    // Biến cho hiệu ứng nhấp nháy
    public float blinkSpeed = 3f;
    public Color activeColor = Color.red;
    public Color warningColor = new Color(1f, 0.3f, 0.3f, 0.7f);
    public bool useAlphaBlink = true;
    public bool useColorBlink = true;

    private float nextFireTime;
    public LayerMask playerLayer;

    private Renderer[] phamViRenderers;
    private float blinkTimer = 0f;

    private void Start()
    {
        if (phamVi != null)
        {
            phamViRenderers = phamVi.GetComponentsInChildren<Renderer>();
            SetPhamViVisibility(false); // Ẩn ban đầu
        }
    }

    private void Update()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange, playerLayer);

        if (hits.Length > 0)
        {
            Transform target = hits[0].transform;

            if (!phamVi.activeSelf)
            {
                phamVi.SetActive(true);
                SetPhamViVisibility(true);
                blinkTimer = 0f;
            }

            UpdateBlinkEffect();

            if (Time.time >= nextFireTime)
            {
                Shoot(target);
                nextFireTime = Time.time + 1f / fireRate;
            }
        }
        else
        {
            if (phamVi.activeSelf)
            {
                SetPhamViVisibility(false);
                phamVi.SetActive(false);
            }
        }
    }

    private void UpdateBlinkEffect()
    {
        if (phamViRenderers == null || phamViRenderers.Length == 0) return;

        blinkTimer += Time.deltaTime * blinkSpeed;

        foreach (Renderer rend in phamViRenderers)
        {
            if (rend == null) continue;

            Material[] materials = rend.materials;
            foreach (Material mat in materials)
            {
                if (useAlphaBlink && mat.HasProperty("_Color"))
                {
                    Color currentColor = mat.color;
                    // Tạo hiệu ứng nhấp nháy bằng alpha
                    float alpha = 0.5f + 0.5f * Mathf.Sin(blinkTimer * Mathf.PI);
                    currentColor.a = alpha;
                    mat.color = currentColor;
                }

                if (useColorBlink && mat.HasProperty("_Color"))
                {
                    // Tạo hiệu ứng nhấp nháy bằng màu sắc
                    float lerpValue = 0.5f + 0.5f * Mathf.Sin(blinkTimer * Mathf.PI);
                    Color lerpedColor = Color.Lerp(warningColor, activeColor, lerpValue);

                    if (!useAlphaBlink)
                    {
                        // Giữ alpha nguyên nếu không dùng alpha blink
                        lerpedColor.a = mat.color.a;
                    }

                    mat.color = lerpedColor;
                }
            }
        }
    }

    private void SetPhamViVisibility(bool isVisible)
    {
        if (phamViRenderers == null) return;
        foreach (Renderer rend in phamViRenderers)
        {
            if (rend != null)
                rend.enabled = isVisible;
        }
    }

    private void Shoot(Transform target)
    {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        bullet.transform.SetParent(null);
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
            bulletScript.Setup(target, damage);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
