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
        txtPing.text = ms + "ms";

        if (ms < 50)
            txtPing.color = Color.green;
        else if (ms <= 120)
            txtPing.color = Color.yellow;
        else
            txtPing.color = Color.red;
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
