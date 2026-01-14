using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public class SD_PopupTwoButton : BaseDialog
{
    public TextMeshProUGUI txtContent, txtDongY, txtHuyBo;
    public Button btnOk, btnHuyBo;
    private UnityAction _ClickOk;
    private UnityAction _ClickExit;
    //public Image imgBG;
    private void Start()
    {
        btnOk.onClick.AddListener(ClickOk);
        btnHuyBo.onClick.AddListener(ClickExit);
    }

    private void ClickOk()
    {
        // AudioManager.Instance.AudioClick();
        if (_ClickOk != null)
            _ClickOk.Invoke();
        Close();
    }
    
    private void ClickExit()
    {
        // AudioManager.Instance.AudioClick();
        if (_ClickExit != null)
            _ClickExit.Invoke();
        Close();
    }
    
    public void ShowPopupTwoButton(string title, string content, UnityAction actionOk = null, UnityAction actionExit = null, string tDongY = "Đồng ý")
    {
        Open();
        txtDongY.text = tDongY;
        txtTitle.text = title;
        txtContent.text = content;
        txtContent.fontSize = 32;
        _ClickOk = actionOk;
        _ClickExit = actionExit;
    }
    
    public void ShowPopupTwoButton(string title, string content, string txtBtnExit, UnityAction actionOk = null, UnityAction actionExit = null)
    {
        Open();
        txtDongY.text = "Đồng ý";
        txtTitle.text = title;
        txtContent.text = content;
        txtContent.fontSize = 32;
        txtHuyBo.text = txtBtnExit;
        _ClickOk = actionOk;
        _ClickExit = actionExit;
    }
    
    public void ShowPopupTwoButtonCountDownTime(string title, string content, int timeCountDown, UnityAction actionOk = null, UnityAction actionExit = null)
    {
        Open();
        txtTitle.text = title;
        txtHuyBo.text = "Tắt mời chơi";
        _ClickOk = actionOk;
        _ClickExit = actionExit;
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
        countDownMessage = content + "\n\n\n"+timeCountDown + "s";
        txtContent.text = countDownMessage;
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

    public void SetContentText(string txt)
    {
        txtContent.text = txt;
    }
    public void Destroy()
    {
        Destroy(gameObject);
    }
}
