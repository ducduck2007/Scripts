using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemPhongVip : MonoBehaviour
{
    public Button btnItem;
    public TextMeshProUGUI txtLoaiPhong, txtThoiGian, txtCuoc;

    public void Start()
    {
        btnItem.onClick.AddListener(ClickItem);
    }

    private void ClickItem()
    {
        AudioManager.Instance.AudioClick();
        DialogController.Instance.PopupTimTran.Show(true);
        DialogController.Instance.DialogChonPhong.phongVip.Show(false);
        DialogController.Instance.DialogChonPhong.Show(false);
    }
}
