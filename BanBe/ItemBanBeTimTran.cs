using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemBanBeTimTran : MonoBehaviour
{
    [SerializeField] private Image imgAvatar;
    [SerializeField] private TextMeshProUGUI txtLevel, txtName, txtTrangThai;
    [SerializeField] private Button btnMoiBan, btnInfoPlayer;
    private DataFriend _info;

    public void Start()
    {
        btnMoiBan.onClick.AddListener(SetMoiBan);
        btnInfoPlayer.onClick.AddListener(ClickAvatar);
    }

    public void SetInfo(DataFriend data)
    {
        _info = data;

        txtLevel.text = data.level.ToString();
        txtName.text = data.tenHienThi;
        txtTrangThai.text = data.isOnline ? AgentLV.GetColorTextBlue("online") : AgentLV.GetColorTextGray("offline");
    }

    private void SetMoiBan()
    {
        AudioManager.Instance.AudioClick();

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