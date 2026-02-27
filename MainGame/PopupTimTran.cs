using TMPro;
using UIPool;
using UnityEngine;
using UnityEngine.UI;

public class PopupTimTran : ScaleScreen
{
    [Header("Buttons")]
    public Button btnBack, btnHome;
    public Button btnTimTran, btnHuyGhep;

    [Header("UI Elements")]
    public TextMeshProUGUI txtIdPhong, txtTimeTim, txtTrangThaiTim;
    public GameObject objDemTime, objHuBtn;
    public ItemPlayerGhepTran[] itemPlayers;
    public GridPoolGroup gridPoolGroup;

    [Header("Party Info Display")]
    public TextMeshProUGUI txtPartyInfo;
    public GameObject objPartyPanel;

    private float timeWaiting;
    private bool isFindingMatch;

    private int _teamSize = 1;
    private int _modeId = 1;
    private bool _isPartyMode = false;
    private bool _isSubscribed = false;

    protected override void Start()
    {
        base.Start();

        // Button listeners - Safe check
        if (btnBack) btnBack.onClick.AddListener(ClickBack);
        if (btnHome) btnHome.onClick.AddListener(ClickHome);
        if (btnTimTran) btnTimTran.onClick.AddListener(ClickTimTran);
        if (btnHuyGhep) btnHuyGhep.onClick.AddListener(ClickHuyTimTran);

        if (MatchFoundDataBase.Instance != null)
        {
            MatchFoundDataBase.Instance.OnMatchFound += OnMatchFoundEvent;
            MatchFoundDataBase.Instance.OnMatchReady += OnMatchReadyEvent;
        }
    }

    private void OnMatchFoundEvent()
    {
        // Optional: Hiển thị thông báo đang chờ
        Debug.Log("[PopupTimTran] Match found, waiting for all players...");

        if (txtTrangThaiTim)
        {
            txtTrangThaiTim.text = "Đã tìm thấy trận, đang chờ...";
        }

        // Có thể show toast
        if (ThongBaoController.Instance)
        {
            ThongBaoController.Instance.ShowThongBaoNhanh("Đã tìm thấy trận đấu!");
        }
    }

    private void OnMatchReadyEvent()
    {
        // Match ready, popup sẽ tự đóng khi CMD 106 mở màn chọn tướng
        Debug.Log("[PopupTimTran] Match ready!");
    }
    protected override void OnEnable()
    {
        base.OnEnable();

        // Reset state
        if (btnTimTran) btnTimTran.interactable = true;
        isFindingMatch = false;
        if (txtTrangThaiTim) txtTrangThaiTim.text = "Sẵn sàng";
        if (objDemTime) objDemTime.SetActive(false);
        if (objHuBtn) objHuBtn.SetActive(false);

        // Apply UI
        ApplyModeUI();
        SetPlayerData();
        SetData(true);

        // ✅ Subscribe to party events (safe)
        SubscribePartyEvents();
    }

    // ✅ Unsubscribe khi OnDisable
    protected override void OnDisable()
    {
        base.OnDisable();
        UnsubscribePartyEvents();
    }

    // ✅ Cleanup khi destroy
    private void OnDestroy()
    {
        UnsubscribePartyEvents();

        // Unsubscribe match events
        if (MatchFoundDataBase.Instance != null)
        {
            MatchFoundDataBase.Instance.OnMatchFound -= OnMatchFoundEvent;
            MatchFoundDataBase.Instance.OnMatchReady -= OnMatchReadyEvent;
        }
    }

    // ========== SAFE SUBSCRIBE/UNSUBSCRIBE ==========
    private void SubscribePartyEvents()
    {
        // ✅ Check if already subscribed
        if (_isSubscribed) return;

        // ✅ Safe null check
        if (PartyDataBase.Instance != null)
        {
            PartyDataBase.Instance.OnPartyChanged += OnPartyDataChanged;
            PartyDataBase.Instance.OnPartyDisbanded += OnPartyDisbanded;
            _isSubscribed = true;
        }
    }

    private void UnsubscribePartyEvents()
    {
        // ✅ Check if subscribed
        if (!_isSubscribed) return;

        // ✅ Safe null check
        if (PartyDataBase.Instance != null)
        {
            PartyDataBase.Instance.OnPartyChanged -= OnPartyDataChanged;
            PartyDataBase.Instance.OnPartyDisbanded -= OnPartyDisbanded;
        }

        _isSubscribed = false;
    }

    // ========== PUBLIC METHODS - Set Mode ==========
    public void SetModeThuong(int teamSize)
    {
        _teamSize = teamSize <= 1 ? 1 : (teamSize <= 3 ? 3 : 5);

        // Map mode IDs cho map 3 đường (theo server: modeId 4,5,6)
        if (_teamSize == 1) _modeId = 4;      // 1v1 map 3 đường
        else if (_teamSize == 3) _modeId = 5; // 3v3 map 3 đường
        else _modeId = 6;                     // 5v5 map 3 đường

        _isPartyMode = true;
        ApplyModeUI();
    }

