using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TruLinh : MonoBehaviour
{
    public int idTru;
    private float attackRange = 300f - 2f;
    public float fireRate = 1f;
    public GameObject bulletPrefab, prefabNotru;
    public Transform firePoint;
    private int damage = 5;

    public LayerMask playerLayer;
    private float nextFireTime;
    private Transform currentTarget;

    public GameObject phamVi;

    public float blinkSpeed = 3f;
    public Color activeColor = Color.red;
    public Color warningColor = new Color(1f, 0.3f, 0.3f, 0.7f);
    public bool useAlphaBlink = true;
    public bool useColorBlink = true;

    private Renderer[] phamViRenderers;
    private float blinkTimer;

    [Header("Line Attack")]
    public LineRenderer attackLine;
    public float lineWidth = 2f;
    public Color lineStartColor = Color.red;
    public Color lineEndColor = Color.yellow;
    public float lineStartYOffset = -95f;
    public float lineEndYOffset = 15f;

    [Header("HP System (from server)")]
    public int currentHP = -1;
    public int maxHP = -1;

    public TMP_Text txtHP;
    public Image hpFillImage;

    private bool hasHpFromServer = false;

    [Header("Proximity Effect (Merged)")]
    public GameObject effectPrefab;
    public Camera cam;
    public float checkInterval = 0.5f;
    public float boundsPadding = 1.5f;

    private GameObject spawnedEffect;
    private Renderer effectRenderer;
    private Renderer objectRenderer;
    private Bounds effectBounds;
    private Bounds objectBounds;
    private bool isEffectVisible;
    private float checkTimer;
    private bool effectInitialized;

    [Header("HP Tween")]
    public float hpTweenDuration = 0.25f;

    [Header("Death Tween")]
    public float deathMoveY = -80f;
    public float deathRotateZ = 90f;
    public float deathTweenDuration = 1f;
    public float deathEffectDelay = 0.5f;

    private Tween hpTween;
    private bool isDying = false;

    public bool IsMainTurret => idTru == 18 || idTru == 19; // 18: xanh, 19: đỏ
    public bool IsDying => isDying;

    private void Start()
    {
        if (phamVi != null)
        {
            phamViRenderers = phamVi.GetComponentsInChildren<Renderer>(true);
            SetPhamViVisibility(false);
            phamVi.SetActive(false);
        }

        InitializeLineRenderer();
        InitializeProximityEffect();
        UpdateHP(currentHP, maxHP);
    }

    private void Update()
    {
        HandleCombat();
        UpdateBlinkEffect();
        UpdateProximityEffect();
    }

    private void HandleCombat()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange, playerLayer);

        if (hits.Length > 0)
        {
            currentTarget = hits[0].transform;

            if (phamVi != null && !phamVi.activeSelf)
            {
                phamVi.SetActive(true);
                SetPhamViVisibility(true);
                blinkTimer = 0f;
            }

            UpdateAttackLine();

            if (Time.time >= nextFireTime)
            {
                nextFireTime = Time.time + 1f / fireRate;
            }
        }
        else
        {
            currentTarget = null;

            if (attackLine != null)
                attackLine.enabled = false;

            if (phamVi != null && phamVi.activeSelf)
            {
                SetPhamViVisibility(false);
                phamVi.SetActive(false);
            }
        }
    }

    private void InitializeLineRenderer()
    {
        if (attackLine == null)
        {
            GameObject lineObj = new GameObject("AttackLine");
            lineObj.transform.SetParent(transform);
            attackLine = lineObj.AddComponent<LineRenderer>();
        }

        attackLine.startWidth = lineWidth;
        attackLine.endWidth = lineWidth;
        attackLine.material = new Material(Shader.Find("Sprites/Default"));
        attackLine.positionCount = 2;
        attackLine.enabled = false;

        Gradient g = new Gradient();
        g.SetKeys(
            new[] {
                new GradientColorKey(lineStartColor, 0f),
                new GradientColorKey(lineEndColor, 1f)
            },
            new[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f)
            }
        );
        attackLine.colorGradient = g;
    }

    private void UpdateAttackLine()
    {
        if (currentTarget == null || attackLine == null) return;

        attackLine.enabled = true;

        Vector3 start = firePoint != null ? firePoint.position : transform.position;
        start.y += lineStartYOffset;

        Vector3 end = currentTarget.position;
        Collider col = currentTarget.GetComponent<Collider>();
        if (col != null)
            end = col.bounds.center;

        end.y += lineEndYOffset;

        attackLine.SetPosition(0, start);
        attackLine.SetPosition(1, end);
    }

    private void UpdateBlinkEffect()
    {
        if (phamViRenderers == null || !phamVi.activeSelf) return;

        blinkTimer += Time.deltaTime * blinkSpeed;

        foreach (var rend in phamViRenderers)
        {
            if (rend == null) continue;

            foreach (var mat in rend.materials)
            {
                if (!mat.HasProperty("_Color")) continue;

                float t = 0.5f + 0.5f * Mathf.Sin(blinkTimer * Mathf.PI);

                Color c = useColorBlink
                    ? Color.Lerp(warningColor, activeColor, t)
                    : mat.color;

                if (useAlphaBlink)
                    c.a = 0.5f + 0.5f * Mathf.Sin(blinkTimer * Mathf.PI);

                mat.color = c;
            }
        }
    }

    private void SetPhamViVisibility(bool visible)
    {
        if (phamViRenderers == null) return;
        foreach (var r in phamViRenderers)
            if (r != null) r.enabled = visible;
    }

    private void SetHpUIVisible(bool visible)
    {
        if (hpFillImage != null) hpFillImage.enabled = visible;
        if (txtHP != null) txtHP.enabled = visible;
    }

    public void UpdateHP(int hp, int hpMax)
    {
        if (hpMax <= 0) return;

        // Clamp HP để tránh âm / vượt max
        hp = Mathf.Clamp(hp, 0, hpMax);

        int prev = currentHP;

        currentHP = hp;
        maxHP = hpMax;

        // ===== FIX RECONNECT =====
        // Nếu lần đầu nhận HP từ server mà HP đã = 0 => trụ thường phải biến mất ngay
        // (case reconnect: prefab spawn lại nhưng server nói trụ đã chết)
        if (!hasHpFromServer && currentHP <= 0)
        {
            if (!IsMainTurret)
            {
                Destroy(gameObject);
            }
            else
            {
                // main turret: vẫn cập nhật UI để EndGameFlow xử lý
                hasHpFromServer = true;
                SetHpUIVisible(true);
                if (txtHP != null) txtHP.text = currentHP.ToString();
                if (hpFillImage != null) hpFillImage.fillAmount = 0f;
            }
            return;
        }

        // Trường hợp object spawn ra mà đã chết sẵn trên server (giữ lại cho chắc)
        if (prev < 0 && currentHP <= 0)
        {
            if (!IsMainTurret)
            {
                Destroy(gameObject);
            }
            return;
        }

        if (!hasHpFromServer)
        {
            hasHpFromServer = true;
            SetHpUIVisible(true);
        }

        float pct = Mathf.Clamp01((float)currentHP / maxHP);

        if (hpFillImage != null)
        {
            hpTween?.Kill();
            hpTween = hpFillImage
                .DOFillAmount(pct, hpTweenDuration)
                .SetEase(Ease.OutQuad);
        }

        if (txtHP != null)
            txtHP.text = currentHP.ToString();

        // Auto death CHỈ áp dụng cho trụ thường, không áp dụng cho trụ main
        if (!IsMainTurret && prev > 0 && currentHP <= 0 && !isDying)
            OnDeath();
    }

    public void OnDeath()
    {
        if (isDying) return;
        isDying = true;

        // Tắt attack & vùng phạm vi
        attackLine?.gameObject.SetActive(false);
        if (phamVi != null) phamVi.SetActive(false);

        Transform t = transform;

        DOVirtual.DelayedCall(deathEffectDelay, () =>
        {
            // Hiệu ứng vỡ trụ
            if (prefabNotru != null)
                Instantiate(prefabNotru, t.position, t.rotation);

            Vector3 startPos = t.position;
            Vector3 startRot = t.eulerAngles;

            // Tất cả trụ đều tụt xuống theo Y
            Vector3 targetPos = startPos + new Vector3(0f, deathMoveY, 0f);

            // Khác nhau ở trục xoay:
            // - Trụ main (id 18, 19): xoay theo Y
            // - Trụ thường: xoay theo Z
            Vector3 targetRot;
            if (IsMainTurret)
            {
                // Xoay quanh trục Y
                targetRot = startRot + new Vector3(0f, deathRotateZ, 0f);
            }
            else
            {
                // Xoay quanh trục Z (như hiệu ứng cũ của các trụ bé)
                targetRot = startRot + new Vector3(0f, 0f, deathRotateZ);
            }

            Sequence seq = DOTween.Sequence();

            // Move xuống
            seq.Join(
                t.DOMove(targetPos, deathTweenDuration)
                 .SetEase(Ease.InQuad)
            );

            // Xoay theo trục tương ứng
            seq.Join(
                t.DORotate(targetRot, deathTweenDuration, RotateMode.Fast)
                 .SetEase(Ease.InQuad)
            );

            seq.OnComplete(() =>
            {
                Destroy(gameObject);
            });
        });
    }

    private void InitializeProximityEffect()
    {
        if (cam == null)
            cam = Camera.main;

        if (effectPrefab != null && spawnedEffect == null)
        {
            spawnedEffect = Instantiate(effectPrefab, transform.position, Quaternion.identity, transform);
            spawnedEffect.SetActive(false);

            effectRenderer = spawnedEffect.GetComponentInChildren<Renderer>();
            if (effectRenderer != null)
            {
                effectBounds = effectRenderer.bounds;
                effectBounds.Expand(boundsPadding);
            }
        }

        objectRenderer = GetComponentInChildren<Renderer>();
        if (objectRenderer != null)
        {
            objectBounds = objectRenderer.bounds;
            objectBounds.Expand(boundsPadding);
        }

        effectInitialized = true;
    }

    private void UpdateProximityEffect()
    {
        if (!effectInitialized || spawnedEffect == null) return;

        checkTimer += Time.deltaTime;
        if (checkTimer < checkInterval) return;
        checkTimer = 0f;

        if (cam == null)
            cam = Camera.main;
        if (cam == null) return;

        UpdateBounds();

        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);
        bool visible =
            GeometryUtility.TestPlanesAABB(planes, objectBounds) &&
            GeometryUtility.TestPlanesAABB(planes, effectBounds);

        if (isEffectVisible != visible)
        {
            isEffectVisible = visible;
            spawnedEffect.SetActive(isEffectVisible);
        }
    }

    private void UpdateBounds()
    {
        objectBounds.center = transform.position;
        if (spawnedEffect != null)
            effectBounds.center = spawnedEffect.transform.position;
    }

    public void Shoot(Transform target)
    {
        if (bulletPrefab == null || firePoint == null || target == null)
            return;

        GameObject bullet = Instantiate(
            bulletPrefab,
            firePoint.position,
            firePoint.rotation
        );

        bullet.transform.SetParent(null);

        if (bullet.TryGetComponent<Bullet>(out var b))
        {
            b.Setup(target, damage);
        }
    }
}
