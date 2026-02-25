using TMPro;
using UnityEngine;
using System.Text;

public class ItemTooltipUI : MonoBehaviour
{
    [Header("Root")]
    public RectTransform root;

    [Header("Single Text")]
    public TextMeshProUGUI txtInfo;

    private void Awake()
    {
        if (root == null)
            root = GetComponent<RectTransform>();
    }

    public void Bind(ItemInfoData d)
    {
        if (d == null) return;

        string s = BuildText(d);
        if (txtInfo != null) txtInfo.text = s;
    }

    // ✅ cho phép Canvas khác dùng chung formatter
    public static string BuildText(ItemInfoData d)
    {
        if (d == null) return "";

        StringBuilder sb = new StringBuilder(256);

        sb.AppendLine($"<b><size=120%>{d.nameItem}</size></b>");
        sb.AppendLine($"Tier {d.tier}   |   Giá: {d.giaMua}");
        sb.AppendLine("");

        if (!string.IsNullOrEmpty(d.moTaNgan))
            sb.AppendLine($"<i>{d.moTaNgan}</i>\n");

        if (!string.IsNullOrEmpty(d.moTa))
            sb.AppendLine($"{d.moTa}\n");

        Append(sb, "DMG Vật Lý", d.dmgVatLy);
        Append(sb, "DMG Phép", d.dmgPhep);
        Append(sb, "Giáp", d.giap);
        Append(sb, "Kháng Phép", d.khangPhep);
        Append(sb, "Máu", d.mauToiDa);
        Append(sb, "Mana", d.manaToiDa);
        Append(sb, "Tốc Đánh", d.tocDanh);
        Append(sb, "Tốc Chạy", d.tocChay);
        Append(sb, "Chí Mạng", d.chiMang);
        Append(sb, "Hút Máu", d.hutMau);
        Append(sb, "Hút Máu Phép", d.hutMauPhep);
        Append(sb, "Xuyên Giáp", d.xuyenGiap);
        Append(sb, "Xuyên Kháng Phép", d.xuyenKhangPhep);
        Append(sb, "Hồi Máu/s", d.hoiMauGiay);
        Append(sb, "Hồi Mana/s", d.hoiManaGiay);
        Append(sb, "Giảm Hồi Chiêu", d.giamHoiChieu);
        Append(sb, "Tầm Đánh", d.tamDanh);

        return sb.ToString();
    }

    private static void Append(StringBuilder sb, string label, int value)
    {
        if (value == 0) return;
        sb.AppendLine($"• {label}: <b>{value}</b>");
    }
}
