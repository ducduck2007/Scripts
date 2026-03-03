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

    private Coroutine routine;
    private CanvasGroup _cg;

    protected override void OnEnable()
    {
        base.OnEnable();

        if (_cg == null)
        {
            _cg = GetComponent<CanvasGroup>();
            if (_cg == null) _cg = gameObject.AddComponent<CanvasGroup>();
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

            if (data != null)
                PopulateTeams(data);

            if (routine != null) StopCoroutine(routine);
            routine = StartCoroutine(AnimateDots());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PopulateTeams(LoadVaoTranData data)
    {
        if (data == null) return;
        SetupTeam(doiXanh, data.teamXanh);
        SetupTeam(doiDo, data.teamDo);
    }

    private void SetupTeam(Transform teamContainer, List<LoadVaoTranData.PlayerEntry> players)
    {
        if (teamContainer == null) return;

        int childCount = teamContainer.childCount;
        int playerCount = players.Count;
        int startIndex = (childCount - playerCount) / 2;

        for (int i = 0; i < childCount; i++)
        {
            Transform item = teamContainer.GetChild(i);
            int playerIndex = i - startIndex;

            if (playerIndex >= 0 && playerIndex < playerCount)
            {
                item.gameObject.SetActive(true);
                SetItemPlayer(item, players[playerIndex]);
            }
            else
            {
                item.gameObject.SetActive(false);
            }
        }
    }

    private void SetItemPlayer(Transform item, LoadVaoTranData.PlayerEntry info)
    {
        var txtName = item.Find("txtName");
        if (txtName != null)
        {
            var tmp = txtName.GetComponent<TextMeshProUGUI>();
            if (tmp != null) tmp.text = info.displayName;
        }

        var avatarTuong = item.Find("AvatarTuong");
        if (avatarTuong != null)
        {
            var img = avatarTuong.GetComponent<Image>();
            int index = info.heroType - 1;
            if (img != null && index >= 0 && index < heroAvatars.Length && heroAvatars[index] != null)
            {
                img.sprite = heroAvatars[index];
            }
        }
    }

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