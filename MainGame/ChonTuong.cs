using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChonTuong : ScaleScreen
{
    public TextMeshProUGUI txtTenPlayer, txtTenTuong, txtTenTuong2, txtTrangThai;
    public Image imgTuongChon;
    public Button btnChon;

    public Toggle tg1, tg2, tg3, tg4, tg5, tg6;

    public GameObject[] tuong;
    public GameObject objChieuThuc, huBtn;

    public Sprite[] sprAvtTuong;

    private int heroType;

    public HeroShowcase2D showcase2D;
    public HeroShowcaseProfile[] showcaseProfiles;

    [Header("BG Color Control")]
    public Image bgImage;
    public float[] bgReturnDelays;
    public float bgFadeDuration = 0.3f;

    Coroutine _bgCo;

    static readonly Color DARK_COLOR = new Color32(34, 34, 34, 255);
    static readonly Color NORMAL_COLOR = new Color32(255, 255, 255, 255);

    protected override void Start()
    {
        base.Start();

        if (tg1) tg1.onValueChanged.AddListener(isOn => { if (isOn) OnToggleSelected(1); });
        if (tg2) tg2.onValueChanged.AddListener(isOn => { if (isOn) OnToggleSelected(2); });
        if (tg3) tg3.onValueChanged.AddListener(isOn => { if (isOn) OnToggleSelected(3); });
        if (tg4) tg4.onValueChanged.AddListener(isOn => { if (isOn) OnToggleSelected(4); });
        if (tg5) tg5.onValueChanged.AddListener(isOn => { if (isOn) OnToggleSelected(5); });
        if (tg6) tg6.onValueChanged.AddListener(isOn => { if (isOn) OnToggleSelected(6); });

        if (btnChon) btnChon.onClick.AddListener(KhoaTuong);
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        if (DialogController.Instance != null && DialogController.Instance.PopupTimTran != null)
            DialogController.Instance.PopupTimTran.Show(false);

        SetData();
    }

    public void Show(bool val = true)
    {
        gameObject.SetActive(val);
    }

    public void SetData()
    {
        if (txtTenPlayer)
            txtTenPlayer.text = UserData.Instance.UserName;

        ResetToggles();

        heroType = 1;

        if (tg1)
            tg1.SetIsOnWithoutNotify(true);

        SelectHero(heroType);
        StatusBtnChon(true);
    }

    private void ResetToggles()
    {
        if (tg1) tg1.SetIsOnWithoutNotify(false);
        if (tg2) tg2.SetIsOnWithoutNotify(false);
        if (tg3) tg3.SetIsOnWithoutNotify(false);
        if (tg4) tg4.SetIsOnWithoutNotify(false);
        if (tg5) tg5.SetIsOnWithoutNotify(false);
        if (tg6) tg6.SetIsOnWithoutNotify(false);
    }

    private void OnToggleSelected(int selectedHeroType)
    {
        AudioManager.Instance.AudioClick();
        heroType = selectedHeroType;
        SelectHero(heroType);
    }

    private void SelectHero(int selectedHeroType)
    {
        if (objChieuThuc)
            objChieuThuc.SetActive(true);

        int heroIndex = selectedHeroType - 1;

        if (tuong != null)
        {
            for (int i = 0; i < tuong.Length; i++)
            {
                if (tuong[i])
                    tuong[i].SetActive(i == heroIndex);
            }
        }

        if (showcase2D != null &&
            showcaseProfiles != null &&
            heroIndex >= 0 &&
            heroIndex < showcaseProfiles.Length)
        {
            showcase2D.PlayFor(
                tuong[heroIndex].transform,
                showcaseProfiles[heroIndex]
            );
        }

        HandleBgColor(heroIndex);

        if (imgTuongChon && sprAvtTuong != null &&
            heroIndex >= 0 && heroIndex < sprAvtTuong.Length)
        {
            imgTuongChon.sprite = sprAvtTuong[heroIndex];
        }

        string heroName = B.Instance.GetNameTuong(selectedHeroType);
        if (txtTenTuong) txtTenTuong.text = heroName;
        if (txtTenTuong2) txtTenTuong2.text = heroName;
    }

    void HandleBgColor(int heroIndex)
    {
        if (bgImage == null)
            return;

        if (_bgCo != null)
            StopCoroutine(_bgCo);

        bgImage.color = DARK_COLOR;

        float delay = 8.6f;

        if (bgReturnDelays != null &&
            heroIndex >= 0 &&
            heroIndex < bgReturnDelays.Length)
        {
            delay = bgReturnDelays[heroIndex];
        }

        _bgCo = StartCoroutine(CoReturnBg(delay));
    }

    IEnumerator CoReturnBg(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);

        if (bgImage == null)
            yield break;

        Color start = bgImage.color;
        float t = 0f;

        while (t < bgFadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float k = t / bgFadeDuration;
            bgImage.color = Color.Lerp(start, NORMAL_COLOR, k);
            yield return null;
        }

        bgImage.color = NORMAL_COLOR;
    }

    public void StatusBtnChon(bool val)
    {
        if (btnChon) btnChon.interactable = val;
        if (huBtn) huBtn.SetActive(val);
    }

    private void KhoaTuong()
    {
        AudioManager.Instance.AudioClick();

        if (heroType > 0)
        {
            SendData.SelectHero(heroType);
        }
        else
        {
            ThongBaoController.Instance.Toast.ShowToast("Bạn chưa chọn tướng.");
        }
    }
}