using UnityEngine;
public class LoginController : ManualSingleton<LoginController>
{
    public Register prefabRegister;
    public LoginSever prefabLoginSever;

    [Header("Social Login First Screens")]
    public LoginSever prefabLoginGoogleFirst;
    public LoginSever prefabLoginFbFirst;

    private LoginSever _serverLogin;
    internal LoginSever ServerLogin
    {
        get
        {
            if (_serverLogin == null)
                _serverLogin = AgentUnity.InstanceObject<LoginSever>(prefabLoginSever, transform);
            return _serverLogin;
        }
    }

    private Register _registerView;
    internal Register RegisterView
    {
        get
        {
            if (_registerView == null)
                _registerView = AgentUnity.InstanceObject<Register>(prefabRegister, transform);
            _registerView.transform.SetAsLastSibling();
            return _registerView;
        }
    }

    // private void Start()
    // {
    //     ServerLogin.View(true);
    // }
    private void Start()
    {
        int lastLogin = AgentUnity.GetInt(KeyLocalSave.PP_LAST_LOGIN_TYPE, KeyLocalSave.LOGIN_TYPE_NORMAL);

        if (lastLogin == KeyLocalSave.LOGIN_TYPE_GOOGLE && prefabLoginGoogleFirst != null)
        {
            var screen = AgentUnity.InstanceObject<LoginSever>(prefabLoginGoogleFirst, transform);
            screen.View(true);
        }
        else if (lastLogin == KeyLocalSave.LOGIN_TYPE_FACEBOOK && prefabLoginFbFirst != null)
        {
            var screen = AgentUnity.InstanceObject<LoginSever>(prefabLoginFbFirst, transform);
            screen.View(true);
        }
        else
        {
            ServerLogin.View(true);
        }
    }

    internal void ShowNormalLogin()
    {
        ServerLogin.View(true);
    }
}