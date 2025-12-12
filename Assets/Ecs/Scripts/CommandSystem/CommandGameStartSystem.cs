using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CommandGameStartSystem : BaseCommandSystem
{
    public CommandGameStartSystem(Contexts contexts) : base(contexts)
    {
    }

    protected override int GetProcessCommand()
    {
        return CMD.GAME_START;
    }

    protected override void Execute(Message msg)
    {
        try
        {
            ShowCheckLoadWait();
            AgentUnity.LogWarning("CMD: GAME_START = " + msg.GetJson());
            GameStateManager.SetState(GameState.IN_GAME);

            string matchId = msg.GetString("matchId");
            string zoneId = msg.GetString("zoneId");
            B.Instance.teamId = msg.GetInt("teamId");

            // ========== LẤY POSITION TỪ SERVER ==========
            B.Instance.PosX = msg.GetFloat("x");
            B.Instance.PosZ = msg.GetFloat("y");
            // ============================================

            int heroType = msg.GetInt("heroType");
            string heroName = msg.GetString("heroName");
            int hp = msg.GetInt("hp");
            int maxHp = msg.GetInt("maxHp");

            Debug.Log($"🎮 GAME START!");

            // Load Play scene
            SceneManager.LoadScene("Play");
        }
        catch (Exception e)
        {
            AgentUnity.LogError(e);
        }
    }
}