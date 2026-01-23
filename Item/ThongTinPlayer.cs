using UnityEngine;

public class ThongTinPlayer
{
    public long idNguoiChoi;
    public string tenHienThi;
    public int level;
    public int avatarId;

    public ThongTinPlayer(long idNguoiChoi, string tenHienThi, int level, int avatarId)
    {
        this.idNguoiChoi = idNguoiChoi;
        this.tenHienThi = tenHienThi;
        this.level = level;
        this.avatarId = avatarId;
    }
}
