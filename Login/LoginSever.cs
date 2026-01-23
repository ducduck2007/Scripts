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
    private Vector2 targetPos;

    // ===== Keyboard handling (fix jump + disable in Editor) =====
    [Header("Keyboard Push Settings")]
    [SerializeField] private float pushUpScreenRatio = 1f / 6f; // tương đương Screen.height/6f
    [SerializeField] private float pushLerpSpeed = 12f;         // tốc độ lerp mượt
    [SerializeField] private int stableFramesRequired = 3;      // debounce để tránh nhấp nháy

    private int visibleStableFrames;
    private int hiddenStableFrames;

    protected override void OnEnable()
    {
        base.OnEnable(); // ScaleScreen → Scale()
        originPos = rootLogin.anchoredPosition;
        targetPos = originPos;

        visibleStableFrames = 0;
        hiddenStableFrames = 0;

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
        // ✅ Không xử lý khi đang chạy trong Editor/giả lập (dù build target Android/iOS)
        if (!Application.isMobilePlatform)
            return;

        if (!TouchScreenKeyboard.isSupported)
            return;

        // ✅ Chỉ xử lý khi đang focus vào 1 trong 2 ô username/password
        bool anyFocused = (inputUserName != null && inputUserName.isFocused)
                       || (inputPassWord != null && inputPassWord.isFocused);

        // ✅ Dùng area.height ổn định hơn visible (visible hay nhấp nháy lúc mở)
        float kbHeight = TouchScreenKeyboard.area.height;
        bool keyboardShown = anyFocused && kbHeight > 0.01f;

        if (keyboardShown)
        {
            visibleStableFrames++;
            hiddenStableFrames = 0;

            // Debounce: chỉ đẩy lên khi trạng thái "shown" ổn định vài frame
            if (visibleStableFrames >= stableFramesRequired)
            {
                float offset = Screen.height * pushUpScreenRatio;
                targetPos = originPos + Vector2.up * offset;
            }
        }
        else
        {
            hiddenStableFrames++;
            visibleStableFrames = 0;

            // Debounce: chỉ kéo xuống khi trạng thái "hidden" ổn định vài frame
            if (hiddenStableFrames >= stableFramesRequired)
            {
                targetPos = originPos;
            }
        }

        // ✅ Di chuyển mượt để tránh nhảy giật
        rootLogin.anchoredPosition = Vector2.Lerp(
            rootLogin.anchoredPosition,
            targetPos,
            Time.unscaledDeltaTime * pushLerpSpeed
        );
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
            SetThongBao("Tài khoản phải từ " + C.LENGTH_MIN_USERNAME + " ký tự và không chứa ký tự đặc biệt.");
            return;
        }

        if (password.Length < C.LENGTH_MIN_PASWORD || !AgentCSharp.CheckPasswordValidate(password))
        {
            SetThongBao("Mật khẩu phải từ " + C.LENGTH_MIN_PASWORD + " ký tự và chứa chữ cái và số.");
            return;
        }

        if (!AgentUnity.CheckNetWork())
        {
            ThongBaoController.Instance.PopupOneButton.ShowPopupOneButton(
                "Thông Báo",
                "Mạng đang bị lỗi, bạn vui lòng kiểm tra lại kết nối Wifi hoặc 3G/4G."
            );
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

        UnityEngine.Networking.UnityWebRequest www =
            AgentUnity.GetHttpPost(CMDApi.LINK_GATEWAY_LOGIN, re.GetJson());

        // www.timeout = 10;
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
                {
                    SetThongBao(j.GetString(Key.MESSAGE));
                }
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
