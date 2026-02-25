using System.Collections.Generic;

public static class PlayerResourceStateCache
{
    private static readonly object _lock = new object();

    // Snapshot gần nhất nhận từ CMD51 (hp/mana/gold/kda/skill/shield...)
    private static readonly Dictionary<long, PlayerResourceData> _last51 = new Dictionary<long, PlayerResourceData>(16);

    // Data gần nhất nhận từ CMD211 (level/exp/maxHp/maxMana/skillPoints...)
    private static readonly Dictionary<long, LevelUpData> _last211 = new Dictionary<long, LevelUpData>(16);

    public static void PutFrom51(PlayerResourceData d)
    {
        if (d == null || d.userId <= 0) return;
        lock (_lock) _last51[d.userId] = Clone(d);
    }

    public static void PutFrom211(LevelUpData d)
    {
        if (d == null || d.userId <= 0) return;
        lock (_lock) _last211[d.userId] = d;
    }

    // Merge ưu tiên CMD211 cho: level/currentExp/expToNext/maxHp/maxMana
    public static PlayerResourceData MergePrefer211(PlayerResourceData incoming51)
    {
        if (incoming51 == null) return null;

        LevelUpData lu = null;
        lock (_lock)
        {
            _last211.TryGetValue(incoming51.userId, out lu);
        }

        if (lu == null) return incoming51;

        // override chỉ khi CMD211 có giá trị hợp lệ
        if (lu.level > 0) incoming51.level = lu.level;

        // exp có thể = 0 hợp lệ, nên chỉ override nếu expToNext > 0 (coi như gói exp hợp lệ)
        if (lu.expToNextLevel > 0)
        {
            incoming51.currentExp = lu.currentExp;
            incoming51.expToNextLevel = lu.expToNextLevel;
        }

        if (lu.maxHp > 0) incoming51.maxHp = lu.maxHp;
        if (lu.maxMana > 0) incoming51.maxMana = lu.maxMana;

        return incoming51;
    }

    // Dùng khi nhận CMD211: nếu đã có last51 thì build merged đầy đủ để update UI ngay
    public static bool TryBuildMerged(long userId, out PlayerResourceData merged)
    {
        merged = null;
        if (userId <= 0) return false;

        PlayerResourceData base51 = null;
        LevelUpData lu = null;

        lock (_lock)
        {
            _last51.TryGetValue(userId, out base51);
            _last211.TryGetValue(userId, out lu);
        }

        if (base51 == null && lu == null) return false;

        merged = (base51 != null) ? Clone(base51) : new PlayerResourceData { userId = userId };

        if (lu != null)
        {
            if (lu.level > 0) merged.level = lu.level;

            if (lu.expToNextLevel > 0)
            {
                merged.currentExp = lu.currentExp;
                merged.expToNextLevel = lu.expToNextLevel;
            }

            if (lu.maxHp > 0) merged.maxHp = lu.maxHp;
            if (lu.maxMana > 0) merged.maxMana = lu.maxMana;
        }

        return true;
    }

    private static PlayerResourceData Clone(PlayerResourceData s)
    {
        return new PlayerResourceData
        {
            userId = s.userId,

            hp = s.hp,
            maxHp = s.maxHp,

            mana = s.mana,
            maxMana = s.maxMana,

            gold = s.gold,
            level = s.level,

            currentExp = s.currentExp,
            expToNextLevel = s.expToNextLevel,

            skill1Level = s.skill1Level,
            skill2Level = s.skill2Level,
            skill3Level = s.skill3Level,

            shield = s.shield,

            kills = s.kills,
            deaths = s.deaths,
            assists = s.assists,

            exp = s.exp
        };
    }

    public static void ClearAll()
    {
        lock (_lock)
        {
            _last51.Clear();
            _last211.Clear();
        }
    }

}

public class LevelUpData
{
    public long userId;
    public int level;
    public int skillPoints;
    public int currentExp;
    public int expToNextLevel;
    public int maxHp;
    public int maxMana;
}
