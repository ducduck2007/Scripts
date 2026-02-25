using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JungleMonster : MonoBehaviour
{
    public int id;
    public int campId;
    public Animator animator;
    public float moveSmooth = 10f;
    public float rotateSmooth = 12f;
    public float deadRespawnTime = 60f;

    [Header("Spawn Delay")]
    public float spawnDelay = 0f;

    public TextMeshProUGUI txtMau;
    public Image imgFill;

    [Header("Camera Culling")]
    public Camera cam;
    public float checkInterval = 0.5f;
    public float boundsPadding = 2f;

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

    [Header("HP Tween")]
    public float hpTweenDuration = 0.25f;
    private Tween hpTween;

    private bool _externalInitDone = false;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    private void Start()
    {
        if (_externalInitDone) return;

        SetMonsterActive(false);

        targetPos = transform.position;
        lastPos = transform.position;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        monsterRenderer = GetComponentInChildren<Renderer>();
        if (monsterRenderer != null)
        {
            monsterBounds = monsterRenderer.bounds;
            monsterBounds.Expand(boundsPadding);
        }
        else
        {
            monsterBounds = new Bounds(transform.position, Vector3.one * 2f);
        }

        monsterCollider = GetComponent<Collider>();
        if (monsterCollider == null)
            monsterCollider = GetComponentInChildren<Collider>();

        if (cam == null)
            cam = Camera.main;
    }

    const float INTERVAL = 1f / 60f;
    float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer < INTERVAL) return;
        timer -= INTERVAL;

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

        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * moveSmooth);
        RotateToMoveDirection();

        float speed = (transform.position - lastPos).magnitude / Time.deltaTime;
        if (animator != null && animator.enabled)
            animator.SetBool("isWalking", speed > 0.05f);

        lastPos = transform.position;

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
            CheckVisibility();
        }
    }

    private void SetMonsterActive(bool active)
    {
        if (active)
        {
            transform.localScale = Vector3.zero;
            gameObject.SetActive(true);
            StartCoroutine(ScaleUpAnimation());

            if (monsterRenderer != null)
                monsterRenderer.enabled = true;
            if (monsterCollider != null)
                monsterCollider.enabled = true;
            if (animator != null && isVisible)
                animator.enabled = true;

            if (imgFill != null) imgFill.enabled = true;
            if (txtMau != null) txtMau.enabled = true;
        }
        else
        {
            if (monsterRenderer != null)
                monsterRenderer.enabled = false;
            if (monsterCollider != null)
                monsterCollider.enabled = false;
            if (animator != null)
                animator.enabled = false;

            transform.localScale = Vector3.zero;

            if (imgFill != null) imgFill.enabled = false;
            if (txtMau != null) txtMau.enabled = false;
        }
    }

    private System.Collections.IEnumerator ScaleUpAnimation()
    {
        float duration = 0.5f;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, t / duration);
            yield return null;
        }

        transform.localScale = originalScale;
    }

    private void CheckVisibility()
    {
        if (!isInitialized)
            return;

        if (cam == null)
        {
            cam = Camera.main;
            if (cam == null) return;
        }

        monsterBounds.center = transform.position;

        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);
        bool nowVisible = GeometryUtility.TestPlanesAABB(planes, monsterBounds);

        if (nowVisible != isVisible)
        {
            isVisible = nowVisible;
            ToggleAnimator(nowVisible);
        }
    }

    private void ToggleAnimator(bool enable)
    {
        if (animator == null || !isInitialized) return;
        animator.enabled = enable;

        if (enable)
        {
            animator.Rebind();
            animator.Update(0f);
        }
    }

    public void UpdateFromServer(float x, float y, int hp, int hpMax)
    {
        targetPos = new Vector3(x, transform.position.y, y);

        if (!isInitialized)
        {
            if (originalScale == Vector3.zero)
                originalScale = transform.localScale;

            if (monsterRenderer == null) monsterRenderer = GetComponentInChildren<Renderer>();
            if (monsterCollider == null)
            {
                monsterCollider = GetComponent<Collider>();
                if (monsterCollider == null) monsterCollider = GetComponentInChildren<Collider>();
            }
            if (animator == null) animator = GetComponentInChildren<Animator>();
            if (cam == null) cam = Camera.main;

            monsterBounds = monsterRenderer != null
                ? monsterRenderer.bounds
                : new Bounds(transform.position, Vector3.one * 2f);
            monsterBounds.Expand(boundsPadding);

            _externalInitDone = true;
            SetMonsterActive(true);
            isInitialized = true;
        }

        if (hpMax > 0)
        {
            maxHP = hpMax;
            currentHP = hp;

            if (txtMau != null)
                txtMau.text = $"{currentHP}/{maxHP}";

            float percent = Mathf.Clamp01((float)currentHP / maxHP);
            if (Mathf.Abs(lastHpPercent - percent) > 0.01f)
            {
                lastHpPercent = percent;
                if (imgFill != null)
                {
                    hpTween?.Kill();
                    hpTween = imgFill.DOFillAmount(percent, hpTweenDuration).SetEase(Ease.OutQuad);
                }
            }

            if (isDead && currentHP > 0)
                ForceRespawn();
        }
    }

    private void RotateToMoveDirection()
    {
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

        if (imgFill != null) imgFill.enabled = false;
        if (txtMau != null) txtMau.enabled = false;
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
            animator.Play("Idle", 0);

        if (monsterCollider != null)
            monsterCollider.enabled = true;
    }

    private void ForceRespawn()
    {
        isDead = false;

        if (animator != null)
            animator.Play("Idle", 0);

        if (monsterCollider != null)
            monsterCollider.enabled = true;
    }

    public void SetVisibility(bool visible)
    {
        if (!isInitialized) return;

        if (isVisible != visible)
        {
            isVisible = visible;
            ToggleAnimator(visible);
        }
    }

    public bool IsInCameraView() => isVisible;

    public void SetSpawnDelay(float delay)
    {
        if (!isInitialized && !isSpawning)
        {
            spawnDelay = delay;
            spawnDelayTimer = delay;
            isSpawning = true;
        }
    }

    public bool IsSpawned() => isInitialized && !isSpawning;
}
