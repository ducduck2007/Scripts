using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CanvasShopItemInGame : ScaleScreen
{
    public Button btnExit, btnMuaItem;
    public TextMeshProUGUI txtInfo, txtMuaBan, txtGold;

    public Image imgTrangBiDaMua1, imgTrangBiDaMua2, imgTrangBiDaMua3;
    public Image imgTrangBiDaMua4, imgTrangBiDaMua5, imgTrangBiDaMua6;

    public Button btnTrangBiDaMua1, btnTrangBiDaMua2, btnTrangBiDaMua3;
    public Button btnTrangBiDaMua4, btnTrangBiDaMua5, btnTrangBiDaMua6;

    [Header("Purchased Slot Border")]
    public Color purchasedSelectedBorderColor = Color.green;
    public float purchasedBorderWidth = 6f;

    [Header("Purchased Slot Border Blink")]
    public bool purchasedBorderBlink = true;
    public float purchasedBlinkSpeed = 3.5f;
    public float purchasedBlinkMinAlpha = 0.25f;

    private Outline[] _purchasedOutlines;
    private Coroutine _purchasedBlinkCo;

    public Transform content;
    public ItemTrangBi itemPrefab;

    public Color selectedBorderColor = new Color(0f, 1f, 1f, 1f);
    public float borderWidth = 10f;

    private readonly List<ItemTrangBi> spawned = new();
    private int _lastVersion = -1;

    private int _selectedItemId = -1;
    private ItemTrangBi _selectedItemUI;

    // ✅ FIX: slot cố định 0..5, không dùng List để tránh dồn slot
    private const int MAX_SLOTS = 6;
    private readonly int[] _purchasedItemIds = new int[MAX_SLOTS];

    private Image[] _purchasedSlots;
    private Button[] _purchasedButtons;

    private int _selectedPurchasedSlot = -1;

    private const string TEXT_MUA = "MUA";
    private const string TEXT_BAN = "BÁN";

    protected override void Start()
    {
        base.Start();
        if (btnExit != null) btnExit.onClick.AddListener(SetExit);
        if (btnMuaItem != null) btnMuaItem.onClick.AddListener(OnClickMuaBan);

        InitPurchasedSlots();
        ItemInfoCache.EnsureDiskLoaded(false);
        SetModeBuy();
        SetButton(false);
    }

    private void OnEnable()
    {
        base.OnEnable();

        ItemInfoCache.OnUpdated -= OnItemCacheUpdated;
        ItemInfoCache.OnUpdated += OnItemCacheUpdated;

        UdpResourceSnapshotSystem.OnPlayerResourceUpdated -= OnPlayerResourceUpdated;
        UdpResourceSnapshotSystem.OnPlayerResourceUpdated += OnPlayerResourceUpdated;

        ItemInfoCache.EnsureRequested(() => SendData.GetItemInfo(), force: false, debugLog: false);

        TryBuildIfNew(true);
        ClearSelections();

        SyncGoldFromHUD();
    }

    private void SyncGoldFromHUD()
    {
        if (txtGold == null) return;
        if (MenuController.Instance?.txtGold == null) return;
        txtGold.text = MenuController.Instance.txtGold.text;
    }

    private void OnDisable()
    {
        ItemInfoCache.OnUpdated -= OnItemCacheUpdated;
        UdpResourceSnapshotSystem.OnPlayerResourceUpdated -= OnPlayerResourceUpdated;
        ClearSelections();
    }

    private void OnPlayerResourceUpdated(PlayerResourceData data, bool isLocal)
    {
        if (!isLocal) return;
        if (txtGold == null) return;
        txtGold.text = data.gold.ToString();
    }

    private void OnItemCacheUpdated() => TryBuildIfNew(false);

    private void TryBuildIfNew(bool force)
    {
        if (!force && _lastVersion == ItemInfoCache.Version) return;
        _lastVersion = ItemInfoCache.Version;
        SetData();
    }

    private void SetExit()
    {
        AudioManager.Instance?.AudioClick();

        if (CanvasController.Instance != null)
            CanvasController.Instance.HideCanvas(gameObject);
        else
            gameObject.SetActive(false);
    }

    private void InitPurchasedSlots()
    {
        _purchasedSlots = new[]
        {
            imgTrangBiDaMua1, imgTrangBiDaMua2, imgTrangBiDaMua3,
            imgTrangBiDaMua4, imgTrangBiDaMua5, imgTrangBiDaMua6
        };

        _purchasedButtons = new[]
        {
            btnTrangBiDaMua1, btnTrangBiDaMua2, btnTrangBiDaMua3,
            btnTrangBiDaMua4, btnTrangBiDaMua5, btnTrangBiDaMua6
        };

        _purchasedOutlines = new Outline[_purchasedSlots.Length];

        for (int i = 0; i < _purchasedSlots.Length; i++)
        {
            if (_purchasedSlots[i] == null) continue;

            var ol = _purchasedSlots[i].GetComponent<Outline>();
            if (ol == null) ol = _purchasedSlots[i].gameObject.AddComponent<Outline>();

            ol.effectColor = purchasedSelectedBorderColor;
            ol.effectDistance = new Vector2(purchasedBorderWidth, purchasedBorderWidth);
            ol.enabled = false;

            _purchasedOutlines[i] = ol;
        }

        for (int i = 0; i < _purchasedButtons.Length; i++)
        {
            int idx = i;
            if (_purchasedButtons[i] == null) continue;
            _purchasedButtons[i].onClick.RemoveAllListeners();
            _purchasedButtons[i].onClick.AddListener(() => OnClickPurchased(idx));
        }

        ClearAllPurchasedSlots();
    }

    private void StopPurchasedBlink()
    {
        if (_purchasedBlinkCo != null)
        {
            StopCoroutine(_purchasedBlinkCo);
            _purchasedBlinkCo = null;
        }
    }

    private void StartPurchasedBlink(int slotIndex)
    {
        StopPurchasedBlink();
        _purchasedBlinkCo = StartCoroutine(CoBlinkPurchasedBorder(slotIndex));
    }

    private IEnumerator CoBlinkPurchasedBorder(int slotIndex)
    {
        while (true)
        {
            if (_purchasedOutlines == null || slotIndex < 0 || slotIndex >= _purchasedOutlines.Length)
                yield break;

            var ol = _purchasedOutlines[slotIndex];
            if (ol == null || !ol.enabled)
                yield break;

            float t = Mathf.PingPong(Time.unscaledTime * purchasedBlinkSpeed, 1f);
            float a = Mathf.Lerp(purchasedBlinkMinAlpha, 1f, t);

            var c = ol.effectColor;
            c.a = a;
            ol.effectColor = c;

            yield return null;
        }
    }

    private void ClearPurchasedBorders()
    {
        StopPurchasedBlink();

        if (_purchasedOutlines == null) return;
        for (int i = 0; i < _purchasedOutlines.Length; i++)
        {
            var ol = _purchasedOutlines[i];
            if (ol == null) continue;

            ol.enabled = false;

            var c = ol.effectColor;
            c.a = 1f;
            ol.effectColor = c;
        }
    }

    private void SetPurchasedBorder(int slotIndex, bool on)
    {
        if (_purchasedOutlines == null) return;
        if (slotIndex < 0 || slotIndex >= _purchasedOutlines.Length) return;

        var ol = _purchasedOutlines[slotIndex];
        if (ol == null) return;

        ol.enabled = on;

        if (on && purchasedBorderBlink) StartPurchasedBlink(slotIndex);
        else StopPurchasedBlink();
    }

    private void OnClickPurchased(int slotIndex)
    {
        if (!HasItemInSlot(slotIndex)) return;

        AudioManager.Instance?.AudioClick();

        ClearPurchasedBorders();
        SetPurchasedBorder(slotIndex, true);

        _selectedPurchasedSlot = slotIndex;

        if (_selectedItemUI != null)
        {
            _selectedItemUI.SetSelected(false);
            _selectedItemUI = null;
            _selectedItemId = -1;
        }

        int itemId = _purchasedItemIds[slotIndex];
        SetInfo(itemId);

        SetModeSell();
        SetButton(true);
    }

    private bool HasItemInSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= MAX_SLOTS) return false;
        return _purchasedItemIds[slotIndex] > 0;
    }

    private void OnClickMuaBan()
    {
        if (_selectedPurchasedSlot >= 0)
        {
            DoSell();
            return;
        }

        DoBuy();
    }

    private void DoBuy()
    {
        if (_selectedItemId <= 0) return;
        if (!CheckCanBuy(_selectedItemId)) return;

        AudioManager.Instance?.AudioClick();
        SendData.SendBuyItemInGame(_selectedItemId);
        SetButton(false);
    }

    private bool CheckCanBuy(int itemId)
    {
        // ✅ FIX: tìm slot trống thay vì dựa vào Count
        int empty = FindFirstEmptySlot();
        if (empty < 0)
        {
            ThongBaoController.Instance?.ShowThongBaoNhanh("Kho trang bị đã đầy!");
            return false;
        }

        if (!ItemInfoCache.TryGet(itemId, out var d) || d == null) return true;

        int gold = GetGold();
        if (gold < d.giaMua)
        {
            ThongBaoController.Instance?.ShowThongBaoNhanh($"Không đủ vàng! Cần {d.giaMua}, hiện có {gold}");
            return false;
        }

        return true;
    }

    private int FindFirstEmptySlot()
    {
        for (int i = 0; i < MAX_SLOTS; i++)
            if (_purchasedItemIds[i] <= 0) return i;
        return -1;
    }

    private int GetGold()
    {
        if (txtGold != null && int.TryParse(txtGold.text, out int g)) return g;
        if (MenuController.Instance?.txtGold == null) return 0;
        return int.TryParse(MenuController.Instance.txtGold.text, out int g2) ? g2 : 0;
    }

    private void DoSell()
    {
        if (!HasItemInSlot(_selectedPurchasedSlot))
        {
            ClearSelections();
            return;
        }

        int itemId = _purchasedItemIds[_selectedPurchasedSlot];

        AudioManager.Instance?.AudioClick();

        // NOTE: Bạn đang gửi SELL theo idItem. Server trả slot về.
        // Quan trọng là UI/client không được "dồn slot" nữa -> đã fix.
        SendData.SendSellItemInGame(itemId);

        SetButton(false);
    }

    /// <summary>
    /// Server có thể trả slot cụ thể. Nếu không truyền slotIndex, client tự nhét vào slot trống.
    /// </summary>
    public void AddPurchasedItem(int itemId, int slotIndex = -1)
    {
        if (_purchasedSlots == null || _purchasedSlots.Length == 0) return;

        if (slotIndex < 0) slotIndex = FindFirstEmptySlot();
        if (slotIndex < 0 || slotIndex >= MAX_SLOTS) return;

        _purchasedItemIds[slotIndex] = itemId;
        LoadIcon(itemId, slotIndex);
    }

    // ✅ FIX: không RemoveAt nữa, chỉ clear đúng slot
    public void RemoveItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= MAX_SLOTS) return;

        _purchasedItemIds[slotIndex] = 0;
        RefreshPurchasedSlots();

        if (_selectedPurchasedSlot == slotIndex)
        {
            _selectedPurchasedSlot = -1;
            ClearPurchasedBorders();
            SetModeBuy();
            SetButton(_selectedItemId > 0);
            if (txtInfo != null) txtInfo.text = "";
        }
    }

    private void RefreshPurchasedSlots()
    {
        for (int i = 0; i < MAX_SLOTS; i++)
        {
            if (_purchasedSlots[i] == null) continue;

            int itemId = _purchasedItemIds[i];
            if (itemId > 0)
            {
                LoadIcon(itemId, i);
            }
            else
            {
                _purchasedSlots[i].sprite = null;
                _purchasedSlots[i].enabled = false;
            }
        }
    }

    private void LoadIcon(int itemId, int slotIndex)
    {
        var img = _purchasedSlots[slotIndex];
        if (img == null) return;

        Sprite sp = Resources.Load<Sprite>($"Sprites/Item/{itemId}");
        if (sp == null)
        {
            img.enabled = false;
            img.sprite = null;
            return;
        }

        img.sprite = sp;
        img.enabled = true;
    }

    private void ClearAllPurchasedSlots()
    {
        for (int i = 0; i < MAX_SLOTS; i++) _purchasedItemIds[i] = 0;
        RefreshPurchasedSlots();
    }

    public void SetData()
    {
        var list = ItemInfoCache.GetAllSorted();

        for (int i = 0; i < spawned.Count; i++)
            if (spawned[i] != null) Destroy(spawned[i].gameObject);
        spawned.Clear();

        if (content == null || itemPrefab == null) return;

        for (int i = 0; i < list.Count; i++)
        {
            var d = list[i];
            var it = Instantiate(itemPrefab, content);
            it.gameObject.SetActive(true);
            it.Init(d.idItem, d.nameItem);
            it.SetOwner(null);
            it.OnClickItem = OnItemClicked;
            spawned.Add(it);
        }

        if (list.Count > 0) OnItemClicked(list[0].idItem);
        else { if (txtInfo != null) txtInfo.text = ""; ClearSelections(); }
    }

    private void OnItemClicked(int itemId)
    {
        AudioManager.Instance?.AudioClick();

        _selectedPurchasedSlot = -1;
        ClearPurchasedBorders();

        SetInfo(itemId);
        SelectItem(itemId);

        SetModeBuy();
        SetButton(true);
    }

    private void SetInfo(int itemId)
    {
        if (txtInfo == null) return;
        if (!ItemInfoCache.TryGet(itemId, out var d) || d == null) { txtInfo.text = ""; return; }
        txtInfo.text = ItemTooltipUI.BuildText(d);
    }

    private void SelectItem(int itemId)
    {
        if (_selectedItemUI != null) _selectedItemUI.SetSelected(false);

        _selectedItemId = itemId;
        _selectedItemUI = spawned.Find(x => x != null && x.GetItemId() == itemId);

        if (_selectedItemUI != null)
            _selectedItemUI.SetSelected(true, selectedBorderColor, borderWidth);
    }

    private void ClearSelections()
    {
        if (_selectedItemUI != null) _selectedItemUI.SetSelected(false);
        _selectedItemUI = null;
        _selectedItemId = -1;

        _selectedPurchasedSlot = -1;
        ClearPurchasedBorders();

        if (txtInfo != null) txtInfo.text = "";
        SetModeBuy();
        SetButton(false);
    }

    private void SetModeBuy()
    {
        if (txtMuaBan != null) txtMuaBan.text = TEXT_MUA;
    }

    private void SetModeSell()
    {
        if (txtMuaBan != null) txtMuaBan.text = TEXT_BAN;
    }

    private void SetButton(bool enabled)
    {
        if (btnMuaItem != null) btnMuaItem.interactable = enabled;
    }
}
