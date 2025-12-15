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

    public const int PLAYERS_IN_ZONE = 11;
    public const int PLAYER_JOINED = 12;
    public const int PLAYER_LEFT = 13;
    public const int RESPAWN = 14;

    // Movement
    public const int MOVEMENT_INPUT = 20;
    public const int STOP_COMMAND = 22;

    // Combat
    public const int ATTACK = 30;
    public const int DAMAGE_DEALT = 31;
    public const int DEATH = 32;
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
}