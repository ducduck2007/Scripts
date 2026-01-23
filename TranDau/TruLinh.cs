using UnityEngine;

public class TruLinh : MonoBehaviour
{
    [Header("HP (from RESOURCE_SNAPSHOT)")]
    public int hp;
    public int maxHp;

    public int idTru;
    public float attackRange = 800f;
    public float fireRate = 1f;
    public GameObject bulletPrefab, prefabNotru;
    public GameObject phamVi;
    public Transform firePoint;
    public int damage = 5;

    // Biến cho hiệu ứng nhấp nháy
    public float blinkSpeed = 3f;
    public Color activeColor = Color.red;
    public Color warningColor = new Color(1f, 0.3f, 0.3f, 0.7f);
    public bool useAlphaBlink = true;
    public bool useColorBlink = true;

    // Biến cho Line Renderer
    public LineRenderer attackLine;
    public float lineWidth = 1f;
    public Color lineStartColor = Color.red;
    public Color lineEndColor = Color.yellow;

    private float nextFireTime;
    public LayerMask playerLayer;

    private Renderer[] phamViRenderers;
    private float blinkTimer = 0f;
    private Transform currentTarget;

    private void Start()
    {
        if (phamVi != null)
        {
            phamViRenderers = phamVi.GetComponentsInChildren<Renderer>();
            SetPhamViVisibility(false); // Ẩn ban đầu
        }

        // Khởi tạo Line Renderer
        InitializeLineRenderer();
    }

    private void InitializeLineRenderer()
    {
        if (attackLine == null)
        {
            // Tạo Line Renderer nếu chưa có
            GameObject lineObj = new GameObject("AttackLine");
            lineObj.transform.SetParent(transform);
            attackLine = lineObj.AddComponent<LineRenderer>();
        }

        attackLine.startWidth = lineWidth;
        attackLine.endWidth = lineWidth;
        attackLine.material = new Material(Shader.Find("Sprites/Default"));

        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(lineStartColor, 0.0f),
                new GradientColorKey(lineEndColor, 1.0f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1.0f, 0.0f),
                new GradientAlphaKey(1.0f, 1.0f)
            }
        );
        attackLine.colorGradient = gradient;

        attackLine.enabled = false;
        attackLine.positionCount = 2;
    }

    private void Update()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange, playerLayer);

        if (hits.Length > 0)
        {
            currentTarget = hits[0].transform;

            if (!phamVi.activeSelf)
            {
                phamVi.SetActive(true);
                SetPhamViVisibility(true);
                blinkTimer = 0f;
            }

            UpdateBlinkEffect();
            UpdateAttackLine();

            if (Time.time >= nextFireTime)
            {
                // Shoot(currentTarget);
                nextFireTime = Time.time + 1f / fireRate;
            }
        }
        else
        {
            currentTarget = null;
            attackLine.enabled = false;

            if (phamVi.activeSelf)
            {
                SetPhamViVisibility(false);
                phamVi.SetActive(false);
            }
        }
    }

    private void UpdateAttackLine()
    {
        if (currentTarget == null || attackLine == null) return;

        // Bật Line Renderer
        attackLine.enabled = true;

        // Đặt điểm bắt đầu (từ firePoint hoặc trung tâm tháp)
        Vector3 startPoint = firePoint != null ? firePoint.position : transform.position;

        // Đặt điểm kết thúc vào giữa nhân vật target (cả 2 trục X và Z)
        Vector3 targetCenter = currentTarget.position;

        // Nếu target có Collider, lấy điểm giữa của collider
        Collider targetCollider = currentTarget.GetComponent<Collider>();
        if (targetCollider != null)
        {
            targetCenter = targetCollider.bounds.center;
        }

        // Cập nhật vị trí cho Line Renderer
        attackLine.SetPosition(0, startPoint);
        attackLine.SetPosition(1, targetCenter);
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

    public void UpdateHpFromServer(int newHp, int newMaxHp)
    {
        if (newMaxHp <= 0) return;

        hp = newHp;
        maxHp = newMaxHp;

        if (hp <= 0)
        {
            OnDeath();
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

    public void Shoot(Transform target)
    {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        bullet.transform.SetParent(null);
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
            bulletScript.Setup(target, damage);
    }

    public void OnDeath()
    {
        // sinh hư nổ trụ
        Instantiate(prefabNotru, transform.position, transform.rotation);

        gameObject.SetActive(false);
    }

    // private void OnDrawGizmosSelected()
    // {
    //     Gizmos.color = Color.red;
    //     Gizmos.DrawWireSphere(transform.position, attackRange);
    // }
}