using UnityEngine;

public class TruLinhProximityEffect : MonoBehaviour
{
    public GameObject effectPrefab;
    public Camera cam;

    [Header("Camera Settings")]
    public float checkInterval = 0.5f; // OPTIMIZATION: Tăng từ 0.2 → 0.5 giây
    public float boundsPadding = 1.5f;

    private GameObject spawnedEffect;
    private bool isEffectVisible = false;
    private Renderer effectRenderer;
    private Renderer objectRenderer;
    private Bounds effectBounds;
    private Bounds objectBounds;
    private float checkTimer = 0f;
    private bool isInitialized = false;

    public void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (cam == null)
            cam = Camera.main;

        if (spawnedEffect == null && effectPrefab != null)
        {
            spawnedEffect = Instantiate(
                effectPrefab,
                transform.position,
                Quaternion.identity,
                transform
            );

            if (spawnedEffect != null)
            {
                spawnedEffect.SetActive(false);

                effectRenderer = spawnedEffect.GetComponentInChildren<Renderer>();
                if (effectRenderer != null)
                {
                    effectBounds = effectRenderer.bounds;
                    effectBounds.Expand(boundsPadding);
                }
            }
        }

        objectRenderer = GetComponentInChildren<Renderer>();
        if (objectRenderer != null)
        {
            objectBounds = objectRenderer.bounds;
            objectBounds.Expand(boundsPadding);
        }

        isInitialized = true;
    }

    // ========== OPTIMIZATION: Xóa INTERVAL timer 60fps, chỉ giữ checkInterval ==========
    void Update()
    {
        if (!isInitialized || spawnedEffect == null) return;

        checkTimer += Time.deltaTime;
        if (checkTimer >= checkInterval)
        {
            checkTimer = 0f;
            UpdateEffectBasedOnCamera();
        }
    }

    private void UpdateEffectBasedOnCamera()
    {
        if (cam == null)
        {
            cam = Camera.main;
            if (cam == null) return;
        }

        bool shouldShowEffect = ShouldShowEffect();

        if (isEffectVisible != shouldShowEffect)
        {
            isEffectVisible = shouldShowEffect;
            spawnedEffect.SetActive(isEffectVisible);
        }
    }

    private bool ShouldShowEffect()
    {
        if (cam == null) return false;

        UpdateBounds();

        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);

        bool objectInView = GeometryUtility.TestPlanesAABB(planes, objectBounds);
        bool effectInView = GeometryUtility.TestPlanesAABB(planes, effectBounds);

        return objectInView && effectInView;
    }

    private void UpdateBounds()
    {
        if (objectRenderer != null)
        {
            objectBounds.center = transform.position;
        }
        else
        {
            objectBounds.center = transform.position;
            objectBounds.size = Vector3.one * 2f;
        }

        if (effectRenderer != null)
        {
            effectBounds.center = spawnedEffect.transform.position;
        }
        else if (spawnedEffect != null)
        {
            effectBounds.center = spawnedEffect.transform.position;
            effectBounds.size = Vector3.one * 2f;
        }
    }

    public void SetCamera(Camera newCamera)
    {
        cam = newCamera;
    }

    private void OnEnable()
    {
        if (spawnedEffect != null && isEffectVisible)
        {
            spawnedEffect.SetActive(true);
        }
    }

    private void OnDisable()
    {
        if (spawnedEffect != null)
        {
            spawnedEffect.SetActive(false);
        }
    }

    public bool IsEffectVisible()
    {
        return isEffectVisible;
    }

    public void SetEffectPrefab(GameObject newPrefab)
    {
        if (spawnedEffect != null)
        {
            Destroy(spawnedEffect);
            spawnedEffect = null;
        }

        effectPrefab = newPrefab;
        Initialize();
    }
}