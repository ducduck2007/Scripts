using System;
using UnityEngine;

public class CommandRespawnSystem : BaseCommandSystem
{
    public CommandRespawnSystem(Contexts contexts) : base(contexts)
    {
    }

    protected override int GetProcessCommand()
    {
        return CMD.RESPAWN;
    }

    protected override void Execute(Message msg)
    {
        try
        {
            AgentUnity.LogError("CMD: RESPAWN = " + msg.GetJson());
            long userId = msg.GetLong("userId");
            float x = msg.GetFloat("x");
            float y = msg.GetFloat("y");
            int hp = msg.GetInt("hp");

            if (userId == UserData.Instance.UserID)
            {
                B.Instance.PosX = x;
                B.Instance.PosZ = y;
                TranDauControl.Instance.playerMove.onRespawn(hp);
            }
            else
            {
                TranDauControl.Instance.playerOther.onRespawn(x, y, hp);
            }
        }
        catch (Exception e)
        {
            AgentUnity.LogError(e);
        }
    }
}