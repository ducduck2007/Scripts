using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;
using Firebase.Extensions;

public class LoginSever : ScaleScreen
{
    public Button btnLogin, btnRegister;
    public Button buttonGoogleLogin;
    public Button buttonFacebookLogin;

    public InputField inputUserName;
    public InputField inputPassWord;
    [SerializeField] public Toggle tgLuuTaiKhoan;

    public RectTransform rootLogin;

    private Vector2 originPos;
    private Vector2 targetPos;

    [Header("Keyboard Push Settings")]
    [SerializeField] private float pushUpScreenRatio = 1f / 6f;
    [SerializeField] private float pushLerpSpeed = 12f;
    [SerializeField] private int stableFramesRequired = 3;

    [Header("Prefab Type")]
    [SerializeField] private bool isSocialFirstScreen = false;
    private int visibleStableFrames;
    private int hiddenStableFrames;
    private bool _keyboardVisible = false;
    private RectTransform _focusedInputRect;


    private Coroutine _timeoutCoroutine;

    private void StartLoginTimeout()
    {
        StopLoginTimeout();
        _timeoutCoroutine = StartCoroutine(IELoginTimeout());
    }

    private void StopLoginTimeout()
    {
        if (_timeoutCoroutine != null)
        {
            StopCoroutine(_timeoutCoroutine);
            _timeoutCoroutine = null;
        }
    }

    private IEnumerator IELoginTimeout()
    {
        yield return new WaitForSeconds(3f);
        if (!B.Instance.isConnectServerSuccess)
        {
            ShowLoadWait(false);
            SetThongBao("Kết nối không thành công, vui lòng kiểm tra lại mạng hoặc liên hệ bộ phận kỹ thuật để được hỗ trợ!");
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        HideLoginFxIfAny();

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
        if (!Application.isMobilePlatform)
            return;

        // Detect focus qua isFocused, chỉ gọi OnInputFocused 1 lần khi trạng thái thay đổi
        if (inputUserName != null && inputUserName.isFocused && !_keyboardVisible)
        {
            OnInputFocused(inputUserName.GetComponent<RectTransform>(), 0.2f);
        }
        else if (inputPassWord != null && inputPassWord.isFocused && !_keyboardVisible)
        {
            OnInputFocused(inputPassWord.GetComponent<RectTransform>(), 0.3f);
        }

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
        AudioManager.Instance.PlayAudioBg();

        if (DeepLinkHandler.Instance == null)
        {
            GameObject deepLinkObj = new GameObject("DeepLinkHandler");
            deepLinkObj.AddComponent<DeepLinkHandler>();
            DontDestroyOnLoad(deepLinkObj);
        }

        btnLogin.onClick.AddListener(ClickLogin);
        btnRegister.onClick.AddListener(ClickRegister);

        if (buttonGoogleLogin != null)
            buttonGoogleLogin.onClick.AddListener(ClickGoogleLogin);

        if (buttonFacebookLogin != null)
            buttonFacebookLogin.onClick.AddListener(ClickFacebookLogin);

        tgLuuTaiKhoan.onValueChanged.AddListener((arg0 =>
        {
            AudioManager.Instance.AudioClick();
            if (tgLuuTaiKhoan.isOn)
                AgentUnity.SetInt(KeyLocalSave.PP_SavePassword, 0);
            else
                AgentUnity.SetInt(KeyLocalSave.PP_SavePassword, 1);
        }));

        inputUserName.onSubmit.AddListener(_ => inputPassWord.ActivateInputField());

        var rtUsername = inputUserName.GetComponent<RectTransform>();
        var rtPassword = inputPassWord.GetComponent<RectTransform>();

        inputUserName.onSubmit.AddListener(_ => inputPassWord.ActivateInputField());
        inputUserName.onEndEdit.AddListener(_ => OnInputBlurred());
        inputPassWord.onEndEdit.AddListener(_ => OnInputBlurred());

        inputUserName.onEndEdit.AddListener(_ => OnInputBlurred());
        inputPassWord.onEndEdit.AddListener(_ => OnInputBlurred());
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

    public void ClickLogin()
    {
        AudioManager.Instance.AudioClick();
        if (isSocialFirstScreen)
        {
            LoginController.Instance.ShowNormalLogin();
            return;
        }

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
                "Mạng đang bị lỗi, bạn vui lòng kiểm tra lại kết nối Wifi hoặc 5G."
            );
            return;
        }

        StartCoroutine(ProcessLogin(username, password));
    }

