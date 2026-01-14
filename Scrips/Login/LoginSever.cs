using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LoginSever : ScaleScreen
{
    public Button btnLogin, btnRegister;
    public InputField inputUserName;
    public InputField inputPassWord;
    [SerializeField] public Toggle tgLuuTaiKhoan;

    public RectTransform rootLogin; // gán = Login (RectTransform)

    private Vector2 originPos;
    private bool keyboardOpened;

    protected override void OnEnable()
    {
        base.OnEnable(); // ScaleScreen → Scale()
        originPos = rootLogin.anchoredPosition;

        if (AgentUnity.GetInt(KeyLocalSave.PP_SavePassword) == 0)
        {
            inputUserName.text = AgentUnity.GetString(KeyLocalSave.PP_USERNAME);
            inputPassWord.text = AgentUnity.GetString(KeyLocalSave.PP_PASSWORD);
        }
        else
        {
            inputUserName.text = String.Empty;
            inputPassWord.text = String.Empty;
        }

        tgLuuTaiKhoan.isOn = AgentUnity.GetInt(KeyLocalSave.PP_SavePassword) == 0;
    }

    void Update()
    {
#if UNITY_ANDROID || UNITY_IOS
        if (!TouchScreenKeyboard.isSupported)
            return;
            
        if (TouchScreenKeyboard.visible)
        {
            if (!keyboardOpened)
            {
                keyboardOpened = true;

                rootLogin.anchoredPosition =
                    originPos + Vector2.up * (Screen.height / 6f);
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

    protected override void Start()
    {
        base.Start();
        btnLogin.onClick.AddListener(ClickLogin);
        btnRegister.onClick.AddListener(ClickRegister);
        tgLuuTaiKhoan.onValueChanged.AddListener((arg0 =>
        {
            AudioManager.Instance.AudioClick();
            if (tgLuuTaiKhoan.isOn)
            {
                AgentUnity.SetInt(KeyLocalSave.PP_SavePassword, 0);
            }
            else
            {
                AgentUnity.SetInt(KeyLocalSave.PP_SavePassword, 1);
            }
        }));
        inputUserName.onSubmit.AddListener(_ =>
        {
            inputPassWord.ActivateInputField();
        });
    }

    public void ClickLogin()
    {
        AudioManager.Instance.AudioClick();
        B.Instance.UserName = inputUserName.text;
        B.Instance.PassWord = inputPassWord.text;
        string username = inputUserName.text.Trim();
        string password = inputPassWord.text.Trim();

        if (username.Length < C.LENGTH_MIN_USERNAME || !AgentCSharp.CheckUsernameValidate(username))
        {
            SetThongBao(
                "Tài khoản phải từ " + C.LENGTH_MIN_USERNAME + " ký tự và không chứa ký tự đặc biệt.");
            return;
        }

        if (password.Length < C.LENGTH_MIN_PASWORD || !AgentCSharp.CheckPasswordValidate(password))
        {
            SetThongBao(
                "Mật khẩu phải từ " + C.LENGTH_MIN_PASWORD + " ký tự và chứa chữ cái và số.");
            return;
        }
        if (!AgentUnity.CheckNetWork())
        {
            ThongBaoController.Instance.PopupOneButton.ShowPopupOneButton("Thông Báo", "Mạng đang bị lỗi, bạn vui lòng kiểm tra lại kết nối Wifi hoặc 3G/4G.");
            return;
        }
        StartCoroutine(ProcessLogin(username, password));
    }

    private IEnumerator ProcessLogin(string userName, string passWord)
    {
        int cmd = CMDApi.API;
        ApiSend re = new ApiSend(cmd);
        try
        {
            // re.Put("version", Res.VERSION);
            re.Put("username", userName);
            re.Put("password", passWord);
            // re.Put("platform", CMD.TYPE_FLATFORM);
            // re.Put("provider", CMD.PROVIDER_ID);
            // re.Put("imei", AgentUnity.GetImeiDevice());
            // re.Put("loginType", loginType);
            // re.Put("ip", AgentUnity.GetLocalIPAddress());
            // re.Put("macAddress",AgentUnity.GetMacAddress());
            // re.Put("uuid",AgentUnity.GetUUID());
            // re.Put("deviceType", SystemInfo.deviceType.ToString());
            // re.Put("deviceName", SystemInfo.deviceName);
            // re.Put("deviceModel", SystemInfo.deviceModel);
            // re.Put("operatingSystemFamily", SystemInfo.operatingSystemFamily.ToString());
            // re.Put("operatingSystem", SystemInfo.operatingSystem);
            // re.Put("tokenFirebase", AgentUnity.GetString(KeyLocalSave.PP_TOKEN_FIREBASE));
        }
        catch (Exception e)
        {
            AgentUnity.LogError(e);
            ShowLoadWait(false);
        }

        UnityEngine.Networking.UnityWebRequest www = AgentUnity.GetHttpPost(CMDApi.LINK_GATEWAY_LOGIN, re.GetJson());
        //        www.timeout = 10;
        yield return www.SendWebRequest();

        try
        {
            if (www.isNetworkError)
            {
                ShowLoadWait(false);
                SetThongBao(www.error);
                yield break;
            }
            else if (www.isHttpError)
            {
                ShowLoadWait(false);
                SetThongBao(www.error);
                yield break;
            }
            else
            {
                ShowLoadWait(false);
                JObjectCustom j = JObjectCustom.From(www.downloadHandler.text);
                if (j.GetInt("status") == 1)
                {
                    // bool updateVersion = j.GetBool("updateVersion");
                    // Res.LINK_UPDATE = j.GetString("linkUpdate");

                    // if (updateVersion)
                    // {
                    //     ShowLoadWait(false);
                    //     string thongBaoUpdate = j.GetString("message");
                    //     SetThongBao(thongBaoUpdate);
                    //     yield break;
                    // }

                    B.Instance.Keyhash = j.GetString("keyhash");
                    AgentUnity.SetString(KeyLocalSave.PP_USERNAME, userName);
                    AgentUnity.SetString(KeyLocalSave.PP_PASSWORD, passWord);

                    Res.IP = j.GetString("server");
                    Res.PORT = j.GetInt("port");
                    SendData.OnLoginGame();
                }
                else
                    SetThongBao(j.GetString(Key.MESSAGE));
            }
        }
        catch (Exception e)
        {
            ShowLoadWait(false);
            AgentUnity.LogError(e);
        }
    }

    private void ShowLoadWait(bool val)
    {
        LoadController.Instance.ShowLoadWait(val);
    }

    public void ClickRegister()
    {
        AudioManager.Instance.AudioClick();
        LoginController.Instance.RegisterView.Show(true);
        View(false);
    }

    private void SetThongBao(string msg)
    {
        ThongBaoController.Instance.PopupOneButton.ShowPopupOneButton("Thông Báo", msg);
    }

    public void View(bool val)
    {
        gameObject.SetActive(val);
    }
}
