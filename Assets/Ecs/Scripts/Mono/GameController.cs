using Entitas;
using UnityEngine;

public class GameController : ManualSingleton<GameController>
{
    private Contexts _contexts;
    private Systems _systems;

    public bool IsLogin()
    {
        return _contexts.game.isLoginSuccess;
    }
    
    protected override void Awake()
    {
        base.Awake();
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Application.runInBackground = true;
        Application.targetFrameRate = C.TARGET_FRAME;
        Resources.UnloadUnusedAssets();
    }

    private void Start()
    {
        _contexts = Contexts.sharedInstance;
        _systems = new GameSystems(_contexts);
        _systems.Initialize();
    }

    private void Update()
    {
        _systems.Execute();
        _systems.Cleanup();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        _systems.TearDown();
    }
}