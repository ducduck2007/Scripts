using UnityEngine;
using UnityEngine.UI;

public class AgentLV
{
    public static string GetColorTitleText(object data)
    {
        return "<color=#F6DCA5FF>" + data + "</color>";
    }
    public static string GetColorTextYellow(object data)
    {
        return "<color=#FFE00CFF>" + data + " </color>";
    }
    internal static string GetColorTextWhite(object data)
    {
        return "<color=#FFFFFFFF>" + data + " </color>";
    }
    
    internal static string GetColorTextWhite2(object data)
    {
        return "<color=#FFFFFFFF>" + data + "</color>";
    }
    
    internal static string GetColorTextRed(object data)
    {
        return "<color=#FF0000FF>" + data + " </color>";
    }
    
    internal static string GetColorTextXamCam(object data)
    {
        return "<color=#E9CA74>" + data + " </color>";
    }
    
    
    internal static string GetColorTextGreen(object data)  // xanh tiêu chuẩn
    {
        return "<color=#2c8d06>" + data + " </color>";
    }
    
    internal static string GetColorTextXanhLaCay(object data)  // xanh đậm
    {
        return "<color=#00ec11>" + data + " </color>";
    }
    
    internal static string GetColorTextXanhLaCay2(object data)  // xanh nhạt
    {
        return "<color=#98ed23>" + data + "</color>";
    }
    
    internal static string GetColorTextMoneyRechage(object data)  // xanh nhạt
    {
        return "<color=#8DFF00>" + data + "</color>";
    }
    
    internal static string GetColorTextGray(object data)
    {
        return "<color=#808080FF>" + data + " </color>";
    }
    
    internal static string GetColorTextNau(object data)
    {
        return "<color=#6a4e34>" + data + " </color>";
    }
    // màu vàng cam
    internal static string GetColorTextOrange(object data)
    {
        return "<color=#FAB909FF>" + data + " </color>";
    }
    
    internal static string GetColorTextOrange2(object data)
    {
        return "<color=#FAB909FF>" + data + "</color>";
    }
    
    // màu xanh da trời nhạt
    internal static string GetColorTextBlue(object data)
    {
        return "<color=#008BFFFF>" + data + " </color>";
    }

    public static Color GetColor(int colorId)
    {
        Color color = Color.white;
        switch (colorId)
        {
            case C.COLOR_GREEN:
                return Color.green;
            case C.COLOR_BLUE:
                return Color.blue;
            case C.COLOR_VIOLET:
                return AgentUnity.GetColor(178, 0, 255);
            case C.COLOR_ORANGE:
                return AgentUnity.GetColor(255, 153, 0);
            case C.COLOR_RED:
                return Color.red;
            case C.COLOR_GREY:
                return AgentUnity.GetColor(92, 84, 84, 255);
            case C.COLOR_KIENTRUCMO:
                return AgentUnity.GetColor(155, 155, 155, 255);
        }
        return color;
    }
    internal static Color GetColorDisableButton() { return AgentUnity.GetColor(200F, 200F, 200F, 128F); }

    internal static void DebugLogJson(object data)
    {
        Debug.LogWarning(Newtonsoft.Json.JsonConvert.SerializeObject(data));
    }
    internal static string GetLogJson(object data)
    {
        return Newtonsoft.Json.JsonConvert.SerializeObject(data);
    }
    internal static bool CheckSuccess(Message msg)
    {
        return (msg.GetInt(Key.RESULT) == C.TRUE);
    }
    internal static string GetKeyNew(Message msg)
    {
#if UNITY_EDITOR
        return "Cmd: " + msg.cmd + " - KeyNew: " + msg.GetKeyNew();
#else
        return string.Empty;
#endif
    }
    internal static string GetKeyNew(JObjectCustom j)
    {
#if UNITY_EDITOR
        return "JObjectCustom - KeyNew: " + j.GetKeyNew();
#else
        return string.Empty;
#endif
    }
    internal static string LogMessage(Message msg)
    {
        return msg.GetString(Key.MESSAGE);
    }
    // internal static void InitDrag()
    // {
    //     C.timeStartDrag = Time.time;
    // }
    // internal static bool CheckDragMap()
    // {
    //     float check = Time.time - C.timeStartDrag;
    //     if (check > C.TIME_CLICK)
    //     {
    //         return true;
    //     }
    //     if (check < 0)
    //     {
    //         return true;
    //     }
    //     return false;
    // }
    internal static int[] GetArrayStarTarget() { return new int[] { 5, 10, 20, 30 }; }
    internal static string GetInfoArchitectureId(int infoId, long architectureId)
    {
        return infoId + string.Empty + architectureId;
    }

    internal static void AddButtonToPreventDialogClick(Transform parent)
    {
        Button btn = parent.gameObject.AddComponent<Button>();
        btn.transition = Selectable.Transition.None;
    }
    
    internal static string GetShortNamePlayer(string displayName)
    {
        return displayName.Length > C.MAX_LENGTH_NAME_PLAYER ? displayName.Substring(C.ZERO, C.MAX_LENGTH_NAME_PLAYER) + "..." : displayName;
    }
    
}
