using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemPlayerGhepTran : MonoBehaviour
{
    public TextMeshProUGUI txtTen;
    public Image imgAvatar, imgKhung;
    public Button btnPlayer;
    public GameObject objPlayer, objTrong;
    public long idPlayer;

    private void Start()
    {
        if (btnPlayer != null)
            btnPlayer.onClick.AddListener(ClickPlayer);
    }

    public void ClickPlayer()
    {
        // DialogController.Instance.ShowDialogThongTinPlayer(idPlayer);
    }

    public void SetData(ThongTinPlayer data, bool isSang)
    {
        if (objTrong != null) objTrong.SetActive(!isSang);

        bool hasData = data != null;
        if (objPlayer != null) objPlayer.SetActive(hasData);

        if (!hasData)
        {
            idPlayer = 0;
            if (txtTen != null) txtTen.text = "";
            return;
        }

        idPlayer = data.idNguoiChoi;
        if (txtTen != null) txtTen.text = data.tenHienThi ?? "";
    }
}
