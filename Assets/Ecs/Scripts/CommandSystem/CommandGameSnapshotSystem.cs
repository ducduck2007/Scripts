using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Command System xử lý GAME_SNAPSHOT (CMD 50) từ server
/// Gọi GameManager.UpdateGameSnapshot để spawn/update entities
/// </summary>
public class CommandGameSnapshotSystem : BaseCommandSystem
{
    private int snapshotCount = 0;
    private float lastLogTime = 0f;

    public CommandGameSnapshotSystem(Contexts contexts) : base(contexts)
    {
    }

    protected override int GetProcessCommand()
    {
        return CMD.GAME_SNAPSHOT;  // 50
    }

    protected override void Execute(Message msg)
    {
        snapshotCount++;

        try
        {
            // AgentUnity.LogError("CMD: GAME_SNAPSHOT = " + msg.GetJson());

            List<PlayerOutPutSv> players = msg.GetClassList<PlayerOutPutSv>("players");
            List<JungleMonsterOutPutSv> monsters = msg.GetClassList<JungleMonsterOutPutSv>("monsters");
            if (TranDauControl.Instance != null)
            {
                TranDauControl.Instance.Init(players);
                TranDauControl.Instance.InitMonster(monsters);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"   Message: {e.Message}");
            Debug.LogError($"   Stack: {e.StackTrace}");
        }
    }
}