using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemYeuCauKetBan : MonoBehaviour
{
    [SerializeField] private Image imgAvatar;
    [SerializeField] private TextMeshProUGUI txtLevel, txtName, txtTrangThai;
    [SerializeField] private Button btnDongY, btnHuy, btnInfoPlayer;
    private DataFriend _info;

    public void Start()
    {
        btnDongY.onClick.AddListener(SetDongY);
        btnHuy.onClick.AddListener(SetHuy);
        btnInfoPlayer.onClick.AddListener(ClickAvatar);

    }

    public void SetInfo(DataFriend data)
    {
        _info = data;
        txtLevel.text = data.level.ToString();
        txtName.text = data.tenHienThi;
        if (data.isOnline)
        {
            txtTrangThai.text = AgentLV.GetColorTextBlue("online");
        }
        else
        {
            txtTrangThai.text = AgentLV.GetColorTextGray("offline");
        }
    }

    private void SetDongY()
    {
        AudioManager.Instance.AudioClick();
        SendData.OnKetBan(_info.idNguoiChoi);
    }

    private void SetHuy()
    {
        AudioManager.Instance.AudioClick();
        SendData.OnTuChoiKetBan(_info.idNguoiChoi);
    }
    
    public void OnEnable()
    {
        // txtDongY.text = B.Instance.GetText(IdLanguage.ChapNhan);
        // txtHuy.text = B.Instance.GetText(IdLanguage.Huy);
    }
    private void ClickAvatar()
    {
        AudioManager.Instance.AudioClick();
        // if (!UserData.Instance.CheckPlayer(_info.userId))
        // {
        //     SendData.OnViewInfoPlayer(_info.userId);
        // }
    }
    
}
