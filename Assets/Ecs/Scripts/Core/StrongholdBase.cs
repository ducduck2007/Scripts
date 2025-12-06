using System.Collections.Generic;
using UnityEngine;

public class StrongholdBase
{
    public int id;
    public long userId;
    public int areaId;
    public long type;
    public string name;
    public int avatarId;
    public int level;
    public int chienLuc;

    /// <summary>
    /// -1 là chưa xếp hạng
    /// </summary>
    public int rankingArea;

    /// <summary>
    /// -1 là chưa xếp hạng
    /// </summary>
    public int rankingArena;

    public string[,] positionDefend;
    public int typeTranHinh;
    public int levelTranHinh;
    public int attackAreaToDay;
    public int attackArenaToDay;
    public int attackPhubanToDay;
    public int numBuyAttackPhuBan;
    public int receivedGiftArena;
    public Dictionary<int, bool> mapRankGiftReceivedToDay;
    public int numAttackPhuBan;
    public int attackMineToDay;
    public int numBuySlotArena;
    public int lastRankReceivedGift;
    public int numTurnResetNormalGateToday; //số lần reset đánh phụ bản thường hàng ngày
    public int numTurnResetTinhAnhGateToDay; // số lần reset đánh phụ bản trong ngày đã dùng
    public int numTurnResetTinhAnhGateDaily;
    public int numTurnResetNormalGateDaily; //tương tự 2 biến bên dưới với số lần reset và đã dùng của kịch bản tinh anh
    public long timeThanhChay;
    public List<TranHinhSaveData> listTranHinhSave = new List<TranHinhSaveData>();
    public List<MapVassalId> mapVassalId;// thông tin danh sách chư hầu hiện có
    public Owner owner;  // thông tin chủ của mình
    
    private readonly Dictionary<int, TranHinhSaveData> dicSaveTranHinh = new Dictionary<int, TranHinhSaveData>();

    public void AddTranHinh()
    {
        dicSaveTranHinh.Clear();
        dicSaveTranHinh.Add(typeTranHinh, new TranHinhSaveData
        {
            typeTranHinh = typeTranHinh,
            positionDefend = positionDefend
        });
        foreach (var tranHinhSave in listTranHinhSave)
        {
            if (dicSaveTranHinh.ContainsKey(tranHinhSave.typeTranHinh)) return;
            dicSaveTranHinh.Add(tranHinhSave.typeTranHinh, new TranHinhSaveData
            {
                typeTranHinh = tranHinhSave.typeTranHinh,
                positionDefend = tranHinhSave.positionDefend
            });
        }
    }

    public void UpdateTranHinh(TranHinhSaveData data)
    {
        // AgentCSharp.DebugFullClass(data);
        if (dicSaveTranHinh.ContainsKey(data.typeTranHinh))
            dicSaveTranHinh[data.typeTranHinh].positionDefend = data.positionDefend;
        else
            dicSaveTranHinh.Add(data.typeTranHinh, new TranHinhSaveData
            {
                typeTranHinh = data.typeTranHinh,
                positionDefend = data.positionDefend
            });
    }

    public string[,] GetTranHinhSaveByType(int typeTranHinhSave)
    {
        return dicSaveTranHinh.ContainsKey(typeTranHinhSave) ? dicSaveTranHinh[typeTranHinhSave].positionDefend : null;
    }
}

public class MapVassalId
{
    public int level;
    public long vassalId;
    public string name;
    public bool isSaiKhien; // true: có thể sai khiến, false : không thể sai khiến
    public string guildName;
    public int avatarId;

    public MapVassalId(int level,long vassalId,string name,bool isSaiKhien,string guildName,int avatarId)
    {
        this.level = level;
        this.vassalId = vassalId;
        this.name = name;
        this.isSaiKhien = isSaiKhien;
        this.guildName = guildName;
        this.avatarId = avatarId;
    }
}

public class Owner
{
    public int avatarId;
    public int level;
    public string name;
    public string guildName;
}

public class TranHinhSaveData
{
    public int typeTranHinh;
    public string[,] positionDefend;
}