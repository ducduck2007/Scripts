using System;
using System.Collections.Generic;
using Entitas;
using UnityEngine;

public class LoginSuccessSystem : ReactiveSystem<GameEntity>
{
    private Contexts _contexts;

    public LoginSuccessSystem(Contexts contexts) : base(contexts.game)
    {
        _contexts = contexts;
    }

    protected override ICollector<GameEntity> GetTrigger(IContext<GameEntity> context)
    {
        return context.CreateCollector(GameMatcher.LoginSuccess);
    }

    protected override bool Filter(GameEntity entity)
    {
        return entity.isLoginSuccess;
    }

    protected override void Execute(List<GameEntity> entities)
    {
        // SceneLoadFunction.Instance.ShowLoadWait(false);
        // SceneLoadFunction.Instance.ShowLoadPercentChangeInfo(true);
        Queue<Action> queue = RequestDataWhenLoggedIn.RequestQueue();
        _contexts.network.SetRequestQueue(queue, queue.Count, 0);
    }
}