    public void SetPartyMode(int modeId, int teamSize)
    {
        _modeId = modeId;
        _teamSize = teamSize <= 1 ? 1 : (teamSize <= 3 ? 3 : 5);
        _isPartyMode = true;
        ApplyModeUI();
    }

    // ========== UI UPDATE ==========
    private void ApplyModeUI()
    {
        // ✅ Safe null check
        if (itemPlayers != null && itemPlayers.Length > 0)
        {
            for (int i = 0; i < itemPlayers.Length; i++)
            {
                if (itemPlayers[i] != null)
                {
                    itemPlayers[i].gameObject.SetActive(i < _teamSize);
                }
            }
        }

        UpdatePartyInfoDisplay();
    }

    private void UpdatePartyInfoDisplay()
    {
        // ✅ Safe null checks
        if (txtPartyInfo)
        {
            if (PartyDataBase.Instance != null && PartyDataBase.Instance.IsInParty)
            {
                txtPartyInfo.text = $"Party: {PartyDataBase.Instance.MemberCount}/{PartyDataBase.Instance.MaxMembers}";
            }
            else
            {
                txtPartyInfo.text = "Solo";
            }
        }

        if (objPartyPanel)
        {
            bool showPanel = PartyDataBase.Instance != null
                          && PartyDataBase.Instance.IsInParty
                          && PartyDataBase.Instance.MemberCount > 1;
            objPartyPanel.SetActive(showPanel);
        }
    }

    // ========== BUTTON CLICKS ==========
    private void ClickBack()
    {
        if (AudioManager.Instance) AudioManager.Instance.AudioClick();

        if (PartyDataBase.Instance != null && PartyDataBase.Instance.IsInParty)
        {
            if (isFindingMatch)
            {
                SendData.PartyCancelFind();
            }

            SendData.LeaveParty();
        }

        if (DialogController.Instance && DialogController.Instance.DialogChonPhong)
        {
            DialogController.Instance.DialogChonPhong.Show(true);
        }

        Show(false);
    }


    private void ClickHome()
    {
        if (AudioManager.Instance) AudioManager.Instance.AudioClick();

        if (PartyDataBase.Instance != null && PartyDataBase.Instance.IsInParty)
        {
            if (isFindingMatch)
            {
                SendData.PartyCancelFind();
            }

            SendData.LeaveParty();
        }

        if (UiControl.Instance && UiControl.Instance.MainGame1)
        {
            UiControl.Instance.MainGame1.Show(true);
        }

        if (DialogController.Instance && DialogController.Instance.DialogChonPhong)
        {
            DialogController.Instance.DialogChonPhong.Show(false);
        }

        Show(false);
    }
    private void ClickTimTran()
    {
        if (AudioManager.Instance) AudioManager.Instance.AudioClick();

        if (_isPartyMode)
        {
            // ✅ PartyDataBase.Instance không bao giờ null
            if (!PartyDataBase.Instance.IsInParty)
            {
                Debug.Log("[PopupTimTran] Auto creating party");
                SendData.CreateParty();
                StartCoroutine(DelayedFindMatch());
            }
            else
            {
                if (!PartyDataBase.Instance.IsLeader(UserData.Instance.UserID))
                {
                    if (ThongBaoController.Instance)
                    {
                        ThongBaoController.Instance.ShowThongBaoNhanh("Chỉ trưởng nhóm mới có thể tìm trận!");
                    }
                    return;
                }

                SendData.PartyFindMatch(_modeId);
                StartFindingMatchUI();
            }
        }
        else
        {
            SendData.FindMatch();
            StartFindingMatchUI();
        }
    }

    private System.Collections.IEnumerator DelayedFindMatch()
    {
        yield return new WaitForSeconds(0.5f);

        if (PartyDataBase.Instance != null && PartyDataBase.Instance.IsInParty)
        {
            SendData.PartyFindMatch(_modeId);
            StartFindingMatchUI();
        }
        else
        {
            if (ThongBaoController.Instance)
            {
                ThongBaoController.Instance.ShowThongBaoNhanh("Không thể tạo party!");
            }
        }
    }

    private void ClickHuyTimTran()
    {
        if (AudioManager.Instance) AudioManager.Instance.AudioClick();
        SendData.PartyCancelFind();
        StopFindingMatchUI();
    }

