using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogTrangBi : ScaleScreen, IItemTooltipHost
{
    public Button btnExit;
    public TextMeshProUGUI txtSl;

    [Header("List UI")]
    public Transform content;
    public ItemTrangBi itemPrefab;

    [Header("Tooltip")]
    public ItemTooltipUI tooltipPrefab;     // kéo prefab tooltip vào đây
    public RectTransform tooltipParent;     // thường là canvas/dialog root
    private ItemTooltipUI _tooltip;

    private readonly List<ItemTrangBi> spawned = new();
    private int _lastVersion = -1;

    protected override void Start()
    {
        base.Start();
        if (btnExit != null) btnExit.onClick.AddListener(SetExit);

        ItemInfoCache.EnsureDiskLoaded(false);

        // tạo tooltip instance 1 lần
        EnsureTooltipCreated();
        HideTooltip();
    }

    private void EnsureTooltipCreated()
    {
        if (_tooltip != null) return;
        if (tooltipPrefab == null) return;

        var parent = tooltipParent != null ? tooltipParent : transform as RectTransform;
        _tooltip = Object.Instantiate(tooltipPrefab, parent);
        _tooltip.gameObject.SetActive(false);

        // QUAN TRỌNG: tooltip không chặn raycast để tránh PointerExit nháy
        var cg = _tooltip.GetComponent<CanvasGroup>();
        if (cg == null) cg = _tooltip.gameObject.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;
        cg.interactable = false;
    }

    private void OnEnable()
    {
        base.OnEnable();

        ItemInfoCache.OnUpdated -= OnItemCacheUpdated;
        ItemInfoCache.OnUpdated += OnItemCacheUpdated;

        TryBuildIfNew(force: true);
    }

    private void OnDisable()
    {
        ItemInfoCache.OnUpdated -= OnItemCacheUpdated;
        HideTooltip();
    }

    private void OnItemCacheUpdated()
    {
        TryBuildIfNew(force: false);
    }

    private void TryBuildIfNew(bool force)
    {
        if (!force && _lastVersion == ItemInfoCache.Version) return;
        _lastVersion = ItemInfoCache.Version;
        SetData();
    }

    private void SetExit()
    {
        AudioManager.Instance.AudioClick();
        Show(false);

        if (MainGame1.Instance != null) MainGame1.Instance.Show(true);
        else
        {
            var main = FindObjectOfType<MainGame1>(true);
            if (main != null) main.Show(true);
        }
    }

    public void Show(bool val = true) => gameObject.SetActive(val);

    public void SetData()
    {
        var list = ItemInfoCache.GetAllSorted();

        for (int i = 0; i < spawned.Count; i++)
            if (spawned[i] != null) Destroy(spawned[i].gameObject);
        spawned.Clear();

        if (txtSl != null) txtSl.text = list.Count.ToString();

        if (content == null || itemPrefab == null)
        {
            Debug.LogError("[DialogTrangBi] content/itemPrefab null!");
            return;
        }

        EnsureTooltipCreated();

        for (int i = 0; i < list.Count; i++)
        {
            var d = list[i];
            var it = Object.Instantiate(itemPrefab, content);
            it.gameObject.SetActive(true);
            it.Init(d.idItem, d.nameItem);

            // gán dialog để item gọi tooltip
            it.SetOwner(this);

            spawned.Add(it);
        }

#if UNITY_EDITOR
        Debug.Log($"[DialogTrangBi] Build items count={list.Count} version={ItemInfoCache.Version}");
#endif
    }

    // =========================
    // Tooltip API for items
    // =========================
    public void ShowTooltip(int itemId, RectTransform anchor)
    {
        EnsureTooltipCreated();
        if (_tooltip == null) return;

        if (!ItemInfoCache.TryGet(itemId, out var d) || d == null)
        {
            Debug.LogWarning($"[DialogTrangBi] ShowTooltip missing itemId={itemId}");
            return;
        }

        _tooltip.Bind(d);
        _tooltip.gameObject.SetActive(true);

        var tipRt = _tooltip.root != null ? _tooltip.root : _tooltip.GetComponent<RectTransform>();
        if (tipRt == null || anchor == null) return;

        // Canvas root để clamp
        var canvas = tipRt.GetComponentInParent<Canvas>();
        if (canvas == null) { tipRt.position = anchor.position; return; }

        var canvasRt = canvas.transform as RectTransform;
        if (canvasRt == null) { tipRt.position = anchor.position; return; }

        // Force layout để tooltip có size đúng (vì text mới Bind xong)
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(tipRt);

        // Convert anchor -> local point in canvas
        var cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

        Vector2 anchorLocal;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRt,
            RectTransformUtility.WorldToScreenPoint(cam, anchor.position),
            cam,
            out anchorLocal
        );

        // ====== CHỌN hướng show mặc định: bên phải + hơi xuống ======
        // Bạn có thể chỉnh offset tùy UI
        Vector2 offset = new Vector2(20f, -10f);

        // đặt pivot để dễ clamp (pivot top-left thường ổn)
        tipRt.pivot = new Vector2(0f, 1f);

        Vector2 desired = anchorLocal + offset;

        // ====== CLAMP vào canvas ======
        Vector2 tipSize = tipRt.rect.size;
        Rect canvasRect = canvasRt.rect;

        // pivot (0,1): left-top
        float minX = canvasRect.xMin;
        float maxX = canvasRect.xMax - tipSize.x;

        float minY = canvasRect.yMin + tipSize.y;
        float maxY = canvasRect.yMax;

        // clamp theo localPosition
        desired.x = Mathf.Clamp(desired.x, minX, maxX);
        desired.y = Mathf.Clamp(desired.y, minY, maxY);

        tipRt.anchoredPosition = desired;

        // ====== Nếu item nằm sát mép phải mà tooltip vẫn bị đè, flip sang trái ======
        // (tùy bạn có muốn không; nên bật để UX ngon hơn)
        bool overflowRight = (anchorLocal.x + offset.x + tipSize.x) > canvasRect.xMax;
        if (overflowRight)
        {
            // pivot top-right để bám sang trái
            tipRt.pivot = new Vector2(1f, 1f);
            desired = anchorLocal + new Vector2(-20f, -10f);

            float minX2 = canvasRect.xMin + tipSize.x;
            float maxX2 = canvasRect.xMax;

            desired.x = Mathf.Clamp(desired.x, minX2, maxX2);
            desired.y = Mathf.Clamp(desired.y, minY, maxY);

            tipRt.anchoredPosition = desired;
        }

        // ====== Nếu item sát mép dưới, flip lên trên ======
        bool overflowBottom = (anchorLocal.y + offset.y - tipSize.y) < canvasRect.yMin;
        if (overflowBottom)
        {
            // pivot bottom-left để kéo tooltip lên trên
            tipRt.pivot = new Vector2(0f, 0f);
            desired = anchorLocal + new Vector2(20f, 10f);

            float minY2 = canvasRect.yMin;
            float maxY2 = canvasRect.yMax - tipSize.y;

            float minX3 = canvasRect.xMin;
            float maxX3 = canvasRect.xMax - tipSize.x;

            desired.x = Mathf.Clamp(desired.x, minX3, maxX3);
            desired.y = Mathf.Clamp(desired.y, minY2, maxY2);

            tipRt.anchoredPosition = desired;
        }
    }

    public void HideTooltip()
    {
        if (_tooltip != null) _tooltip.gameObject.SetActive(false);
    }
}
