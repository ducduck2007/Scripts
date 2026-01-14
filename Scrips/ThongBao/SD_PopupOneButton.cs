using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SD_PopupOneButton : BaseDialog
{
    //[SerializeField] private TextMeshProUGUI txtTitle;
    public TextMeshProUGUI txtContent, txtDongY;
    public Button btnOk;
    private UnityAction _ClickOk;
    //public Image imgBG;
    private void Start()
    {
        btnOk.onClick.AddListener(ClickOk);
    }

    private void ClickOk()
    {
        // AudioManager.Instance.AudioClick();
        if (_ClickOk != null)
            _ClickOk.Invoke();
        Close();
    }
    public void ShowPopupOneButton(string title, string content, UnityAction actionOk = null)
    {
        Open();
        txtDongY.text = "Đồng ý";
        txtTitle.text = title;
        txtContent.text = content;
        txtContent.fontSize = 32;
        _ClickOk = actionOk;
    }
    
    public void ShowPopupBiVetDanh(string title, string content, string textDongy, UnityAction actionOk = null)
    {
        Open();
        txtDongY.text = textDongy;
        _btnExit.gameObject.SetActive(false);
        _btnClose.interactable = false;
        txtTitle.text = title;
        txtContent.text = content;
        txtContent.fontSize = 32;
        _ClickOk = actionOk;
    }
    
    public void ShowPopupDisconect(string content, UnityAction actionOk = null)
    {
        Open();
        txtDongY.text = "Đồng ý";
        _btnExit.gameObject.SetActive(false);
        btnOk.gameObject.SetActive(true);
        _btnClose.interactable = false;
        txtTitle.text = "Thông Báo";
        txtContent.text = content;
        txtContent.fontSize = 32;
        _ClickOk = actionOk;
    }
    
    public void ShowPopupMang(string content)
    {
        Open();
        _btnExit.gameObject.SetActive(false);
        btnOk.gameObject.SetActive(false);
        _btnClose.interactable = false;
        txtTitle.text = "Thông Báo";
        txtContent.text = content;
        txtContent.fontSize = 32;
    }
    
    public void ShowPopupThongBao(string content, UnityAction actionOk = null)
    {
        Open();
        txtDongY.text = "Đồng ý";
        txtTitle.text = "Thông Báo";
        txtContent.text = content;
        txtContent.fontSize = 32;
        _ClickOk = actionOk;
    }
    public void ShowPopupOneButtonCountDownTime(string title, string content, int timeCountDown, UnityAction actionOk = null)
    {
        Open();
        txtTitle.text = title;
        _ClickOk = actionOk;
        gameObject.SetActive(true);
        if (ieCountDown != null)
        {
            StopCoroutine(ieCountDown);
            ieCountDown = ProcessShowCountDownTime(content, timeCountDown);
            StartCoroutine(ieCountDown);
        }
        else
        {
            ieCountDown = ProcessShowCountDownTime(content, timeCountDown);
            StartCoroutine(ieCountDown);
        }
    }
    private bool _isProcessCountDown;

    private IEnumerator ieCountDown;
     
    private IEnumerator ProcessShowCountDownTime(string content, int timeCountDown)
    {
        Open();
        string countDownMessage;
        _isProcessCountDown = true;
        while (timeCountDown > C.ZERO_LONG)
        {
            countDownMessage = content + "\n\n\n"+timeCountDown + "s";
            txtContent.text = countDownMessage;
            yield return new WaitForSeconds(1);
            timeCountDown -= C.ONE;
            if(timeCountDown == C.ZERO) Destroy(gameObject);
        }
        _isProcessCountDown = false;
        yield return null;
    }
    
    private void OnDisable()
    {
        if (_isProcessCountDown)
        {
            _isProcessCountDown = false;
            StopAllCoroutines();
        }
    }
    
    public void Destroy()
    {
        Destroy(gameObject);
    }
}
