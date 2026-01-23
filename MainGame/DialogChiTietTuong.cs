using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;

[System.Serializable]
public class TextItem
{
    public GameObject gameObjectChieu;

    public TextMeshProUGUI txtNameChieuThuc;
    public TextMeshProUGUI txtNumberNangLuong;
    public TextMeshProUGUI txtHoiChieu;
    public TextMeshProUGUI txtMoTa;

    public TextMeshProUGUI txtNumberLv1;
    public TextMeshProUGUI txtNumberLv2;
    public TextMeshProUGUI txtNumberLv3;
    public TextMeshProUGUI txtNumberLv4;
    public TextMeshProUGUI txtNumberLv5;
    public TextMeshProUGUI txtNumberLv6;
}

public class DialogChiTietTuong : ScaleScreen
{
    [SerializeField] Button btnExit, btnThuocTinh, btnMuaTuong, btnPrevTuong, btnNextTuong;
    [SerializeField] Image imgKhungNen, imgKhoangTrong, imgLineToLevel;
    [SerializeField] Toggle tgChieu1, tgChieu2, tgChieu3, tgNoiTai;

    [Header("Text Items (Shared Skill Form)")]
    [SerializeField] private List<TextItem> textItems; // dùng chung 1 form: lấy index 0

    [SerializeField] TextMeshProUGUI txtTenTuong, txtBatTatHoiChieu;

    [Header("Thuộc tính (bật khi click btnThuocTinh)")]
    [SerializeField] GameObject thuocTinh;

    [SerializeField]
    TextMeshProUGUI txtCongVatLy, txtMau, txtGiap, txtCongPhep, txtNangLuongToiDa, txtGiapPhep, txtTocDanh, txtGiamHoiChieu,
        txtTyLeChiMang, txtTocChay, txtXuyenGiap, txtXuyenGiapPhep, txtHutMau, txtHutMauPhep, txtTamDanh, txtSatThuongChiMang, txtHoiMau,
        txtHoiNangLuong, txtKhangHieuUng;

    Toggle[] tgs;
    bool inited;

    JObject _heroSkillObj; // cache object CMD62 để bấm toggle render lại

    static readonly string[] Cmd61Keys =
    {
        "danhSachTuong",
        "danhSachChiSoTuong",
        "chiSoTuong",
        "danhSach",
        "data",
        "result",
        "payload"
    };

    void Awake() => Ensure();

    protected override void OnEnable()
    {
        base.OnEnable(); // gọi Scale() của ScaleScreen để scale theo màn hình
        Ensure();        // init listener/UI 1 lần
    }

