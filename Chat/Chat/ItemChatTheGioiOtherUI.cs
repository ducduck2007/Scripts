using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemChatTheGioiOtherUI : MonoBehaviour
{
    [Header("Refs")]
    public TMP_Text txtName;
    public TMP_Text txtTime;
    public TMP_Text txtChat;
    public Image imgAvatar;

    public void SetData(ChatWorldMessage m)
    {
        if (m == null) return;

        if (txtName) txtName.text = $"{m.fromDisplayName} (Lv {m.fromLevel})";
        if (txtChat) txtChat.text = m.content ?? "";

        if (txtTime)
        {
            DateTime dt = FromUnix(m.timestamp);
            txtTime.text = dt.ToString("HH:mm");
        }

        if (imgAvatar)
        {
            // placeholder - không đổi sprite nếu chưa có
            // imgAvatar.sprite = ...
        }
    }

    private static DateTime FromUnix(long ts)
    {
        if (ts > 1_000_000_000_000L)
            return DateTimeOffset.FromUnixTimeMilliseconds(ts).LocalDateTime;
        return DateTimeOffset.FromUnixTimeSeconds(ts).LocalDateTime;
    }
}
