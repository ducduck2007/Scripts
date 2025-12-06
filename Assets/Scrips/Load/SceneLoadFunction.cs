using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SceneLoadFunction : ManualSingleton<SceneLoadFunction>
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
            {
                _loadWait = AgentUnity.InstanceObject(Load(PathResource.LoadWait), transform);
                float scale = (float)Screen.width / 1952;
                if ((float)Screen.height/900 > (float)Screen.width/1952)
                {
                    scale = (float)Screen.height / 900;
                }
                AgentUnity.ScaleBg(_loadWait);
            }
            return _loadWait;
        }
    }
    
    private GameObject _loadWaitNoDelay;
    public GameObject LoadWaitNoDelay
    {
        get
        {
            if (_loadWaitNoDelay == null)
            {
                _loadWaitNoDelay = AgentUnity.InstanceObject(Load(PathResource.LoadWait), transform);
                float scale = (float)Screen.width / 1952;
                if ((float)Screen.height/900 > (float)Screen.width/1952)
                {
                    scale = (float)Screen.height / 900;
                }
                AgentUnity.ScaleBg(_loadWaitNoDelay);
            }
            return _loadWaitNoDelay;
        }
    }

    
    
    
    
    // private LoadMang _loadMang;
    // public LoadMang LoadMang
    // {
    //     get
    //     {
    //         if (_loadMang == null)
    //         {
    //             _loadMang = AgentUnity.InstanceObject<LoadMang>(Load(PathResource.LoadMang), transform);
    //         }
    //         return _loadMang;
    //     }
    // }

    // private LoadPercentChangeInfo _loadPercenChangeInfo;

    // public LoadPercentChangeInfo LoadPercentChangeInfo
    // {
    //     get
    //     {
    //         if (_loadPercenChangeInfo == null)
    //         {
    //             _loadPercenChangeInfo = AgentUnity.InstanceObject<LoadPercentChangeInfo>(Load(PathResource.LoadPercent), transform);
    //         }
    //         return _loadPercenChangeInfo;
    //     }
    // }
    

    public void ShowLoadWait(bool val = true, int time = 10, float timecheck = 1f)
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
    public bool isLoad = false;

    private IEnumerator _enumLoadWait;
    private IEnumerator EnumLoadWait(int time, float timecheck)
    {
        yield return new WaitForSeconds(timecheck);
        if (this.isLoad)
        {
            LoadWait.SetActive(true);
            yield return new WaitForSeconds(time);
            DestroyLoadWaitData();
        }

        // if (OnOffDialog.Instance.IsOnServerLogin)
        {
            // không thể đăng nhập tới Server
            ShowThongBaoNoConnectServer();
        }

        yield return null;
    }
    
    private void ShowThongBaoNoConnectServer()
    {
        // Debug.Log("Show Thong Bao No Connect Server");
        string message = "Không thể kết nối tới Server";
        // SuperDialog.Instance.PopupOneButton.ShowPopupDisconect(message, () =>
        // {
        //     NetworkControler.Instance.OnExitGame();
        //     AgentUnity.LoadScene(MoveScene.MAIN_GAME);
        // });
    }
    
    private void DestroyLoadWaitData()
    {
        if (LoadWait.activeInHierarchy)
        {
            Destroy(LoadWait.gameObject);
        }
    }

    public void DestroyAllChildsToTime(Transform parent, float time = 6f, bool val = true)
    {
        if (val)
        {
            StopCoroutine(coroutine(parent, time));
            StartCoroutine(coroutine(parent, time));
        }
        else
            StopCoroutine(coroutine(parent, time));
    }

    private IEnumerator coroutine(Transform parent, float time)
    {
        yield return new WaitForSeconds(time);
        foreach (Transform t in parent)
        {
            Object.Destroy(t.gameObject);
        }
    }


    public void ShowLoadWaitNoDelay(bool val = true, float time = 8F)
    {
        if (val)
        {
            LoadWaitNoDelay.SetActive(true);
            if (_isCheckLoadNoDelay != null)
            {
                StopCoroutine(_isCheckLoadNoDelay);
                _isCheckLoadNoDelay = IECheckLoadNoDelay(time);
                StartCoroutine(_isCheckLoadNoDelay);
            }
            else
            {
                _isCheckLoadNoDelay = IECheckLoadNoDelay(time);
                StartCoroutine(_isCheckLoadNoDelay);
            }
        }
        else
        {
            if (_isCheckLoadNoDelay != null)
            {
                StopCoroutine(_isCheckLoadNoDelay);
                DestroyLoadWaitNoDelay();
            }
            LoadWaitNoDelay.SetActive(false);
        }
    }
    
    private IEnumerator _isCheckLoadNoDelay = null;

    private IEnumerator IECheckLoadNoDelay(float time)
    {
        yield return new WaitForSeconds(time);
        DestroyLoadWaitNoDelay();
    }
    
    public void ShowLoadWaitConnectServer(bool val = true, float time = 8F)
    {
        if (val)
        {
            LoadWaitNoDelay.SetActive(true);
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
                DestroyLoadWaitNoDelay();
                StopCoroutine(_isCheckConnect);
            }
            LoadWaitNoDelay.SetActive(false);
        }
    }

    private IEnumerator _isCheckConnect = null;

    private IEnumerator IECheckConnect(float time)
    {
        yield return new WaitForSeconds(time);
        DestroyLoadWaitNoDelay();
        if (!B.Instance.isConnectServerSuccess)
        {
            NetworkControler.Instance.OnDisconnectServer("");
        }
    }
    
    private void DestroyLoadWait()
    {
        if (LoadWait.activeInHierarchy)
        {
            Destroy(LoadWait.gameObject);
        }
    }
    
    private void DestroyLoadWaitNoDelay()
    {
        if (LoadWaitNoDelay.activeInHierarchy)
        {
            Destroy(LoadWaitNoDelay.gameObject);
        }
    }

}