    void Ensure()
    {
        if (inited) return;

        tgs = new[] { tgChieu1, tgChieu2, tgChieu3, tgNoiTai };

        if (btnExit)
        {
            btnExit.onClick.RemoveAllListeners();
            btnExit.onClick.AddListener(() => { Click(); Show(false); });
        }

        if (btnThuocTinh)
        {
            btnThuocTinh.onClick.RemoveAllListeners();
            btnThuocTinh.onClick.AddListener(() => { Click(); SetDetail(4, true); }); // 4 = thuộc tính
        }

        if (btnMuaTuong)
        {
            btnMuaTuong.onClick.RemoveAllListeners();
            btnMuaTuong.onClick.AddListener(() =>
            {
                Click();
                ClickTinhNangAn();
            });
        }

        if (btnPrevTuong)
        {
            btnPrevTuong.onClick.RemoveAllListeners();
            btnPrevTuong.onClick.AddListener(() =>
            {
                Click();
                ClickTinhNangAn();
            });
        }

        if (btnNextTuong)
        {
            btnNextTuong.onClick.RemoveAllListeners();
            btnNextTuong.onClick.AddListener(() =>
            {
                Click();
                ClickTinhNangAn();
            });
        }

        for (int i = 0; i < tgs.Length; i++)
        {
            int idx = i;
            var tg = tgs[i];
            if (!tg) continue;

            tg.onValueChanged.RemoveAllListeners();
            tg.onValueChanged.AddListener(on =>
            {
                if (!on) return;
                Click();
                SetDetail(idx, true);
            });
        }

        var btn = imgKhoangTrong ? imgKhoangTrong.GetComponent<Button>() : null;
        if (btn)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => { Click(); SetDetail(-1, false); });
        }

        if (thuocTinh) thuocTinh.SetActive(false);

        inited = true;
    }

    void InitUI()
    {
        SetDetail(-1, false);
        for (int i = 0; i < tgs.Length; i++) tgs[i]?.SetIsOnWithoutNotify(false);
    }

    void SetDetail(int index, bool show)
    {
        bool isSkill = show && index >= 0 && index <= 3; // 0..2: chiêu, 3: nội tại
        bool isAttr = show && index == 4;               // 4: thuộc tính

        bool showOverlay = show && (isSkill || isAttr);
        if (imgKhungNen) imgKhungNen.gameObject.SetActive(showOverlay);
        if (imgKhoangTrong) imgKhoangTrong.gameObject.SetActive(showOverlay);

        if (isSkill)
        {
            if (thuocTinh) thuocTinh.SetActive(false);
            ShowSharedSkillPanel();
            RenderSkillToSharedTextItem(index, _heroSkillObj);
        }
        else if (isAttr)
        {
            HideAllSkillPanels();
            if (thuocTinh) thuocTinh.SetActive(true); // chỉ bật khi click btnThuocTinh
            ShowAttributesTexts(true);
        }
        else
        {
            HideAllSkillPanels();
            if (thuocTinh) thuocTinh.SetActive(false); // bấm imgKhoangTrong hoặc Init -> off
            ShowAttributesTexts(false);
        }

        if (txtBatTatHoiChieu) txtBatTatHoiChieu.gameObject.SetActive(isSkill && index != 3); // tgNoiTai -> tắt

        if (!show)
        {
            for (int i = 0; i < tgs.Length; i++) tgs[i]?.SetIsOnWithoutNotify(false);
        }
    }

    void ShowAttributesTexts(bool on)
    {
        // nếu các txt này nằm trong thuocTinh thì bật/tắt thuocTinh là đủ,
        // nhưng vẫn bật/tắt thêm để không bị trường hợp text nằm ngoài root.
        SetTmpActive(txtCongVatLy, on);
        SetTmpActive(txtMau, on);
        SetTmpActive(txtGiap, on);
        SetTmpActive(txtCongPhep, on);
        SetTmpActive(txtNangLuongToiDa, on);
        SetTmpActive(txtGiapPhep, on);
        SetTmpActive(txtTocDanh, on);
        SetTmpActive(txtGiamHoiChieu, on);
        SetTmpActive(txtTyLeChiMang, on);
        SetTmpActive(txtTocChay, on);
        SetTmpActive(txtXuyenGiap, on);
        SetTmpActive(txtXuyenGiapPhep, on);
        SetTmpActive(txtHutMau, on);
        SetTmpActive(txtHutMauPhep, on);
        SetTmpActive(txtTamDanh, on);
        SetTmpActive(txtSatThuongChiMang, on);
        SetTmpActive(txtHoiMau, on);
        SetTmpActive(txtHoiNangLuong, on);
        SetTmpActive(txtKhangHieuUng, on);
    }

    static void SetTmpActive(TextMeshProUGUI t, bool on)
    {
        if (!t) return;
        t.gameObject.SetActive(on);
    }

    TextItem GetSharedTextItem()
    {
        if (textItems == null || textItems.Count == 0) return null;
        return textItems[0];
    }

    void ShowSharedSkillPanel()
    {
        var it = GetSharedTextItem();
        if (it == null) return;

        if (it.gameObjectChieu) it.gameObjectChieu.SetActive(true);
        SetTextItemActive(it, true);
    }

    void HideAllSkillPanels()
    {
        if (textItems == null || textItems.Count == 0) return;

        for (int i = 0; i < textItems.Count; i++)
        {
            var it = textItems[i];
            if (it == null) continue;

            if (it.gameObjectChieu) it.gameObjectChieu.SetActive(false);
            SetTextItemActive(it, false);
        }

        if (imgLineToLevel) imgLineToLevel.gameObject.SetActive(false);
    }

    static void SetTextItemActive(TextItem it, bool on)
    {
        if (it == null) return;

        if (it.txtNameChieuThuc) it.txtNameChieuThuc.gameObject.SetActive(on);
        if (it.txtNumberNangLuong) it.txtNumberNangLuong.gameObject.SetActive(on);
        if (it.txtHoiChieu) it.txtHoiChieu.gameObject.SetActive(on);
        if (it.txtMoTa) it.txtMoTa.gameObject.SetActive(on);

        if (it.txtNumberLv1) it.txtNumberLv1.gameObject.SetActive(on);
        if (it.txtNumberLv2) it.txtNumberLv2.gameObject.SetActive(on);
        if (it.txtNumberLv3) it.txtNumberLv3.gameObject.SetActive(on);
        if (it.txtNumberLv4) it.txtNumberLv4.gameObject.SetActive(on);
        if (it.txtNumberLv5) it.txtNumberLv5.gameObject.SetActive(on);
        if (it.txtNumberLv6) it.txtNumberLv6.gameObject.SetActive(on);
    }

    bool RenderSkillToSharedTextItem(int skillIndex, JObject heroObj)
    {
        if (heroObj == null) return false;
        if (skillIndex < 0 || skillIndex > 3) return false;

        var ui = GetSharedTextItem();
        if (ui == null) return false;

        string kName, kDesc;
        if (skillIndex == 0) { kName = "tenKyNang1"; kDesc = "moTaKyNang1"; }
        else if (skillIndex == 1) { kName = "tenKyNang2"; kDesc = "moTaKyNang2"; }
        else if (skillIndex == 2) { kName = "tenKyNang3"; kDesc = "moTaKyNang3"; }
        else { kName = "tenNoiTai"; kDesc = "moTaNoiTai"; } // nội tại dùng form chung

        var skillName = (string)heroObj[kName] ?? "";
        var raw = NormalizeNewlines((string)heroObj[kDesc] ?? "");

        ParseSkillText(raw, out string moTaChinh, out string hoiChieu, out string nangLuong, out string[] lvNums);

        if (ui.txtNameChieuThuc) { ui.txtNameChieuThuc.gameObject.SetActive(true); ui.txtNameChieuThuc.text = skillName; }
        if (ui.txtMoTa) { ui.txtMoTa.gameObject.SetActive(true); ui.txtMoTa.text = moTaChinh; }

        if (ui.txtHoiChieu) { ui.txtHoiChieu.gameObject.SetActive(true); ui.txtHoiChieu.text = string.IsNullOrWhiteSpace(hoiChieu) ? "-" : hoiChieu; }
        if (ui.txtNumberNangLuong) { ui.txtNumberNangLuong.gameObject.SetActive(true); ui.txtNumberNangLuong.text = string.IsNullOrWhiteSpace(nangLuong) ? "-" : nangLuong; }

        bool hasAnyLv = HasAnyLevel(lvNums);

        if (imgLineToLevel) imgLineToLevel.gameObject.SetActive(hasAnyLv);

        if (ui.txtNumberLv1) { ui.txtNumberLv1.gameObject.SetActive(hasAnyLv); ui.txtNumberLv1.text = SafeLv(lvNums, 0); }
        if (ui.txtNumberLv2) { ui.txtNumberLv2.gameObject.SetActive(hasAnyLv); ui.txtNumberLv2.text = SafeLv(lvNums, 1); }
        if (ui.txtNumberLv3) { ui.txtNumberLv3.gameObject.SetActive(hasAnyLv); ui.txtNumberLv3.text = SafeLv(lvNums, 2); }
        if (ui.txtNumberLv4) { ui.txtNumberLv4.gameObject.SetActive(hasAnyLv); ui.txtNumberLv4.text = SafeLv(lvNums, 3); }
        if (ui.txtNumberLv5) { ui.txtNumberLv5.gameObject.SetActive(hasAnyLv); ui.txtNumberLv5.text = SafeLv(lvNums, 4); }
        if (ui.txtNumberLv6) { ui.txtNumberLv6.gameObject.SetActive(hasAnyLv); ui.txtNumberLv6.text = SafeLv(lvNums, 5); }

        return true;
    }

    static bool HasAnyLevel(string[] arr)
    {
        if (arr == null) return false;
        for (int i = 0; i < arr.Length; i++)
            if (!string.IsNullOrWhiteSpace(arr[i])) return true;
        return false;
    }

    static string SafeLv(string[] arr, int idx)
    {
        if (arr == null || idx < 0 || idx >= arr.Length) return "";
        return arr[idx] ?? "";
    }

    static void ParseSkillText(string raw, out string moTaChinh, out string hoiChieu, out string nangLuong, out string[] lvNums)
    {
        moTaChinh = "";
        hoiChieu = "";
        nangLuong = "";
        lvNums = null;

        if (string.IsNullOrEmpty(raw)) return;

        string bestGroup = null;
        int bestCount = 0;

        var matches = Regex.Matches(raw, @"(?<!\d)(\d+(?:\.\d+)?(?:/\d+(?:\.\d+)?){2,5})(?!\d)");
        for (int i = 0; i < matches.Count; i++)
        {
            var g = matches[i].Groups[1].Value;
            int count = 1;
            for (int j = 0; j < g.Length; j++) if (g[j] == '/') count++;

            if (count > bestCount)
            {
                bestCount = count;
                bestGroup = g;
            }
        }

        if (!string.IsNullOrEmpty(bestGroup))
        {
            var parts = bestGroup.Split('/');
            lvNums = new string[6];
            for (int i = 0; i < lvNums.Length; i++) lvNums[i] = "";

            int n = parts.Length;
            if (n > 6) n = 6;

            for (int i = 0; i < n; i++) lvNums[i] = parts[i].Trim();
        }

        var lines = raw.Split('\n');
        var sbMoTa = new StringBuilder(256);

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (line.Length == 0) continue;

            if (StartsWithKey(line, "Hồi chiêu"))
            {
                var v = AfterColon(line).Trim();
                hoiChieu = ExtractFirstNumberOrFirstPart(v);
                continue;
            }

            if (StartsWithKey(line, "Năng lượng") || StartsWithKey(line, "Tiêu hao"))
            {
                var v = AfterColon(line).Trim();
                nangLuong = ExtractFirstNumberOrFirstPart(v);
                continue;
            }

            if (sbMoTa.Length > 0) sbMoTa.AppendLine();
            sbMoTa.Append(line);
        }

        moTaChinh = sbMoTa.ToString().Trim();
    }

    static string ExtractFirstNumberOrFirstPart(string v)
    {
        if (string.IsNullOrWhiteSpace(v)) return "";

        int slash = v.IndexOf('/');
        if (slash >= 0) v = v.Substring(0, slash).Trim();

        var m = Regex.Match(v, @"-?\d+(?:\.\d+)?");
        return m.Success ? m.Value : "";
    }

    static bool StartsWithKey(string line, string key)
    {
        return line.StartsWith(key, System.StringComparison.OrdinalIgnoreCase)
            || line.StartsWith(key + ":", System.StringComparison.OrdinalIgnoreCase);
    }

    static string AfterColon(string line)
    {
        int idx = line.IndexOf(':');
        if (idx < 0) return "";
        return line.Substring(idx + 1);
    }

    void Click() => AudioManager.Instance?.AudioClick();

    public void Show(bool val = true)
    {
        Ensure();
        gameObject.SetActive(val);
        if (val) InitUI();
    }

    // ===== CMD 62: Mô tả kỹ năng =====
    public void SetDataFromCmd62(Message msg)
    {
        Ensure();

        var arr = ExtractArrayForCmd62(msg);
        if (arr == null || arr.Count == 0) return;

        int id = HeroSelectionCache.IdLoaiTuong;
        var o = FindHeroObj(arr, id, "idTuong", "idLoaiTuong", "id");
        if (o == null) return;

        _heroSkillObj = o;

        if (txtTenTuong)
        {
            var serverName = (string)o["ten"];
            txtTenTuong.text = !string.IsNullOrEmpty(HeroSelectionCache.TenLoaiTuong)
                ? HeroSelectionCache.TenLoaiTuong
                : (!string.IsNullOrEmpty(serverName) ? serverName : $"Tướng #{GetId(o)}");
        }

        HideAllSkillPanels();
        SetDetail(-1, false);
    }

    static JArray ExtractArrayForCmd62(Message msg)
    {
        if (msg.ConstainsKey("danhSachKyNang"))
        {
            try { return msg.GetJArray("danhSachKyNang"); } catch { }
        }

        try
        {
            var root = JObject.FromObject(msg);
            var a = root.SelectToken("data.danhSachKyNang") as JArray;
            if (a != null && a.Count > 0) return a;
        }
        catch { }

        return null;
    }

    static string NormalizeNewlines(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        return s.Replace("\\r\\n", "\n").Replace("\\n", "\n").Replace("\r\n", "\n");
    }

    // ===== CMD 61: Chỉ số tướng =====
    public void SetDataFromCmd61(Message msg)
    {
        Ensure();

        var arr = ExtractArrayForCmd61(msg);
        if (arr == null || arr.Count == 0)
        {
            RenderAttributesFromCmd61(null);
            SetDetail(-1, false);
            return;
        }

        int id = HeroSelectionCache.IdLoaiTuong;
        var o = FindHeroObj(arr, id, "idLoaiTuong", "idTuong", "id");
        if (o == null)
        {
            RenderAttributesFromCmd61(null);
            SetDetail(-1, false);
            return;
        }

        if (txtTenTuong)
        {
            var serverName = (string)o["ten"];
            txtTenTuong.text = !string.IsNullOrEmpty(HeroSelectionCache.TenLoaiTuong)
                ? HeroSelectionCache.TenLoaiTuong
                : (!string.IsNullOrEmpty(serverName) ? serverName : $"Tướng #{GetId(o)}");
        }

        RenderAttributesFromCmd61(o);

        // không tự bật thuộc tính, chỉ gán data; panel vẫn OFF cho tới khi user bấm btnThuocTinh
        if (thuocTinh) thuocTinh.SetActive(false);
        ShowAttributesTexts(false);

        SetDetail(-1, false);
    }

    void RenderAttributesFromCmd61(JObject o)
    {
        SetStatInt(txtCongVatLy, o, "satThuong");
        SetStatInt(txtCongPhep, o, "satThuongPhep");
        SetStatInt(txtMau, o, "mauToiDa");
        SetStatInt(txtNangLuongToiDa, o, "nangLuongToiDa");
        SetStatInt(txtGiap, o, "giapVatLy");
        SetStatInt(txtGiapPhep, o, "giapPhep");
        SetStatInt(txtTocChay, o, "tocDoDiChuyen");
        SetStatInt(txtXuyenGiap, o, "xuyenGiapVatLy");
        SetStatInt(txtXuyenGiapPhep, o, "xuyenGiapPhep");
        SetStatInt(txtTamDanh, o, "tamDanh");

        SetStatFloat(txtTocDanh, o, "tocDoDanh");
        SetStatFloat(txtGiamHoiChieu, o, "giamHoiChieu");
        SetStatFloat(txtHutMau, o, "hutMau");
        SetStatFloat(txtHoiMau, o, "hoiMau");
        SetStatFloat(txtHoiNangLuong, o, "hoiNangLuong");
        SetStatFloat(txtKhangHieuUng, o, "khangHieuUng");

        SetStatFloatMulti(txtTyLeChiMang, o, "tyLeChiMang", "tiLeChiMang", "tyLeChiMangVatLy", "tiLeChiMangVatLy");
        SetStatFloatMulti(txtHutMauPhep, o, "hutMauPhep");
        SetStatFloatMulti(txtSatThuongChiMang, o, "satThuongChiMang");
    }

    static void SetStatInt(TextMeshProUGUI t, JObject o, string key)
    {
        if (!t) return;
        int? v = TryGetInt(o, key);
        t.text = v.HasValue ? v.Value.ToString() : "--";
    }

    static void SetStatFloat(TextMeshProUGUI t, JObject o, string key)
    {
        if (!t) return;
        float? v = TryGetFloat(o, key);
        t.text = v.HasValue ? v.Value.ToString("0.##") : "--";
    }

    static void SetStatFloatMulti(TextMeshProUGUI t, JObject o, params string[] keys)
    {
        if (!t) return;
        float? v = null;
        if (o != null && keys != null)
        {
            for (int i = 0; i < keys.Length; i++)
            {
                v = TryGetFloat(o, keys[i]);
                if (v.HasValue) break;
            }
        }
        t.text = v.HasValue ? v.Value.ToString("0.##") : "--";
    }

    static int? TryGetInt(JObject o, string key)
    {
        if (o == null || string.IsNullOrEmpty(key)) return null;

        var token = o[key];
        if (token == null || token.Type == JTokenType.Null) return null;

        try
        {
            if (token.Type == JTokenType.Integer) return token.Value<int>();
            if (token.Type == JTokenType.Float) return (int)token.Value<float>();
            if (token.Type == JTokenType.String)
            {
                if (int.TryParse(token.Value<string>(), out int n)) return n;
                if (float.TryParse(token.Value<string>(), out float f)) return (int)f;
            }
        }
        catch { }

        return null;
    }

    static float? TryGetFloat(JObject o, string key)
    {
        if (o == null || string.IsNullOrEmpty(key)) return null;

        var token = o[key];
        if (token == null || token.Type == JTokenType.Null) return null;

        try
        {
            if (token.Type == JTokenType.Float) return token.Value<float>();
            if (token.Type == JTokenType.Integer) return token.Value<int>();
            if (token.Type == JTokenType.String)
            {
                if (float.TryParse(token.Value<string>(), out float f)) return f;
                if (int.TryParse(token.Value<string>(), out int n)) return n;
            }
        }
        catch { }

        return null;
    }

    static JArray ExtractArrayForCmd61(Message msg)
    {
        for (int i = 0; i < Cmd61Keys.Length; i++)
        {
            var k = Cmd61Keys[i];
            if (!msg.ConstainsKey(k)) continue;

            try
            {
                var a = msg.GetJArray(k);
                if (a != null && a.Count > 0) return a;
            }
            catch { }
        }
        return null;
    }

    static JObject FindHeroObj(JArray arr, int id, params string[] idKeys)
    {
        if (arr == null) return null;

        for (int i = 0; i < arr.Count; i++)
        {
            if (arr[i] is not JObject o) continue;
            if (HasAnyId(o, id, idKeys)) return o;
        }

        for (int i = 0; i < arr.Count; i++)
            if (arr[i] is JObject o) return o;

        return null;
    }

    static bool HasAnyId(JObject o, int id, string[] keys)
    {
        for (int i = 0; i < keys.Length; i++)
        {
            var v = (int?)o[keys[i]];
            if (v.HasValue && v.Value == id) return true;
        }
        return false;
    }

    static int GetId(JObject o)
    {
        return (int?)o["idLoaiTuong"]
            ?? (int?)o["idTuong"]
            ?? (int?)o["id"]
            ?? 0;
    }
}
