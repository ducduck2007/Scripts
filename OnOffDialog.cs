public class OnOffDialog
{
    protected static OnOffDialog instance;
 
    internal static OnOffDialog Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new OnOffDialog();
            }
 
            return instance;
        }
    }

    
    public bool isOnChatHeThong { get; set; }
    public bool isOnChatTheGioi { get; set; }
    public bool isOnGoiYKetBan = false;
    public bool isOnYeuCauKetBan = false;
    public bool isOnBanBe = false;
    public bool isOnLoiMoiKetBan = false;
    public bool isOnLoadMang = false;
    
}
