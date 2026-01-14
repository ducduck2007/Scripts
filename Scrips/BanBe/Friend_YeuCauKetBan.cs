using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UIPool;

public class Friend_YeuCauKetBan : MonoBehaviour
{
    public VerticalPoolGroup gridPoolGroup;
    
    public void SetData(bool isLoadFirst)
    {
        FriendDataBase.Instance.ListDataFriendRequest.Sort((t1, t2) => t2.level.CompareTo(t1.level));
        InitPool();
        gridPoolGroup.SetAdapter(AgentUIPool.GetListObject<DataFriend>(FriendDataBase.Instance.ListDataFriendRequest), isLoadFirst);
    }

    private void InitPool()
    {
        gridPoolGroup.HowToUseCellData(delegate(GameObject go, object data)
        {
            ItemYeuCauKetBan item = go.GetComponent<ItemYeuCauKetBan>();
            item.SetInfo((DataFriend) data);
        });
    }

    private void OnEnable()
    {
        SetData(true);
        OnOffDialog.Instance.isOnYeuCauKetBan = true;
    }

    private void OnDisable()
    {
        OnOffDialog.Instance.isOnYeuCauKetBan = false;
    }
}
