using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupPartyMatchFound : ScaleScreen
{
    [SerializeField] private TextMeshProUGUI txtContent;
    [SerializeField] private TextMeshProUGUI txtTimer;
    [SerializeField] private TextMeshProUGUI txtProgress;
    [SerializeField] private Button btnAccept, btnDecline;

    protected override void Start()
    {
        base.Start();

        if (btnAccept) btnAccept.onClick.AddListener(OnAccept);
        if (btnDecline) btnDecline.onClick.AddListener(OnDecline);

        // Subscribe to timer
        if (MatchFoundDataBase.Instance != null)
        {
            MatchFoundDataBase.Instance.OnTimerTick += UpdateTimer;
            MatchFoundDataBase.Instance.OnAcceptProgressUpdated += UpdateProgress;
            MatchFoundDataBase.Instance.OnMatchReady += OnMatchReady;
            MatchFoundDataBase.Instance.OnMatchCancelled += OnMatchCancelled;
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        // Unsubscribe when disabled
        if (MatchFoundDataBase.Instance != null)
        {
            MatchFoundDataBase.Instance.OnTimerTick -= UpdateTimer;
            MatchFoundDataBase.Instance.OnAcceptProgressUpdated -= UpdateProgress;
            MatchFoundDataBase.Instance.OnMatchReady -= OnMatchReady;
            MatchFoundDataBase.Instance.OnMatchCancelled -= OnMatchCancelled;
        }
    }

    public void ShowMatchFound()
    {
        UpdateUI();
        Show(true);
    }

    private void UpdateUI()
    {
        if (txtContent)
        {
            var data = MatchFoundDataBase.Instance;
            txtContent.text = $"Tìm thấy trận {data.TeamSize}v{data.TeamSize}!\n{data.MapName}";
        }

        UpdateProgress();
        UpdateTimer(MatchFoundDataBase.Instance.GetRemainingSeconds());
    }

    private void UpdateTimer(float seconds)
    {
        if (txtTimer)
        {
            txtTimer.text = $"{Mathf.CeilToInt(seconds)}s";
        }
    }

    private void UpdateProgress()
    {
        if (txtProgress)
        {
            var data = MatchFoundDataBase.Instance;
            txtProgress.text = $"Đã chấp nhận: {data.AcceptedCount}/{data.TotalPlayers}";
        }
    }

    private void OnAccept()
    {
        AudioManager.Instance.AudioClick();
        SendData.PartyAcceptMatch();

        // Disable buttons after accept
        if (btnAccept) btnAccept.interactable = false;
        if (btnDecline) btnDecline.interactable = false;
    }

    private void OnDecline()
    {
        AudioManager.Instance.AudioClick();
        SendData.PartyDeclineMatch();
        Show(false);
    }

    private void OnMatchReady()
    {
        Show(false);
        // UI sẽ tự động chuyển sang màn chọn tướng khi nhận CMD ROOM_START_GAME
    }

    private void OnMatchCancelled()
    {
        Show(false);
    }

    public void Show(bool val = true)
    {
        gameObject.SetActive(val);

        if (val)
        {
            // Re-enable buttons when showing
            if (btnAccept) btnAccept.interactable = true;
            if (btnDecline) btnDecline.interactable = true;
        }
    }
}