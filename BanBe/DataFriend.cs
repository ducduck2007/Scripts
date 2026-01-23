using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataFriend
{
    public long idNguoiChoi;   // userId người chơi
    public int level;   // cấp độ người chơi
    public string tenHienThi;   // tên người chơi
    public bool isOnline;   // trực tuyến hay không? 1- online, 0-off
    public int idAvatar;   // thời gian logout gần nhất

    public int isDaGuiYcKetBan = 0;

    public DataFriend() { }
    public DataFriend(long idNguoiChoi, int level, string tenHienThi, bool isOnline, int idAvatar)
    {
        this.idNguoiChoi = idNguoiChoi;
        this.level = level;
        this.tenHienThi = tenHienThi;
        this.isOnline = isOnline;
        this.idAvatar = idAvatar;
    }

    public DataFriend(long idNguoiChoi, int level, string tenHienThi, bool isOnline, int idAvatar, int isDaGuiYcKetBan)
    {
        this.idNguoiChoi = idNguoiChoi;
        this.level = level;
        this.tenHienThi = tenHienThi;
        this.isOnline = isOnline;
        this.idAvatar = idAvatar;
        this.isDaGuiYcKetBan = isDaGuiYcKetBan;
    }
}
