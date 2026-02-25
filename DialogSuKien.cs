using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogSuKien : ScaleScreen
{
    [Header("Exit Buttons (kéo nhiều button vào đây)")]
    public Button[] btnExits;

    [Header("Header UI (kéo nếu có)")]
    public TextMeshProUGUI txtTitle;
    public TextMeshProUGUI txtTimeRange;
    public TextMeshProUGUI txtDescription;

    [Header("Nav Buttons")]
    public Button btnPrev, btnNext;

    [Header("List UI")]
    public Transform content;
    public ItemSK itemPrefab;

    // ====== DATA CACHE ======
    private JArray _events;        // danh sách event
    private int _eventIndex = -1;  // index hiện tại

    protected override void Start()
    {
        base.Start();

        if (btnExits != null)
        {
            foreach (var btn in btnExits)
                if (btn != null)
                    btn.onClick.AddListener(OnExitClicked);
        }

        if (btnPrev != null) btnPrev.onClick.AddListener(OnPrevClicked);
        if (btnNext != null) btnNext.onClick.AddListener(OnNextClicked);
    }

    private void OnExitClicked()
    {
        AudioManager.Instance.AudioClick();
        Show(false);
    }

    private void OnPrevClicked()
    {
        AudioManager.Instance.AudioClick();
        MoveEvent(-1);
    }

    private void OnNextClicked()
    {
        AudioManager.Instance.AudioClick();
        MoveEvent(+1);
    }

    public void Show(bool val = true)
    {
        gameObject.SetActive(val);

        // nếu đã có data rồi thì mở dialog sẽ build luôn
        if (val && _events != null && _events.Count > 0 && _eventIndex >= 0)
            BuildUI();
    }

    /// <summary>
    /// Bị gọi từ CommandGetEventInfoSystem sau khi nhận data
    /// </summary>
    public void ApplyEvents(JArray eventsArr, int startIndex)
    {
        _events = eventsArr;

        if (_events == null || _events.Count == 0)
        {
            _eventIndex = -1;
            RefreshNavButtons();
            return;
        }

        _eventIndex = Mathf.Clamp(startIndex, 0, _events.Count - 1);
        RefreshNavButtons();

        if (isActiveAndEnabled)
            BuildUI();
    }

    private void MoveEvent(int delta)
    {
        if (_events == null || _events.Count == 0 || _eventIndex < 0) return;
        if (_events.Count == 1) return;

        _eventIndex += delta;

        // wrap-around
        if (_eventIndex < 0) _eventIndex = _events.Count - 1;
        else if (_eventIndex >= _events.Count) _eventIndex = 0;

        RefreshNavButtons();
        BuildUI();
    }

    private void RefreshNavButtons()
    {
        bool hasMany = _events != null && _events.Count > 1;
        if (btnPrev != null) btnPrev.interactable = hasMany;
        if (btnNext != null) btnNext.interactable = hasMany;
    }

    private void BuildUI()
    {
        if (_events == null || _events.Count == 0 || _eventIndex < 0) return;

        var evt = _events[_eventIndex];
        if (evt == null) return;

        // ----- HEADER -----
        string nameEvent = evt.Value<string>("nameEvent");
        string mota = evt.Value<string>("mota");
        string timeStart = evt.Value<string>("timeStart");
        string timeEnd = evt.Value<string>("timeEnd");

        if (txtTitle != null) txtTitle.text = nameEvent ?? "";
        if (txtDescription != null) txtDescription.text = mota ?? "";
        if (txtTimeRange != null) txtTimeRange.text = $"Thời gian sự kiện: {timeStart} - {timeEnd}";

        // ----- CLEAR ITEMS CŨ -----
        if (content == null || itemPrefab == null) return;

        for (int i = content.childCount - 1; i >= 0; i--)
            Destroy(content.GetChild(i).gameObject);

        // ----- TẠO ITEM THEO dsMoc -----
        var dsMoc = evt["dsMoc"] as JArray;
        if (dsMoc == null) return;

        foreach (var moc in dsMoc)
        {
            var item = Instantiate(itemPrefab, content);
            item.SetupFromJson(moc);
        }
    }
}
