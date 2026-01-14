using UnityEngine;
using UnityEngine.UI;

public class RutThuong : ScaleScreen
{
    public Button btnClose, btnAddTien;

    protected override void Start()
    {
        base.Start();
        btnClose.onClick.AddListener(() =>
        {
            AudioManager.Instance.AudioClick();
            Show(false);
        });
        btnAddTien.onClick.AddListener(ClickAddTien);
    }

    private void ClickAddTien()
    {
        AudioManager.Instance.AudioClick();
        MoneyController.Instance.NapTien.Show();
        Show(false);
    }

    public void Show(bool val = true)
    {
        gameObject.SetActive(val);
    }
}
