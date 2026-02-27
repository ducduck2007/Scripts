using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

enum FieldName
{
    PASSWORD,
    REPASSWORD
}

public class Register : ScaleScreen
{
    public TMP_InputField ifUsername, ifPassword, ifRePassword;
    public Button btnRegister, btnQuayLai;
    [SerializeField] private Toggle tgEyePassword, tgEyeRePassword;
    private TouchScreenKeyboard _keyboard;

    public RectTransform rootLogin; // gán = Login (RectTransform)

    private Vector2 originPos;
    private Vector2 targetPos;

    // ===== Keyboard handling (fix jump + disable in Editor) =====
    [Header("Keyboard Push Settings")]
    [SerializeField] private float pushUpScreenRatio = 1f / 4f; // trước đây bạn dùng Screen.height/4f
    [SerializeField] private float pushLerpSpeed = 12f;         // tốc độ lerp mượt
    [SerializeField] private int stableFramesRequired = 3;      // debounce để tránh nhấp nháy

    private bool _keyboardVisible = false;
    private RectTransform _focusedInputRect;

    private int visibleStableFrames;
    private int hiddenStableFrames;

    // Use this for initialization
    protected override void Start()
    {
        base.Start();
        btnRegister.onClick.AddListener(ClickRegister);
        ifUsername.characterLimit = C.LENGTH_MAX_USERNAME;
        ifPassword.characterLimit = C.LENGTH_MAX_PASWORD;
        btnQuayLai.onClick.AddListener(ClickLogin);
        tgEyePassword.onValueChanged.AddListener(arg0 => SetEyePassword());
        tgEyeRePassword.onValueChanged.AddListener(arg0 => SetEyeRePassword());

        ifUsername.onSubmit.AddListener(_ => ifPassword.ActivateInputField());
        ifPassword.onSubmit.AddListener(_ => ifRePassword.ActivateInputField());

        ifUsername.onSelect.AddListener(_ => OnInputFocused(ifUsername.GetComponent<RectTransform>(), 0.2f));
        ifPassword.onSelect.AddListener(_ => OnInputFocused(ifPassword.GetComponent<RectTransform>(), 0.3f));
        ifRePassword.onSelect.AddListener(_ => OnInputFocused(ifRePassword.GetComponent<RectTransform>(), 0.4f));

        ifUsername.onDeselect.AddListener(_ => OnInputBlurred());
        ifPassword.onDeselect.AddListener(_ => OnInputBlurred());
        ifRePassword.onDeselect.AddListener(_ => OnInputBlurred());
    }

    private void OnInputFocused(RectTransform inputRect, float ratio)
    {
        _focusedInputRect = inputRect;
        _keyboardVisible = true;

        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, inputRect.position);
        float targetScreenY = Screen.height * ratio;

