public class PlayerInfoChatBase
{
    public int avatarId;
    public int level;
    public string displayName;
    public long chienLuc;
    public long rank;
    public string danhHieu;
    public string uyDanh;
    public int vip;
    public string guildName;
    public long userId;
    public long ownerId;
    public Owner owner;

    
    public PlayerInfoChatBase(int avatarId, int level, string displayName, long chienLuc, long rank, int vip, string guildName, long userId, long ownerId = 0, Owner owner  = null)
    {
        this.avatarId = avatarId;
        this.level = level;
        this.displayName = displayName;
        this.chienLuc = chienLuc;
        this.rank = rank;
        this.vip = vip;
        this.guildName = guildName;
        this.userId = userId;
        this.ownerId = ownerId;
        this.owner = owner;
    }
}
