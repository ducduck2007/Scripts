using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DialogChonPhong : ScaleScreen
{
    public Toggle tgThuong, tgVip;
    public Button btnDauTap, btnBack;
    public Button btnPvp1, btnPvp3, btnPvp5, btnPvp1Vip, btnPvp3Vip, btnPvp5Vip, btnChienNhanh;
    public ChonPhong_PhongVip phongVip;

    protected override void Start()
    {
        base.Start();
        btnBack.onClick.AddListener(ClickBack);
        btnDauTap.onClick.AddListener(ClickDauTap);
        btnPvp1Vip.onClick.AddListener(ClickPhongVip);

        btnPvp1.onClick.AddListener(() => ClickPvpThuong(1));
        btnPvp3.onClick.AddListener(() => ClickPvpThuong(3));
        btnPvp5.onClick.AddListener(() => ClickPvpThuong(5));

        btnPvp3Vip.onClick.AddListener(ClickTinhNangAn);
        btnPvp5Vip.onClick.AddListener(ClickTinhNangAn);
        btnChienNhanh.onClick.AddListener(ClickTinhNangAn);

        tgThuong.onValueChanged.AddListener(agr =>
        {
            if (tgThuong) AudioManager.Instance.AudioClick();
        });
        tgVip.onValueChanged.AddListener(agr =>
        {
            if (tgVip) AudioManager.Instance.AudioClick();
        });
    }

    private int GetModeIdThuong(int teamSize)
    {
        if (teamSize <= 1) return 4;      // 1v1 map 3 đường
        if (teamSize <= 3) return 5;      // 3v3 map 3 đường
        return 6;                         // 5v5 map 3 đường
    }

    private void ClickPvpThuong(int teamSize)
    {
        AudioManager.Instance.AudioClick();

        int modeId = GetModeIdThuong(teamSize);

        // luôn set mode trước (y như cũ)
        var popup = DialogController.Instance.PopupTimTran;
        popup.SetPartyMode(modeId, teamSize);

        // lưu cờ "đang chờ mở popup sau khi tạo party"
        PartyDataBase.Instance.PendingOpenPopupTimTran = true;

        if (!PartyDataBase.Instance.IsInParty)
        {
            SendData.CreateParty(); // chờ CMD 92 quyết định
        }
        else
        {
            // nếu đã có party, mở luôn như cũ
            PartyDataBase.Instance.PendingOpenPopupTimTran = false;
            popup.Show(true);
            Show(false);
        }
    }


    private void OpenPopupTimTran(int modeId, int teamSize)
    {
        var popup = DialogController.Instance.PopupTimTran;
        popup.SetPartyMode(modeId, teamSize);
        popup.Show(true);
        Show(false);
    }

    private void ClickBack()
    {
        AudioManager.Instance.AudioClick();
        UiControl.Instance.MainGame1.Show();
        Show(false);
    }

    public void ClickDauTap()
    {
        AudioManager.Instance.AudioClick();

        // Reset dữ liệu game
        B.Instance.heroPlayer = 0;
        B.Instance.teamId = 0;

        StopAllCoroutines();
        CancelInvoke();
        Resources.UnloadUnusedAssets();
        System.GC.Collect();

        ThongBaoController.Instance.LoadVaoTran.SetLoadScene("DauTapTest");
    }

    public void ClickPhongVip()
    {
        AudioManager.Instance.AudioClick();
        phongVip.Show();
    }

    private void ClickPvp1()
    {
        AudioManager.Instance.AudioClick();
        DialogController.Instance.PopupTimTran.Show(true);
        Show(false);
    }

    public void Show(bool val = true)
    {
        gameObject.SetActive(val);
    }

    private void ClickTinhNangAn()
    {
        AudioManager.Instance.AudioClick();
        if (ThongBaoController.Instance)
        {
            ThongBaoController.Instance.ShowThongBaoNhanh("Tính năng đang phát triển!");
        }
    }
}