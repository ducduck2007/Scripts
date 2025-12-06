using UnityEngine;

public class LoginController : MonoBehaviour
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

    private void Start()
    {
        ServerLogin.View(true);
    }
}