    private IEnumerator ProcessLogin(string userName, string passWord)
    {
        ShowLoadWait(true);
        StartLoginTimeout();

        int cmd = CMDApi.API;
        ApiSend re = new ApiSend(cmd);

        try
        {
            re.Put("username", userName);
            re.Put("password", passWord);
        }
        catch (Exception e)
        {
            AgentUnity.LogError(e);
            ShowLoadWait(false);
        }

        UnityEngine.Networking.UnityWebRequest www =
            AgentUnity.GetHttpPost(CMDApi.LINK_GATEWAY_LOGIN, re.GetJson());

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
                    StopLoginTimeout();
                    B.Instance.Keyhash = j.GetString("keyhash");

                    AgentUnity.SetString(KeyLocalSave.PP_USERNAME, userName);
                    AgentUnity.SetString(KeyLocalSave.PP_PASSWORD, passWord);
                    AgentUnity.SetInt(KeyLocalSave.PP_LAST_LOGIN_TYPE, KeyLocalSave.LOGIN_TYPE_NORMAL);

                    Res.IP = j.GetString("server");
                    Res.PORT = j.GetInt("port");

                    Debug.Log("Gateway login successful! Now login game server...");

                    SendData.OnLoginGame();
                }
                else
                {
                    StopLoginTimeout();
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

    private void ClickGoogleLogin()
    {
        AudioManager.Instance.AudioClick();

        if (!AgentUnity.CheckNetWork())
        {
            ThongBaoController.Instance.PopupOneButton.ShowPopupOneButton(
                "Thông Báo",
                "Mạng đang bị lỗi, bạn vui lòng kiểm tra lại kết nối Wifi hoặc 5G."
            );
            return;
        }

        if (Application.isEditor)
        {
            SetThongBao("Google Sign-In không hoạt động trong Unity Editor. Vui lòng build APK!");
            Debug.LogWarning("Google Sign-In không hoạt động trong Unity Editor!");
            return;
        }

        if (FirebaseInitializer.Instance == null || !FirebaseInitializer.Instance.IsReady)
        {
            SetThongBao("Firebase đang khởi động. Vui lòng đợi 2-3 giây và thử lại!");
            return;
        }

        ShowLoadWait(true);
        StartGoogleSignIn();
    }

    private async void StartGoogleSignIn()
    {
        try
        {
            Debug.Log("Creating Google OAuth provider...");

            var providerData = new Firebase.Auth.FederatedOAuthProviderData
            {
                ProviderId = "google.com"
            };

            var provider = new Firebase.Auth.FederatedOAuthProvider(providerData);

            var result = await FirebaseInitializer.Instance.Auth.SignInWithProviderAsync(provider);

            if (result != null && result.User != null)
            {
                string idToken = await result.User.TokenAsync(false);
                StartCoroutine(ProcessFirebaseLogin(idToken, KeyLocalSave.LOGIN_TYPE_GOOGLE));
            }
            else
            {
                ShowLoadWait(false);
                SetThongBao("Đăng nhập Google thất bại");
            }
        }
        catch (Firebase.FirebaseException fbEx)
        {
            ShowLoadWait(false);
            if (fbEx.InnerException != null)
            {
                Debug.LogError($"Inner: {fbEx.InnerException.Message}");
            }
            SetThongBao($"Lỗi đăng nhập Google.\n\n{fbEx.Message}");
        }
        catch (System.Exception e)
        {
            ShowLoadWait(false);
            SetThongBao($"Lỗi đăng nhập Google.\n\n{e.Message}");
        }
    }

    private void ClickFacebookLogin()
    {
        AudioManager.Instance.AudioClick();

        if (!AgentUnity.CheckNetWork())
        {
            ThongBaoController.Instance.PopupOneButton.ShowPopupOneButton(
                "Thông Báo",
                "Mạng đang bị lỗi, bạn vui lòng kiểm tra lại kết nối Wifi hoặc 5G."
            );
            return;
        }

        if (Application.isEditor)
        {
            SetThongBao("Facebook Login không hoạt động trong Unity Editor. Vui lòng build APK!");
            return;
        }

        if (FirebaseInitializer.Instance == null || !FirebaseInitializer.Instance.IsReady)
        {
            SetThongBao("Firebase đang khởi động. Vui lòng đợi 2-3 giây và thử lại!");
            return;
        }

        string facebookAppId = "1975857273355517";
        string redirectUri = "https://fir-login-df67f.firebaseapp.com/oauth/callback";
        string oauthUrl = $"https://www.facebook.com/v12.0/dialog/oauth?client_id={facebookAppId}&redirect_uri={redirectUri}&response_type=token&scope=public_profile";

        Application.OpenURL(oauthUrl);

        if (DeepLinkHandler.Instance != null)
        {
            DeepLinkHandler.Instance.OnDeepLinkReceived += OnFacebookDeepLinkReceived;
        }
        else
        {
            SetThongBao("Lỗi hệ thống. Vui lòng khởi động lại app!");
        }
    }

    private void OnFacebookDeepLinkReceived(string url)
    {
        if (DeepLinkHandler.Instance != null)
        {
            DeepLinkHandler.Instance.OnDeepLinkReceived -= OnFacebookDeepLinkReceived;
        }

        if (url.Contains("access_token="))
        {
            ShowLoadWait(true);

            int startIndex = url.IndexOf("access_token=") + "access_token=".Length;
            int endIndex = url.IndexOf("&", startIndex);
            if (endIndex == -1) endIndex = url.Length;

            string accessToken = url.Substring(startIndex, endIndex - startIndex);

            Firebase.Auth.Credential credential = FacebookAuthProvider.GetCredential(accessToken);

            FirebaseInitializer.Instance.Auth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    ShowLoadWait(false);
                    SetThongBao("Đăng nhập Facebook bị hủy");
                    return;
                }
                if (task.IsFaulted)
                {
                    ShowLoadWait(false);
                    SetThongBao("Lỗi đăng nhập Facebook. Vui lòng thử lại!");
                    return;
                }

                FirebaseUser user = task.Result;
                // GetFirebaseTokenAndLogin(user);
                GetFirebaseTokenAndLogin(user, KeyLocalSave.LOGIN_TYPE_FACEBOOK);
            });
        }
        else
        {
            SetThongBao("Lỗi xác thực Facebook");
        }
    }

    private void GetFirebaseTokenAndLogin(FirebaseUser user, int loginType = 0)
    {
        user.TokenAsync(false).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                ShowLoadWait(false);
                SetThongBao("Lỗi lấy thông tin xác thực");
                return;
            }

            string idToken = task.Result;
            // StartCoroutine(ProcessFirebaseLogin(idToken));
            StartCoroutine(ProcessFirebaseLogin(idToken, loginType));
        });
    }

    private IEnumerator ProcessFirebaseLogin(string idToken, int loginType = 0)
    {
        StartLoginTimeout();
        int cmd = CMDApi.API;
        ApiSend re = new ApiSend(cmd);

        try
        {
            re.Put("idToken", idToken);
        }
        catch (Exception e)
        {
            AgentUnity.LogError(e);
            ShowLoadWait(false);
        }

        UnityEngine.Networking.UnityWebRequest www =
            AgentUnity.GetHttpPost(CMDApi.LINK_GATEWAY_FIREBASE_LOGIN, re.GetJson());

        yield return www.SendWebRequest();

        try
        {
            if (www.isNetworkError || www.isHttpError)
            {
                ShowLoadWait(false);
                SetThongBao(www.error);
                yield break;
            }

            ShowLoadWait(false);
            JObjectCustom j = JObjectCustom.From(www.downloadHandler.text);

            if (j.GetInt("status") == 1)
            {
                StopLoginTimeout();
                B.Instance.Keyhash = j.GetString("keyhash");
                B.Instance.UserName = j.GetString("userName");
                AgentUnity.SetInt(KeyLocalSave.PP_LAST_LOGIN_TYPE, loginType);

                Debug.Log($"Firebase gateway login OK - Username: {B.Instance.UserName}");

                Res.IP = j.GetString("server");
                Res.PORT = j.GetInt("port");

                SendData.OnLoginGame();
            }
            else
            {
                StopLoginTimeout();
                SetThongBao(j.GetString(Key.MESSAGE));
            }
        }
        catch (Exception e)
        {
            ShowLoadWait(false);
            AgentUnity.LogError(e);
        }
    }

    public void ClickRegister()
    {
        AudioManager.Instance.AudioClick();
        LoginController.Instance.RegisterView.Show(true);
        View(false);
    }

    private void ShowLoadWait(bool val)
    {
        LoadController.Instance.ShowLoadWait(val);
    }

    private void SetThongBao(string msg)
    {
        ThongBaoController.Instance.PopupOneButton.ShowPopupOneButton("Thông Báo", msg);
    }

    public void View(bool val)
    {
        gameObject.SetActive(val);
    }

    private void HideLoginFxIfAny()
    {
        var fx = FindLoginFxIncludeInactive();
        if (fx == null) return;

        fx.enabled = false;

        var cg = fx.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = 0f;
            cg.blocksRaycasts = false;
            cg.interactable = false;
        }
    }

    private static AutoPlayPingPong FindLoginFxIncludeInactive()
    {
#if UNITY_2022_2_OR_NEWER
        var all = UnityEngine.Object.FindObjectsByType<AutoPlayPingPong>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );
        foreach (var fx in all)
        {
            if (fx != null && fx.gameObject != null && fx.gameObject.CompareTag("LoginAppearanceFX"))
                return fx;
        }
        return null;
#else
        var all = Resources.FindObjectsOfTypeAll<AutoPlayPingPong>();
        foreach (var fx in all)
        {
            if (fx != null && fx.gameObject != null && fx.gameObject.CompareTag("LoginAppearanceFX"))
                return fx;
        }
        return null;
#endif
    }
}
