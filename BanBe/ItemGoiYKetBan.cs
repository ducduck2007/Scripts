using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemGoiYKetBan : MonoBehaviour
{
    [SerializeField] private Image imgAvatar;
    [SerializeField] private TextMeshProUGUI txtLevel, txtName, txtTrangThai;
    [SerializeField] private Button btnKetBan, btnInfoPlayer;
    private DataFriend _info;

    public void Start()
    {
        btnKetBan.onClick.AddListener(SetKetBan);
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

        _isDuocMoiKb = false;
        btnKetBan.gameObject.SetActive(true);
        if (!FriendDataBase.Instance.CheckIsBanBe(data.idNguoiChoi))
        {
            if (!FriendDataBase.Instance.CheckNhanLoiMoi(data.idNguoiChoi))
            {
                if (FriendDataBase.Instance.CheckDaGuiKetbanChua(data.idNguoiChoi))
                {
                    //txtKetBan.text = B.Instance.GetText(IdLanguage.DaMoi);
                    btnKetBan.interactable = false;
                }
                else
                {
                    //txtKetBan.text = B.Instance.GetText(IdLanguage.GuiLoiMoi);
                    btnKetBan.interactable = true;
                }
            }
            else
            {
                _isDuocMoiKb = false;
                //txtKetBan.text = B.Instance.GetText(IdLanguage.ChapNhan);
                btnKetBan.interactable = true;
            }
        }
        else
        {
            btnKetBan.gameObject.SetActive(false);
        }
    }

    private bool _isDuocMoiKb;
    
    private void SetKetBan()
    {
        AudioManager.Instance.AudioClick();
        SendData.OnGuiYeuCauKetBan(_info.idNguoiChoi);
    }

    public void OnEnable()
    {
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
