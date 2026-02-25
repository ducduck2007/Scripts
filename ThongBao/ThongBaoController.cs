using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ThongBaoController : ManualSingleton<ThongBaoController>
{
    private GameObject Load(string namePath)
    {
        return Resources.Load(namePath) as GameObject;
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);

        // Đảm bảo luôn render trên cùng
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null) canvas.sortingOrder = 999;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CloseLoading();
    }

    private void CloseLoading()
    {
        if (_loadVaoTran != null)
        {
            Destroy(_loadVaoTran.gameObject);
            _loadVaoTran = null;
        }
    }

    // ========== LOADING VÀO TRẬN ==========
    private LoadVaoTran _loadVaoTran;
    public LoadVaoTran LoadVaoTran
    {
        get
        {
            if (_loadVaoTran == null)
                _loadVaoTran = AgentUnity.InstanceObject<LoadVaoTran>(Load(PathResource.LoadVaoTran), transform);
            _loadVaoTran.transform.SetAsLastSibling();
            return _loadVaoTran;
        }
    }

    // ========== POPUP ONE BUTTON ==========
    private SD_PopupOneButton _PopupOneButton;
    public SD_PopupOneButton PopupOneButton
    {
        get
        {
            if (_PopupOneButton == null)
                _PopupOneButton = AgentUnity.InstanceObject<SD_PopupOneButton>(Load(PathResource.PopupOneButton), transform);
            _PopupOneButton.transform.SetAsLastSibling();
            return _PopupOneButton;
        }
    }

    // ========== POPUP TWO BUTTON ==========
    private SD_PopupTwoButton _PopupTwoButton;
    public SD_PopupTwoButton PopupTwoButton
    {
        get
        {
            if (_PopupTwoButton == null)
                _PopupTwoButton = AgentUnity.InstanceObject<SD_PopupTwoButton>(Load(PathResource.PopupTwoButton), transform);
            _PopupTwoButton.transform.SetAsLastSibling();
            return _PopupTwoButton;
        }
    }

    // ========== TOAST ==========
    private SD_Toast _toast;
    internal SD_Toast Toast
    {
        get
        {
            if (_toast == null)
                _toast = AgentUnity.InstanceObject<SD_Toast>(Load(PathResource.SD_Toast), transform);
            return _toast;
        }
    }

    // ========== LOAD MẠNG ==========
    private LoadMang _loadMang;
    internal LoadMang LoadMang
    {
        get
        {
            if (_loadMang == null)
                _loadMang = AgentUnity.InstanceObject<LoadMang>(Load(PathResource.LoadMang), transform);
            return _loadMang;
        }
    }

    // ========== THÔNG BÁO NHANH (Toast) ==========
    public void ShowThongBaoNhanh(string content)
    {
        if (string.IsNullOrEmpty(content)) return;
        Toast.ShowToast(content);
    }

    public void ShowToast(string content)
    {
        if (string.IsNullOrEmpty(content)) return;
        Toast.ShowToast(content);
    }

    public void MakeToast(string content)
    {
        if (string.IsNullOrEmpty(content)) return;
        Toast.MakeToast(content);
    }

    // ========== POPUP LỜI MỜI VÀO PARTY ==========
    // private LoiMoiVaoParty _loiMoiVaoParty;
    // public LoiMoiVaoParty LoiMoiVaoParty
    // {
    //     get
    //     {
    //         if (_loiMoiVaoParty == null)
    //             _loiMoiVaoParty = AgentUnity.InstanceObject<LoiMoiVaoParty>(Load(PathResource.LoiMoiVaoParty), transform);
    //         _loiMoiVaoParty.transform.SetAsLastSibling();
    //         return _loiMoiVaoParty;
    //     }
    // }

    // public void ShowLoiMoiVaoParty(int partyId, string inviterName, long inviterId, int memberCount, int maxMembers)
    // {
    //     LoiMoiVaoParty.SetInfo(partyId, inviterName, inviterId, memberCount, maxMembers);
    // }

    // ========== POPUP PARTY MATCH FOUND ==========
    private PopupPartyMatchFound _partyMatchFound;
    public PopupPartyMatchFound PartyMatchFound
    {
        get
        {
            if (_partyMatchFound == null)
                _partyMatchFound = AgentUnity.InstanceObject<PopupPartyMatchFound>(Load(PathResource.PopupPartyMatchFound), transform);
            _partyMatchFound.transform.SetAsLastSibling();
            return _partyMatchFound;
        }
    }

    public void ShowPartyMatchFound()
    {
        PartyMatchFound.ShowMatchFound();
    }

    // ========== PARTY INVITE (PopupTwoButton) ==========
    public void ShowPartyInviteTwoButton(int partyId, string inviterName, long inviterId, int memberCount, int maxMembers)
    {
        string title = "Lời mời vào nhóm";
        string content = $"{inviterName} mời bạn vào party ({memberCount}/{maxMembers}).\nBạn có muốn tham gia không?";

        PopupTwoButton.ShowPopupTwoButton(
            title,
            content,
            "Từ chối",
            actionOk: () =>
            {
                // ✅ báo trước: khi server trả CMD 95 thì mở PopupTimTran
                PartyDataBase.Instance.PendingOpenPopupTimTran = true;

                // CMD 95 request
                SendData.AcceptPartyInvite(partyId);
            },
            actionExit: () =>
            {
                SendData.DeclinePartyInvite(partyId);
            }
        );
    }

}