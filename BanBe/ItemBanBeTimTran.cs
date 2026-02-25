using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemBanBeTimTran : MonoBehaviour
{
    [SerializeField] private Image imgAvatar;
    [SerializeField] private TextMeshProUGUI txtLevel, txtName, txtTrangThai;
    [SerializeField] private Button btnMoiBan, btnInfoPlayer;

    private DataFriend _info;

    private void Start()
    {
        if (btnMoiBan != null)
            btnMoiBan.onClick.AddListener(SetMoiBan);

        if (btnInfoPlayer != null)
            btnInfoPlayer.onClick.AddListener(ClickAvatar);
    }

    public void SetInfo(DataFriend data)
    {
        _info = data;

        if (txtLevel != null)
            txtLevel.text = data.level.ToString();

        if (txtName != null)
            txtName.text = data.tenHienThi;

        if (txtTrangThai != null)
            txtTrangThai.text = data.isOnline
                ? AgentLV.GetColorTextBlue("online")
                : AgentLV.GetColorTextGray("offline");
    }

    private void SetMoiBan()
    {
        if (_info == null) return;

        AudioManager.Instance?.AudioClick();

        // Không cho tự mời chính mình
        if (_info.idNguoiChoi == UserData.Instance.UserID)
            return;

        // Nếu offline thì không mời
        if (!_info.isOnline)
        {
            ThongBaoController.Instance?.ShowThongBaoNhanh("Người chơi đang offline");
            return;
        }

        // Gửi lệnh mời party
        SendData.InviteToParty(_info.idNguoiChoi);
    }

    private void ClickAvatar()
    {
        AudioManager.Instance?.AudioClick();

        // Ví dụ xem info player
        // if (!UserData.Instance.CheckPlayer(_info.userId))
        // {
        //     SendData.OnViewInfoPlayer(_info.userId);
        // }
    }
}
