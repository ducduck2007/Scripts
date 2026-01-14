using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

public class LoadController : ManualSingleton<LoadController>
{
    private GameObject Load(string namePath)
    {
        return Resources.Load(namePath) as GameObject;
    }

    private GameObject _loadWait;
    public GameObject LoadWait
    {
        get
        {
            if (_loadWait == null)
                _loadWait = AgentUnity.InstanceObject(Load(PathResource.LoadWait), transform);
            return _loadWait;
        }
    }
    
    private GameObject _loadWaitData;
    public GameObject LoadWaitData
    {
        get
        {
            if (_loadWaitData == null)
                _loadWaitData = AgentUnity.InstanceObject(Load(PathResource.LoadWaitData), transform);
            return _loadWaitData;
        }
    }
    
    private LoadPercentChangeInfo _loadPercenChangeInfo;

    public LoadPercentChangeInfo LoadPercentChangeInfo
    {
        get
        {
            if (_loadPercenChangeInfo == null)
                _loadPercenChangeInfo =
                    AgentUnity.InstanceObject<LoadPercentChangeInfo>(Load(PathResource.LoadPercent), transform);
            return _loadPercenChangeInfo;
        }
    }

    public bool isLoad = false;
    public void ShowCheckLoadWait(bool val, int time = 10, float timecheck = 0.2f)
    {
        this.isLoad = val;
        if (val)
        {
            if (_enumLoadWait != null)
            {
                StopCoroutine(_enumLoadWait);
                _enumLoadWait = EnumLoadWait(time, timecheck);
                StartCoroutine(_enumLoadWait);
            }
            else
            {
                _enumLoadWait = EnumLoadWait(time, timecheck);
                StartCoroutine(_enumLoadWait);
            }
        }
        else
        {
            if (_enumLoadWait != null)
            {
                StopCoroutine(_enumLoadWait);
            }

            DestroyLoadWaitData();
        }
    }

    private IEnumerator _enumLoadWait;
    private IEnumerator EnumLoadWait(int time, float timecheck)
    {
        yield return new WaitForSeconds(timecheck);
        if (this.isLoad)
        {
            LoadWaitData.SetActive(true);
            yield return new WaitForSeconds(time);
            DestroyLoadWaitData();
        }
        yield return null;
    }
    
    private void DestroyLoadWaitData()
    {
        if (LoadWaitData.activeInHierarchy)
        {
            Destroy(LoadWaitData.gameObject);
        }
    }
    
    public void ShowLoadWait(bool val = true, float time = 15F)
    {
        LoadWait.SetActive(val);
        if (val)
        {
            StopCoroutine(CheckLoadWait(time));
            StartCoroutine(CheckLoadWait(time));
        }
        else
        {
            StopCoroutine(CheckLoadWait(time));
            Destroy(LoadWait.gameObject);
        }
    }
    
    public void ShowLoadWaitConnectServer(bool val = true, float time = 8F)
    {
        if (val)
        {
            LoadWait.SetActive(true);
            if (_isCheckConnect != null)
            {
                StopCoroutine(_isCheckConnect);
                _isCheckConnect = IECheckConnect(time);
                StartCoroutine(_isCheckConnect);
            }
            else
            {
                _isCheckConnect = IECheckConnect(time);
                StartCoroutine(_isCheckConnect);
            }
        }
        else
        {
            if (_isCheckConnect != null)
            {
                DestroyLoadWait();
                StopCoroutine(_isCheckConnect);
            }
        }
    }

    private IEnumerator _isCheckConnect = null;

    private IEnumerator IECheckConnect(float time)
    {
        yield return new WaitForSeconds(time);
        DestroyLoadWait();
        if (!B.Instance.isConnectServerSuccess)
        {
            NetworkControler.Instance.OnDisconnectServer("");
        }
    }

    private IEnumerator CheckLoadWait(float time)
    {
        yield return new WaitForSeconds(time);
        DestroyLoadWait();
    }
    
    private void DestroyLoadWait()
    {
        if (LoadWait.activeInHierarchy)
        {
            Destroy(LoadWait.gameObject);
        }
    }

    public void DestroyAllChildsToTime(Transform parent, float time = 1f, bool val = true)
    {
        if (val)
        {
            StopCoroutine(Coroutine(parent, time));
            StartCoroutine(Coroutine(parent, time));
        }
        else
            StopCoroutine(Coroutine(parent, time));
    }

    private IEnumerator Coroutine(Transform parent, float time)
    {
        yield return new WaitForSeconds(time);
        foreach (Transform t in parent)
        {
            Object.Destroy(t.gameObject);
        }
    }

    

    public void ShowLoadPercentChangeInfo(bool isShow)
    {
        C.SetBusy(isShow);
        LoadPercentChangeInfo.ShowLoadPercent(isShow);
    }

    
}
