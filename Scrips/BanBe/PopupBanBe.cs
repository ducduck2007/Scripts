using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PopupBanBe : ScaleScreen
{
    public Button btnClose;
    [SerializeField] private TextMeshProUGUI txtBanBe, txtGoiYKetBan, txtYeuCauKetBan;
    [SerializeField] private Toggle tgBanBe, tgGoiYKetBan, tgYeuCauKetBan;
    public Friend_Friend banBe;
    public Friend_GoiYKetBan goiYKetBan;
    public Friend_YeuCauKetBan yeuCauKetBan;
    public GameObject notyYc;
    
    protected override void Start()
    {
        base.Start();
        tgBanBe.onValueChanged.AddListener(arg0 =>
        {
            AudioManager.Instance.AudioClick();
            if (tgBanBe.isOn)
            {
                banBe.SetData(true);
            }
        });
        tgGoiYKetBan.onValueChanged.AddListener(arg0 =>
        {
            AudioManager.Instance.AudioClick();
            if (tgGoiYKetBan.isOn)
            {
                if (DemTimeControl.Instance.GetTimeGoiYKetBan() <= 0)
                {
                    SendData.OnFindGoiYKetBan();
                    DemTimeControl.Instance.StartDemTimeListGoiYKetBan(20);
                }
                else
                {
                    goiYKetBan.SetData(true);
                }
            }
        });
        tgYeuCauKetBan.onValueChanged.AddListener(arg0 =>
        {
            AudioManager.Instance.AudioClick();
            if (tgYeuCauKetBan.isOn)
            {
                
            }
        });
        btnClose.onClick.AddListener(() => 
        { 
            AudioManager.Instance.AudioClick();
            Show(false); 
        });
    }

    public void ShowBanBe()
    {
        Show();
        tgBanBe.isOn = true;
        banBe.gameObject.SetActive(true);
        goiYKetBan.gameObject.SetActive(false);
        yeuCauKetBan.gameObject.SetActive(false);
        banBe.SetData(true);
    }

    public void ShowYeuCauKetBan()
    {
        Show();
        tgYeuCauKetBan.isOn = true;
        banBe.gameObject.SetActive(false);
        goiYKetBan.gameObject.SetActive(false);
        yeuCauKetBan.gameObject.SetActive(true);
        yeuCauKetBan.SetData(true);
    }

    public void ShowTimBan()
    {
        SendData.OnFindGoiYKetBan();
        Show();
        tgGoiYKetBan.isOn = true;
        banBe.gameObject.SetActive(false);
        goiYKetBan.gameObject.SetActive(true);
        yeuCauKetBan.gameObject.SetActive(false);
        goiYKetBan.SetData(true);
    }
    
    public void CheckNotyBb()
    {
        notyYc.gameObject.SetActive(FriendDataBase.Instance.ListDataFriendRequest.Count > 0);
    }
    
    protected override void OnEnable()
    {
        base.OnEnable();
        // OnOffDialog.Instance.isOnDialogBanBe = true;
        // txtBanBe.text = B.Instance.GetText(IdLanguage.BanBe);
        // txtGoiYKetBan.text = B.Instance.GetText(IdLanguage.GoiYKetBan);
        // txtYeuCauKetBan.text = B.Instance.GetText(IdLanguage.YeuCauKetBan);
        // txtDanhSachDen.text = B.Instance.GetText(IdLanguage.Danhsachden);
        //txtTitle.text = B.Instance.GetText(IdLanguage.TITLE_BANBE);
        CheckNotyBb();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        // OnOffDialog.Instance.isOnDialogBanBe = false;
    }

    public void Show(bool val = true)
    {
        gameObject.SetActive(val);
    }
}
