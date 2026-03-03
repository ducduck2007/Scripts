using System.Collections.Generic;

public class LoadVaoTranData
{
    public List<PlayerEntry> teamXanh = new List<PlayerEntry>();
    public List<PlayerEntry> teamDo = new List<PlayerEntry>();

    public class PlayerEntry
    {
        public long userId;
        public string displayName;
        public int heroType;

        public PlayerEntry(long userId, string displayName, int heroType)
        {
            this.userId = userId;
            this.displayName = displayName;
            this.heroType = heroType;
        }
    }
}