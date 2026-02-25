using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemTrangBi : MonoBehaviour
{
    private int itemId;

    public TextMeshProUGUI nameItem;
    public Button btnShowChiTiet;
    public Image imgTrangBi;

    [Header("Selection Border (Runtime)")]
    private Outline _borderOutline;

    private IItemTooltipHost _owner;

    public System.Action<int> OnClickItem;

    public void SetOwner(IItemTooltipHost owner) => _owner = owner;

    public int GetItemId() => itemId;

    public void Init(int id, string ten)
    {
        itemId = id;

        if (nameItem != null) nameItem.text = ten;

        if (imgTrangBi == null)
            imgTrangBi = GetComponentInChildren<Image>(true);

        LoadIcon(itemId);

        if (btnShowChiTiet == null)
            btnShowChiTiet = GetComponentInChildren<Button>(true);

        if (btnShowChiTiet == null)
        {
            Debug.LogError("ItemTrangBi: Không tìm thấy Button trong prefab!");
            return;
        }

        btnShowChiTiet.onClick.RemoveAllListeners();
        btnShowChiTiet.onClick.AddListener(() => OnClickItem?.Invoke(itemId));

        SetupHoldEvents(btnShowChiTiet.gameObject);

        SetupBorderOutline();
    }

    private void SetupBorderOutline()
    {
        if (imgTrangBi == null) return;

        _borderOutline = imgTrangBi.GetComponent<Outline>();
        if (_borderOutline == null)
        {
            _borderOutline = imgTrangBi.gameObject.AddComponent<Outline>();
        }

        _borderOutline.enabled = false;
    }

    public void SetSelected(bool selected, Color borderColor = default, float borderWidth = 4f)
    {
        if (_borderOutline == null) return;

        _borderOutline.enabled = selected;

        if (selected)
        {
            _borderOutline.effectColor = borderColor == default 
                ? new Color(0f, 1f, 1f, 1f)
                : borderColor;

            _borderOutline.effectDistance = new Vector2(borderWidth, borderWidth);
        }
    }

    private void SetupHoldEvents(GameObject go)
    {
        var trigger = go.GetComponent<EventTrigger>();
        if (trigger == null) trigger = go.AddComponent<EventTrigger>();
        trigger.triggers ??= new System.Collections.Generic.List<EventTrigger.Entry>();
        trigger.triggers.Clear();

        Add(trigger, EventTriggerType.PointerDown, _ => OnHoldStart());
        Add(trigger, EventTriggerType.PointerUp, _ => OnHoldEnd());

        Add(trigger, EventTriggerType.BeginDrag, _ => OnHoldEnd());
        Add(trigger, EventTriggerType.Drag, _ => OnHoldEnd());
        Add(trigger, EventTriggerType.EndDrag, _ => OnHoldEnd());
    }

    private void Add(EventTrigger trigger, EventTriggerType type, System.Action<BaseEventData> cb)
    {
        var entry = new EventTrigger.Entry { eventID = type };
        entry.callback.AddListener((data) => cb(data));
        trigger.triggers.Add(entry);
    }

    private void OnHoldStart()
    {
        if (_owner == null) return;

        var anchor = btnShowChiTiet != null
            ? btnShowChiTiet.GetComponent<RectTransform>()
            : (transform as RectTransform);

        _owner.ShowTooltip(itemId, anchor);
    }

    private void OnHoldEnd()
    {
        if (_owner == null) return;
        _owner.HideTooltip();
    }

    private void LoadIcon(int id)
    {
        if (imgTrangBi == null)
        {
            Debug.LogWarning($"[ItemTrangBi] imgTrangBi null (id={id})");
            return;
        }

        string path = $"Sprites/Item/{id}";
        Sprite sp = Resources.Load<Sprite>(path);

        if (sp == null)
        {
            Debug.LogWarning($"[ItemTrangBi] Không tìm thấy sprite: {path}");
            imgTrangBi.enabled = false;
            return;
        }

        imgTrangBi.sprite = sp;
        imgTrangBi.enabled = true;
    }
}