using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupGhepTran : ScaleScreen
{
    public Button btnSanSang;
    public TextMeshProUGUI txtTime, txtSluong, txtTrangThai;
    public ItemPlayerGhepTran[] itemPlayers1;
    public ItemPlayerGhepTran[] itemPlayers2;

    private bool _hasAccepted;

    protected override void Start()
    {
        base.Start();
        if (btnSanSang) btnSanSang.onClick.AddListener(ClickSanSang);
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        _hasAccepted = false;
        if (btnSanSang) btnSanSang.interactable = true;
        if (txtTrangThai) txtTrangThai.text = "Trận đấu tìm thấy!";

        if (MatchFoundDataBase.Instance != null)
        {
            UpdateAcceptUI(
                MatchFoundDataBase.Instance.AcceptedCount,
                MatchFoundDataBase.Instance.TotalPlayers
            );
            UpdateTimerUI(MatchFoundDataBase.Instance.GetRemainingSeconds());
        }

        // Subscribe events
        MatchFoundDataBase.Instance.OnTimerTick += UpdateTimerUI;
        MatchFoundDataBase.Instance.OnAcceptProgressUpdated += OnAcceptProgressUpdated;
        MatchFoundDataBase.Instance.OnMatchCancelled += OnMatchCancelled;
        MatchFoundDataBase.Instance.OnMatchReady += OnMatchReady;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (MatchFoundDataBase.Instance != null)
        {
            MatchFoundDataBase.Instance.OnTimerTick -= UpdateTimerUI;
            MatchFoundDataBase.Instance.OnAcceptProgressUpdated -= OnAcceptProgressUpdated;
            MatchFoundDataBase.Instance.OnMatchCancelled -= OnMatchCancelled;
            MatchFoundDataBase.Instance.OnMatchReady -= OnMatchReady;
        }
    }

    private void UpdateTimerUI(float remainingSeconds)
    {
        if (txtTime)
            txtTime.text = Mathf.CeilToInt(remainingSeconds).ToString();
    }

    private void UpdateAcceptUI(int accepted, int total)
    {
        if (txtSluong)
            txtSluong.text = $"{accepted}/{total}";
    }

    private void OnAcceptProgressUpdated()
    {
        if (MatchFoundDataBase.Instance == null) return;
        UpdateAcceptUI(
            MatchFoundDataBase.Instance.AcceptedCount,
            MatchFoundDataBase.Instance.TotalPlayers
        );
    }

    private void OnMatchCancelled()
    {
        Show(false);

        if (PartyDataBase.Instance != null)
            PartyDataBase.Instance.UpdateState("IDLE");

        if (ThongBaoController.Instance)
            ThongBaoController.Instance.ShowThongBaoNhanh("Trận đấu bị hủy do có người từ chối!");
    }

    private void OnMatchReady()
    {
        Show(false);
    }

    private void ClickSanSang()
    {
        if (_hasAccepted) return;
        _hasAccepted = true;

        if (btnSanSang) btnSanSang.interactable = false;
        if (txtTrangThai) txtTrangThai.text = "Đã sẵn sàng, đang chờ...";

        SendData.PartyAcceptMatch();
    }

    public void Show(bool val = true)
    {
        gameObject.SetActive(val);
    }
}