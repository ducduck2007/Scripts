using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemBanBe : MonoBehaviour
{
    [SerializeField] private Image imgAvatar;
    [SerializeField] private TextMeshProUGUI txtLevel, txtName, txtTrangThai;
    [SerializeField] private Button btnHuyBan, btnInfoPlayer;
    private DataFriend _info;

    public void Start()
    {
        btnHuyBan.onClick.AddListener(SetHuyBan);
        btnInfoPlayer.onClick.AddListener(ClickAvatar);
    }

    public void SetInfo(DataFriend data)
    {
        _info = data;

        txtLevel.text = data.level.ToString();
        txtName.text = data.tenHienThi;
        txtTrangThai.text = data.isOnline ? AgentLV.GetColorTextBlue("online") : AgentLV.GetColorTextGray("offline");
    }

    private void SetHuyBan()
    {
        AudioManager.Instance.AudioClick();
        ThongBaoController.Instance.PopupOneButton.ShowPopupOneButton("Thông báo",
            "Xác nhận xóa bạn",
            delegate { SendData.OnDeleteFriend(_info.idNguoiChoi); });
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