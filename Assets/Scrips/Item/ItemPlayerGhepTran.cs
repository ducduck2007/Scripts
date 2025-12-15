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

    public void SetData(string ten, long userid)
    {
        objPlayer.SetActive(true);
        objTrong.SetActive(false);
        txtTen.text = ten;
        idPlayer = userid;
    }
}
