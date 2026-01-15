public class CMD
{
    // 1- Happy Farm, 3-Fanpage, 5-Babylon
#if UNITY_ANDROID
    public const int PROVIDER_ID = 1;
    public const int TYPE_FLATFORM = TypePlatform.ANDROID;
#elif UNITY_IOS
	// 2- HappyFarm, 4-Testfly, 6-Game Off
    public const int PROVIDER_ID = 6;
    public const int TYPE_FLATFORM = TypePlatform.IOS;
#else
    public const int PROVIDER_ID = 1;
    public const int TYPE_FLATFORM = TypePlatform.PC;
#endif
    public const string GAME_ID = "03";
    public const int LOGIN_GAME = 0; // Login game
    public const int RESPAWN = 14;

    // Movement
    public const int MOVEMENT_INPUT = 20;
    public const int STOP_COMMAND = 22;

    // Combat
    public const int ATTACK = 30;
    public const int DAMAGE_DEALT = 31;
    public const int DEATH = 32;
    public const int TRU_BAN_MINH = 34;
    public const int TRU_BAN_LINH = 35;
    public const int PUT_TRU_BAN = 36;

    // Matchmaking
    public const int FIND_MATCH = 40;
    public const int MATCH_FOUND = 41;
    public const int SELECT_HERO = 42;
    public const int HERO_SELECTED = 43;
    public const int GAME_START = 44;
    public const int GAME_END = 45;
    public const int CANCEL_FIND_MATCH = 46;

    // Game
    public const int GAME_SNAPSHOT = 50;

    // ds tướng
    public const int GET_DANH_SACH_LOAI_TUONG = 60;

    // Bạn bè
    public const int DATA_FRIEND = 81; // danh sách bạn bè
    public const int GUI_YEU_CAU_KET_BAN = 82; // Gửi yêu cầu kết bạn
    public const int PUT_LOI_MOI_KET_BAN = 83; // Dữ liệu người chơi mời kết bạn
    public const int DONG_Y_KET_BAN = 84; // Kết bạn
    public const int TU_CHOI_KET_BAN = 85; // Từ chối lời mời kết bạn
    public const int DELETE_FRIEND = 86; // Xóa bạn bè
    public const int PUT_TU_CHOI_KET_BAN = 87; // Dữ liệu người chơi từ chối kết bạn với bạn
    public const int FIND_GOI_Y_KET_BAN = 109; // Danh sách gợi ý kết bạn
    public const int FIND_FRIEND = 111; // Tìm kiếm thông tin bạn bè

    // Ghép trận
    public const int TIM_TRAN = 101; // Danh sách gợi ý kết bạn


    public const int NOTIFY_DATA_GAME = 112; // Dữ liệu thông báo trong game
    public const int PUT_NOTIFY_GAME = 113; // Dữ liệu thông báo thêm
    public const int SEND_START_GAME = 999;

    // chat

    public const int CHAT_THE_GIOI = 2001; // Chat thế giới
    public const int CHAT_FRIEND = 2111; // chat bạn bè
    public const int READ_CHAT_FRIEND = 2112; // đọc chát bạn bè
    public const int GET_DATA_CHAT_FRIEND = 2133;  // dữ liệu chat giữa bạn bè
}