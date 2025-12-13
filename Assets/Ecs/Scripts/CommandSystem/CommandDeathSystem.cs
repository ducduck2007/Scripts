using System;
using UnityEngine;

public class CommandDeathSystem : BaseCommandSystem
{
    public CommandDeathSystem(Contexts contexts) : base(contexts)
    {
    }

    protected override int GetProcessCommand()
    {
        return CMD.DEATH;
    }

    protected override void Execute(Message msg)
    {
        try
        {
            AgentUnity.LogError("CMD: DEATH = " + msg.GetJson());
            long killerId = msg.GetLong("killerId");
            string killerName = msg.GetString("killerName");
            long victimId = msg.GetLong("victimId");
            string victimName = msg.GetString("victimName");
            int timeReBorn = msg.GetInt("timeReBorn");
            Debug.Log($"{victimId} killed by {killerName}");

            if (victimId == UserData.Instance.UserID)
            {
                TranDauControl.Instance.playerMove.onDeath();
            }
            else
            {
                TranDauControl.Instance.playerOther.onDeath();
            }
        }
        catch (Exception e)
        {
            AgentUnity.LogError(e);
        }
    }
}