using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainGame1 : ScaleScreen
{
    public static MainGame1 Instance;

    public Button btnChienDau, btnThongTin;
    public TextMeshProUGUI txtLevel, txtName;
    public Button btnBanBe, btnSanh, btnShop, btnTuiDo, btnTuong, btnTrangBi, btnNhiemVu, btnChat;
    public Button btnAddTien, btnGifiCode, btnThu, btnSetting;
    public Button btnNapDau, btnDangNhap, btnSuKien, btnSkDb;
    public TextMeshProUGUI txtPing;
    public Image iconPing;

    public Sprite pingWeakSprite;
    public Sprite pingMidSprite;
    public Sprite pingGoodSprite;

    [SerializeField] private float pingWeakPosX = -93.5f;
    [SerializeField] private float pingMidPosX = -87.5f;
    [SerializeField] private float pingGoodPosX = -82f;

    private int _lastPingState = -1; // 0=weak, 1=mid, 2=good

    protected override void Start()
    {
        base.Start();
        Instance = this;

        btnChienDau.onClick.AddListener(() => { StopLoginFx(); ClickChienDau(); });
        btnBanBe.onClick.AddListener(() => { StopLoginFx(); ClickBanBe(); });
        btnAddTien.onClick.AddListener(() => { StopLoginFx(); ClickAddTien(); });
        btnThongTin.onClick.AddListener(() => { StopLoginFx(); ClickTinhNangAn(); });
        btnSanh.onClick.AddListener(() => { StopLoginFx(); ClickTinhNangAn(); });
        btnShop.onClick.AddListener(() => { StopLoginFx(); ClickTinhNangAn(); });
        btnTuiDo.onClick.AddListener(() => { StopLoginFx(); ClickTinhNangAn(); });
        btnTuong.onClick.AddListener(() => { StopLoginFx(); ClickTuong(); });
        btnTrangBi.onClick.AddListener(() => { StopLoginFx(); ClickTrangBi(); });
        btnNhiemVu.onClick.AddListener(() => { StopLoginFx(); ClickNhiemVu(); });
        btnGifiCode.onClick.AddListener(() => { StopLoginFx(); ClickTinhNangAn(); });
        btnThu.onClick.AddListener(() => { StopLoginFx(); ClickHomThu(); });
        btnSetting.onClick.AddListener(() => { StopLoginFx(); ClickTinhNangAn(); });
        btnNapDau.onClick.AddListener(() => { StopLoginFx(); ClickAddTien(); });
        btnDangNhap.onClick.AddListener(() => { StopLoginFx(); ClickTinhNangAn(); });
        btnSuKien.onClick.AddListener(() => { StopLoginFx(); ClickSuKien(); });
        btnSkDb.onClick.AddListener(() => { StopLoginFx(); ClickTinhNangAn(); });
        btnChat.onClick.AddListener(() => { StopLoginFx(); ClickChat(); });

        ItemInfoCache.EnsureDiskLoaded(debugLog: false);
    }

    void Update()
    {
        if (PingPongGame.Instance == null) return;

        int ms = (int)PingPongGame.Instance.pingTime;
        if (txtPing != null) txtPing.text = ms + "ms";

        int state; // 0=weak,1=mid,2=good
        if (ms < 50)
        {
            state = 2;
            if (txtPing != null) txtPing.color = Color.green;
        }
        else if (ms <= 120)
        {
            state = 1;
            if (txtPing != null) txtPing.color = Color.yellow;
        }
        else
        {
            state = 0;
            if (txtPing != null) txtPing.color = Color.red;
        }

        if (state == _lastPingState) return;
        _lastPingState = state;

        if (iconPing == null) return;

        Sprite sp;
        float x;
        switch (state)
        {
            case 0: sp = pingWeakSprite; x = pingWeakPosX; break;
            case 1: sp = pingMidSprite; x = pingMidPosX; break;
            default: sp = pingGoodSprite; x = pingGoodPosX; break;
        }

        if (sp != null) iconPing.sprite = sp;

        var rt = iconPing.rectTransform;
        var p = rt.anchoredPosition;
        p.x = x;
        rt.anchoredPosition = p;
    }

    // ==== STOP FX LOGIN ====
    public void StopLoginFx()
    {
        var fx = FindObjectOfType<AutoPlayPingPong>(true);
        if (fx != null)
        {
            fx.Stop();
            fx.gameObject.SetActive(false);
            Debug.Log("[LoginFX] Stopped by user click");
        }
    }

    private void ClickAddTien()
    {
        AudioManager.Instance.AudioClick();
        MoneyController.Instance.NapTien.Show();
    }

    private void ClickChat()
    {
        AudioManager.Instance.AudioClick();
        ChatControlController.Instance.DialogChat.Show();
    }

    private void ClickHomThu()
    {
        AudioManager.Instance.AudioClick();
        DialogController.Instance.ShowDialogHomThu();
    }

    private void ClickTuong()
    {
        AudioManager.Instance.AudioClick();
        DialogController.Instance.ShowDialogTuong();
    }

    private void ClickBanBe()
    {
        AudioManager.Instance.AudioClick();
        DialogController.Instance.ShowDialogBanBe();
    }

    private void ClickChienDau()
    {
        AudioManager.Instance.AudioClick();
        // Tắt notify khi vào chiến đấu
        if (NotifyController.Instance != null)
            NotifyController.Instance.StopNotify();
        DialogController.Instance.ShowDialogChonPhong();
        Show(false);
    }

    private void ClickSuKien()
    {
        AudioManager.Instance.AudioClick();
        SendData.GetEventInfo();
        DialogController.Instance.ShowDialogSuKien();
    }

    private void ClickNhiemVu()
    {
        AudioManager.Instance.AudioClick();
        SendData.GetEventInfo();
        DialogController.Instance.ShowDialogSuKien();
    }

    private void ClickTrangBi()
    {
        AudioManager.Instance.AudioClick();
        ItemInfoCache.EnsureRequested(() => SendData.GetItemInfo(), false, false);
        DialogController.Instance.ShowDialogTrangBi();
        Show(false);
    }

    public void SetInfo()
    {
        txtLevel.text = "Lv: " + UserData.Instance.Level;
        txtName.text = UserData.Instance.UserName;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        SetInfo();
        SendData.OnDataFriend();
        SendData.OnNotifyDataGame();
        ItemInfoCache.EnsureRequested(() => SendData.GetItemInfo(), false, false);
    }

    public void Show(bool val = true)
    {
        gameObject.SetActive(val);
    }
}
