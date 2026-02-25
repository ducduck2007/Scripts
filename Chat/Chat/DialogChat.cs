using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogChat : ScaleScreen
{
    [Header("Top Buttons")]
    public Button btnClose, btnExit;

    [Header("Tabs Panels")]
    public GameObject panelWorld;   // GameObjectChat/ChucNang/ChatTheGioi
    public GameObject panelFriend;  // GameObjectChat/ChucNang/ChatBanBe

    [Header("World Chat UI")]
    public TMP_InputField inputChat;     // (dùng chung) GameObjectChat/InputChat/.../InputField (TMP)
    public Button btnSend;               // (dùng chung) GameObjectChat/btnSend
    public ScrollRect scrollWorld;       // GameObjectChat/ChucNang/ChatTheGioi/Scroll View
    public RectTransform contentWorld;   // .../Viewport/Content
    public GameObject itemTemplate;      // ItemChatTheGioiOther (template)

    [Header("Friend Chat UI")]
    public ScrollRect scrollFriend;          // GameObjectChat/ChucNang/ChatBanBe/Scroll View Chat Friend
    public RectTransform contentFriend;      // content chat friend
    public GameObject itemChatBanBeMe;       // template message me
    public GameObject itemChatBanBeOther;    // template message other

    [Header("Friend List UI")]
    public ScrollRect scrollFriendList;      // GameObjectChat/ChucNang/ChatBanBe/Scroll View List Friend
    public RectTransform contentFriendList;  // content list friend
    public GameObject itemFriendChatOnline;  // template friend item

    // =========================
    // State
    // =========================
    private bool _subscribedWorld;
    private bool _wired;
    private long _selectedFriendId = -1;

    // friendId -> name
    private readonly Dictionary<long, string> _friendNameCache = new Dictionary<long, string>();

    protected override void OnEnable()
    {
        base.OnEnable();
        AgentUnity.SetPositionGameObjectUI(transform, new Vector3(0, 0), true, 1f);

        EnsureRefs();
        WireUIOnce();
        HookWorldEvents(true);

        RebuildWorldFromCache();
        BuildFriendList();
        ScrollWorldToBottom();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        HookWorldEvents(false);
    }

    protected override void Start()
    {
        base.Start();
        EnsureRefs();
        WireUIOnce();
    }

    // =========================================================
    // REFS
    // =========================================================
    private void EnsureRefs()
    {
        // ---------- TOP ----------
        if (btnClose == null)
        {
            var t = transform.Find("btnClose");
            if (t) btnClose = t.GetComponent<Button>();
        }

        if (btnExit == null)
        {
            var t = transform.Find("GameObjectChat/btnExit");
            if (t) btnExit = t.GetComponent<Button>();
        }

        // ---------- PANELS ----------
        if (panelWorld == null)
        {
            var t = transform.Find("GameObjectChat/ChucNang/ChatTheGioi");
            if (t) panelWorld = t.gameObject;
        }

        if (panelFriend == null)
        {
            var t = transform.Find("GameObjectChat/ChucNang/ChatBanBe");
            if (t) panelFriend = t.gameObject;
        }

        // ---------- SHARED INPUT + SEND ----------
        if (inputChat == null)
        {
            var t = transform.Find("GameObjectChat/InputChat/objInput/imgInput/InputField (TMP)");
            if (t) inputChat = t.GetComponent<TMP_InputField>();
        }

        if (btnSend == null)
        {
            var t = transform.Find("GameObjectChat/btnSend");
            if (t) btnSend = t.GetComponent<Button>();
        }

        // ---------- WORLD ----------
        if (scrollWorld == null)
        {
            var t = transform.Find("GameObjectChat/ChucNang/ChatTheGioi/Scroll View");
            if (t) scrollWorld = t.GetComponent<ScrollRect>();
        }

        if (contentWorld == null && scrollWorld != null) contentWorld = scrollWorld.content;

        if (itemTemplate == null)
        {
            var t = transform.Find("GameObjectChat/ChucNang/ChatTheGioi/Scroll View/Viewport/Content/ItemChatTheGioiOther");
            if (t) itemTemplate = t.gameObject;
        }

        if (itemTemplate != null && itemTemplate.activeSelf)
            itemTemplate.SetActive(false);

        // ---------- FRIEND LIST ----------
        if (scrollFriendList == null)
        {
            var t = transform.Find("GameObjectChat/ChucNang/ChatBanBe/Scroll View List Friend");
            if (t) scrollFriendList = t.GetComponent<ScrollRect>();
        }

        if (contentFriendList == null && scrollFriendList != null)
            contentFriendList = scrollFriendList.content;

        if (itemFriendChatOnline == null)
        {
            var t = transform.Find("GameObjectChat/ChucNang/ChatBanBe/Scroll View List Friend/Viewport/Content/ItemFriendChatOnline");
            if (t) itemFriendChatOnline = t.gameObject;
        }

        if (itemFriendChatOnline != null && itemFriendChatOnline.activeSelf)
            itemFriendChatOnline.SetActive(false);

        // ---------- FRIEND CHAT ----------
        if (scrollFriend == null)
        {
            var t = transform.Find("GameObjectChat/ChucNang/ChatBanBe/Scroll View Chat Friend");
            if (t) scrollFriend = t.GetComponent<ScrollRect>();
        }

        if (contentFriend == null && scrollFriend != null)
            contentFriend = scrollFriend.content;
    }

    // =========================================================
    // WIRE
    // =========================================================
    private void WireUIOnce()
    {
        if (_wired) return;
        _wired = true;

        if (btnExit) btnExit.onClick.AddListener(SetExit);
        if (btnClose) btnClose.onClick.AddListener(SetExit);

        // UI dùng chung 1 input + 1 send => bind 1 handler "smart"
        if (btnSend)
        {
            btnSend.onClick.RemoveAllListeners();
            btnSend.onClick.AddListener(OnClickSendSmart);
        }

        if (inputChat)
        {
            inputChat.onSubmit.RemoveAllListeners();
            inputChat.onSubmit.AddListener(_ => OnSubmitSendSmart());
        }
    }

    // =========================================================
    // WORLD CHAT
    // =========================================================
    private void HookWorldEvents(bool on)
    {
        if (on && !_subscribedWorld)
        {
            ChatWorldDataBase.OnMessageAdded += OnWorldMessageAdded;
            _subscribedWorld = true;
        }
        else if (!on && _subscribedWorld)
        {
            ChatWorldDataBase.OnMessageAdded -= OnWorldMessageAdded;
            _subscribedWorld = false;
        }
    }

    private void RebuildWorldFromCache()
    {
        if (contentWorld == null || itemTemplate == null) return;

        for (int i = contentWorld.childCount - 1; i >= 0; i--)
        {
            var child = contentWorld.GetChild(i).gameObject;
            if (child == itemTemplate) continue;
            Destroy(child);
        }

        for (int i = 0; i < ChatWorldDataBase.Messages.Count; i++)
        {
            CreateWorldItem(ChatWorldDataBase.Messages[i]);
        }
    }

    private void OnWorldMessageAdded(ChatWorldMessage m)
    {
        if (!gameObject.activeInHierarchy) return;
        if (contentWorld == null || itemTemplate == null) return;

        CreateWorldItem(m);
        ScrollWorldToBottom();
    }

    private void CreateWorldItem(ChatWorldMessage m)
    {
        var go = Instantiate(itemTemplate, contentWorld);
        go.SetActive(true);

        var ui = go.GetComponent<ItemChatTheGioiOtherUI>();
        if (ui == null) ui = go.AddComponent<ItemChatTheGioiOtherUI>();
        ui.SetData(m);
    }

    private void ScrollWorldToBottom()
    {
        if (scrollWorld == null) return;
        Canvas.ForceUpdateCanvases();
        scrollWorld.verticalNormalizedPosition = 0f;
        Canvas.ForceUpdateCanvases();
    }

    // =========================================================
    // FRIEND LIST + SELECT
    // =========================================================
    private void BuildFriendList()
    {
        var list = FriendDataBase.Instance?.ListDataFriend;

        if (contentFriendList == null || itemFriendChatOnline == null) return;
        if (list == null || list.Count == 0) return;

        _friendNameCache.Clear();
        foreach (var f in list)
            _friendNameCache[f.idNguoiChoi] = f.tenHienThi;

        // clear cũ (trừ template)
        for (int i = contentFriendList.childCount - 1; i >= 0; i--)
        {
            var child = contentFriendList.GetChild(i).gameObject;
            if (child == itemFriendChatOnline) continue;
            Destroy(child);
        }

        // online trước
        list.Sort((a, b) => b.isOnline.CompareTo(a.isOnline));

        foreach (var f in list)
        {
            var go = Instantiate(itemFriendChatOnline, contentFriendList);
            go.SetActive(true);

            var item = go.GetComponent<ItemFriendChatOnline>();
            if (item == null) item = go.AddComponent<ItemFriendChatOnline>();
            item.SetData(f);

            var btn = go.GetComponent<Button>();
            if (btn != null)
            {
                long id = f.idNguoiChoi;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => SelectFriend(id));
            }
        }

        Canvas.ForceUpdateCanvases();
        if (scrollFriendList) scrollFriendList.verticalNormalizedPosition = 1f;
    }

    private void SelectFriend(long friendId)
    {
        _selectedFriendId = friendId;

        ClearFriendChatItems();
        SendData.OnReadChatFriend(friendId);

        if (inputChat) inputChat.ActivateInputField();
    }

    private void ClearFriendChatItems()
    {
        if (contentFriend == null) return;

        for (int i = contentFriend.childCount - 1; i >= 0; i--)
            Destroy(contentFriend.GetChild(i).gameObject);

        Canvas.ForceUpdateCanvases();
        if (scrollFriend) scrollFriend.verticalNormalizedPosition = 0f;
    }

    private void ScrollFriendToBottom()
    {
        if (scrollFriend == null) return;
        Canvas.ForceUpdateCanvases();
        scrollFriend.verticalNormalizedPosition = 0f;
        Canvas.ForceUpdateCanvases();
    }

    // =========================================================
    // SMART SEND (WORLD OR FRIEND)
    // =========================================================
    private void OnClickSendSmart()
    {
        AudioManager.Instance.AudioClick();
        TrySendSmart();
    }

    private void OnSubmitSendSmart()
    {
        TrySendSmart();
        if (inputChat) inputChat.ActivateInputField();
    }

    private void TrySendSmart()
    {
        if (inputChat == null) return;

        string content = inputChat.text;
        if (string.IsNullOrWhiteSpace(content)) return;

        content = content.Trim();
        if (content.Length > 500) content = content.Substring(0, 500);

        bool isFriendTab = (panelFriend != null && panelFriend.activeInHierarchy);

        if (isFriendTab)
        {
            if (_selectedFriendId <= 0)
            {
                Debug.LogWarning("[ChatFriend] Chưa chọn bạn để chat.");
                return;
            }

            SendData.OnChatFriend(_selectedFriendId, content);

            // optimistic render (me)
            AddFriendChatLine(
                isMe: true,
                fromUserId: B.Instance.UserIdCong,
                displayName: "Me",
                content: content,
                timestampMs: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

            ScrollFriendToBottom();
        }
        else
        {
            SendData.OnChatTheGioi(content);
        }

        inputChat.text = "";
        inputChat.ForceLabelUpdate();
    }

    // =========================================================
    // FRIEND CHAT RENDER API (handler gọi vào)
    // =========================================================
    public void OnFriendMessageIncoming(long fromUserId, long toUserId, string content, long timestampMs)
    {
        bool belongs =
            (_selectedFriendId > 0) &&
            (fromUserId == _selectedFriendId || toUserId == _selectedFriendId);

        if (!belongs) return;

        bool isMe = (fromUserId == B.Instance.UserIdCong);
        long otherId = isMe ? toUserId : fromUserId;

        string name = isMe ? "Me" : (_friendNameCache.TryGetValue(otherId, out var n) ? n : "Friend");

        AddFriendChatLine(isMe, fromUserId, name, content, timestampMs);
        ScrollFriendToBottom();
    }

    public void OnFriendHistoryLoaded(List<(long fromUserId, long toUserId, string content, long timestampMs)> messages)
    {
        ClearFriendChatItems();

        if (messages == null || messages.Count == 0) return;

        foreach (var m in messages)
        {
            bool isMe = (m.fromUserId == B.Instance.UserIdCong);
            long otherId = isMe ? m.toUserId : m.fromUserId;

            string name = isMe ? "Me" : (_friendNameCache.TryGetValue(otherId, out var n) ? n : "Friend");

            AddFriendChatLine(isMe, m.fromUserId, name, m.content, m.timestampMs);
        }

        ScrollFriendToBottom();
    }

    private void AddFriendChatLine(bool isMe, long fromUserId, string displayName, string content, long timestampMs)
    {
        if (contentFriend == null) return;

        var prefab = isMe ? itemChatBanBeMe : itemChatBanBeOther;
        if (prefab == null) return;

        var go = Instantiate(prefab, contentFriend);
        go.SetActive(true);

        if (isMe)
        {
            var ui = go.GetComponent<ItemChatBanBeMe>();
            if (ui == null) ui = go.AddComponent<ItemChatBanBeMe>();
            ui.SetData(content, timestampMs);
        }
        else
        {
            var ui = go.GetComponent<ItemChatBanBeOther>();
            if (ui == null) ui = go.AddComponent<ItemChatBanBeOther>();
            ui.SetData(displayName, content, timestampMs);
        }
    }

    // =========================================================
    // EXIT
    // =========================================================
    private void SetExit()
    {
        AudioManager.Instance.AudioClick();
        transform.DOLocalMove(new Vector3(-1200f, 0f, 0f), 1f)
            .OnComplete(() => { Show(false); });
    }

    public void Show(bool val = true)
    {
        gameObject.SetActive(val);
    }
}
