using System.Collections.Generic;
using Entitas;
using UnityEngine;

public class CleanupCommandSystem : ReactiveSystem<NetworkEntity>
{
    private readonly Contexts _contexts;
    
    public CleanupCommandSystem(Contexts contexts) : base(contexts.network)
    {
        _contexts = contexts;
    }

    protected override ICollector<NetworkEntity> GetTrigger(IContext<NetworkEntity> context)
    {
        return context.CreateCollector(NetworkMatcher.AllOf(NetworkMatcher.Command,
            NetworkMatcher.MessageData));
    }

    protected override bool Filter(NetworkEntity entity)
    {
        return entity.hasCommand;
    }

    protected override void Execute(List<NetworkEntity> entities)
    {
        foreach (var entity in entities)
        {
            if(!entity.isProcess) Debug.Log("Chưa xử lý lệnh: " + entity.command.value);
            entity.Destroy();
        }
    }
}