    // ========== FINDING MATCH UI STATE ==========
    private void StartFindingMatchUI()
    {
        if (btnTimTran) btnTimTran.interactable = false;
        if (objHuBtn) objHuBtn.SetActive(true);
        if (objDemTime) objDemTime.SetActive(true);
        if (txtTrangThaiTim) txtTrangThaiTim.text = "Đang ghép";

        isFindingMatch = true;
        timeWaiting = 0f;
        UpdateTimerUI();
    }

    public void StopFindingMatchUI()
    {
        if (btnTimTran) btnTimTran.interactable = true;
        if (objHuBtn) objHuBtn.SetActive(false);
        if (objDemTime) objDemTime.SetActive(false);
        if (txtTrangThaiTim) txtTrangThaiTim.text = "Sẵn sàng";

        isFindingMatch = false;
    }

    // ========== TIMER UPDATE ==========
    private void Update()
    {
        if (isFindingMatch)
        {
            timeWaiting += Time.deltaTime;
            UpdateTimerUI();
        }
    }

    private void UpdateTimerUI()
    {
        if (txtTimeTim)
        {
            int totalSeconds = Mathf.FloorToInt(timeWaiting);
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            txtTimeTim.text = $"{minutes:00}:{seconds:00}";
        }
    }

    // ========== DATA SETUP ==========
    public void SetData(bool isLoadFirst)
    {
        // ✅ Safe null checks
        if (FriendDataBase.Instance == null || FriendDataBase.Instance.ListDataFriend == null)
        {
            Debug.LogWarning("[PopupTimTran] FriendDataBase not ready");
            return;
        }

        FriendDataBase.Instance.ListDataFriend.Sort((t1, t2) => t2.isOnline.CompareTo(t1.isOnline));

        InitPool();

        if (gridPoolGroup)
        {
            gridPoolGroup.SetAdapter(
                AgentUIPool.GetListObject<DataFriend>(FriendDataBase.Instance.ListDataFriend),
                isLoadFirst
            );
        }
    }

    private void InitPool()
    {
        if (gridPoolGroup == null) return;

        gridPoolGroup.HowToUseCellData(delegate (GameObject go, object data)
        {
            ItemBanBeTimTran item = go.GetComponent<ItemBanBeTimTran>();
            if (item != null)
            {
                item.SetInfo((DataFriend)data);
            }
        });
    }

    public void SetIdPhong(string idPhong)
    {
        if (txtIdPhong)
        {
            txtIdPhong.text = idPhong;
        }
    }

    public void SetPlayerData()
    {
        if (itemPlayers == null || itemPlayers.Length == 0) return;

        // ✅ Safe check UserData
        if (UserData.Instance == null)
        {
            Debug.LogWarning("[PopupTimTran] UserData.Instance is null");
            return;
        }

        // Set current user data
        if (itemPlayers[0] != null)
        {
            itemPlayers[0].SetData(
                new ThongTinPlayer(
                    UserData.Instance.UserID,
                    UserData.Instance.UserName,
                    UserData.Instance.Level,
                    UserData.Instance.AvatarId
                ),
                true
            );
        }

        // Load party members if in party
        if (PartyDataBase.Instance != null && PartyDataBase.Instance.IsInParty)
        {
            UpdatePartyMembersDisplay();
        }
    }

    private void UpdatePartyMembersDisplay()
    {
        if (itemPlayers == null || PartyDataBase.Instance == null) return;
        if (UserData.Instance == null) return;

        var members = PartyDataBase.Instance.Members;

        int slotIndex = 1;
        foreach (var member in members)
        {
            if (member.idThanhVien == UserData.Instance.UserID) continue;

            if (slotIndex < itemPlayers.Length && itemPlayers[slotIndex] != null)
            {
                itemPlayers[slotIndex].SetData(
                    new ThongTinPlayer(
                        member.idThanhVien,
                        member.tenThanhVien,
                        member.capDoThanhVien,
                        member.anhDaiDienThanhVien
                    ),
                    true
                );

                slotIndex++;
            }
        }

        // Clear remaining slots
        for (int i = slotIndex; i < itemPlayers.Length; i++)
        {
            if (itemPlayers[i] != null)
            {
                itemPlayers[i].SetData(null, false);
            }
        }
    }

    // ========== PARTY EVENTS ==========
    private void OnPartyDataChanged()
    {
        UpdatePartyInfoDisplay();
        UpdatePartyMembersDisplay();
    }

    private void OnPartyDisbanded()
    {
        if (isFindingMatch)
        {
            StopFindingMatchUI();
        }

        UpdatePartyInfoDisplay();

        if (itemPlayers != null)
        {
            for (int i = 1; i < itemPlayers.Length; i++)
            {
                if (itemPlayers[i] != null)
                {
                    itemPlayers[i].SetData(null, false);
                }
            }
        }
    }

    // ========== SHOW/HIDE ==========
    public void Show(bool val = true)
    {
        gameObject.SetActive(val);
    }
}