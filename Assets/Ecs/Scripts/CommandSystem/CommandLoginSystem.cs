using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandLoginSystem : BaseCommandSystem
{
    public CommandLoginSystem(Contexts contexts) : base(contexts)
    {
    }

    protected override int GetProcessCommand()
    {
        return CMD.LOGIN_GAME;
    }

    protected override void Execute(Message msg)
    {
        try
        {
            ShowCheckLoadWait();
            AgentUnity.LogWarning("CMD: LOGIN_GAME = 0" + msg.GetJson());
            if (CheckSuccess(msg))
            {
                long userId = msg.GetLong("userId");
                string userName = msg.GetString("userName");
                int level = msg.GetInt("level");
                int avatarId = msg.GetInt("avatarId");
                int wins = msg.GetInt("wins");      
                int losses = msg.GetInt("losses");
                AgentUnity.SetString(KeyLocalSave.PP_USERNAME, B.Instance.UserName);
                AgentUnity.SetString(KeyLocalSave.PP_PASSWORD, B.Instance.PassWord);
                UiControl.Instance.MainGame.Show(true);
                UiControl.Instance.DestroyLoginController();
            }
        }
        catch (Exception e)
        {
            AgentUnity.LogError(e);
        }
    }
}
