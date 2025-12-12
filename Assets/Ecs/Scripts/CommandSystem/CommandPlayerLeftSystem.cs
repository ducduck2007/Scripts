using System;
using UnityEngine;

public class CommandPlayerLeftSystem : BaseCommandSystem
{
    public CommandPlayerLeftSystem(Contexts contexts) : base(contexts)
    {
    }

    protected override int GetProcessCommand()
    {
        return CMD.PLAYER_LEFT;
    }

    protected override void Execute(Message msg)
    {
        try
        {
            long userId = msg.GetLong("userId");

            
        }
        catch (Exception e)
        {
            AgentUnity.LogError(e);
        }
    }
}