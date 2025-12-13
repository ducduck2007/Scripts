using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

// Chứa dữ liệu dùng chung trong quá trình chơi (chỉ lấy)
internal class B
{
    protected static B instance;

    internal static B Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new B();
            }

            return instance;
        }
    }
    
    internal string UserName { get; set; }
    internal string PassWord { get; set; }
    internal long UserIdCong { get; set; }
    internal string Keyhash { get; set; }
    internal string ServerIdSelect { get; set; }
    internal long UserIdFriend { get; set; }

    public bool accountNew { get; set; }
    public bool LoadBundle = false;
    public bool loadDataNew = false;
    public bool IsAutomaticallyLoginGame = false;
    public bool IsGameReal = false;
    public bool IsThongBaoMang = true;
    public int DemMangYeu = 0;
    public bool isConnectServerSuccess = false;
    public bool LoadDataInfo = false;
    public bool isAndroidNew = true;
    public bool InGame = false;
    
    internal string linkBannerGame = "";
    internal string nameLogin = "";
    internal string loginLinkImgBanner = "";
    internal string loginThongBao = "";
    internal string loginLinkWebGame = "";
    internal string loginLinkFanpage = "";
    internal string loginLinkGroup = "";
    internal string loginHotline = "";
    internal string loginEmail = "";
    internal string linkEvent = "";
    internal string loginBannerLink = "";
    internal string linkUpdateThongTinCaNhan = "";
    internal string linkDoiMatKhau = "";
    internal string loginLinkVote5Sao = "";
    internal string linkFanpafe = "";
    internal string linkYoutube = "";
    internal string linkVote = "";
    internal string linkMangXh = "";
    internal int ShowIcon18 = 1;
    internal int posCloudSelect;
    internal int posSelect;

    public string linkPing = "";
    
    public string linkSelectNap = "";

    public float PosX = 0;
    public float PosZ = 0;
    public int teamId = 0;
}