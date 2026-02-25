using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TurretHpFollow : MonoBehaviour
{
    [Header("Target")]
    public TruLinh target;                 // Trụ cần follow

    [Header("HP UI (Screen Space Canvas)")]
    public RectTransform hpRoot;
    public Image hpFillImage;
    public TMP_Text txtHP;

    [Header("Offset")]
    public Vector3 worldOffset = new Vector3(0, 1.5f, 0);

    private Camera cam;
    private Renderer targetRenderer;
    private bool active;

    private void Awake()
    {
        cam = Camera.main;

        if (target != null)
            targetRenderer = target.GetComponentInChildren<Renderer>();

        if (hpRoot != null)
            hpRoot.gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if (!active || target == null || cam == null || targetRenderer == null)
            return;

        UpdatePosition();
        UpdateValue();
    }

    private void UpdatePosition()
    {
        Bounds b = targetRenderer.bounds;
        Vector3 worldTop = new Vector3(b.center.x, b.max.y, b.center.z) + worldOffset;

        Vector3 vp = cam.WorldToViewportPoint(worldTop);

        // Sau camera
        if (vp.z <= 0f)
        {
            hpRoot.gameObject.SetActive(false);
            return;
        }

        // Clamp viewport (MOBA style)
        vp.x = Mathf.Clamp01(vp.x);
        vp.y = Mathf.Clamp01(vp.y);

        hpRoot.position = new Vector3(
            vp.x * Screen.width,
            vp.y * Screen.height,
            0f
        );

        if (!hpRoot.gameObject.activeSelf)
            hpRoot.gameObject.SetActive(true);
    }

    private void UpdateValue()
    {
        if (target.maxHP <= 0) return;

        float pct = Mathf.Clamp01((float)target.currentHP / target.maxHP);

        if (hpFillImage != null)
            hpFillImage.fillAmount = pct;

        if (txtHP != null)
            txtHP.text = $"{target.currentHP}/{target.maxHP}";

        if (target.currentHP <= 0 && hpRoot.gameObject.activeSelf)
            hpRoot.gameObject.SetActive(false);
    }

    // ==================== API ====================
    public void Bind(TruLinh t)
    {
        target = t;
        targetRenderer = t != null ? t.GetComponentInChildren<Renderer>() : null;
        active = true;

        if (hpRoot != null)
            hpRoot.gameObject.SetActive(true);
    }
}
