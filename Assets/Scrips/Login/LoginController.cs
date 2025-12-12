using UnityEngine;

public class LoginController : ManualSingleton<LoginController>
{    
    public Register prefabRegister;
    public LoginSever prefabLoginSever;

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

    private void Start()
    {
        ServerLogin.View(true);
    }
}
