using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainGame1 : ScaleScreen
{
    public Button btnChienDau, btnThongTin;
    public TextMeshProUGUI txtLevel, txtName;
    public Button btnBanBe, btnSanh, btnShop, btnTuiDo, btnTuong, btnTrangBi, btnNhiemVu, btnChat;
    public Button btnAddTien, btnGifiCode, btnThu, btnSetting;
    public Button btnNapDau, btnDangNhap, btnSuKien, btnSkDb;

    protected override void Start()
    {
        base.Start();
        AudioManager.Instance.PlayAudioBg();
        btnChienDau.onClick.AddListener(ClickChienDau);
        btnBanBe.onClick.AddListener(ClickBanBe);
        btnAddTien.onClick.AddListener(ClickAddTien);
        btnThongTin.onClick.AddListener(ClickTinhNangAn);
        btnSanh.onClick.AddListener(ClickTinhNangAn);
        btnShop.onClick.AddListener(ClickTinhNangAn);
        btnTuiDo.onClick.AddListener(ClickTinhNangAn);
        btnTuong.onClick.AddListener(ClickTuong);
        btnTrangBi.onClick.AddListener(ClickTinhNangAn);
        btnNhiemVu.onClick.AddListener(ClickTinhNangAn);
        btnGifiCode.onClick.AddListener(ClickTinhNangAn);
        btnThu.onClick.AddListener(ClickHomThu);
        btnSetting.onClick.AddListener(ClickTinhNangAn);
        btnNapDau.onClick.AddListener(ClickTinhNangAn);
        btnDangNhap.onClick.AddListener(ClickTinhNangAn);
        btnSuKien.onClick.AddListener(ClickTinhNangAn);
        btnSkDb.onClick.AddListener(ClickTinhNangAn);
        btnChat.onClick.AddListener(ClickChat);
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
    }

    public void Show(bool val = true)
    {
        gameObject.SetActive(val);
    }
}
