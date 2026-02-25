using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerResourceSnapshotView : MonoBehaviour
{
    [Header("HP/Mana (Fill)")]
    public Image imgHpFill;
    public Image imgManaFill;

    [Header("HP/Mana (Optional Text)")]
    public TMP_Text txtHpValue;

    [Header("Optional UI Bindings")]
    public TMP_Text txtLevel;
    public TMP_Text txtExp;

    [Header("Options")]
    public bool onlyShowForLocalPlayer;

    [Header("Smooth Fill Settings")]
    [Tooltip("Tốc độ smooth HP/Mana bar (cao hơn = nhanh hơn)")]
    public float fillSmoothSpeed = 5f;

    // Target để lerp
    private float _targetHpRatio;
    private float _targetManaRatio;
    private float _currentHpRatio;
    private float _currentManaRatio;

    public void Apply(PlayerResourceData data, bool isLocal)
    {
        if (onlyShowForLocalPlayer && !isLocal) return;

        // Tính ratio target
        _targetHpRatio = data.maxHp <= 0 ? 0f : Mathf.Clamp01((float)data.hp / data.maxHp);
        _targetManaRatio = data.maxMana <= 0 ? 0f : Mathf.Clamp01((float)data.mana / data.maxMana);

        // Text update ngay (không smooth)
        if (txtHpValue != null) txtHpValue.text = $"{data.hp}/{data.maxHp}";

        if (txtLevel != null) txtLevel.text = data.level.ToString();

        int curExp = data.GetCurrentExp();
        if (txtExp != null) txtExp.text = $"{curExp}/{data.expToNextLevel}";
    }

    private void OnEnable()
    {
        ResetView();
    }

    private void Start()
    {
        // Khởi tạo giá trị ban đầu
        if (imgHpFill != null)
        {
            _currentHpRatio = imgHpFill.fillAmount;
            _targetHpRatio = _currentHpRatio;
        }

        if (imgManaFill != null)
        {
            _currentManaRatio = imgManaFill.fillAmount;
            _targetManaRatio = _currentManaRatio;
        }
    }

    private void Update()
    {
        if (imgHpFill != null)
        {
            _currentHpRatio = Mathf.Lerp(_currentHpRatio, _targetHpRatio, Time.deltaTime * fillSmoothSpeed);
            imgHpFill.fillAmount = _currentHpRatio;
        }

        if (imgManaFill != null)
        {
            _currentManaRatio = Mathf.Lerp(_currentManaRatio, _targetManaRatio, Time.deltaTime * fillSmoothSpeed);
            imgManaFill.fillAmount = _currentManaRatio;
        }
    }

    public void ResetView()
    {
        _targetHpRatio = _currentHpRatio = 1f;
        _targetManaRatio = _currentManaRatio = 1f;

        if (imgHpFill != null) imgHpFill.fillAmount = 1f;
        if (imgManaFill != null) imgManaFill.fillAmount = 1f;

        if (txtHpValue != null) txtHpValue.text = "";
        if (txtLevel != null) txtLevel.text = "1";
        if (txtExp != null) txtExp.text = "0/0";
    }
}
