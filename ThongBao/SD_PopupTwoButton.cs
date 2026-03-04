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

    private IEnumerator ieCountDown;
    private bool _isProcessCountDown;

    protected override void Awake()
    {
        base.Awake();

        if (btnOk != null)
            btnOk.onClick.AddListener(ClickOk);

        if (btnHuyBo != null)
            btnHuyBo.onClick.AddListener(ClickExit);
    }

    private void ClickOk()
    {
        _ClickOk?.Invoke();
        Close();
    }

    private void ClickExit()
    {
        _ClickExit?.Invoke();
        Close();
    }

    public void ShowPopupTwoButton(
        string title,
        string content,
        UnityAction actionOk = null,
        UnityAction actionExit = null,
        string tDongY = "Đồng ý")
    {
        ResetState();

        Open();
        txtDongY.text = tDongY;
        txtTitle.text = title;
        txtContent.text = content;
        txtContent.fontSize = 32;

        _ClickOk = actionOk;
        _ClickExit = actionExit;
    }

    public void ShowPopupTwoButton(
        string title,
        string content,
        string txtBtnExit,
        UnityAction actionOk = null,
        UnityAction actionExit = null)
    {
        ResetState();

        Open();
        txtDongY.text = "Đồng ý";
        txtHuyBo.text = txtBtnExit;
        txtTitle.text = title;
        txtContent.text = content;
        txtContent.fontSize = 32;

        _ClickOk = actionOk;
        _ClickExit = actionExit;
    }

    public void ShowPopupTwoButtonCountDownTime(
        string title,
        string content,
        int timeCountDown,
        UnityAction actionOk = null,
        UnityAction actionExit = null)
    {
        ResetState();

        Open();
        txtTitle.text = title;
        txtHuyBo.text = "Tắt mời chơi";

        _ClickOk = actionOk;
        _ClickExit = actionExit;

        ieCountDown = ProcessShowCountDownTime(content, timeCountDown);
        StartCoroutine(ieCountDown);
    }

    private IEnumerator ProcessShowCountDownTime(string content, int timeCountDown)
    {
        _isProcessCountDown = true;

        while (timeCountDown > 0)
        {
            txtContent.text = content + "\n\n\n" + timeCountDown + "s";
            yield return new WaitForSeconds(1);
            timeCountDown--;
        }

        _isProcessCountDown = false;
        ieCountDown = null;

        Close(); // KHÔNG Destroy
    }

    private void ResetState()
    {
        if (ieCountDown != null)
        {
            StopCoroutine(ieCountDown);
            ieCountDown = null;
        }

        _isProcessCountDown = false;
        _ClickOk = null;
        _ClickExit = null;
    }

    private void OnDisable()
    {
        ResetState();
    }

    public void SetContentText(string txt)
    {
        txtContent.text = txt;
    }
}