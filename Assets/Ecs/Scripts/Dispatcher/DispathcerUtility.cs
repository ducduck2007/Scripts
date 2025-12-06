using System;
using System.Collections.Generic;

public static class DispathcerUtility
{
    public static void Invoke(Action action)
    {
        if (Contexts.sharedInstance.game.hasPending)
        {
            Queue<Action> pending = Contexts.sharedInstance.game.pending.value;
            lock (pending)
            {
                pending.Enqueue(action);
            }
        }
        else
        {
            if(action != null) action.Invoke();
        }
    }
}
