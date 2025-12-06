using System.Collections.Generic;
using Entitas;
using UnityEngine;
using UnityEngine.Events;

public abstract class BaseCommandSystem : ReactiveSystem<NetworkEntity>
{
    private readonly Contexts _contexts;
    
    public BaseCommandSystem(Contexts contexts) : base(contexts.network)
    {
        _contexts = contexts;
    }

    protected override ICollector<NetworkEntity> GetTrigger(IContext<NetworkEntity> context)
    {
        return context.CreateCollector(NetworkMatcher.AllOf(NetworkMatcher.Command,
            NetworkMatcher.MessageData));
    }

    protected override bool Filter(NetworkEntity entity)
    {
        return entity.hasCommand && entity.command.value == GetProcessCommand() && entity.hasMessageData;
    }

    protected abstract int GetProcessCommand();
    protected abstract void Execute(Message msg);

    protected override void Execute(List<NetworkEntity> entities)
    {
        foreach (var entity in entities)
        {
            entity.isProcess = true;
            Execute(entity.messageData.value);
        }
    }

    protected void SetThongBao(string message)
    {
        if (message.Length > 2)
        {
            // DialogControl.Instance.ShowMessage(message);
        }
    }

    // protected void PopUpTwoBtn(string message,UnityAction ok = null,UnityAction cancel =null,string title=Res.TITLE_THONG_BAO)
    // {
    //     if (message.Length > 2)
    //     {
    //         SuperDialog.Instance.PopupTwoButton.ShowPopupTwoButton(title,message,ok,cancel);
    //     }
    // }
    protected void SetThongBao(Message msg)
    {
        SetThongBao(msg.GetString(Key.MESSAGE));
    }

    protected string GetLogMessage(Message msg)
    {
        return msg.GetString(Key.MESSAGE);
    }
    
    protected bool CheckSuccess(Message msg)
    {
        return msg.GetBool(Key.RESULT) == true;
    }

    /// <summary>
    /// Cái trường hợp thực hiện thất bại yêu cầu
    /// </summary>
    /// <param name="msg"></param>
    /// <returns></returns>
    protected void CheckFailedResult(Message msg)
    {
        if (msg.GetString(Key.MESSAGE) == "Thất bại ")
        {
            SetFailedFxMessage();
        }
        else
        {
            ShowToast(msg);
        }
    }
    
    protected void CheckFailedResult2(Message msg)
    {
        if (msg.GetString(Key.MESSAGE) == "Thất bại")
        {
            SetFailedFxMessage();
        }
        else
        {
            ShowToast(msg);
        }
    }
    
    protected void ShowToast(Message msg)
    {
        ShowToast(msg.GetString(Key.MESSAGE),105,3.5f, 5f);
    }

    protected void ShowToast(string message,int value = 100,float time = 1.5f,float destroy = 1.5f)
    {
        if (message.Length > 2)
        {
            // SuperDialog.Instance.Toast.ShowToast(message,value,time,destroy);
        }
    }

    protected void ShowPopUpNapTien(Message msg)
    {
        // SuperDialog.Instance.PopupTwoButton.ShowPopupNapTien(msg.GetString(Key.MESSAGE));
    }
    
    // protected void ShowPopUpNapBac(Message msg)
    // {
    //     SuperDialog.Instance.PopupTwoButton.ShowPopupNapBac();
    // }

    protected void ShowLoadWait()
    {
        LoadController.Instance.ShowLoadWait(false);
    }
    
    protected void ShowCheckLoadWait()
    {
        LoadController.Instance.ShowCheckLoadWait(false);
    }
    
    // protected void ReloadInfoPlayer()
    // {
    //     MenuController.Instance.MainMenu.SetInfoPlayer();
    // }

    // protected void ReloadVipData()
    // {
    //     ReloadInfoPlayer();
    //     if(OnOffDialog.Instance.isOnDialogVip) DialogControlPanel.Instance.DialogMMVip.Init();
    //     if(OnOffDialog.Instance.IsOnDialogRecharge) DialogControlPanel.Instance.DialogMMRecharge.ReloadVipData();
    //     if (OnOffDialog.Instance.IsOnPurchasePack) DialogControl.MainMenu.DialogMMPhucLoi.PurchasePack.InitPurchasePackData();
    // }

