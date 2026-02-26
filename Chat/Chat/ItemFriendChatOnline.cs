using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemFriendChatOnline : MonoBehaviour
{
    [Header("UI Refs")]
    [SerializeField] private Image imgAvatar;
    [SerializeField] private TextMeshProUGUI txtName;
    [SerializeField] private TextMeshProUGUI txtTrangThai;

    private DataFriend _info;

    // Nếu muốn khác màu online/offline thì set 2 màu này trong Inspector
    [Header("Status Colors (optional)")]
    [SerializeField] private Color onlineColor = Color.green;
    [SerializeField] private Color offlineColor = Color.gray;

    private void Awake()
    {
        if (txtName == null || txtTrangThai == null)
        {
            var texts = GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var t in texts)
            {
                var n = t.name.ToLower();
                if (txtName == null && (n.Contains("name") || n.Contains("ten"))) txtName = t;
                if (txtTrangThai == null && (n.Contains("trangthai") || n.Contains("status") || n.Contains("online"))) txtTrangThai = t;
            }
        }

        if (imgAvatar == null)
        {
            var imgs = GetComponentsInChildren<Image>(true);
            foreach (var im in imgs)
            {
                var n = im.name.ToLower();
                if (n.Contains("avatar"))
                {
                    imgAvatar = im;
                    break;
                }
            }
        }
    }

    public void SetData(DataFriend info)
    {
        _info = info;
        if (_info == null) return;

        if (txtName) txtName.text = Safe(_info.tenHienThi, "Unknown");

        bool isOnline = _info.isOnline;
        if (txtTrangThai)
        {
            txtTrangThai.text = isOnline ? "Online" : "Offline";
            txtTrangThai.color = isOnline ? onlineColor : offlineColor;
        }

        // Avatar: nếu mày đã có sprite avatar sẵn từ id/avatarUrl thì gán ở đây.
        // Tạm thời: chỉ bật/tắt avatar theo điều kiện có data.
        if (imgAvatar)
        {
            imgAvatar.enabled = true;

            // Ví dụ nếu DataFriend có Sprite avatarSprite:
            // if (_info.avatarSprite != null) imgAvatar.sprite = _info.avatarSprite;

            // Nếu không có dữ liệu avatar thì có thể disable:
            // imgAvatar.enabled = (_info.avatarSprite != null);
        }
    }

    private string Safe(string s, string fallback)
    {
        return string.IsNullOrEmpty(s) ? fallback : s;
    }

#if UNITY_EDITOR
    // Auto-wire nhanh khi quên kéo ref
    private void Reset()
    {
        var texts = GetComponentsInChildren<TextMeshProUGUI>(true);
        if (texts != null)
        {
            foreach (var t in texts)
            {
                var name = t.name.ToLower();
                if (txtName == null && (name.Contains("name") || name.Contains("ten")))
                    txtName = t;

                if (txtTrangThai == null && (name.Contains("trangthai") || name.Contains("status") || name.Contains("online")))
                    txtTrangThai = t;
            }
        }

        if (imgAvatar == null)
        {
            var imgs = GetComponentsInChildren<Image>(true);
            foreach (var im in imgs)
            {
                var n = im.name.ToLower();
                if (n.Contains("avatar") || n.Contains("imgavatar"))
                {
                    imgAvatar = im;
                    break;
                }
            }
        }
    }
#endif
}
