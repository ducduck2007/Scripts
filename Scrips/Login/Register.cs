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
    private bool keyboardOpened;

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
        ifUsername.onSubmit.AddListener(_ =>
        {
            ifPassword.ActivateInputField();
        });
        ifPassword.onSubmit.AddListener(_ =>
        {
            ifRePassword.ActivateInputField();
        });
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        originPos = rootLogin.anchoredPosition;
        Invoke("DelayRegister", 1f);
        tgEyePassword.isOn = true;
        tgEyeRePassword.isOn = true;
    }

    void Update()
    {
#if UNITY_ANDROID || UNITY_IOS
        if (TouchScreenKeyboard.visible)
        {
            if (!keyboardOpened)
            {
                keyboardOpened = true;

                rootLogin.anchoredPosition =
                    originPos + Vector2.up * (Screen.height / 4f);
            }
        }
        else
        {
            if (keyboardOpened)
            {
                keyboardOpened = false;
                rootLogin.anchoredPosition = originPos;
            }
        }
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
        return _keyboard.status == TouchScreenKeyboard.Status.Done;
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
            // case FieldName.GMAIL:
            //     ifGmail.ActivateInputField();
            //     _keyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default, false, false, false,
            //         false,
            //         "Nhập gmail");
            //     break;
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
        // ifGmail.text = t;
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
            SetThongBao(
                "Tài khoản phải từ " + C.LENGTH_MIN_USERNAME + " ký tự và không chứa ký tự đặc biệt.");
            return;
        }
        
        if (password != rePassword)
        {
            SetThongBao("Hai mật khẩu không khớp nhau.");
            return;
        }

        if (password.Length < C.LENGTH_MIN_PASWORD || !AgentCSharp.CheckPasswordValidate(password))
        {
            SetThongBao(
                "Mật khẩu phải từ " + C.LENGTH_MIN_PASWORD + " ký tự và chứa chữ cái và số.");
            return;
        }

        ShowLoadWait(true);
        StartCoroutine(ProcessRegister(username, password, rePassword));
    }

    private IEnumerator ProcessRegister(string userName, string passWord,
        string rePassword)
    {
        ApiSend re = new ApiSend(CMDApi.API);
        re.Put("username", userName);
        re.Put("password", passWord);
//         re.Put("platform", CMD.TYPE_FLATFORM);
//         re.Put("provider", CMD.PROVIDER_ID);
//         re.Put("imei", AgentUnity.GetImeiDevice());
//         // re.Put("imei", "1769efb46c10fb4c8a3198106d5c8fde5e01c408");
//         re.Put("ip", AgentUnity.GetLocalIPAddress());
//         re.Put("macAddress",AgentUnity.GetMacAddress());
//         re.Put("uuid",AgentUnity.GetUUID());
//         re.Put("deviceType", SystemInfo.deviceType.ToString());
//         re.Put("deviceName", SystemInfo.deviceName);
//         re.Put("deviceModel", SystemInfo.deviceModel);
//         re.Put("operatingSystemFamily", SystemInfo.operatingSystemFamily.ToString());
//         re.Put("operatingSystem", SystemInfo.operatingSystem);
// //        re.Put("language", AgentUnity.GetInt(KeyLocalSave.PP_ID_LANGUAGE));
//         re.Put("tokenFirebase", AgentUnity.GetString(KeyLocalSave.PP_TOKEN_FIREBASE));
        UnityEngine.Networking.UnityWebRequest www = AgentUnity.GetHttpPost(CMDApi.LINK_GATEWAY_REGISTER, re.GetJson());
//        www.timeout = 10;
        yield return www.SendWebRequest();

        if (www.isNetworkError)
        {
            ShowLoadWait(false);
            SetThongBao(www.error);
            AgentUnity.LogError(www.error);
            yield break;
        }
        else if (www.isHttpError)
        {
            ShowLoadWait(false);
            SetThongBao(www.error);
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
                // SuperDialog.Instance.Toast.MakeToast(j.GetString(Key.MESSAGE), 0.8f);
                ClickLogin();
            }
            else
            {
                SetThongBao(j.GetString(Key.MESSAGE));
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