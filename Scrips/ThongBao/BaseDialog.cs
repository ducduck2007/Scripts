using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BaseDialog : MonoBehaviour
{
    [SerializeField] protected Button _btnClose;
    [SerializeField] protected Button _btnExit;
    public GameObject bg;
    public Transform tranScale;
    [SerializeField] protected TextMeshProUGUI txtTitle; 


    private const float animTime = 0.18f;
    protected virtual void Awake()
    {
        if (bg != null)
        {
            AgentUnity.ScaleBg(bg);
        }
        
        if (tranScale != null)
        {
            AgentUnity.ScaleTranform(tranScale);
        }
        
        _btnClose.onClick.AddListener(Close);
        _btnExit.onClick.AddListener(Close);
    }

    protected void Open()
    {
        gameObject.SetActive(true);
        gameObject.transform.SetAsLastSibling();
        // AudioManager.Instance.AudioClick();
    }

    protected void Close()
    {
        // AudioManager.Instance.AudioClick();
        // _tranBg.DOScale(new Vector3(0.3f, 0.3f, 0), animTime).OnComplete(() => _tranBg.GetComponent<Image>().DOColor(Color.clear, animTime).SetEase(_animEaseClose)).SetEase(_animEaseClose).OnComplete(() => { Destroy(gameObject); });
        Destroy(gameObject);
    }

    public void Show(bool val = true)
    {
        gameObject.SetActive(val);
    }
}