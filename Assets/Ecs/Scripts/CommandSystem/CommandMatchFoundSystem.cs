using System;
using UnityEngine;

public class CommandMatchFoundSystem : BaseCommandSystem
{
    public CommandMatchFoundSystem(Contexts contexts) : base(contexts)
    {
    }

    protected override int GetProcessCommand()
    {
        return CMD.MATCH_FOUND;  // 41
    }

    protected override void Execute(Message msg)
    {
        try
        {
            AgentUnity.LogWarning("CMD: MATCH_FOUND (41) = " + msg.GetJson());

            string matchId = msg.GetString("matchId");
            int playerCount = msg.GetInt("playerCount");
            int teamId = msg.GetInt("teamId");

            Debug.Log($"MATCH FOUND!");
            Debug.Log($"   Match: {matchId}");
            Debug.Log($"   Players: {playerCount}");
            Debug.Log($"   Team: {teamId}");

            // Auto select melee hero (heroType = 0)
            Debug.Log("Auto selecting Melee hero...");
            SendData.SelectHero(0);
        }
        catch (Exception e)
        {
            AgentUnity.LogError(e);
        }
    }
}