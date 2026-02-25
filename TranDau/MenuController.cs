using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class MenuController : ScaleScreen
{
    public static MenuController Instance;

    public JoystickController joystick;
    public TMP_Text txtTime;

    [Header("Chat In Match UI")]
    public ScrollRect chatScrollRect;
    public Transform chatContent;
    public ChatInMatchItem chatItemPrefab;
    public TMP_InputField inputChatInMatch;
    public Button iconChat;
    public GameObject objectChat;

    [Header("Chat Auto Hide (feature)")]
    [SerializeField] private bool autoHideChatMessages = true;
    [SerializeField] private float chatVisibleSeconds = 7f;
    [SerializeField] private float chatFadeDuration = 0.12f;

    [SerializeField] private CanvasGroup chatCanvasGroup;

    private Coroutine _chatAutoHideCo;
    private Coroutine _focusChatCo;

    public Button btnDanhThuong, btnShowItem, btnSendMsTeam, btnSendMsAll;

    [Serializable]
    public class SkillUI
    {
        public Button btnCast;
        public Image imgCooldown;
        public TMP_Text txtCooldown;

        public Button btnPlusLevel;

        [Tooltip("Kéo thả theo thứ tự: level 1 -> level N")]
        public Image[] levelIcons;
    }

    [Header("Skills")]
    public SkillUI skill1 = new SkillUI();
    public SkillUI skill2 = new SkillUI();
    public SkillUI skill3 = new SkillUI();

    [Header("HUD - Local Player")]
    public TMP_Text txtGold;

    [Header("HUD - K/D/A")]
    public TMP_Text tongTySoBenXanh;
    public TMP_Text tongTySoBenDo;
    public TMP_Text txtK;
    public TMP_Text txtD;
    public TMP_Text txtA;

    [Header("Debug")]
    public bool debugHud = false;
    public long forceUserId = 0;

    [Header("Hero - From CMD 43")]
    public int localHeroIndex = -1;
    public int localHeroType = 0;

    [Header("Skill Icons (Resources)")]
    [SerializeField] private bool autoApplySkillIcons = true;
    [SerializeField] private string kaynFolder = "Sprites/img_skills/kayn";
    [SerializeField] private string leonaFolder = "Sprites/img_skills/leona";

    private int _lastAppliedHeroType = int.MinValue;

    [Header("Cooldown - Fallback (nếu server chưa gửi time_cd)")]
    [SerializeField] private float fallbackCooldownTime = 4f;

    [Header("Skill Level Icon Alpha")]
    [Range(0f, 1f)]
    [SerializeField] private float lockedAlpha = 36f / 255f;

    private Coroutine _cd1;
    private Coroutine _cd2;
    private Coroutine _cd3;

    private long _localUserId;

    private static readonly Color32 COLOR_UNLOCKED = new Color32(255, 255, 255, 255);
    private int _lastLevel = int.MinValue;

    private int _spentPoints = 0;
    private bool _spentInitFromServer = false;

    private int _srvS1, _srvS2, _srvS3;

    private int _predS1, _predS2, _predS3;
    private bool _pendingS1, _pendingS2, _pendingS3;

    [Header("Chat Panel Slide")]
    [SerializeField] private Vector3 chatPosHidden = new Vector3(0f, 220f, 0f);
    [SerializeField] private Vector3 chatPosShown = new Vector3(0f, 0f, 0f);
    [SerializeField] private float chatSlideDuration = 0.22f;
    [SerializeField] private Ease chatSlideEase = Ease.OutCubic;

    private bool _chatShown = false;
    private Tween _chatTween;

    private bool _prewarmed = false;

    protected override void OnEnable()
    {
        base.OnEnable();

        Instance = this;

        ResetHudState();

        _localUserId = UserData.Instance != null ? UserData.Instance.UserID : 0;
        UdpResourceSnapshotSystem.OnPlayerResourceUpdated += OnAnyPlayerUpdated;

        SyncLocalHeroFromCmd43();
        ApplySkillIconsIfNeeded(true);

        InitChatPanelState();
        InitChatAutoHideState();
    }

    private void OnDisable()
    {
        UdpResourceSnapshotSystem.OnPlayerResourceUpdated -= OnAnyPlayerUpdated;

        if (_chatAutoHideCo != null)
        {
            StopCoroutine(_chatAutoHideCo);
            _chatAutoHideCo = null;
        }

        if (_focusChatCo != null)
        {
            StopCoroutine(_focusChatCo);
            _focusChatCo = null;
        }
    }

    protected override void Start()
    {
        base.Start();

        InitCooldownUI();
        InitSkillLevelIcons();

        if (btnShowItem != null) btnShowItem.onClick.AddListener(ShowItemShop);

        if (skill1.btnCast != null) skill1.btnCast.onClick.AddListener(TungChieu1);
        if (skill2.btnCast != null) skill2.btnCast.onClick.AddListener(TungChieu2);
        if (skill3.btnCast != null) skill3.btnCast.onClick.AddListener(TungChieu3);

        EnsureHoldTracker(skill1.btnCast);
        EnsureHoldTracker(skill2.btnCast);
        EnsureHoldTracker(skill3.btnCast);

        var t1 = EnsureHoldTracker(skill1.btnCast); if (t1 != null) t1.skillIndex = 1;
        var t2 = EnsureHoldTracker(skill2.btnCast); if (t2 != null) t2.skillIndex = 2;
        var t3 = EnsureHoldTracker(skill3.btnCast); if (t3 != null) t3.skillIndex = 3;


        if (skill1.btnPlusLevel != null) skill1.btnPlusLevel.onClick.AddListener(() => OnClickPlus(1));
        if (skill2.btnPlusLevel != null) skill2.btnPlusLevel.onClick.AddListener(() => OnClickPlus(2));
        if (skill3.btnPlusLevel != null) skill3.btnPlusLevel.onClick.AddListener(() => OnClickPlus(3));

        if (btnSendMsTeam != null)
            btnSendMsTeam.onClick.AddListener(() => OnClickSendChat(1));

        if (btnSendMsAll != null)
            btnSendMsAll.onClick.AddListener(() => OnClickSendChat(0));

        if (btnDanhThuong != null)
        {
            btnDanhThuong.onClick.AddListener(() =>
            {
                if (TranDauControl.Instance != null && TranDauControl.Instance.playerMove != null)
                    TranDauControl.Instance.playerMove.NormalAttack();
            });
        }

        if (iconChat != null)
            iconChat.onClick.AddListener(ToggleChatPanel);

        SetPlusButtonsVisible(false, false, false);
        RefreshSkillCastLockOverlays();

        if (!_prewarmed && LoadController.Instance != null)
        {
            _prewarmed = true;
            LoadController.Instance.PrewarmSafe();
        }
    }

    private void Update()
    {
        if (B.Instance == null) return;

        if (localHeroIndex != B.Instance.heroPlayer)
        {
            SyncLocalHeroFromCmd43();
            ApplySkillIconsIfNeeded(false);
        }
        else if (_lastAppliedHeroType != localHeroType)
        {
            ApplySkillIconsIfNeeded(false);
        }

        UpdateGameTimeUI();
        UpdateScoreUI();
    }

    private void InitChatPanelState()
    {
        if (objectChat == null) return;

        var cur = objectChat.transform.localPosition;
        if (Vector3.Distance(cur, chatPosHidden) < 0.01f)
            _chatShown = false;
        else if (Vector3.Distance(cur, chatPosShown) < 0.01f)
            _chatShown = true;
        else
        {
            objectChat.transform.localPosition = chatPosHidden;
            _chatShown = false;
        }
    }

    private void InitChatAutoHideState()
    {
        if (!autoHideChatMessages) return;

        var cg = EnsureChatCanvasGroup();
        if (cg == null) return;

        cg.alpha = 0f;
        cg.blocksRaycasts = false;
        cg.interactable = false;
    }

    private void ToggleChatPanel()
    {
        if (objectChat == null) return;

        _chatShown = !_chatShown;

        _chatTween?.Kill();
        Vector3 target = _chatShown ? chatPosShown : chatPosHidden;

        _chatTween = objectChat.transform
            .DOLocalMove(target, chatSlideDuration)
            .SetEase(chatSlideEase)
            .SetUpdate(true);

        if (_chatShown && inputChatInMatch != null)
        {
            if (_focusChatCo != null) StopCoroutine(_focusChatCo);
            _focusChatCo = StartCoroutine(CoFocusChatInputNextFrame());
        }
    }

    private IEnumerator CoFocusChatInputNextFrame()
    {
        yield return null;
        _focusChatCo = null;

        if (inputChatInMatch == null) yield break;
        inputChatInMatch.Select();
        inputChatInMatch.ActivateInputField();
    }

    private void SyncLocalHeroFromCmd43()
    {
        if (B.Instance == null)
        {
            localHeroIndex = -1;
            localHeroType = 0;
            return;
        }

        localHeroIndex = B.Instance.heroPlayer;
        localHeroType = (localHeroIndex >= 0) ? (localHeroIndex + 1) : 0;
    }

    private void ApplySkillIconsIfNeeded(bool force)
    {
        if (!autoApplySkillIcons) return;
        if (!force && _lastAppliedHeroType == localHeroType) return;

        string folder = null;
        if (localHeroType == 1) folder = kaynFolder;
        else if (localHeroType == 2) folder = leonaFolder;

        if (string.IsNullOrEmpty(folder))
        {
            _lastAppliedHeroType = localHeroType;
            return;
        }

        ApplyBtnSprite(skill1.btnCast, $"{folder}/1");
        ApplyBtnSprite(skill2.btnCast, $"{folder}/2");
        ApplyBtnSprite(skill3.btnCast, $"{folder}/3");

        _lastAppliedHeroType = localHeroType;
    }

    private void ApplyBtnSprite(Button btn, string resourcesPathNoExt)
    {
        if (btn == null) return;

        Image img = btn.GetComponent<Image>();
        if (img == null) return;

        Sprite sp = Resources.Load<Sprite>(resourcesPathNoExt);
        if (sp == null)
        {
            Debug.LogWarning($"[MenuController] Missing sprite in Resources: {resourcesPathNoExt}.png");
            return;
        }

        img.sprite = sp;
        img.enabled = true;
    }

    private void InitCooldownUI()
    {
        HideCooldown(skill1.imgCooldown, skill1.txtCooldown);
        HideCooldown(skill2.imgCooldown, skill2.txtCooldown);
        HideCooldown(skill3.imgCooldown, skill3.txtCooldown);
    }

    private void HideCooldown(Image img, TMP_Text txt)
    {
        if (img != null)
        {
            img.gameObject.SetActive(false);
            img.fillAmount = 0f;
        }
        if (txt != null)
        {
            txt.gameObject.SetActive(false);
            txt.text = "";
        }
    }

    private void ShowLockedOverlayOnly(Image img, TMP_Text txt)
    {
        if (img != null)
        {
            img.gameObject.SetActive(true);
            img.fillAmount = 1f;
        }
        if (txt != null)
        {
            txt.gameObject.SetActive(false);
            txt.text = "";
        }
    }

    private Color32 GetLockedColor32()
    {
        byte a = (byte)Mathf.Clamp(Mathf.RoundToInt(lockedAlpha * 255f), 0, 255);
        return new Color32(255, 255, 255, a);
    }

    private void InitSkillLevelIcons()
    {
        var locked = GetLockedColor32();
        SetAllIconsLocked(skill1, locked);
        SetAllIconsLocked(skill2, locked);
        SetAllIconsLocked(skill3, locked);
    }

    private void SetAllIconsLocked(SkillUI ui, Color32 locked)
    {
        if (ui == null || ui.levelIcons == null) return;
        for (int i = 0; i < ui.levelIcons.Length; i++)
        {
            var img = ui.levelIcons[i];
            if (img != null) img.color = locked;
        }
    }

    private int GetMaxLevel(SkillUI ui) => (ui?.levelIcons == null) ? 0 : ui.levelIcons.Length;

    private void ApplySkillLevelIcons(SkillUI ui, int level)
    {
        if (ui == null || ui.levelIcons == null) return;
        int max = ui.levelIcons.Length;
        int lv = Mathf.Clamp(level, 0, max);

        var locked = GetLockedColor32();
        for (int i = 0; i < ui.levelIcons.Length; i++)
        {
            var img = ui.levelIcons[i];
            if (img == null) continue;
            img.color = (i < lv) ? COLOR_UNLOCKED : locked;
        }
    }

    private SkillUI GetSkillUI(int skillIndex)
    {
        switch (skillIndex)
        {
            case 1: return skill1;
            case 2: return skill2;
            case 3: return skill3;
            default: return null;
        }
    }

    private bool IsCooldownActive(int skillIndex)
    {
        switch (skillIndex)
        {
            case 1: return (B.Instance != null && B.Instance.isCooldownSkill1);
            case 2: return (B.Instance != null && B.Instance.isCooldownSkill2);
            case 3: return (B.Instance != null && B.Instance.isCooldownSkill3);
            default: return false;
        }
    }

    private int GetDisplayedLevel(int skillIndex)
    {
        switch (skillIndex)
        {
            case 1: return Mathf.Max(_srvS1, _predS1);
            case 2: return Mathf.Max(_srvS2, _predS2);
            case 3: return Mathf.Max(_srvS3, _predS3);
            default: return 0;
        }
    }

    private string GetHeroKey()
    {
        switch (localHeroType)
        {
            case 1: return "Kayn";
            case 2: return "Leona";
            default:
                Debug.LogWarning($"[Audio] Unknown heroType: {localHeroType}");
                return null;
        }
    }

    private bool HasAtLeastOneLevel(int skillIndex) => GetDisplayedLevel(skillIndex) >= 1;

    private void RefreshSkillCastLockOverlays()
    {
        RefreshSkillCastLockOverlay(1);
        RefreshSkillCastLockOverlay(2);
        RefreshSkillCastLockOverlay(3);
    }

    private void RefreshSkillCastLockOverlay(int skillIndex)
    {
        var ui = GetSkillUI(skillIndex);
        if (ui == null) return;

        bool unlocked = HasAtLeastOneLevel(skillIndex);

        if (unlocked && IsCooldownActive(skillIndex))
        {
            if (ui.btnCast != null) ui.btnCast.interactable = false;
            return;
        }

        if (!unlocked)
        {
            if (ui.btnCast != null) ui.btnCast.interactable = false;
            ShowLockedOverlayOnly(ui.imgCooldown, ui.txtCooldown);
        }
        else
        {
            if (ui.btnCast != null) ui.btnCast.interactable = true;
            HideCooldown(ui.imgCooldown, ui.txtCooldown);
        }
    }

    public void ApplyServerCooldown(int skillId, float timeCdSeconds)
    {
        if (timeCdSeconds <= 0f) return;
        StartCooldown(skillId, timeCdSeconds);
    }

    // Helper gọi tracker báo cast
    private void NotifyCastFired(Button btnCast)
    {
        if (btnCast == null) return;
        var go = (btnCast.targetGraphic != null) ? btnCast.targetGraphic.gameObject : btnCast.gameObject;
        var tr = go.GetComponent<SkillButtonHoldTracker>();
        tr?.OnCastFired();
    }

    public void TungChieu1()
    {
        if (!HasAtLeastOneLevel(1)) { RefreshSkillCastLockOverlay(1); return; }
        var pm = TranDauControl.Instance?.playerMove;
        if (pm == null) return;

        int autoFlag = GetAutoFlagFromBtn(skill1.btnCast);
        NotifyCastFired(skill1.btnCast);          // ★ báo tracker

        if (!pm.TryCastSkill(1, autoFlag)) return;

        AudioManager.Instance.PlayHeroSound(GetHeroKey(), AudioManager.HeroSoundType.Skill);
        AudioManager.Instance.PlaySkillSound(GetHeroKey());
        SendData.SendAttack(0, 3, transform.position, 1);
        StartCooldown(1, fallbackCooldownTime);
    }

    public void TungChieu2()
    {
        if (!HasAtLeastOneLevel(2)) { RefreshSkillCastLockOverlay(2); return; }
        var pm = TranDauControl.Instance?.playerMove;
        if (pm == null) return;

        int autoFlag = GetAutoFlagFromBtn(skill2.btnCast);
        NotifyCastFired(skill2.btnCast);          // ★

        if (!pm.TryCastSkill(2, autoFlag)) return;

        AudioManager.Instance.PlayHeroSound(GetHeroKey(), AudioManager.HeroSoundType.Skill);
        AudioManager.Instance.PlaySkillSound(GetHeroKey());
        SendData.SendAttack(0, 3, transform.position, 2);
        StartCooldown(2, fallbackCooldownTime);
    }

    public void TungChieu3()
    {
        if (!HasAtLeastOneLevel(3)) { RefreshSkillCastLockOverlay(3); return; }
        var pm = TranDauControl.Instance?.playerMove;
        if (pm == null) return;

        int autoFlag = GetAutoFlagFromBtn(skill3.btnCast);
        NotifyCastFired(skill3.btnCast);          // ★

        if (!pm.TryCastSkill(3, autoFlag)) return;

        AudioManager.Instance.PlayHeroSound(GetHeroKey(), AudioManager.HeroSoundType.Skill);
        AudioManager.Instance.PlaySkillSound(GetHeroKey());
        SendData.SendAttack(0, 3, transform.position, 3);
        StartCooldown(3, fallbackCooldownTime);
    }

    private void StartCooldown(int skill, float durationSeconds)
    {
        durationSeconds = Mathf.Max(0.1f, durationSeconds);

        switch (skill)
        {
            case 1:
                if (_cd1 != null) StopCoroutine(_cd1);
                _cd1 = StartCoroutine(CooldownCoroutine(1, skill1.imgCooldown, skill1.txtCooldown, skill1.btnCast, durationSeconds));
                if (B.Instance != null) B.Instance.isCooldownSkill1 = true;
                break;

            case 2:
                if (_cd2 != null) StopCoroutine(_cd2);
                _cd2 = StartCoroutine(CooldownCoroutine(2, skill2.imgCooldown, skill2.txtCooldown, skill2.btnCast, durationSeconds));
                if (B.Instance != null) B.Instance.isCooldownSkill2 = true;
                break;

            case 3:
                if (_cd3 != null) StopCoroutine(_cd3);
                _cd3 = StartCoroutine(CooldownCoroutine(3, skill3.imgCooldown, skill3.txtCooldown, skill3.btnCast, durationSeconds));
                if (B.Instance != null) B.Instance.isCooldownSkill3 = true;
                break;
        }
    }

    private IEnumerator CooldownCoroutine(int skill, Image img, TMP_Text txt, Button btn, float duration)
    {
        if (btn != null) btn.interactable = false;

        if (img != null)
        {
            img.gameObject.SetActive(true);
            img.fillAmount = 1f;
        }

        if (txt != null)
        {
            txt.gameObject.SetActive(true);
            txt.text = "";
        }

        float timer = duration;

        while (timer > 0f)
        {
            timer -= Time.deltaTime;

            if (img != null)
                img.fillAmount = Mathf.Clamp01(timer / duration);

            if (txt != null)
            {
                float v = Mathf.Max(0f, Mathf.Ceil(timer * 2f) / 2f);
                txt.text = v.ToString("0.0");
            }

            yield return null;
        }

        if (img != null)
        {
            img.gameObject.SetActive(false);
            img.fillAmount = 0f;
        }

        if (txt != null)
        {
            txt.gameObject.SetActive(false);
            txt.text = "";
        }

        switch (skill)
        {
            case 1: if (B.Instance != null) B.Instance.isCooldownSkill1 = false; break;
            case 2: if (B.Instance != null) B.Instance.isCooldownSkill2 = false; break;
            case 3: if (B.Instance != null) B.Instance.isCooldownSkill3 = false; break;
        }

        RefreshSkillCastLockOverlay(skill);
    }

    private void SetPlusButtonsVisible(bool s1, bool s2, bool s3)
    {
        if (skill1.btnPlusLevel != null) skill1.btnPlusLevel.gameObject.SetActive(s1);
        if (skill2.btnPlusLevel != null) skill2.btnPlusLevel.gameObject.SetActive(s2);
        if (skill3.btnPlusLevel != null) skill3.btnPlusLevel.gameObject.SetActive(s3);
    }

    private int GetTotalPointsByLevel(int level) => Mathf.Max(0, level);
    private int GetRemainingPoints(int level) => Mathf.Max(0, GetTotalPointsByLevel(level) - _spentPoints);

    private void RefreshPlusButtonsByRules(int playerLevel)
    {
        int remaining = GetRemainingPoints(playerLevel);
        if (remaining <= 0)
        {
            SetPlusButtonsVisible(false, false, false);
            return;
        }

        int s1 = GetDisplayedLevel(1);
        int s2 = GetDisplayedLevel(2);
        int s3 = GetDisplayedLevel(3);

        bool canShowS1 = s1 < GetMaxLevel(skill1);
        bool canShowS2 = s2 < GetMaxLevel(skill2);
        bool canShowS3 = (playerLevel >= 4) && (s3 < GetMaxLevel(skill3));

        SetPlusButtonsVisible(canShowS1, canShowS2, canShowS3);
    }

    private void OnClickPlus(int skillIndex)
    {
        int playerLevel = _lastLevel == int.MinValue ? 0 : _lastLevel;

        if (GetRemainingPoints(playerLevel) <= 0) return;
        if (skillIndex == 3 && playerLevel < 4) return;

        int max = (skillIndex == 1) ? GetMaxLevel(skill1) :
                  (skillIndex == 2) ? GetMaxLevel(skill2) :
                  GetMaxLevel(skill3);

        int currentShown = GetDisplayedLevel(skillIndex);
        if (currentShown >= max) return;

        switch (skillIndex)
        {
            case 1:
                _predS1 = Mathf.Clamp(currentShown + 1, 0, max);
                _pendingS1 = true;
                ApplySkillLevelIcons(skill1, _predS1);
                break;

            case 2:
                _predS2 = Mathf.Clamp(currentShown + 1, 0, max);
                _pendingS2 = true;
                ApplySkillLevelIcons(skill2, _predS2);
                break;

            case 3:
                _predS3 = Mathf.Clamp(currentShown + 1, 0, max);
                _pendingS3 = true;
                ApplySkillLevelIcons(skill3, _predS3);
                break;
        }

        _spentPoints++;
        RefreshPlusButtonsByRules(playerLevel);

        RefreshSkillCastLockOverlay(skillIndex);

        SendData.SendSkillUpgrade(skillIndex);
    }

    private void ClearPendingIfServerCaughtUp()
    {
        if (_pendingS1 && _srvS1 >= _predS1) _pendingS1 = false;
        if (_pendingS2 && _srvS2 >= _predS2) _pendingS2 = false;
        if (_pendingS3 && _srvS3 >= _predS3) _pendingS3 = false;

        if (!_pendingS1) _predS1 = _srvS1;
        if (!_pendingS2) _predS2 = _srvS2;
        if (!_pendingS3) _predS3 = _srvS3;
    }

    private void OnAnyPlayerUpdated(PlayerResourceData data, bool isLocal)
    {
        if (_localUserId == 0 && UserData.Instance != null)
            _localUserId = UserData.Instance.UserID;

        if (forceUserId != 0)
        {
            if (data.userId != forceUserId) return;
            ApplyToHUD(data);
            return;
        }

        bool reallyLocal = isLocal || (_localUserId != 0 && data.userId == _localUserId);
        if (!reallyLocal) return;

        ApplyToHUD(data);
    }

    private void ApplyToHUD(PlayerResourceData data)
    {
        if (data != null && data.level != _lastLevel)
            _lastLevel = data.level;

        _srvS1 = Mathf.Clamp(data.skill1Level, 0, GetMaxLevel(skill1));
        _srvS2 = Mathf.Clamp(data.skill2Level, 0, GetMaxLevel(skill2));
        _srvS3 = Mathf.Clamp(data.skill3Level, 0, GetMaxLevel(skill3));

        if (!_spentInitFromServer)
        {
            int serverSpent = Mathf.Clamp(_srvS1 + _srvS2 + _srvS3, 0, 3);
            _spentPoints = Mathf.Max(_spentPoints, serverSpent);
            _spentInitFromServer = true;
        }

        if (!_pendingS1) _predS1 = Mathf.Max(_predS1, _srvS1);
        if (!_pendingS2) _predS2 = Mathf.Max(_predS2, _srvS2);
        if (!_pendingS3) _predS3 = Mathf.Max(_predS3, _srvS3);

        ClearPendingIfServerCaughtUp();

        int showS1 = GetDisplayedLevel(1);
        int showS2 = GetDisplayedLevel(2);
        int showS3 = GetDisplayedLevel(3);

        if (txtGold != null) txtGold.text = data.gold.ToString();

        if (txtK != null) txtK.text = data.kills.ToString();
        if (txtD != null) txtD.text = data.deaths.ToString();
        if (txtA != null) txtA.text = data.assists.ToString();

        ApplySkillLevelIcons(skill1, showS1);
        ApplySkillLevelIcons(skill2, showS2);
        ApplySkillLevelIcons(skill3, showS3);

        RefreshPlusButtonsByRules(_lastLevel);
        RefreshSkillCastLockOverlays();

        SyncLocalHeroFromCmd43();
        ApplySkillIconsIfNeeded(false);
    }

    private void OnDestroy()
    {
        if (_cd1 != null) StopCoroutine(_cd1);
        if (_cd2 != null) StopCoroutine(_cd2);
        if (_cd3 != null) StopCoroutine(_cd3);

        _chatTween?.Kill();
        _chatTween = null;

        if (_chatAutoHideCo != null) StopCoroutine(_chatAutoHideCo);
        _chatAutoHideCo = null;

        if (_focusChatCo != null) StopCoroutine(_focusChatCo);
        _focusChatCo = null;
    }

    private void UpdateGameTimeUI()
    {
        if (txtTime == null) return;

        float current = GameTimerManager.Instance != null ? GameTimerManager.Instance.GetTimeElapsed() : 0f;
        int minutes = Mathf.FloorToInt(current / 60f);
        int seconds = Mathf.FloorToInt(current % 60f);
        txtTime.text = $"{minutes:00}:{seconds:00}";
    }

    private void UpdateScoreUI()
    {
        if (ScoreManager.Instance == null) return;

        int scoreBlue = ScoreManager.Instance.GetScoreByTeam(1);
        int scoreRed = ScoreManager.Instance.GetScoreByTeam(2);

        if (tongTySoBenXanh != null)
            tongTySoBenXanh.text = scoreBlue.ToString();

        if (tongTySoBenDo != null)
            tongTySoBenDo.text = scoreRed.ToString();
    }

    private void ResetHudState()
    {
        _lastLevel = int.MinValue;

        _spentPoints = 0;
        _spentInitFromServer = false;

        _srvS1 = _srvS2 = _srvS3 = 0;

        _predS1 = _predS2 = _predS3 = 0;
        _pendingS1 = _pendingS2 = _pendingS3 = false;

        if (_cd1 != null) { StopCoroutine(_cd1); _cd1 = null; }
        if (_cd2 != null) { StopCoroutine(_cd2); _cd2 = null; }
        if (_cd3 != null) { StopCoroutine(_cd3); _cd3 = null; }

        if (B.Instance != null)
        {
            B.Instance.isCooldownSkill1 = false;
            B.Instance.isCooldownSkill2 = false;
            B.Instance.isCooldownSkill3 = false;
        }

        SetPlusButtonsVisible(false, false, false);

        if (txtGold != null) txtGold.text = "0";
        if (txtK != null) txtK.text = "0";
        if (txtD != null) txtD.text = "0";
        if (txtA != null) txtA.text = "0";
    }

    private void ShowItemShop()
    {
        if (CanvasController.Instance != null)
            CanvasController.Instance.ShowCanvas("CanvasShopItem");
        SendData.GetItemInfo();
    }

    private void OnClickSendChat(int chatType)
    {
        if (inputChatInMatch == null) return;

        string text = inputChatInMatch.text.Trim();
        if (string.IsNullOrEmpty(text))
            return;

        AudioManager.Instance.AudioClick();

        SendData.SendChatInMatch(chatType, text);

        inputChatInMatch.text = string.Empty;

        inputChatInMatch.Select();
        inputChatInMatch.ActivateInputField();
    }

    public void AddChatMessage(long userId,
                       string displayName,
                       int chatType,
                       string content,
                       long timestamp)
    {
        if (chatItemPrefab == null || chatContent == null)
        {
            Debug.LogWarning("[MenuController] Chat UI chưa được gán prefab hoặc content.");
            return;
        }

        var item = Instantiate(chatItemPrefab, chatContent);
        var comp = item.GetComponent<ChatInMatchItem>();
        if (comp != null)
            comp.Setup(displayName, content, timestamp, chatType);

        if (chatScrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            chatScrollRect.verticalNormalizedPosition = 0f;
        }

        if (autoHideChatMessages)
        {
            ShowChatMessagesNow();

            if (_chatAutoHideCo != null) StopCoroutine(_chatAutoHideCo);
            _chatAutoHideCo = StartCoroutine(CoAutoHideChatMessages());
        }
    }

    private CanvasGroup EnsureChatCanvasGroup()
    {
        if (chatCanvasGroup != null) return chatCanvasGroup;

        if (chatScrollRect != null)
            chatCanvasGroup = chatScrollRect.GetComponent<CanvasGroup>();

        if (chatCanvasGroup == null && chatScrollRect != null)
            chatCanvasGroup = chatScrollRect.gameObject.AddComponent<CanvasGroup>();

        return chatCanvasGroup;
    }

    private void ShowChatMessagesNow()
    {
        var cg = EnsureChatCanvasGroup();
        if (cg == null) return;

        cg.alpha = 1f;
        cg.blocksRaycasts = false;
        cg.interactable = false;
    }

    private IEnumerator CoAutoHideChatMessages()
    {
        yield return new WaitForSecondsRealtime(chatVisibleSeconds);

        var cg = EnsureChatCanvasGroup();
        if (cg == null) yield break;

        float t = 0f;
        float from = cg.alpha;
        float dur = Mathf.Max(0.01f, chatFadeDuration);

        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(from, 0f, t / dur);
            yield return null;
        }

        cg.alpha = 0f;
        cg.blocksRaycasts = false;
        cg.interactable = false;

        _chatAutoHideCo = null;
    }

    private SkillButtonHoldTracker EnsureHoldTracker(Button btn)
    {
        if (btn == null) return null;

        // Gắn lên đúng object nhận raycast (thường là Image targetGraphic)
        GameObject go = (btn.targetGraphic != null) ? btn.targetGraphic.gameObject : btn.gameObject;

        var tr = go.GetComponent<SkillButtonHoldTracker>();
        if (tr == null) tr = go.AddComponent<SkillButtonHoldTracker>();

        tr.threshold = 0.5f;
        tr.previewDelay = 0.5f; // đồng bộ yêu cầu

        return tr;
    }

    private int GetAutoFlagFromBtn(Button btnCast)
    {
        if (btnCast == null) return 0;

        // Lấy tracker từ đúng object
        var go = (btnCast.targetGraphic != null) ? btnCast.targetGraphic.gameObject : btnCast.gameObject;
        var tr = go.GetComponent<SkillButtonHoldTracker>();

        float dur = (tr != null) ? tr.GetHoldDurationNow() : -1f;
        bool quick = (tr != null) && tr.IsQuickTapNow();

#if UNITY_EDITOR
        Debug.Log($"[CAST TAP] dur={dur:0.000}s quick={quick}");
#endif

        return quick ? 1 : 0;
    }

    public void ShowAimCanvasForSkill(int skillIndex)
    {
        var pm = TranDauControl.Instance != null ? TranDauControl.Instance.playerMove : null;
        if (pm == null) return;

        pm.DisableAllAimCanvases();

        int type = pm.GetSkillTypeForSkillId_Public(skillIndex);
        var go = pm.GetAimCanvasByType(type);
        if (go != null) go.SetActive(true);
    }

    public void HideAllAimCanvases()
    {
        var pm = TranDauControl.Instance != null ? TranDauControl.Instance.playerMove : null;
        if (pm == null) return;

        pm.DisableAllAimCanvases();
    }
}
