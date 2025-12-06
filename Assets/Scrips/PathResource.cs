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


    public const string MainMenu = "Prefab/Menu/MainMenu";
    public const string MainGame = "MainGame/MainGame";

    public const string LoadWait = "Load/LoadWait";
    public const string LoadWaitData = "Load/LoadWait";
    public const string LoadPercent = "Load/LoadPercent";
    
}