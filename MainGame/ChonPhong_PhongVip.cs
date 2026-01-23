using UnityEngine;
using UnityEngine.UI;

public class ChonPhong_PhongVip : ScaleScreen
{
    public Button btnRutTien, btnBack, btnAddTien;

    protected override void Start()
    {
        base.Start();
        btnBack.onClick.AddListener(() =>
        {
           AudioManager.Instance.AudioClick();
           Show(false); 
        });
        btnRutTien.onClick.AddListener(SetRutThuong);
        btnAddTien.onClick.AddListener(ClickAddTien);
    }

    private void ClickAddTien()
    {
        AudioManager.Instance.AudioClick();
        MoneyController.Instance.NapTien.Show();
    }

    private void SetRutThuong()
    {
        AudioManager.Instance.AudioClick();
        MoneyController.Instance.RutThuong.Show();
    }

    public void Show(bool val = true)
    {
        gameObject.SetActive(val);
    }
}
