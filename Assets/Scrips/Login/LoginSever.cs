using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LoginSever : ScaleScreen
{
    public Button btnLogin, btnRegister;
    public InputField inputUserName;
    public InputField inputPassWord;
    public Toggle tgNhoMatKhau;

    public override void Start()
    {
        base.Start();
        btnLogin.onClick.AddListener(ClickLogin);
        btnRegister.onClick.AddListener(ClickRegister);
    }

    public void ClickLogin()
    {
        B.Instance.UserName = inputUserName.text;
        B.Instance.PassWord = inputPassWord.text;
        string username = inputUserName.text.Trim();
        string password = inputPassWord.text.Trim();
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
        LoginController.Instance.RegisterView.Show(true);
        View(false);
    }

    private void OnEnable()
    {
        inputUserName.text = AgentUnity.GetString(KeyLocalSave.PP_USERNAME);
        inputPassWord.text = AgentUnity.GetString(KeyLocalSave.PP_PASSWORD);
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
