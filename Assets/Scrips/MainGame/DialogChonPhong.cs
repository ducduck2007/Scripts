using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DialogChonPhong : ScaleScreen
{
    public Toggle tgThuong, tgVip;
    public Button btnDauTap, btnBack;
    public Button btnPvp1;

    protected override void Start()
    {
        base.Start();
        btnBack.onClick.AddListener(ClickBack);
        btnDauTap.onClick.AddListener(ClickDauTap);
        btnPvp1.onClick.AddListener(ClickPvp1);
    }
    private void ClickBack()
    {
        Show(false);
    }

    private void ClickDauTap()
    {
        SceneManager.LoadScene("DauTap");
    }

    private void ClickPvp1()
    {
        PopupController.Instance.PopupTimTran.Show(true);
        Show(false);
    }

    public void Show(bool val = true)
    {
        gameObject.SetActive(val);
    }
}
