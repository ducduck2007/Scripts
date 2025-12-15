using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupTimTran : ScaleScreen
{
    public Button btnBack, btnHome;
    public Button btnTimTran, btnHuyGhep;
    public TextMeshProUGUI txtIdPhong, txtTimeTim, txtTrangThaiTim;
    public GameObject objDemTime;
    public ItemPlayerGhepTran[] itemPlayers;

    private float timeWaiting;
    private bool isFindingMatch;

    protected override void Start()
    {
        base.Start();
        btnBack.onClick.AddListener(ClickBack);
        btnHome.onClick.AddListener(ClickHome);
        btnTimTran.onClick.AddListener(ClickTimTran);
        btnHuyGhep.onClick.AddListener(ClickHuyTimTran);
    }

    private void ClickBack()
    {
        DialogController.Instance.DialogChonPhong.Show(true);
        Show(false);
    }   
    private void ClickHome()
    {
        UiControl.Instance.MainGame1.Show(true);
        DialogController.Instance.DialogChonPhong.Show(false);
        Show(false);
    }
    private void ClickTimTran()
    {
        SendData.FindMatch();
    }
    
    public void SetIdPhong(string idPhong)
    {
        txtIdPhong.text = idPhong;
    }
    public void SetPlayerData()
    {
        itemPlayers[0].SetData(UserData.Instance.UserName, 1001);
    }

    public void TimTran()
    {
        btnTimTran.interactable = false;
        isFindingMatch = true;
        timeWaiting = 0f;
        objDemTime.SetActive(true);
        txtTrangThaiTim.text = "Đang ghép";
        UpdateTimerUI();
    }

    private void Update()
    {
        if (isFindingMatch)
        {
            timeWaiting += Time.deltaTime;
            UpdateTimerUI();
        }
    }

    void UpdateTimerUI()
    {
        int totalSeconds = Mathf.FloorToInt(timeWaiting);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        txtTimeTim.text = $"{minutes:00}:{seconds:00}";
    }
    private void ClickHuyTimTran()
    {
        SendData.CancelFindMatch(); // nếu có API hủy tìm trận

        btnTimTran.interactable = true;
        isFindingMatch = false;
        txtTrangThaiTim.text = "Sẵn sàng";
        objDemTime.SetActive(false);   
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        btnTimTran.interactable = true;
        isFindingMatch = false;
        txtTrangThaiTim.text = "Sẵn sàng";
        objDemTime.SetActive(false);
        SetPlayerData();
    }
    public void Show(bool val = true)
    {
        gameObject.SetActive(val);
    }
}
