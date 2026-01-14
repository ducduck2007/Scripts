using TMPro;
using UIPool;
using UnityEngine;
using UnityEngine.UI;

public class PopupTimTran : ScaleScreen
{
    public Button btnBack, btnHome;
    public Button btnTimTran, btnHuyGhep;
    public TextMeshProUGUI txtIdPhong, txtTimeTim, txtTrangThaiTim;
    public GameObject objDemTime, objHuBtn;
    public ItemPlayerGhepTran[] itemPlayers;
    
    public GridPoolGroup gridPoolGroup;

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
        AudioManager.Instance.AudioClick();
        DialogController.Instance.DialogChonPhong.Show(true);
        Show(false);
    }   
    private void ClickHome()
    {
        AudioManager.Instance.AudioClick();
        UiControl.Instance.MainGame1.Show(true);
        DialogController.Instance.DialogChonPhong.Show(false);
        Show(false);
    }
    private void ClickTimTran()
    {
        AudioManager.Instance.AudioClick();
        SendData.FindMatch();
    }
    public void SetData(bool isLoadFirst)
    {
        FriendDataBase.Instance.ListDataFriend.Sort((t1, t2) => t2.isOnline.CompareTo(t1.isOnline));
        InitPool();
        gridPoolGroup.SetAdapter(AgentUIPool.GetListObject<DataFriend>(FriendDataBase.Instance.ListDataFriend), isLoadFirst);
    }

    private void InitPool()
    {
        gridPoolGroup.HowToUseCellData(delegate(GameObject go, object data)
        {
            ItemBanBeTimTran item = go.GetComponent<ItemBanBeTimTran>();
            item.SetInfo((DataFriend) data);
        });
    }
    public void SetIdPhong(string idPhong)
    {
        txtIdPhong.text = idPhong;
    }
    public void SetPlayerData()
    {
        itemPlayers[0].SetData(new ThongTinPlayer(UserData.Instance.UserID, UserData.Instance.UserName, UserData.Instance.Level, UserData.Instance.AvatarId), true);
    }

    public void TimTran()
    {
        btnTimTran.interactable = false;
        objHuBtn.SetActive(false);
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
        AudioManager.Instance.AudioClick();
        SendData.CancelFindMatch(); // nếu có API hủy tìm trận

        btnTimTran.interactable = true;
        objHuBtn.SetActive(true);
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
        SetData(true);
    }
    public void Show(bool val = true)
    {
        gameObject.SetActive(val);
    }
}
