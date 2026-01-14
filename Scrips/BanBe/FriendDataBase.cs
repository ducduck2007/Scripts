using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

internal class FriendDataBase
{
    protected static FriendDataBase instance;

    internal static FriendDataBase Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new FriendDataBase();
            }

            return instance;
        }
    }

    public List<DataFriend> ListDataFriend = new List<DataFriend>();
    public List<DataFriend> ListDataFriendFind = new List<DataFriend>();
    public List<DataFriend> ListDataFriendRequest = new List<DataFriend>();
    public List<DataFriend> ListDataFriendSendRequest = new List<DataFriend>();
    public List<DataFriend> ListDataBlackList = new List<DataFriend>();
    public List<DataFriend> ListFindGoiYKetBan = new List<DataFriend>();

    public bool isTimKiem = false;
    

    public void UpdateOnlineFriend(long userId, bool isOn)
    {
        foreach (var dic in ListDataFriend)
        {
            if (dic.idNguoiChoi == userId)
            {
                dic.isOnline = isOn;
            }
        }

        if (OnOffDialog.Instance.isOnBanBe)
        {
            DialogController.Instance.DialogBanBe.banBe.SetData(false);
        }
    }

    public string GetNameFriend(long userId)
    {
        foreach (var dic in ListDataFriend)
        {
            if (dic.idNguoiChoi == userId)
            {
                return dic.tenHienThi;
            }
        }

        return "";
    }
    
    public void RemoveInListFriend(long userId, List<DataFriend> list)
    {
        DataFriend dataRemove = GetDataFriend(userId, list);
        if (dataRemove != null)
        {
            list.Remove(dataRemove);
        }
    }

    public DataFriend GetDataFriend(long userId, List<DataFriend> list)
    {
        foreach (var dic in list)
        {
            if (dic.idNguoiChoi == userId)
            {
                return dic;
            }
        }

        return null;
    }

    public void AddFriend(DataFriend data)
    {
        bool isAdd = true;
        foreach (var dic in ListDataFriend)
        {
            if (dic.idNguoiChoi == data.idNguoiChoi)
            {
                // đã có dữ liệu
                isAdd = false;
            }
        }

        if (isAdd)
            ListDataFriend.Add(data);
    }
    
    public bool CheckNhanLoiMoi(long userId)
    {
        foreach (var dic in ListDataFriendRequest)
        {
            if (dic.idNguoiChoi == userId)
            {
                return true;
            }
        }

        return false;
    }
    public bool CheckDaGuiKetbanChua(long userId)
    {
        foreach (var dic in ListDataFriendSendRequest)
        {
            if (dic.idNguoiChoi == userId)
            {
                return true;
            }
        }

        return false;
    }
    public bool CheckIsBanBe(long userId)
    {
        if (ListDataFriend.Count == 0)
            return false;
        
        foreach (var dic in ListDataFriend)
        {
            if (dic.idNguoiChoi == userId)
            {
                return true;
            }
        }

        return false;
    }
    
    public bool CheckIsBlackBanBe(long userId)
    {
        foreach (var dic in ListDataBlackList)
        {
            if (dic.idNguoiChoi == userId)
            {
                return true;
            }
        }

        return false;
    }
    
    
    internal int GetViTriOnFriend(long usedId)
    {
        for (int i = 0; i < ListDataFriend.Count; i++)
        {
            if (ListDataFriend[i].idNguoiChoi == usedId)
                return i;
        }

        return 0;
    }

    public void ClearData()
    {
        ListDataFriend.Clear();
        ListDataFriendFind.Clear();
        ListDataFriendRequest.Clear();
        ListDataFriendSendRequest.Clear();
        ListDataBlackList.Clear();
        ListFindGoiYKetBan.Clear();
    }
}
