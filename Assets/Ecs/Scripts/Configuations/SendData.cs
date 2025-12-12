using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Send Request & data to SERVER
/// </summary>
public class SendData
{
    public static void SendMessage(Message msg)
    {
        NetworkEntity entity = Contexts.sharedInstance.network.CreateEntity();
        entity.AddMessageData(msg);
        entity.isSend = true;
    }


    internal static void OnLoginGame()
    {
        B.Instance.isConnectServerSuccess = false;
        ShowLoadWait();
        AgentUnity.LogWarning("SendData: OnLoginGame");
        Message msg = new Message(CMD.LOGIN_GAME);
        msg.PutInt("userId", 0);
        msg.PutInt("serverId", 0);
        msg.PutString("userName", B.Instance.UserName);
        msg.PutString("password", B.Instance.PassWord);
        msg.PutString("keyhash", "");
        msg.PutString("tokenFirebase", "");
        msg.PutInt("version", 0);
        msg.PutString("platform", "");
        msg.PutString("providerId", "");
        SendMessage(msg);
    }

    private static void ShowLoadWait(int time = 18)
    {
        LoadController.Instance.ShowCheckLoadWait(true, time);
    }

    internal static void FindMatch()
    {
        AgentUnity.LogWarning("SendData: FindMatch");
        Message msg = new Message(CMD.FIND_MATCH);
        SendMessage(msg);
        GameStateManager.SetState(GameState.FINDING_MATCH);
    }

    internal static void CancelFindMatch()
    {
        AgentUnity.LogWarning("SendData: FindMatch");
        Message msg = new Message(CMD.CANCEL_FIND_MATCH);
        SendMessage(msg);
    }

    internal static void SelectHero(int heroType)
    {
        AgentUnity.LogWarning($"SendData: SelectHero type={heroType}");
        Message msg = new Message(CMD.SELECT_HERO);
        msg.PutInt("heroType", heroType);
        SendMessage(msg);
    }

    // ========== MOVEMENT ==========

    internal static void SendMovementInput(int dirX, int dirY, bool running, Vector3 position)
    {
        Message msg = new Message(CMD.MOVEMENT_INPUT);
        msg.PutInt("dirX", dirX);
        msg.PutInt("dirY", dirY);
        msg.PutBool("running", running);
        msg.PutInt("x", (int)(position.x)); // Convert Unity units to server units
        msg.PutInt("y", (int)(position.z));
        SendMessage(msg);
    }

    internal static void SendStop(Vector3 position)
    {
        Message msg = new Message(CMD.STOP_COMMAND);
        msg.PutInt("x", (int)position.x);
        msg.PutInt("y", (int)position.z);
        SendMessage(msg);
    }

    // ========== COMBAT ==========

    internal static void SendAttack(long targetId, int targetType, Vector3 position, int skillId)
    {
        AgentUnity.LogWarning($"SendData: Attack target={targetId} type={targetType}");
        Message msg = new Message(CMD.ATTACK);
        msg.PutLong("targetId", targetId);
        msg.PutInt("targetType", targetType);
        msg.PutInt("skillId", skillId);
        msg.PutInt("x", (int)(position.x));
        msg.PutInt("y", (int)(position.z));
        SendMessage(msg);
    }
}