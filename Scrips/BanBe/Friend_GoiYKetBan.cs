using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using TMPro;
using UIPool;
using UnityEngine;
using UnityEngine.UI;

public class Friend_GoiYKetBan : MonoBehaviour
{
    [SerializeField] public Button btnTimKiem, btnLamMoi;
    [SerializeField] public TextMeshProUGUI txtNhapTen, txtTime;
    public TMP_InputField ifName;
    public VerticalPoolGroup gridPoolGroup;

    public void Start()
    {
        btnLamMoi.onClick.AddListener(SetLamMoi);
        btnTimKiem.onClick.AddListener(SetTimKiem);
    }

    public void SetData(bool isLoadFirst)
    {
        foreach (var dic in FriendDataBase.Instance.ListFindGoiYKetBan)
        {
            if (FriendDataBase.Instance.CheckDaGuiKetbanChua(dic.idNguoiChoi))
                dic.isDaGuiYcKetBan = 1;
            else
                dic.isDaGuiYcKetBan = 0;
        }
        
        FriendDataBase.Instance.ListFindGoiYKetBan.Sort((t1, t2) => t1.isDaGuiYcKetBan.CompareTo(t2.isDaGuiYcKetBan));
        InitPool();
        gridPoolGroup.SetAdapter(AgentUIPool.GetListObject<DataFriend>(FriendDataBase.Instance.ListFindGoiYKetBan), isLoadFirst);
    }

    private void InitPool()
    {
        gridPoolGroup.HowToUseCellData(delegate(GameObject go, object data)
        {
            ItemGoiYKetBan item = go.GetComponent<ItemGoiYKetBan>();
            item.SetInfo((DataFriend) data);
        });
    }

    public void SetFindFiend(bool isLoadFirst)
    {
        ifName.text = String.Empty;
        foreach (var dic in FriendDataBase.Instance.ListDataFriendFind)
        {
            if (FriendDataBase.Instance.CheckDaGuiKetbanChua(dic.idNguoiChoi))
                dic.isDaGuiYcKetBan = 1;
            else
                dic.isDaGuiYcKetBan = 0;
        }
        FriendDataBase.Instance.ListDataFriendFind.Sort((t1, t2) => t1.isDaGuiYcKetBan.CompareTo(t2.isDaGuiYcKetBan));
        
        InitPool();
        gridPoolGroup.SetAdapter(AgentUIPool.GetListObject<DataFriend>(FriendDataBase.Instance.ListDataFriendFind), isLoadFirst);
    }
    
    private void SetLamMoi()
    {
        AudioManager.Instance.AudioClick();
        if (DemTimeControl.Instance.GetTimeGoiYKetBan() <= 0)
        {
            SendData.OnFindGoiYKetBan();
            DemTimeControl.Instance.StartDemTimeListGoiYKetBan(20);
        }
        else
        {
            ThongBaoController.Instance.ShowToast("Chưa đến thời gian làm mới");
        }
    }

    private void SetTimKiem()
    {
        AudioManager.Instance.AudioClick();
        string name = ifName.text;
        if (name.Length >= C.LENGTH_MIN_DISPLAYNAME)
        {
            SendData.OnFindFriend(name);
        }
        else
        {
            ThongBaoController.Instance.PopupOneButton.ShowPopupOneButton("Thông Báo", "Tên nhân vật phải từ " + C.LENGTH_MIN_DISPLAYNAME + " ký tự");
        }
    }
    
    private void OnEnable()
    {
        OnOffDialog.Instance.isOnGoiYKetBan = true;
        FriendDataBase.Instance.isTimKiem = false;
        ifName.text = String.Empty;
        SetData(true);
    }

    private void OnDisable()
    {
        OnOffDialog.Instance.isOnGoiYKetBan = false;
    }
}
