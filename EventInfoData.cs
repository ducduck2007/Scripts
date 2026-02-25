using System;
using System.Collections.Generic;

[Serializable]
public class EventRewardData
{
    public int loaiQua;   // 1 vàng, 2 kim cương, ...
    public int qua;       // id / value
    public int soLuong;   // số lượng
}

[Serializable]
public class EventMocData
{
    public int idMoc;
    public string tenMoc;
    public int thuTu;
    public int giaTri;
    public int loaiYeuCau;   // 1 đăng nhập, 2 kill, 3 mời, 4 share
    public int soLuongQua;

    public List<EventRewardData> dsQua = new List<EventRewardData>();
}

[Serializable]
public class EventInfoData
{
    public int idEvent;
    public string eventCode;
    public string nameEvent;
    public string mota;
    public string bannerUrl;
    public string iconUrl;

    public int eventCategory;
    public int typeEvent;
    public int typeReset;
    public int uuTien;
    public int soMoc;

    public DateTime timeStart;
    public DateTime timeEnd;
    public string timeReset;

    public List<EventMocData> dsMoc = new List<EventMocData>();
}
