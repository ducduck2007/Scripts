using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupGhepTran : ScaleScreen
{
    public Button btnSanSang;
    public TextMeshProUGUI txtTime, txtSluong, txtTrangThai;
    public ItemPlayerGhepTran[] itemPlayers1;
    public ItemPlayerGhepTran[] itemPlayers2;

    [Header("Staged Reveal")]
    [SerializeField] private GameObject giua;
    [SerializeField] private RectTransform phai;
    [SerializeField] private RectTransform trai;
    [SerializeField] private float delayGiua = 0.15f;
    [SerializeField] private float delayPhaiTrai = 0.3f;
    [SerializeField] private float slideDuration = 0.4f;
    [SerializeField] private float slideOffset = 1000;

    [Header("Rotating Image")]
    [SerializeField] private RectTransform imgGiuaRotate;
    [SerializeField] private float rotateSpeed = 135f;

    private bool _hasAccepted;
    private bool _isRotating;

    protected override void Start()
    {
        base.Start();
        if (btnSanSang) btnSanSang.onClick.AddListener(ClickSanSang);
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        _hasAccepted = false;
        _isRotating = true;
        if (btnSanSang) btnSanSang.interactable = true;
        if (txtTrangThai) txtTrangThai.text = "Chấp nhận!";

        if (MatchFoundDataBase.Instance != null)
        {
            UpdateAcceptUI(
                MatchFoundDataBase.Instance.AcceptedCount,
                MatchFoundDataBase.Instance.TotalPlayers
            );
            UpdateTimerUI(MatchFoundDataBase.Instance.GetRemainingSeconds());
            InitTeamUI();
        }

        MatchFoundDataBase.Instance.OnTimerTick += UpdateTimerUI;
        MatchFoundDataBase.Instance.OnAcceptProgressUpdated += OnAcceptProgressUpdated;
        MatchFoundDataBase.Instance.OnMatchCancelled += OnMatchCancelled;
        MatchFoundDataBase.Instance.OnMatchReady += OnMatchReady;

        StartCoroutine(StagedReveal());
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        _isRotating = false;
        StopAllCoroutines();

        if (imgGiuaRotate) imgGiuaRotate.localRotation = Quaternion.identity;

        if (MatchFoundDataBase.Instance != null)
        {
            MatchFoundDataBase.Instance.OnTimerTick -= UpdateTimerUI;
            MatchFoundDataBase.Instance.OnAcceptProgressUpdated -= OnAcceptProgressUpdated;
            MatchFoundDataBase.Instance.OnMatchCancelled -= OnMatchCancelled;
            MatchFoundDataBase.Instance.OnMatchReady -= OnMatchReady;
        }
    }

    private void Update()
    {
        if (MatchFoundDataBase.Instance != null)
        {
            MatchFoundDataBase.Instance.Update();
        }

        if (_isRotating && imgGiuaRotate)
        {
            imgGiuaRotate.Rotate(0f, 0f, -rotateSpeed * Time.unscaledDeltaTime);
        }
    }

    private IEnumerator StagedReveal()
    {
        if (giua) giua.SetActive(false);
        if (phai) phai.gameObject.SetActive(false);
        if (trai) trai.gameObject.SetActive(false);

        yield return new WaitForSeconds(delayGiua);
        if (giua) giua.SetActive(true);

        yield return new WaitForSeconds(delayPhaiTrai);

        Coroutine c1 = null, c2 = null;
        if (phai)
        {
            phai.gameObject.SetActive(true);
            c1 = StartCoroutine(SlideFromOffscreen(phai, slideOffset, slideDuration));
        }
        if (trai)
        {
            trai.gameObject.SetActive(true);
            c2 = StartCoroutine(SlideFromOffscreen(trai, -slideOffset, slideDuration));
        }

        if (c1 != null) yield return c1;
        if (c2 != null) yield return c2;
    }

    private IEnumerator SlideFromOffscreen(RectTransform rt, float startOffset, float duration)
    {
        Vector2 targetPos = rt.anchoredPosition;
        Vector2 startPos = new Vector2(targetPos.x + startOffset, targetPos.y);
        rt.anchoredPosition = startPos;
        rt.localScale = Vector3.one;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            rt.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            yield return null;
        }
        rt.anchoredPosition = targetPos;

        yield return StartCoroutine(BounceEffect(rt));
    }

    private IEnumerator BounceEffect(RectTransform rt)
    {
        // Expand: 1.0 → 1.12
        float elapsed = 0f;
        float expandDur = 0.15f;
        while (elapsed < expandDur)
        {
            elapsed += Time.unscaledDeltaTime;
            float scale = Mathf.Lerp(1f, 1.12f, elapsed / expandDur);
            rt.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }

        // Shrink: 1.12 → 0.94
        elapsed = 0f;
        float shrinkDur = 0.12f;
        while (elapsed < shrinkDur)
        {
            elapsed += Time.unscaledDeltaTime;
            float scale = Mathf.Lerp(1.12f, 0.94f, elapsed / shrinkDur);
            rt.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }

        // Bounce: 0.94 → 1.03
        elapsed = 0f;
        float bounceDur = 0.1f;
        while (elapsed < bounceDur)
        {
            elapsed += Time.unscaledDeltaTime;
            float scale = Mathf.Lerp(0.94f, 1.03f, elapsed / bounceDur);
            rt.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }

        // Settle: 1.03 → 1.0
        elapsed = 0f;
        float settleDur = 0.08f;
        while (elapsed < settleDur)
        {
            elapsed += Time.unscaledDeltaTime;
            float scale = Mathf.Lerp(1.03f, 1f, elapsed / settleDur);
            rt.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }

        rt.localScale = Vector3.one;
    }

    // ========== TEAM UI ==========

    /// <summary>
    /// Gọi khi CMD 103 về - fill data 2 đội vào itemPlayers
    /// </summary>
    private void InitTeamUI()
    {
        var data = MatchFoundDataBase.Instance;
        if (data == null) return;

        // Đội 1 (Phai)
        for (int i = 0; i < itemPlayers1.Length; i++)
        {
            if (i < data.Team1Players.Count)
            {
                var p = data.Team1Players[i];
                itemPlayers1[i].SetMatchPlayer(p.UserId, p.Accepted);
            }
            else
            {
                itemPlayers1[i].ClearSlot();
            }
        }

        // Đội 2 (Trai)
        for (int i = 0; i < itemPlayers2.Length; i++)
        {
            if (i < data.Team2Players.Count)
            {
                var p = data.Team2Players[i];
                itemPlayers2[i].SetMatchPlayer(p.UserId, p.Accepted);
            }
            else
            {
                itemPlayers2[i].ClearSlot();
            }
        }
    }

    /// <summary>
    /// Gọi khi CMD 104 về - cập nhật trạng thái accepted từng player (ẩn/hiện Img)
    /// </summary>
    private void UpdateAcceptStatusUI()
    {
        var data = MatchFoundDataBase.Instance;
        if (data == null) return;

        foreach (var item in itemPlayers1)
        {
            long uid = item.idPlayer;
            if (uid == 0) continue;
            foreach (var p in data.Team1Players)
            {
                if (p.UserId == uid) { item.SetAccepted(p.Accepted); break; }
            }
        }

        foreach (var item in itemPlayers2)
        {
            long uid = item.idPlayer;
            if (uid == 0) continue;
            foreach (var p in data.Team2Players)
            {
                if (p.UserId == uid) { item.SetAccepted(p.Accepted); break; }
            }
        }
    }

    // ========== UI UPDATES ==========

    private void UpdateTimerUI(float remainingSeconds)
    {
        if (txtTime) txtTime.text = Mathf.CeilToInt(remainingSeconds).ToString();
    }

    private void UpdateAcceptUI(int accepted, int total)
    {
        if (txtSluong) txtSluong.text = $"{accepted}/{total}";
    }

    private void OnAcceptProgressUpdated()
    {
        if (MatchFoundDataBase.Instance == null) return;
        UpdateAcceptUI(
            MatchFoundDataBase.Instance.AcceptedCount,
            MatchFoundDataBase.Instance.TotalPlayers
        );
        UpdateAcceptStatusUI();
    }

    private void OnMatchCancelled()
    {
        Show(false);

        if (DialogController.Instance != null)
        {
            var popup = DialogController.Instance.PopupTimTran;
            if (popup != null)
            {
                popup.Show(true);
                popup.StopFindingMatchUI();
            }
        }
    }

    private void OnMatchReady()
    {
        Show(false);
    }

    // ========== BUTTONS ==========

    private void ClickSanSang()
    {
        if (_hasAccepted) return;
        _hasAccepted = true;
        if (btnSanSang) btnSanSang.interactable = false;
        if (txtTrangThai) txtTrangThai.text = "Đã sẵn sàng, đang chờ...";
        SendData.PartyAcceptMatch();
    }

    public void Show(bool val = true)
    {
        gameObject.SetActive(val);
    }
}