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
        if (!B.Instance.InGame)
        {
            _loginController = AgentUnity.InstanceObject<LoginController>(Load(PathResource.LoginController), transform);
        }
        else
        {
            MainGame1.Show(true);
        }
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

    private MainGame1 _mainGame1;
    public MainGame1 MainGame1
    {
        get
        {
            if (_mainGame1 == null)
            {
                _mainGame1 = AgentUnity.InstanceObject<MainGame1>(Load(PathResource.MainGame1), transform);
            }

            return _mainGame1;
        }
    }

}
