using System;
using System.Collections.Generic;
using Entitas;

public class DispatcherSystem : IInitializeSystem, IExecuteSystem
{
    private Contexts _contexts;
    private Queue<Action> _pending;
    
    public DispatcherSystem(Contexts contexts)
    {
        _contexts = contexts;
    }

    public void Initialize()
    {
        _contexts.game.ReplacePending(new Queue<Action>());
        _pending = _contexts.game.pending.value;
    }
    
    public void Execute()
    {
        lock (_pending)
        {
            while (_pending.Count > 0)
            {
                _pending.Dequeue().Invoke();
            }
        }          
    }
}