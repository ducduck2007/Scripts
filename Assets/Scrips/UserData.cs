using UnityEngine;

public class UserData : PlayerInfo
{
    protected static UserData instance;

    internal static UserData Instance
    {
        get
        {
            if (instance == null)
            {
                Reset();
            }

            return instance;
        }
    }
    internal static void Reset()
    {
        instance = new UserData();
    }


}
