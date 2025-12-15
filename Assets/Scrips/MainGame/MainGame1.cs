using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainGame1 : ScaleScreen
{
    public Button btnChienDau;
    public TextMeshProUGUI txtLevel, txtName;

    protected override void Start()
    {
        base.Start();
        btnChienDau.onClick.AddListener(ClickChienDau);
    }

    private void ClickChienDau()
    {
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
    }

    public void Show(bool val = true)
    {
        gameObject.SetActive(val);
    }
}
