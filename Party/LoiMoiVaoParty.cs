using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoiMoiVaoParty : ScaleScreen
{
    [SerializeField] private TextMeshProUGUI txtContent;
    [SerializeField] private Button btnDongY, btnHuy, btnClose;

    private int _partyId = -1;
    private string _inviterName = "";
    private long _inviterId = 0;
    private int _memberCount = 0;
    private int _maxMembers = 5;

    protected override void Start()
    {
        base.Start();

        if (btnDongY) btnDongY.onClick.AddListener(OnAccept);
        if (btnHuy) btnHuy.onClick.AddListener(OnDecline);
        if (btnClose) btnClose.onClick.AddListener(OnDecline);
    }

    public void SetInfo(int partyId, string inviterName, long inviterId, int memberCount, int maxMembers)
    {
        _partyId = partyId;
        _inviterName = inviterName ?? "";
        _inviterId = inviterId;
        _memberCount = memberCount;
        _maxMembers = maxMembers;

        UpdateUI();
        Show(true);
    }

    private void UpdateUI()
    {
        if (txtContent)
        {
            string who = string.IsNullOrEmpty(_inviterName) ? "Một người chơi" : _inviterName;
            txtContent.text = $"{who} mời bạn vào Party ({_memberCount}/{_maxMembers})";
        }
    }

    private void OnAccept()
    {
        AudioManager.Instance.AudioClick();

        if (_partyId > 0)
        {
            SendData.AcceptPartyInvite(_partyId);
        }

        Show(false);
    }

    private void OnDecline()
    {
        AudioManager.Instance.AudioClick();

        if (_partyId > 0)
        {
            SendData.DeclinePartyInvite(_partyId);
        }

        Show(false);
    }

    public void Show(bool val = true)
    {
        gameObject.SetActive(val);
    }
}