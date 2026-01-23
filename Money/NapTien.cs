using UnityEngine;
using UnityEngine.UI;

public class NapTien : ScaleScreen
{
    public Button btnClose, btnVip, btnTinhNang;

    protected override void Start()
    {
        base.Start();
        btnClose.onClick.AddListener(() =>
        {
            AudioManager.Instance.AudioClick();
            Show(false);
        });
        btnVip.onClick.AddListener(ClickTinhNangAn);
        btnTinhNang.onClick.AddListener(ClickTinhNangAn);
    }

    public void Show(bool val = true)
    {
        gameObject.SetActive(val);
    }
}
