using System.Text;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemSK : MonoBehaviour
{
    [Header("UI Refs")]
    public TextMeshProUGUI txtTitle;      // tên mốc
    public TextMeshProUGUI txtCondition;  // mô tả điều kiện
    public TextMeshProUGUI txtRewards;    // mô tả quà
    public Button btnNhan;

    private int _idMoc;

    public void SetupFromJson(JToken moc)
    {
        _idMoc = moc.Value<int>("idMoc");

        if (txtTitle != null)
            txtTitle.text = moc.Value<string>("tenMoc") ?? "";

        if (txtCondition != null)
        {
            int loaiYeuCau = moc.Value<int>("loaiYeuCau");
            int giaTri = moc.Value<int>("giaTri");
            txtCondition.text = BuildCondition(loaiYeuCau, giaTri);
        }

        if (txtRewards != null)
        {
            var dsQua = moc["dsQua"] as JArray;
            txtRewards.text = BuildRewards(dsQua);
        }

        if (btnNhan != null)
        {
            btnNhan.onClick.RemoveAllListeners();
            btnNhan.onClick.AddListener(OnClickNhan);
        }
    }

    private void OnClickNhan()
    {
        // TODO: gửi lệnh nhận thưởng mốc _idMoc
        Debug.Log($"[ItemSK] Click Nhận idMoc={_idMoc}");
    }

    private string BuildCondition(int loaiYeuCau, int giaTri)
    {
        switch (loaiYeuCau)
        {
            case 1: return $"Đăng nhập {giaTri} ngày";
            case 2: return $"Tiêu diệt {giaTri} mạng";
            case 3: return $"Mời {giaTri} người bạn";
            case 4: return $"Share {giaTri} lần";
            default: return $"Yêu cầu: {giaTri}";
        }
    }

    private string BuildRewards(JArray dsQua)
    {
        if (dsQua == null || dsQua.Count == 0)
            return "Không có quà";

        var sb = new StringBuilder();
        for (int i = 0; i < dsQua.Count; i++)
        {
            var q = dsQua[i];
            if (i > 0) sb.Append(" + ");

            int loaiQua = q.Value<int>("loaiQua");
            int soLuong = q.Value<int>("soLuong");

            sb.AppendFormat("{0}x {1}", soLuong, MapRewardName(loaiQua));
        }
        return sb.ToString();
    }

    private string MapRewardName(int loaiQua)
    {
        // theo ghi chú trong file excel bạn gửi
        switch (loaiQua)
        {
            case 1: return "Vàng";
            case 2: return "Kim cương";
            case 3: return "Tướng";
            case 4: return "Skin";
            case 5: return "Item";
            case 6: return "Rương quà";
            case 7: return "Danh hiệu";
            case 8: return "Khung avatar";
            case 9: return "Điểm đặc biệt";
            default: return "Quà";
        }
    }
}
