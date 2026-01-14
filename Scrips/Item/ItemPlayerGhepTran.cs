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
        {
            btnPlayer.onClick.AddListener(ClickPlayer);
        }
    }

    public void ClickPlayer()
    {
        // DialogController.Instance.ShowDialogThongTinPlayer(idPlayer);
    }

    public void SetData(ThongTinPlayer data, bool isSang)
    {
        objTrong.SetActive(isSang == false);
        objPlayer.SetActive(data != null);
        txtTen.text = data.tenHienThi;
        idPlayer = data.idNguoiChoi;
    }
}
