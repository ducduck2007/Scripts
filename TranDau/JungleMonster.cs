using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JungleMonster : MonoBehaviour
{
    public int id;
    public Animator animator;
    public float moveSmooth = 10f;
    public float rotateSmooth = 12f;
    public float deadRespawnTime = 60f;

    [Header("Spawn Delay")]
    public float spawnDelay = 0f; // Thời gian delay trước khi spawn (giây)

    public TextMeshProUGUI txtMau;
    public Image imgFill;

    [Header("Camera Culling")]
    public Camera cam; // Camera chính để kiểm tra tầm nhìn
    public float checkInterval = 0.5f; // Interval kiểm tra để tối ưu
    public float boundsPadding = 2f; // Padding thêm cho bounds để tránh tắt/bật liên tục

    private int currentHP;
    private int maxHP;
    private bool isDead = false;
    private bool isSpawning = false;
    private bool isInitialized = false;
    private Vector3 targetPos;
    private Vector3 lastPos;
    private float respawnTimer = 0f;
    private float spawnDelayTimer = 0f;
    private float lastHpPercent = -1f;
    private float checkTimer = 0f;
    private bool isVisible = false;
    private Renderer monsterRenderer;
    private Bounds monsterBounds;
    private Collider monsterCollider;
    private Vector3 originalScale;

    private void Start()
    {
        // Lưu scale gốc
        originalScale = transform.localScale;

        // Ẩn hoàn toàn monster cho đến khi hết delay
        SetMonsterActive(false);

        targetPos = transform.position;
        lastPos = transform.position;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        // Lấy Renderer để lấy bounds
        monsterRenderer = GetComponentInChildren<Renderer>();
        if (monsterRenderer != null)
        {
            monsterBounds = monsterRenderer.bounds;
            monsterBounds.Expand(boundsPadding);
        }
        else
        {
            // Fallback nếu không có Renderer
            monsterBounds = new Bounds(transform.position, Vector3.one * 2f);
        }

        // Lấy Collider
        monsterCollider = GetComponent<Collider>();
        if (monsterCollider == null)
            monsterCollider = GetComponentInChildren<Collider>();

        // Nếu không gán camera, tìm camera chính
        if (cam == null)
            cam = Camera.main;

        // Khởi tạo delay spawn
        if (spawnDelay > 0)
        {
            spawnDelayTimer = spawnDelay;
            isSpawning = true;
        }
        else
        {
            // Không có delay, kích hoạt ngay
            SetMonsterActive(true);
            isInitialized = true;
        }
    }

    const float INTERVAL = 1f / 60f;
    float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer < INTERVAL) return;

        timer -= INTERVAL;

        // Xử lý delay spawn
        if (isSpawning)
        {
            HandleSpawnDelay();
            return;
        }

        if (!isInitialized)
            return;

        if (isDead)
        {
            HandleRespawn();
            return;
        }

        // Cập nhật vị trí
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * moveSmooth);
        RotateToMoveDirection();

        // Tính toán tốc độ di chuyển
        float speed = (transform.position - lastPos).magnitude / Time.deltaTime;

        // Cập nhật animator nếu đang bật
        if (animator != null && animator.enabled)
            animator.SetBool("isWalking", speed > 0.05f);

        lastPos = transform.position;

        // Kiểm tra visibility với interval
        checkTimer += Time.deltaTime;
        if (checkTimer >= checkInterval)
        {
            checkTimer = 0f;
            CheckVisibility();
        }
    }

    private void HandleSpawnDelay()
    {
        spawnDelayTimer -= Time.deltaTime;

        if (spawnDelayTimer <= 0)
        {
            isSpawning = false;
            SetMonsterActive(true);
            isInitialized = true;

            // Force kiểm tra visibility ngay sau khi spawn
            CheckVisibility();
        }
    }

    private void SetMonsterActive(bool active)
    {
        if (active)
        {
            // Hiện monster với animation scale
            transform.localScale = Vector3.zero;
            gameObject.SetActive(true);

            // Animation scale up
            StartCoroutine(ScaleUpAnimation());

            if (monsterRenderer != null)
                monsterRenderer.enabled = true;

            if (monsterCollider != null)
                monsterCollider.enabled = true;

            // Bật animator nếu cần
            if (animator != null && isVisible)
                animator.enabled = true;
        }
        else
        {
            // Ẩn hoàn toàn
            if (monsterRenderer != null)
                monsterRenderer.enabled = false;

            if (monsterCollider != null)
                monsterCollider.enabled = false;

            if (animator != null)
                animator.enabled = false;

            transform.localScale = Vector3.zero;
        }
    }

    private System.Collections.IEnumerator ScaleUpAnimation()
    {
        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, t);
            yield return null;
        }

        transform.localScale = originalScale;
    }

    private void CheckVisibility()
    {
        if (!isInitialized || cam == null)
        {
            cam = Camera.main;
            if (cam == null) return;
        }

        // Cập nhật bounds nếu monster di chuyển
        monsterBounds.center = transform.position;

        // Kiểm tra xem monster có trong frustum của camera không
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);
        bool nowVisible = GeometryUtility.TestPlanesAABB(planes, monsterBounds);

        if (nowVisible != isVisible)
        {
            isVisible = nowVisible;
            ToggleAnimator(isVisible);
        }
    }

    private void ToggleAnimator(bool enable)
    {
        if (animator == null || !isInitialized) return;

        if (enable)
        {
            // Bật animator
            if (!animator.enabled)
            {
                animator.enabled = true;
                animator.Rebind(); // Reset animation state
                animator.Update(0f); // Cập nhật ngay lập tức
            }
        }
        else
        {
            // Tắt animator
            if (animator.enabled)
            {
                animator.enabled = false;
            }
        }
    }

    public void UpdateFromServer(float x, float y, int hp, int hpMax)
    {
        // Không cập nhật nếu chưa hết spawn delay
        if (isSpawning)
            return;

        if (!isInitialized)
        {
            SetMonsterActive(true);
            isInitialized = true;
        }

        // ✅ CMD50: hpMax = 0 => chỉ update vị trí
        targetPos = new Vector3(x, transform.position.y, y);

        if (hpMax <= 0)
            return;

        // ✅ CMD51: có hp/hpMax hợp lệ
        maxHP = hpMax;
        currentHP = hp;

        if (txtMau != null)
            txtMau.text = AgentCSharp.ShowMoneyFull(hp);

        float percent = Mathf.Clamp01((float)currentHP / hpMax);
        if (Mathf.Abs(lastHpPercent - percent) > 0.01f)
        {
            lastHpPercent = percent;
            if (imgFill != null)
                imgFill.fillAmount = percent;
        }

        if (isDead && currentHP > 0)
            ForceRespawn();
    }

    private void RotateToMoveDirection()
    {
        if (!isInitialized) return;

        Vector3 dir = targetPos - transform.position;
        dir.y = 0;

        if (dir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime * rotateSmooth);
        }
    }

    public void Die()
    {
        if (!isInitialized) return;

        isDead = true;

        if (animator != null && animator.enabled)
        {
            animator.SetBool("isWalking", false);
            animator.SetTrigger("Die");
        }

        respawnTimer = deadRespawnTime;

        if (monsterCollider != null)
            monsterCollider.enabled = false;
    }

    private void HandleRespawn()
    {
        respawnTimer -= Time.deltaTime;
        if (respawnTimer <= 0)
            Respawn();
    }

    private void Respawn()
    {
        isDead = false;

        if (animator != null)
        {
            animator.ResetTrigger("Die");
            if (animator.enabled)
                animator.Play("Idle", 0);
        }

        if (monsterCollider != null)
            monsterCollider.enabled = true;
    }

    private void ForceRespawn()
    {
        isDead = false;

        if (animator != null)
        {
            animator.ResetTrigger("Die");
            if (animator.enabled)
                animator.Play("Idle", 0);
        }

        if (monsterCollider != null)
            monsterCollider.enabled = true;
    }

    // Phương thức để force bật/tắt từ bên ngoài nếu cần
    public void SetVisibility(bool visible)
    {
        if (!isInitialized) return;

        if (isVisible != visible)
        {
            isVisible = visible;
            ToggleAnimator(visible);
        }
    }

    // Hàm tiện ích để kiểm tra nhanh
    public bool IsInCameraView()
    {
        return isVisible;
    }

    // Hàm để thiết lập spawn delay từ code (nếu cần)
    public void SetSpawnDelay(float delay)
    {
        if (!isInitialized && !isSpawning)
        {
            spawnDelay = delay;
            spawnDelayTimer = delay;
            isSpawning = true;
        }
    }

    // Kiểm tra xem monster đã spawn chưa
    public bool IsSpawned()
    {
        return isInitialized && !isSpawning;
    }
}