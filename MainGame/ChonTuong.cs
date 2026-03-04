using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChonTuong : ScaleScreen
{
    public TextMeshProUGUI txtTenPlayer, txtTenTuong, txtTenTuong2, txtTrangThai, txtTimeDemNguoc;
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

    static readonly Color DARK_COLOR = new Color32(40, 40, 40, 255);
    static readonly Color NORMAL_COLOR = new Color32(255, 255, 255, 255);

    private bool _daDemNguoc;

    public Transform canvasShowHieuUngUI;

    [System.Serializable]
    public class HieuUngConfig
    {
        public GameObject prefab;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale = Vector3.one;
        public string layer = "camLayer3D";
        public float delayShow = 0f;
        public float destroyAfter = 0f;
    }

    [System.Serializable]
    public class HeroHieuUngGroup
    {
        public HieuUngConfig[] configs;
    }

    [Header("Hieu Ung")]
    public HeroHieuUngGroup[] heroHieuUngs;
    public Transform hieuUngParent;

    private List<GameObject> _currentHieuUngs = new List<GameObject>();
    private List<Coroutine> _hieuUngCos = new List<Coroutine>();

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

        ThongBaoController.Instance.PrewarmLoadVaoTran();
        SetData();
    }

    private void OnDisable()
    {
        if (canvasShowHieuUngUI != null)
            canvasShowHieuUngUI.gameObject.SetActive(false);

        foreach (var co in _hieuUngCos)
        {
            if (co != null) StopCoroutine(co);
        }
        _hieuUngCos.Clear();

        foreach (var go in _currentHieuUngs)
        {
            if (go != null) Destroy(go);
        }
        _currentHieuUngs.Clear();

        Debug.Log($"[ChonTuong] OnDisable called! _daDemNguoc={_daDemNguoc}, stackTrace={System.Environment.StackTrace}");
    }

    public void Show(bool val = true)
    {
        gameObject.SetActive(val);
    }

    public void SetData()
    {
        _daDemNguoc = false;

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
        // Nếu đang chọn đúng hero hiện tại thì bỏ qua
        if (heroType == selectedHeroType)
            return;

        AudioManager.Instance.AudioClick();
        heroType = selectedHeroType;
        SelectHero(heroType);
    }

    private void SelectHero(int selectedHeroType)
    {
        if (objChieuThuc)
            objChieuThuc.SetActive(true);

        int heroIndex = selectedHeroType - 1;

        if (tuong == null || tuong.Length == 0) return;
        if (heroIndex < 0 || heroIndex >= tuong.Length) return;

        for (int i = 0; i < tuong.Length; i++)
        {
            if (tuong[i])
                tuong[i].SetActive(i == heroIndex);
        }

        if (tuong[heroIndex] != null && showcase2D != null)
        {
            HeroShowcaseProfile profile = null;

            if (showcaseProfiles != null &&
                heroIndex >= 0 &&
                heroIndex < showcaseProfiles.Length)
            {
                profile = showcaseProfiles[heroIndex];
            }

            if (profile != null)
            {
                showcase2D.PlayFor(tuong[heroIndex].transform, profile);
            }
            else
            {
                showcase2D.ForceIdle(tuong[heroIndex].transform, true);
            }
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

        SpawnHieuUng(heroIndex);
    }

    private void SpawnHieuUng(int heroIndex)
    {
        // Hủy coroutine cũ
        foreach (var co in _hieuUngCos)
        {
            if (co != null) StopCoroutine(co);
        }
        _hieuUngCos.Clear();

        // Hủy hiệu ứng cũ
        foreach (var go in _currentHieuUngs)
        {
            if (go != null) Destroy(go);
        }
        _currentHieuUngs.Clear();

        if (heroHieuUngs == null || heroIndex < 0 || heroIndex >= heroHieuUngs.Length)
            return;

        HeroHieuUngGroup group = heroHieuUngs[heroIndex];
        if (group == null || group.configs == null)
            return;

        foreach (var config in group.configs)
        {
            if (config == null || config.prefab == null)
                continue;

            var co = StartCoroutine(CoSpawnHieuUng(config));
            _hieuUngCos.Add(co);
        }
    }

    private IEnumerator CoSpawnHieuUng(HieuUngConfig config)
    {
        if (config.delayShow > 0f)
            yield return new WaitForSeconds(config.delayShow);

        if (!gameObject.activeInHierarchy)
            yield break;

        if (canvasShowHieuUngUI != null)
            canvasShowHieuUngUI.gameObject.SetActive(true);

        GameObject go = Instantiate(config.prefab, hieuUngParent);
        go.transform.localPosition = config.position;
        go.transform.localEulerAngles = config.rotation;
        go.transform.localScale = config.scale;

        if (!string.IsNullOrEmpty(config.layer))
        {
            int layerId = LayerMask.NameToLayer(config.layer);
            if (layerId >= 0)
                SetLayerRecursive(go, layerId);
            else
                Debug.LogWarning($"[ChonTuong] Layer '{config.layer}' không tồn tại!");
        }

        _currentHieuUngs.Add(go);

        if (config.destroyAfter > 0f)
        {
            yield return new WaitForSeconds(config.destroyAfter);
            if (go != null)
            {
                _currentHieuUngs.Remove(go);
                Destroy(go);
            }

            // Tắt canvas nếu không còn hiệu ứng nào
            if (_currentHieuUngs.Count == 0 && canvasShowHieuUngUI != null)
                canvasShowHieuUngUI.gameObject.SetActive(false);
        }
    }

    private void SetLayerRecursive(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform child in go.transform)
            SetLayerRecursive(child.gameObject, layer);
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

        if (heroType <= 0)
        {
            ThongBaoController.Instance.Toast.ShowToast("Bạn chưa chọn tướng.");
            return;
        }

        if (_daDemNguoc) return;
        _daDemNguoc = true;

        StatusBtnChon(false);
        StartCoroutine(CoDemNguocRoiVaoTran());
    }

    private IEnumerator CoDemNguocRoiVaoTran()
    {
        for (int i = 5; i >= 1; i--)
        {
            if (txtTimeDemNguoc) txtTimeDemNguoc.text = i.ToString();
            yield return new WaitForSeconds(1f);
        }
        if (txtTimeDemNguoc) txtTimeDemNguoc.text = "";

        ThongBaoController.Instance.LoadVaoTran.Show(true);

        SceneReadyGate.Reset();
        SendData.SelectHero(heroType);
        AudioManager.Instance.StopAudioBg();

        ThongBaoController.Instance.StartLoadScene("Play");
    }
}