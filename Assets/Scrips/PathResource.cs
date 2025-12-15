using System;
using UnityEngine;
using UnityEngine.UI;

public class PathResource
{
    private static Sprite GetSprite(string path)
    {
        return Resources.Load<Sprite>(path);
    }

    private static GameObject GetGameObj(string path)
    {
        return Resources.Load<GameObject>(path);
    }


    #region Login

    public const string LoginController = "Login/LoginController";

    #endregion


    public const string PopupOneButton = "ThongBao/PopupOneButton";
    public const string PopupTwoButton = "ThongBao/PopupTwoButton";
    public const string MainGame = "MainGame/MainGame";
    public const string MainGame1 = "MainGame/MainGame1";
    public const string DialogChonPhong = "MainGame/ChonPhong";
    public const string PopupTimTran = "MainGame/TimTran";

    public const string LoadWait = "Load/LoadWait";
    public const string LoadWaitData = "Load/LoadWait";
    public const string LoadPercent = "Load/LoadPercent";
    
}