        if (screenPos.y < targetScreenY)
        {
            float offsetY = targetScreenY - screenPos.y;
            targetPos = originPos + Vector2.up * offsetY;
        }
        else
        {
            targetPos = originPos;
        }
    }

    private void OnInputBlurred()
    {
        _keyboardVisible = false;
        _focusedInputRect = null;
        targetPos = originPos;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        originPos = rootLogin.anchoredPosition;
        targetPos = originPos;

        visibleStableFrames = 0;
        hiddenStableFrames = 0;
        _keyboardVisible = false;
        _focusedInputRect = null;

        Invoke("DelayRegister", 1f);
        // tgEyePassword.isOn = true;
        // tgEyeRePassword.isOn = true;

        tgEyePassword.isOn = false;
        tgEyeRePassword.isOn = false;

        ifPassword.contentType = TMP_InputField.ContentType.Standard;
        ifRePassword.contentType = TMP_InputField.ContentType.Standard;
        ifPassword.ForceLabelUpdate();
        ifRePassword.ForceLabelUpdate();
    }

    void Update()
    {
#if UNITY_ANDROID || UNITY_IOS
        if (!Application.isMobilePlatform)
            return;

        // Di chuyển mượt để tránh nhảy giật
        rootLogin.anchoredPosition = Vector2.Lerp(
            rootLogin.anchoredPosition,
            targetPos,
            Time.unscaledDeltaTime * pushLerpSpeed
        );
#endif
    }

    private void SetEyePassword()
    {
        AudioManager.Instance.AudioClick();
        if (ifPassword.contentType == TMP_InputField.ContentType.Standard)
            ifPassword.contentType = TMP_InputField.ContentType.Password;
        else
            ifPassword.contentType = TMP_InputField.ContentType.Standard;

        ifPassword.ForceLabelUpdate();
    }

    private void SetEyeRePassword()
    {
        AudioManager.Instance.AudioClick();
        if (ifRePassword.contentType == TMP_InputField.ContentType.Standard)
            ifRePassword.contentType = TMP_InputField.ContentType.Password;
        else
            ifRePassword.contentType = TMP_InputField.ContentType.Standard;
        ifRePassword.ForceLabelUpdate();
    }

    private void OnNextLine(FieldName field)
    {
        if (ifUsername.text.Length <= 0)
            return;
        if (ifUsername.text.Length <= 8)
            return;
        if (ifRePassword.text.Length <= 8)
            return;

        if (CheckNextLineCondition())
        {
            StopAllCoroutines();
            StartCoroutine(ActiveInputField(field));
        }
    }

    private bool CheckNextLineCondition()
    {
        return _keyboard != null && _keyboard.status == TouchScreenKeyboard.Status.Done;
    }

    private IEnumerator ActiveInputField(FieldName field)
    {
        yield return new WaitForSeconds(0.6f);
        switch (field)
        {
            case FieldName.PASSWORD:
                ifPassword.ActivateInputField();
                _keyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default, false, false, true, false,
                    "Nhập mật khẩu");
                ifPassword.contentType = TMP_InputField.ContentType.Password;
                break;

            case FieldName.REPASSWORD:
                ifRePassword.ActivateInputField();
                _keyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default, false, false, true, false,
                    "Nhập lại mật khẩu");
                ifRePassword.contentType = TMP_InputField.ContentType.Password;
                break;
        }
    }

    private void DelayRegister()
    {
        ifUsername.ActivateInputField();
        _keyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default, false, false, false, false,
            "Nhập tài khoản");
    }

    public void ClickLogin()
    {
        AudioManager.Instance.AudioClick();
        ifUsername.text = string.Empty;
        ifPassword.text = string.Empty;
        ifRePassword.text = string.Empty;

        LoginController.Instance.ServerLogin.View(true);
        Show(false);
    }

    private void ClickRegister()
    {
        AudioManager.Instance.AudioClick();
        string username = ifUsername.text.Trim();
        string password = ifPassword.text.Trim();
        string rePassword = ifRePassword.text.Trim();

        if (username.Length < C.LENGTH_MIN_USERNAME || !AgentCSharp.CheckUsernameValidate(username))
        {
            SetThongBao("Tài khoản phải từ " + C.LENGTH_MIN_USERNAME + " ký tự và không chứa ký tự đặc biệt.");
            return;
        }

        if (password != rePassword)
        {
            SetThongBao("Hai mật khẩu không khớp nhau.");
            return;
        }

        if (password.Length < C.LENGTH_MIN_PASWORD || !AgentCSharp.CheckPasswordValidate(password))
        {
            SetThongBao("Mật khẩu phải từ " + C.LENGTH_MIN_PASWORD + " ký tự và chứa chữ cái và số.");
            return;
        }

        ShowLoadWait(true);
        StartCoroutine(ProcessRegister(username, password, rePassword));
    }

    private IEnumerator ProcessRegister(string userName, string passWord, string rePassword)
    {
        ApiSend re = new ApiSend(CMDApi.API);
        re.Put("username", userName);
        re.Put("password", passWord);

        UnityEngine.Networking.UnityWebRequest www =
            AgentUnity.GetHttpPost(CMDApi.LINK_GATEWAY_REGISTER, re.GetJson());

        yield return www.SendWebRequest();

        if (www.isNetworkError)
        {
            ShowLoadWait(false);
            ThongBaoController.Instance.ShowThongBaoNhanh(www.error);
            AgentUnity.LogError(www.error);
            yield break;
        }
        else if (www.isHttpError)
        {
            ShowLoadWait(false);
            ThongBaoController.Instance.ShowThongBaoNhanh(www.error);
            AgentUnity.LogError(www.error);
            yield break;
        }
        else
        {
            JObjectCustom j = JObjectCustom.From(www.downloadHandler.text);
            if (j.GetInt("status") == 1)
            {
                AgentUnity.SetString(KeyLocalSave.PP_USERNAME, userName);
                AgentUnity.SetString(KeyLocalSave.PP_PASSWORD, passWord);

                ThongBaoController.Instance.ShowThongBaoNhanh("Đăng ký thành công!");
                ClickLogin();
            }
            else
            {
                ThongBaoController.Instance.ShowThongBaoNhanh(j.GetString(Key.MESSAGE));
            }

            ShowLoadWait(false);
        }
    }

    private void SetThongBao(string msg)
    {
        ThongBaoController.Instance.PopupOneButton.ShowPopupOneButton("Thông Báo", msg);
    }

    private void ShowLoadWait(bool val)
    {
        LoadController.Instance.ShowLoadWait(val);
    }

    public void Show(bool val)
    {
        gameObject.SetActive(val);
    }
}
