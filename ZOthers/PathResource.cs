using System;
using UnityEngine;
using UnityEngine.UI;

public class PathResource
{
    private static Sprite GetSprite(string path)
    {
        return Resources.Load<Sprite>(path);
    }

    private static GameObject GetGameObj(string path)
    {
        return Resources.Load<GameObject>(path);
    }


    #region Login

    public const string LoginController = "Login/LoginController";

    #endregion


    public const string PopupOneButton = "ThongBao/PopupOneButton";
    public const string PopupTwoButton = "ThongBao/PopupTwoButton";
    public const string LoadMang = "ThongBao/LoadMang";
    public const string SD_Toast = "ThongBao/Toast";
    public const string MainGame = "MainGame/MainGame";
    public const string MainGame1 = "MainGame/MainGame1";
    public const string DialogChonPhong = "MainGame/ChonPhong";
    public const string PopupTimTran = "MainGame/TimTran";
    public const string ChonTuong = "MainGame/ChonTuong";

    public const string RutThuong = "Money/RutThuong";
    public const string NapTien = "Money/NapTien";

    public const string LoadWait = "Load/LoadWait";
    public const string LoadWaitData = "Load/LoadWait";
    public const string LoadPercent = "Load/LoadPercentChangeInfo";
    // public const string LoadVaoTran = "Load/LoadVaoTran";
    public const string LoadVaoTran = "Load/LoadVaoTran2";

    public const string DialogBanBe = "BanBe/PopupBanBe";
    public const string LoiMoiKetBan = "BanBe/LoiMoiKetBan";
    public const string DialogChat = "Chat/Chat";
    public const string DialogHomThu = "HomThu/DialogHomThu";
    public const string DialogTuong = "Tuong/DialogTuong";
    public const string DialogChiTietTuong = "Tuong/DialogChiTietTuong";
    public const string DialogSuKien = "Tuong/DialogSuKien";
    public const string DialogNhiemVu = "Tuong/DialogNhiemVu";
    public const string DialogTrangBi = "Item/DialogTrangBi";

    public const string LoiMoiVaoParty = "Party/LoiMoiVaoParty";
    public const string PopupPartyMatchFound = "Party/PopupPartyMatchFound";
    public const string PopupGhepTran = "MainGame/GhepTran";

}