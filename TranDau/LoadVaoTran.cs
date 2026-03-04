using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadVaoTran : ScaleScreen
{
    public TextMeshProUGUI loadingText;

    [Header("Team Containers")]
    public Transform doiXanh;
    public Transform doiDo;

    [Header("Hero Avatars")]
    public Sprite[] heroAvatars;
    public Sprite defaultAvatar;

    private Coroutine routine;
    private CanvasGroup _cg;

    // ==============================
    // TeamSize đọc trực tiếp từ runtime
    // ==============================
    private int TeamSize => Mathf.Clamp(MatchRuntime.TeamSize, 1, 5);

    protected override void OnEnable()
    {
        base.OnEnable();

        if (_cg == null)
        {
            _cg = GetComponent<CanvasGroup>();
            if (_cg == null)
                _cg = gameObject.AddComponent<CanvasGroup>();

            SetVisible(false);
        }
    }

    private void SetVisible(bool val)
    {
        if (_cg == null) return;

        _cg.alpha = val ? 1f : 0f;
        _cg.interactable = val;
        _cg.blocksRaycasts = val;
    }

    public void Prewarm()
    {
        SetVisible(false);
    }

    public void Show(bool val = true)
    {
        Show(val, null);
    }

    public void Show(bool val, LoadVaoTranData data)
    {
        if (val)
        {
            SetVisible(true);
            PlayLoadGate.Reset();

            if (PopupController.Instance != null)
                PopupController.Instance.ChonTuong.Show(false);

            ResetTeamsUI();

            if (data != null)
                PopulateTeams(data);

            if (routine != null)
                StopCoroutine(routine);

            routine = StartCoroutine(AnimateDots());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // =====================================================
    // RESET UI THEO MODE SERVER (CĂN GIỮA)
    // =====================================================

    private void ResetTeamsUI()
    {
        ResetTeamContainer(doiXanh);
        ResetTeamContainer(doiDo);
    }

    private void ResetTeamContainer(Transform teamContainer)
    {
        if (teamContainer == null) return;

        int totalSlots = teamContainer.childCount;
        int startIndex = (totalSlots - TeamSize) / 2;
        int endIndex = startIndex + TeamSize;

        for (int i = 0; i < totalSlots; i++)
        {
            Transform item = teamContainer.GetChild(i);

            bool active = (i >= startIndex && i < endIndex);
            item.gameObject.SetActive(active);

            if (active)
                SetDefaultItem(item);
        }
    }

    private void SetDefaultItem(Transform item)
    {
        var txtName = item.Find("txtName")?.GetComponent<TextMeshProUGUI>();
        if (txtName != null)
            txtName.text = "Đang chờ...";

        var avatar = item.Find("AvatarTuong")?.GetComponent<Image>();
        if (avatar != null)
            avatar.sprite = defaultAvatar;
    }

    // =====================================================
    // POPULATE DATA (CĂN GIỮA + MAP PLAYER)
    // =====================================================

    public void PopulateTeams(LoadVaoTranData data)
    {
        if (data == null) return;

        SetupTeam(doiXanh, data.teamXanh);
        SetupTeam(doiDo, data.teamDo);
    }

    private void SetupTeam(Transform teamContainer, List<LoadVaoTranData.PlayerEntry> players)
    {
        if (teamContainer == null) return;

        int totalSlots = teamContainer.childCount;
        int startIndex = (totalSlots - TeamSize) / 2;
        int endIndex = startIndex + TeamSize;

        int playerCount = players.Count;
        int playerIndex = 0;

        for (int i = 0; i < totalSlots; i++)
        {
            Transform item = teamContainer.GetChild(i);

            if (i < startIndex || i >= endIndex)
            {
                item.gameObject.SetActive(false);
                continue;
            }

            item.gameObject.SetActive(true);

            if (playerIndex < playerCount)
            {
                SetItemPlayer(item, players[playerIndex]);
                playerIndex++;
            }
            else
            {
                SetDefaultItem(item);
            }
        }
    }

    private void SetItemPlayer(Transform item, LoadVaoTranData.PlayerEntry info)
    {
        var txtName = item.Find("txtName")?.GetComponent<TextMeshProUGUI>();
        if (txtName != null)
            txtName.text = info.displayName;

        var avatar = item.Find("AvatarTuong")?.GetComponent<Image>();
        if (avatar != null)
        {
            int index = info.heroType - 1;

            if (index >= 0 &&
                index < heroAvatars.Length &&
                heroAvatars[index] != null)
            {
                avatar.sprite = heroAvatars[index];
            }
            else
            {
                avatar.sprite = defaultAvatar;
            }
        }
    }

    // =====================================================
    // LOADING DOTS
    // =====================================================

    private IEnumerator AnimateDots()
    {
        int dotCount = 0;

        while (true)
        {
            dotCount = (dotCount + 1) % 4;

            if (loadingText != null)
                loadingText.text = "Đang vào trận" + new string('.', dotCount);

            yield return new WaitForSeconds(0.4f);
        }
    }
}