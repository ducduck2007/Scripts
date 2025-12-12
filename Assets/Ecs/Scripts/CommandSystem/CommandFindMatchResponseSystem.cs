using System;
using UnityEngine;

public class CommandFindMatchResponseSystem : BaseCommandSystem
{
    public CommandFindMatchResponseSystem(Contexts contexts) : base(contexts)
    {
    }

    protected override int GetProcessCommand()
    {
        return CMD.FIND_MATCH;  // 40
    }

    protected override void Execute(Message msg)
    {
        try
        {
            AgentUnity.LogWarning("CMD: FIND_MATCH response (40) = " + msg.GetJson());

            string status = msg.GetString("status");
            int queueSize = msg.GetInt("queueSize");

            UiControl.Instance.MainGame.TimTran();
        }
        catch (Exception e)
        {
            AgentUnity.LogError(e);
        }
    }
}