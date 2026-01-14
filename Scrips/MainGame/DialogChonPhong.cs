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
        btnPvp1.onClick.AddListener(ClickPvp1);
        btnPvp3.onClick.AddListener(ClickTinhNangAn);
        btnPvp5.onClick.AddListener(ClickTinhNangAn);
        btnPvp3Vip.onClick.AddListener(ClickTinhNangAn);
        btnPvp5Vip.onClick.AddListener(ClickTinhNangAn);
        btnChienNhanh.onClick.AddListener(ClickTinhNangAn);
        tgThuong.onValueChanged.AddListener(agr =>
        {
            if (tgThuong)
            {
                AudioManager.Instance.AudioClick();
            }
        });
        tgVip.onValueChanged.AddListener(agr =>
        {
            if (tgVip)
            {
                AudioManager.Instance.AudioClick();
            }
        });
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

        // Dừng tất cả Coroutines đang chạy
        StopAllCoroutines();

        // Hủy tất cả các tweens/animations (nếu dùng DOTween)
        // DOTween.KillAll();

        // Xóa các listeners/events để tránh memory leak
        // EventManager.Instance?.RemoveAllListeners();

        // Dừng tất cả audio đang phát
        // AudioManager.Instance?.StopAllSounds();

        // Xóa object pools nếu có
        // ObjectPoolManager.Instance?.ClearAllPools();

        // Hủy các timers/invoke đang chạy
        CancelInvoke();

        // Xóa cache và unused assets
        Resources.UnloadUnusedAssets();

        // Gọi Garbage Collector (tùy chọn, có thể gây giật lag)
        System.GC.Collect();

        // Load scene mới
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
}
