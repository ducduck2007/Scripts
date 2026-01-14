using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UIPool;
using UnityEngine;

public class Friend_Friend : MonoBehaviour
{
    public VerticalPoolGroup gridPoolGroup;
    [SerializeField] public TextMeshProUGUI txtSoBan;
    
    public void SetData(bool isLoadFirst)
    {
        txtSoBan.text = "Bạn bè: " + FriendDataBase.Instance.ListDataFriend.Count;
        FriendDataBase.Instance.ListDataFriend.Sort((t1, t2) => t2.level.CompareTo(t1.level));
        InitPool();
        gridPoolGroup.SetAdapter(AgentUIPool.GetListObject<DataFriend>(FriendDataBase.Instance.ListDataFriend), isLoadFirst);
    }

    private void InitPool()
    {
        gridPoolGroup.HowToUseCellData(delegate(GameObject go, object data)
        {
            ItemBanBe item = go.GetComponent<ItemBanBe>();
            item.SetInfo((DataFriend) data);
        });
    }
    
    
    private void OnEnable()
    {
        SetData(true);
        OnOffDialog.Instance.isOnBanBe = true;
    }

    private void OnDisable()
    {
        OnOffDialog.Instance.isOnBanBe = false;
    }
}
