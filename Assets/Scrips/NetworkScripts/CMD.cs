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
	
}