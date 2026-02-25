using System;
using TMPro;
using UnityEngine;

public class ChatInMatchItem : MonoBehaviour
{
    public TMP_Text txtLine;

    [Header("Colors")]
    [Tooltip("Màu tin nhắn đồng đội (team mình) / chatType = 1")]
    public Color allyColor = new Color32(120, 230, 255, 255);   // xanh nhạt

    [Tooltip("Màu tin nhắn team địch / chatType = 0 (All)")]
    public Color enemyColor = new Color32(255, 170, 170, 255);  // đỏ nhạt

    public void Setup(string displayName,
                      string content,
                      long timestamp,
                      int chatType)
    {
        if (txtLine == null) return;

        // timestamp từ server đang là millis -> dùng FromUnixTimeMilliseconds
        string timeStr;
        try
        {
            var dt = DateTimeOffset
                .FromUnixTimeMilliseconds(timestamp)   // <== quan trọng
                .ToLocalTime()
                .DateTime;

            timeStr = dt.ToString("HH:mm:ss");
        }
        catch
        {
            // Nếu format khác thì in raw (đỡ bị crash)
            timeStr = timestamp.ToString();
        }

        string prefix = (chatType == 1) ? "[Đội]" : "[All]";

        // Ví dụ: "12:30:01 [Đội] Tên: Nội dung"
        txtLine.text = $"{timeStr} {prefix} {displayName}: {content}";

        // Màu: tạm thời coi chatType = 1 là đồng đội, = 0 là team địch / all
        txtLine.color = (chatType == 1) ? allyColor : enemyColor;
    }
}
