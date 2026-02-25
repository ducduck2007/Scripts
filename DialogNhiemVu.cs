using UnityEngine;
using UnityEngine.UI;

public class DialogNhiemVu : ScaleScreen
{
    [Header("Exit Buttons (kéo nhiều button vào đây)")]
    public Button[] btnExits;

    public Button btnDemo1;
    public Button btnDemo2;
    public Button btnDemo3;
    public Button btnDemo4;
    public Button btnDemo5;

    protected override void Start()
    {
        base.Start();

        if (btnExits != null)
        {
            foreach (var btn in btnExits)
            {
                if (btn != null)
                    btn.onClick.AddListener(OnExitClicked);
            }
        }

        btnDemo1.onClick.AddListener(ClickTinhNangAn);
        btnDemo2.onClick.AddListener(ClickTinhNangAn);
        btnDemo3.onClick.AddListener(ClickTinhNangAn);
        btnDemo4.onClick.AddListener(ClickTinhNangAn);
        btnDemo5.onClick.AddListener(ClickTinhNangAn);
    }

    private void OnExitClicked()
    {
        AudioManager.Instance.AudioClick();
        Show(false);
    }

    public void Show(bool val = true)
    {
        gameObject.SetActive(val);
    }
}
