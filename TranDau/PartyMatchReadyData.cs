using System.Collections.Generic;

[System.Serializable]
public class PartyMatchReadyData
{
    public int soNguoiMoiDoi;
    public int soNguoiHienTai;
    public int soNguoiToiDa;
    public int idCheDoChoi;
    public int loaiBanDo;

    public List<PlayerRoomInfo> doi1;
    public List<PlayerRoomInfo> doi2;
}

[System.Serializable]
public class PlayerRoomInfo
{
    public long idNguoiChoi;
    public string tenHienThi;
    public int level;
    public int idAvatar;
}