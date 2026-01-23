using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class NotifyController : ManualSingleton<NotifyController>
{
    [SerializeField] private Text txtTextScroll;
    [SerializeField] private GameObject khungNotify;
    public bool isPlay = false;
    
    private void Start()
    {
        khungNotify.gameObject.SetActive(false);
        isPlay = false;
    }

    public void SetUpdateNotify()
    {
        if (!isPlay)
        {
            SetPlayNotify();
        }
    }

    public void StopNotify()
    {
        if (_iENotify != null)
        {
            StopCoroutine(_iENotify);
        }
        khungNotify.gameObject.SetActive(false);
        B.Instance.ListNotifyData.Clear();
    }

    private void OnEnable()
    {
        SetPlayNotify();
    }

    public void SetPlayNotify()
    {
        if (B.Instance.ListNotifyData.Count > 0)
        {
            if (_iENotify != null)
            {
                StopCoroutine(_iENotify);
                _iENotify = SetNotify();
                StartCoroutine(_iENotify);
            }
            else
            {
                _iENotify = SetNotify();
                StartCoroutine(_iENotify); 
            }
        }
    }
    
    private IEnumerator _iENotify;
    
    private IEnumerator SetNotify()
    {
        isPlay = true;
        B.Instance.ListNotifyData.Sort((t1, t2) => t1.index.CompareTo(t2.index));
        int totalTime = B.Instance.ListNotifyData[0].totalTime;
        
        // if (UserData.Instance.CurrentStepGuide >= C.MAX_STEP_GUIDE)
        {
            khungNotify.gameObject.SetActive(true);
            txtTextScroll.text = B.Instance.ListNotifyData[0].content;
            float w = LayoutUtility.GetPreferredWidth(txtTextScroll.rectTransform);
            txtTextScroll.transform.DOKill();
            StartCoroutine(MoveObject(new Vector3(640 + w/2, 0, 0), new Vector3(-w/2 - 400, -1.1f, 0), B.Instance.ListNotifyData[0].time));
        }

        yield return new WaitForSeconds(totalTime);
        B.Instance.ListNotifyData.Remove(B.Instance.ListNotifyData[0]);
        khungNotify.gameObject.SetActive(false);
        SetPlayNotify();
    }
    
    private IEnumerator MoveObject(Vector3 startPos, Vector3 targetPos, float duration)
    {
        float time = 0;
        float rate = 1 / duration;
        while (time < 1)
        {
            time += rate * Time.deltaTime;
            txtTextScroll.transform.localPosition = Vector3.Lerp(startPos, targetPos, time);
            yield return 0;
        }

        isPlay = false;
    }

    private void OnDisable()
    {
        if (_iENotify != null)
        {
            isPlay = false;
            StopCoroutine(_iENotify);
            if (B.Instance.ListNotifyData.Count > 0)
            {
                B.Instance.ListNotifyData.Remove(B.Instance.ListNotifyData[0]);
            }
            khungNotify.gameObject.SetActive(false);
        }
    }
}
