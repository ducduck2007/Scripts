using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemTuong : MonoBehaviour
{
    private int idTuong;
    public TextMeshProUGUI txtName;
    public Button btn;

    public void Init(int id, string ten)
    {
        idTuong = id;
        if (txtName != null) txtName.text = ten;

        if (btn == null) btn = GetComponentInChildren<Button>(true);
        if (btn == null)
        {
            Debug.LogError("ItemTuong: Không tìm thấy Button trong prefab!");
            return;
        }

        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        HeroSelectionCache.IdLoaiTuong = idTuong;
        HeroSelectionCache.TenLoaiTuong = (txtName != null) ? txtName.text : "";

        SendData.GetChiSoTuong();        // CMD 61
        SendData.GetInfoMoTaKNTuong();   // CMD 62
    }
}