    // protected void OnWaitCombatGG(CombatDataBase combat, int typeCombat, int type)
    // {
    //     CombatGGData.Instance.typeCombat = typeCombat;
    //     Service.GetCombatController().Combat.OnSetupCombat(combat, typeCombat, type);
    //     Service.GetCombatController().SetBg(combat.typeCombat);
    // }
    
    // protected void OnWaitCombatQuocChien(QuocChienChiTietData data, int typeCombat)
    // {
    //     // MapController.Instance.MapQuocChien.Show(false);
    //     CombatGGData.Instance.typeCombat = typeCombat;
    //     Service.GetCombatController().CombatQuocChien.OnSetupQuocChien(data);
    //     Service.GetCombatController().SetBg(CombatType.QC_QUOC_CHIEN);
    // }
    
    // // protected void PushChatTheGioi(ChatTheGioiData chatObj)
    // // {
    // //     B.Instance.IsChatTheGioiNew = true;
    // //     B.Instance.MapChatTheGioi.Add(chatObj);
    // //
    // //     string chatFind = "";
    // //     if (chatObj.chat.Contains("<size="))
    // //     {
    // //         string chatNew = chatObj.chat.Substring(0, 9);
    // //         chatFind = chatObj.chat.Replace(chatNew, "<size=15>");
    // //     }
    // //
    // //     if (!OnOffDialog.Instance.IsOnMapQuocChien)
    // //     {
    // //         MenuController.Instance.MainMenu.SetChatMain(AgentLV.GetColorTextRed("[Thế giới] ") + chatObj.displayername + " : " + chatFind, TypeChat.THE_GIOI);
    // //         MenuController.Instance.MainMenu.ShowNotifyChat();
    // //     }
    // //     
    // //     if (OnOffDialog.Instance.isOnChatTheGioi)
    // //     {
    // //         ChatControlController.Instance.ChatControl.chatTheGioi.SetData();
    // //     }
    // // }
    
    // protected void PushChatTheGioi(ChatTheGioiData chatObj)
    // {
    //     B.Instance.IsChatTheGioiNew = true;
    //     B.Instance.MapChatTheGioi.Add(chatObj);
        
    //     if (!OnOffDialog.Instance.IsOnMapQuocChien)
    //     {
    //         MenuController.Instance.MainMenu.SetChatMain(AgentLV.GetColorTextRed("[Thế giới] ") + chatObj.displayername + " : " + chatObj.chat, TypeChat.THE_GIOI);
    //         MenuController.Instance.MainMenu.ShowNotifyChat();
    //     }
        
    //     if (OnOffDialog.Instance.isOnChatTheGioi)
    //     {
    //         ChatControlController.Instance.ChatControl.chatTheGioi.SetData();
    //     }
    // }
    
    // protected void PushChatQuocGia(ChatQuocGiaData chatObj)
    // {
    //     B.Instance.IsChatQuocGiaNew = true;
    //     B.Instance.MapChatQuocGia.Add(chatObj);
    //     if (OnOffDialog.Instance.IsOnMapQuocChien)
    //     {
    //         MapController.Instance.MapQuocChien.Menu.SetChatMain(AgentLV.GetColorTextRed("[Quốc gia] ") + chatObj.displayername + " : " + chatObj.chat, TypeChat.QUOC_GIA);
    //         MapController.Instance.MapQuocChien.Menu.ShowNotifyChat();
    //     }

