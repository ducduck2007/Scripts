using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoiMoiKetBan : ScaleScreen
{
    [SerializeField] public TextMeshProUGUI txtContent, txtDongY, txtHuy;
    [SerializeField] public Button btnDongY, btnHuy, btnClose;
    private DataFriend _info;

    protected override void Start()
    {
        base.Start();
        btnDongY.onClick.AddListener(SetDongY);
        btnHuy.onClick.AddListener(SetHuy);
        btnClose.onClick.AddListener(() => 
        { 
            AudioManager.Instance.AudioClick();
            Show(false); 
        });
    }

    public void SetInfo(DataFriend data, string content)
    {
        Show();
        _info = data;
        txtContent.text = content;
    }

    private void SetDongY()
    {
        AudioManager.Instance.AudioClick();
        SendData.OnKetBan(_info.idNguoiChoi);
    }

    private void SetHuy()
    {
        AudioManager.Instance.AudioClick();
        SendData.OnTuChoiKetBan(_info.idNguoiChoi);
    }
    
    protected override void OnEnable()
    {
        base.OnEnable();
        OnOffDialog.Instance.isOnLoiMoiKetBan = true;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        OnOffDialog.Instance.isOnLoiMoiKetBan = false;
    }

    public void Show(bool val = true)
    {
        gameObject.SetActive(val);
    }
}
