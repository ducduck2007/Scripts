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
        LoadController.Instance.ShowCheckLoadWait(true);
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

    internal static void OnViewInfoPlayer(long userId)
    {
        // ShowLoadWait();
        // Message msg = new Message(CMD.VIEW_INFO_PLAYER);
        // msg.PutLong("userId", userId);
        // SendMessage(msg);
    }

    private static void ShowLoadWait(int time = 18)
    {
        // SceneLoadFunction.Instance.ShowLoadWait(true, time);
    }
}