    //     if (OnOffDialog.Instance.isOnChatQuocGia)
    //     {
    //         ChatControlController.Instance.ChatControl.chatQuocGia.SetData();
    //     }
    // }
    // protected void PushChatQuocChien(ChatQuocChienData chatObj)
    // {
    //     B.Instance.IsChatQuocChienNew = true;
    //     B.Instance.MapChatQuocChien.Add(chatObj);
    //     if (OnOffDialog.Instance.IsOnMapQuocChien)
    //     {
    //         MapController.Instance.MapQuocChien.Menu.SetChatMain(AgentLV.GetColorTextRed("[Quốc chiến] ") + chatObj.displayername + " : " + chatObj.chat, TypeChat.QUOC_CHIEN);
    //         MapController.Instance.MapQuocChien.Menu.ShowNotifyChat();
    //     }

    //     if (OnOffDialog.Instance.isOnChatQuocChien)
    //     {
    //         ChatControlController.Instance.ChatControl.chatQuocChien.SetData();
    //     }
    // }
    
    
    // protected void PushChatFriend(ChatFriendBase infoChat)
    // {
    //     UserData.Instance.AddInfoChatFriend(infoChat);
    //     ChatControlController.Instance.ChatControl.chatRiengTu.viewContent.SetInfoChat(UserData.Instance.GetFriend(infoChat.userIdTarget));
    //     ChatControlController.Instance.ChatControl.chatRiengTu.viewContent.verticalPool.ScrollToLast(0.2F);
    //     if (!OnOffDialog.Instance.IsOnMapQuocChien)
    //     {
    //         MenuController.Instance.MainMenu.SetChatMain(AgentLV.GetColorTextOrange("[Bạn bè] ") + AgentLV.GetColorTextOrange(infoChat.displayName) + " : " + infoChat.content, TypeChat.RIENG_TU);
    //     }
    //     MenuController.Instance.MainMenu.ShowNotifyChat();
    //     if(!ChatControlController.Instance.ChatControl.chatRiengTu.gameObject.activeInHierarchy)
    //         B.Instance.SetNotify(ChatControlController.Instance.ChatControl.notifyChat,IdNotify.ChatControl.FRIEND);
    // }
    
    // protected void PushChatHeThong(ChatHeThongData chat)
    // {
    //     B.Instance.IsChatHeThongNew = true;
    //     B.Instance.MapChatHeThong.Add(chat);
    //     if (B.Instance.MapChatHeThong.Count > 100)
    //     {
    //         for (int i = 0; i < B.Instance.MapChatHeThong.Count; i++)
    //         {
    //             if (i < 50)
    //             {
    //                 B.Instance.MapChatHeThong.Remove(B.Instance.MapChatHeThong[i]);
    //             }
    //         }
    //     }

    //     if (!OnOffDialog.Instance.IsOnMapQuocChien)
    //     {
    //         MenuController.Instance.MainMenu.SetChatMain(AgentLV.GetColorTextWhite("[Hệ thống] ") + chat.chat, TypeChat.HE_THONG);
    //         MenuController.Instance.MainMenu.ShowNotifyChat();
            
    //         if (OnOffDialog.Instance.isOnChatHeThong)
    //         {
    //             ChatControlController.Instance.ChatControl.chatHeThong.SetData();
    //         }
    //     }
    //     else
    //     {
    //         MapController.Instance.MapQuocChien.Menu.SetChatMain(AgentLV.GetColorTextWhite("[Hệ thống] ") + chat.chat, TypeChat.HE_THONG);
    //         MapController.Instance.MapQuocChien.Menu.ShowNotifyChat();
            
    //         if (OnOffDialog.Instance.isOnChatHeThong)
    //         {
    //             ChatControlController.Instance.ChatControl.chatHeThong.SetData();
    //         }
    //     }
        
    // }

    protected void ShowPopupTwoButton(string title, string content, UnityAction actionOk = null,
        UnityAction actionCancel = null)
    {
        // SuperDialog.Instance.PopupTwoButton.ShowPopupTwoButton(title,content,actionOk,actionCancel);
    }

    protected void SetSuccesFxMessage(string content)
    {
        // SuperDialog.Instance.ShowSuccessFx(content);
    }

    public void SetFailedFxMessage()
    {
        // SuperDialog.Instance.ShowFailedFx();
    }
}