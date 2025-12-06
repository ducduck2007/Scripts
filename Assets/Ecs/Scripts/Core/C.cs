using System.Collections;
using System.Diagnostics.Contracts;
using UnityEngine;

internal class C
{
    internal const int USERID_THAN_DEN = 1;
    internal const int NULL = -1;
    internal const string NULLStr = "-1";
    internal const int LENGTH_MIN_SHOW_TOAST = 0;
    internal const int LENGTH_MIN_OPEN_LINKS = 3;
    internal const int LENGTH_MAX_AVATAR_GUILD = 15;
    
    
    internal const int TARGET_FRAME = 60;
    internal const int STEP_MAX = 35;

    internal const long TIME_LIFE_TOKEN = 1 * 60 * 60 * 1000;

    internal const int LENGTH_MIN_USERNAME = 6,
        LENGTH_MAX_USERNAME = 20,
        LENGTH_MIN_PASWORD = 8,
        LENGTH_MAX_PASWORD = 20,
        LENGTH_MIN_DISPLAYNAME = 6,
        LENGTH_MAX_DISPLAYNAME = 20;
    
    internal static bool IsFirstLogin;
    internal const int TRUE = 1;
    internal const int FALSE = 0;

    //Type Login
    internal const sbyte L_FACEBOOK = 1,
        L_PLAY_NOW = 2,
        L_GMAIL = 3,
        L_LOGIN = 4;

    internal const int IDNAM = 1;
    internal const int IDNU = 2;

    /// <summary>
    /// 1-equipmentQuanVan, 2-equipmentQuanVo, 3-gem
    /// </summary>
    internal static int callXuongRenTinhFrom = 0;

    internal const int MAX_CHAT_WORLD = 500,
        MAX_CHAT_FRIEND = 80,
        MAX_CHAT_SYSTEM = 300;

    internal static int controlCombatCurrent = 0;
    internal static bool isCombatOtherBoss = false;
    
    internal const int ZERO = 0;
    internal const long ZERO_LONG = 0L;
    internal const int ONE = 1;
    internal const float ONE_FLOAT = 1F;
    internal const long ONE_SECOND_TO_MILISECONDS = 1000L;

    internal const int COLOR_GREEN = 1;
    internal const int COLOR_BLUE = 2;
    internal const int COLOR_VIOLET = 3;
    internal const int COLOR_ORANGE = 4;
    internal const int COLOR_RED = 5;
    internal const int COLOR_GREY = 6; // màu xám
    internal const int COLOR_KIENTRUCMO = 7;
    // Level user limit
    
    private static bool _isBusy;
    internal const int MAX_LENGTH_NAME_PLAYER = 16;
    internal const int IdTiengViet = 2;
    internal const int dataMin = 5;

    /// <summary>
    /// Điều kiện hiện hiệu ứng viền
    /// </summary>
    public static int EQUIP_QUALITY_CONDITION = 4;

    /// <summary>
    /// Level quy định ở đồ giám  
    /// </summary>
    public static int LEVEL_DO_GIAM_REQUIRE = 1;

    // Quality Equip Color
    // 4://Vàng
    public static Color yellow = Color.yellow;

    //5: //Đỏ
    public static Color red = Color.red;

    //6: //Tím 
    public static Color violet =  new Color(0.286f, 0f, 0.659f);

    //7: //Cam
    public static Color orange = new Color(1f, 0.647f, 0);

    //8 : Hồng 
    public static Color pink = new Color(1, 0, 0.7333333f);
    
    public static TextureFormat TEXTURE_FORMAT_DOWNLOADED_IMG = TextureFormat.RGBA32;

    /// <summary>
    /// Tướng dùng trong hướng dẫn
    /// </summary>
    public const int GENERAL_GUIDE_INDEX = 2;
    
    
    internal static void SetBusy(bool val)
    {
        _isBusy = val;
    }

    internal static bool IsBusy
    {
        get { return _isBusy; }
    }

}