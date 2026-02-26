using System;
using TMPro;
using UnityEngine;

public class ItemChatBanBeOther : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI txtName;    // optional
    [SerializeField] private TextMeshProUGUI txtMessage;
    [SerializeField] private TextMeshProUGUI txtTime;    // optional

    private void Awake()
    {
        AutoBindIfNeeded();
    }

    private void AutoBindIfNeeded()
    {
        if (txtMessage != null) return;

        var tmps = GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var t in tmps)
        {
            var n = t.name.ToLower();

            if (txtTime == null && (n.Contains("time") || n.Contains("gio")))
            {
                txtTime = t;
                continue;
            }

            if (txtName == null && (n.Contains("name") || n.Contains("ten") || n.Contains("from")))
            {
                txtName = t;
                continue;
            }

            if (txtMessage == null && (n.Contains("msg") || n.Contains("message") || n.Contains("content") || n.Contains("noidung") || n.Contains("text")))
            {
                txtMessage = t;
            }
        }

        if (txtMessage == null && tmps.Length > 0) txtMessage = tmps[0];
    }

    public void SetData(string displayName, string content, long timestampMs)
    {
        AutoBindIfNeeded();

        if (txtName) txtName.text = displayName ?? "";
        if (txtMessage) txtMessage.text = content ?? "";

        if (txtTime)
        {
            txtTime.text = FormatTime(timestampMs);
        }
    }

    private string FormatTime(long timestampMs)
    {
        try
        {
            var dt = DateTimeOffset.FromUnixTimeMilliseconds(timestampMs).ToLocalTime().DateTime;
            return dt.ToString("HH:mm");
        }
        catch
        {
            return "";
        }
    }
}
