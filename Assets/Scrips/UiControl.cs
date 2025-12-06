using UnityEngine;

public class UiControl: ManualSingleton<UiControl>
{
    [SerializeField] private Transform mainCanvas;

    private LoginController _loginController;

    private GameObject Load(string namePath)
    {
        return Resources.Load(namePath) as GameObject;
    }

    private void Start()
    {
        _loginController = AgentUnity.InstanceObject<LoginController>(Load(PathResource.LoginController), transform);
    }

    public void DestroyLoginController()
    {
        if (_loginController != null)
        {
            Destroy(_loginController.gameObject);
            _loginController = null;
        }
    }

    private MainGame _mainGame;
    public MainGame MainGame
    {
        get
        {
            if (_mainGame == null)
            {
                _mainGame = AgentUnity.InstanceObject<MainGame>(Load(PathResource.MainGame), transform);
            }

            return _mainGame;
        }
    }

}
