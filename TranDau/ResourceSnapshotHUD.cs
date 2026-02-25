using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResourceSnapshotHUD : ScaleScreen
{
    public static ResourceSnapshotHUD Instance;

    // =========================
    // References: Gameplay
    // =========================
    [Header("Refs")]
    public JoystickController joystick;

    // =========================
    // Buttons: Attack
    // =========================
    [Header("Buttons - Attack")]
    public Button btnDanhThuong;

    // =========================
    // Skill UI Groups
    // =========================
    [Serializable]
    public class SkillUI
    {
        public Button btnCast;
        public Image imgCooldown;
        public TMP_Text txtCooldown;
        public Button btnPlusLevel;

        [Tooltip("Kéo thả theo thứ tự: level 1 -> level N (N = size của mảng).")]
        public Image[] levelIcons;
    }

    [Header("Skill 1")]
    public SkillUI skill1 = new SkillUI();

    [Header("Skill 2")]
    public SkillUI skill2 = new SkillUI();

    [Header("Skill 3")]
    public SkillUI skill3 = new SkillUI();

    // =========================
    // Local Player HUD (ResourceSnapshotHUD)
    // =========================
    [Header("HUD - Local Player")]
    public TMP_Text txtGold;
    public TMP_Text txtSkills;
    public TMP_Text txtShield;

    [Header("HUD - K/D/A")]
    public TMP_Text txtK;
    public TMP_Text txtD;
    public TMP_Text txtA;

    // =========================
    // Debug
    // =========================
    [Header("Debug")]
    public bool debugHud = true;
    public long forceUserId = 0;

    // =========================
    // Cooldown (Shared)
    // =========================
    [Header("Cooldown - Shared")]
    [SerializeField] private float cooldownTime = 3f;

    private Coroutine _cd1;
    private Coroutine _cd2;
    private Coroutine _cd3;

    // =========================
    // HUD State
    // =========================
    private long _localUserId;

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        _localUserId = UserData.Instance != null ? UserData.Instance.UserID : 0;
        UdpResourceSnapshotSystem.OnPlayerResourceUpdated += OnAnyPlayerUpdated;
    }

    private void OnDisable()
    {
        UdpResourceSnapshotSystem.OnPlayerResourceUpdated -= OnAnyPlayerUpdated;
    }

    private void Start()
    {
        InitCooldownUI();

        // Bind: Skills
        if (skill1.btnCast != null) skill1.btnCast.onClick.AddListener(TungChieu1);
        if (skill2.btnCast != null) skill2.btnCast.onClick.AddListener(TungChieu2);
        if (skill3.btnCast != null) skill3.btnCast.onClick.AddListener(TungChieu3);

        // Bind: Normal attack
        if (btnDanhThuong != null)
        {
            btnDanhThuong.onClick.AddListener(() =>
            {
                TranDauControl.Instance.playerMove.NormalAttack();
            });
        }
    }

    // =========================
    // Init
    // =========================
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

    // =========================
    // (Optional) Level icon helpers
    // =========================
    // Gọi hàm này nếu bạn muốn UI level hiển thị đúng theo level server.
    // Ví dụ: UpdateSkillLevelIcons(skill1, data.skill1Level);
    private void UpdateSkillLevelIcons(SkillUI ui, int level)
    {
        if (ui == null || ui.levelIcons == null) return;

        // level = 0 => tắt hết, level = 1 => bật icon[0], ...
        for (int i = 0; i < ui.levelIcons.Length; i++)
        {
            var img = ui.levelIcons[i];
            if (img != null) img.gameObject.SetActive(i < level);
        }
    }

    // =========================
    // Skill Actions
    // =========================
    public void TungChieu1()
    {
        TranDauControl.Instance.playerMove.CastSkill(1);
        if (skill1.btnCast != null) skill1.btnCast.interactable = false;

        SendData.SendAttack(0, 3, transform.position, 1);
        StartCooldown(1);
    }

    public void TungChieu2()
    {
        TranDauControl.Instance.playerMove.CastSkill(2);
        if (skill2.btnCast != null) skill2.btnCast.interactable = false;

        SendData.SendAttack(0, 3, transform.position, 2);
        StartCooldown(2);
    }

    public void TungChieu3()
    {
        TranDauControl.Instance.playerMove.CastSkill(3);
        if (skill3.btnCast != null) skill3.btnCast.interactable = false;

        SendData.SendAttack(0, 3, transform.position, 3);
        StartCooldown(3);
    }

    // =========================
    // Cooldown (Shared)
    // =========================
    private void StartCooldown(int skill)
    {
        switch (skill)
        {
            case 1:
                if (_cd1 != null) StopCoroutine(_cd1);
                _cd1 = StartCoroutine(CooldownCoroutine(1, skill1.imgCooldown, skill1.txtCooldown, skill1.btnCast));
                B.Instance.isCooldownSkill1 = true;
                break;

            case 2:
                if (_cd2 != null) StopCoroutine(_cd2);
                _cd2 = StartCoroutine(CooldownCoroutine(2, skill2.imgCooldown, skill2.txtCooldown, skill2.btnCast));
                B.Instance.isCooldownSkill2 = true;
                break;

            case 3:
                if (_cd3 != null) StopCoroutine(_cd3);
                _cd3 = StartCoroutine(CooldownCoroutine(3, skill3.imgCooldown, skill3.txtCooldown, skill3.btnCast));
                B.Instance.isCooldownSkill3 = true;
                break;
        }
    }

    private IEnumerator CooldownCoroutine(int skill, Image img, TMP_Text txt, Button btn)
    {
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

        float timer = cooldownTime;

        while (timer > 0f)
        {
            timer -= Time.deltaTime;

            if (txt != null)
            {
                int secondsLeft = Mathf.CeilToInt(timer);
                txt.text = secondsLeft.ToString();
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

        if (btn != null) btn.interactable = true;

        switch (skill)
        {
            case 1: B.Instance.isCooldownSkill1 = false; break;
            case 2: B.Instance.isCooldownSkill2 = false; break;
            case 3: B.Instance.isCooldownSkill3 = false; break;
        }
    }

    // =========================
    // HUD (ResourceSnapshotHUD)
    // =========================
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
        if (txtGold != null) txtGold.text = data.gold.ToString();
        if (txtSkills != null) txtSkills.text = $"{data.skill1Level}/{data.skill2Level}/{data.skill3Level}";
        if (txtShield != null) txtShield.text = data.shield.ToString();

        if (txtK != null) txtK.text = data.kills.ToString();
        if (txtD != null) txtD.text = data.deaths.ToString();
        if (txtA != null) txtA.text = data.assists.ToString();

        // Nếu muốn show level icon theo level:
        // UpdateSkillLevelIcons(skill1, data.skill1Level);
        // UpdateSkillLevelIcons(skill2, data.skill2Level);
        // UpdateSkillLevelIcons(skill3, data.skill3Level);
    }

    // =========================
    // Cleanup
    // =========================
    private void OnDestroy()
    {
        if (_cd1 != null) StopCoroutine(_cd1);
        if (_cd2 != null) StopCoroutine(_cd2);
        if (_cd3 != null) StopCoroutine(_cd3);

        UdpResourceSnapshotSystem.OnPlayerResourceUpdated -= OnAnyPlayerUpdated;
    }
}
