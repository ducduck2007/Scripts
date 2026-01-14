using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ThongBaoController : ManualSingleton<ThongBaoController>
{
    private GameObject Load(string namePath)
    {
        return Resources.Load(namePath) as GameObject;
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CloseLoading();
    }

    private void CloseLoading()
    {
        if (_loadVaoTran != null)
        {
            Destroy(_loadVaoTran.gameObject);
            _loadVaoTran = null; // ❗ bắt buộc
        }
    }

    private LoadVaoTran _loadVaoTran;
    public LoadVaoTran LoadVaoTran
    {
        get
        {
            if (_loadVaoTran == null)
                _loadVaoTran = AgentUnity.InstanceObject<LoadVaoTran>(Load(PathResource.LoadVaoTran), transform);
            _loadVaoTran.transform.SetAsLastSibling();
            return _loadVaoTran;
        }
    }


    private SD_PopupOneButton _PopupOneButton;
    public SD_PopupOneButton PopupOneButton
    {
        get
        {
            if (_PopupOneButton == null)
                _PopupOneButton = AgentUnity.InstanceObject<SD_PopupOneButton>(Load(PathResource.PopupOneButton), transform);
            _PopupOneButton.transform.SetAsLastSibling();
            return _PopupOneButton;
        }
    }


    private SD_PopupTwoButton _PopupTwoButton;
    public SD_PopupTwoButton PopupTwoButton
    {
        get
        {
            if (_PopupTwoButton == null)
                _PopupTwoButton = AgentUnity.InstanceObject<SD_PopupTwoButton>(Load(PathResource.PopupTwoButton), transform);
            _PopupTwoButton.transform.SetAsLastSibling();
            return _PopupTwoButton;
        }
    }
    private SD_Toast _toast;
    internal SD_Toast Toast
    {
        get
        {
            if (_toast == null)
                _toast = AgentUnity.InstanceObject<SD_Toast>(Load(PathResource.SD_Toast), transform);
            return _toast;
        }
    }

    private LoadMang _loadMang;
    internal LoadMang LoadMang
    {
        get
        {
            if (_loadMang == null)
                _loadMang = AgentUnity.InstanceObject<LoadMang>(Load(PathResource.LoadMang), transform);
            return _loadMang;
        }
    }

    public void ShowToast(string content)
    {
        if (content.Length == 0)
            return;

        Toast.ShowToast(content);
    }

    public void MakeToast(string content)
    {
        if (content.Length == 0)
            return;

        Toast.MakeToast(content);
    }
}
