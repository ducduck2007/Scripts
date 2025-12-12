using System;
using System.Net.Sockets;
using Entitas;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Quản lý networking
/// </summary>
public class NetworkControler : ManualSingleton<NetworkControler>, IAnyDisconnectListener
{
    private Contexts _contexts;
    private Systems _systems;

    private void Start()
    {
        if (isDuplicate) return;
        DontDestroyOnLoad(gameObject);
        _contexts = Contexts.sharedInstance;
        var listener = _contexts.network.CreateEntity();
        listener.AddAnyDisconnectListener(this);
        _systems = new NetworkSystems(_contexts);
        _systems.Initialize();
    }

    private void Update()
    {
        _systems.Execute();
        _systems.Cleanup();
    }

    private void OnDisable()
    {
        if (isDuplicate) return; 
        OnResetSystems();
    }

    public void OnAnyDisconnect(NetworkEntity entity, bool value)
    {
        if (value) OnDisconnect();
    }

    public void OnResetSystems()
    {
        NetworkContext networkContext = _contexts.network;
        if (networkContext.hasConnectionThread && networkContext.connectionThread.value.IsAlive)
        {
            networkContext.connectionThread.value.Abort();
        }

        if (networkContext.hasPingThread && networkContext.pingThread.value.IsAlive)
        {
            networkContext.pingThread.value.Abort();
        }

        if (networkContext.hasRecieveMessageThread && networkContext.recieveMessageThread.value.IsAlive)
        {
            networkContext.recieveMessageThread.value.Abort();
        }

        Debug.Log("Reset systems");
        _systems.DeactivateReactiveSystems();
        _contexts.Reset();
    }

    private void OnDisconnect()
    {
        B.Instance.isConnectServerSuccess = false;
        NetworkContext networkContext = _contexts.network;
        if (!networkContext.isConnecting) return;
        networkContext.isConnecting = false;
        Debug.Log("Close current socket!");
        if (networkContext.hasTcpClient)
        {
            if (networkContext.hasStream)
                networkContext.stream.value.Close();
            networkContext.tcpClient.value.Close();

            // UserData.Instance.IsCheckKetNoiThoatGame = false;
        }

        DemTimeControl.Instance.StopAllTime();
        // if (!UserData.Instance.IsCheckKetNoiThoatGame)
        // {
        //     SuperDialog.Instance.transform.SetAsLastSibling();

        //     if (!OnOffDialog.Instance.isOnDialogMessage)
        //     {
        //         SuperDialog.Instance.PopupOneButton.ShowPopupDisconect("Mất kết nối đến server!", delegate
        //         {
        //             AgentUnity.LoadScene(MoveScene.MAIN_GAME);
        //             Resources.UnloadUnusedAssets();
        //             AudioManager.Instance.PlayAudioBg();
        //         });
        //     }
        // }
    }

    public void OnDisconnectServer(string content)
    {
        NetworkContext networkContext = _contexts.network;
        if (!networkContext.isConnecting) return;
        networkContext.isConnecting = false;
        // AgentUnity.Log("Close current socket!");
        // if (content.Length < 3)
        //     SuperDialog.Instance.DialogMessage.ShowMessage("Không thể kết nối đến Server này. Bạn vui lòng thử lại sau.");
        // else
        //     SuperDialog.Instance.DialogMessage.ShowMessage(content);
    }

    internal void OnExitGame()
    {
        NetworkContext networkContext = _contexts.network;
        if (!networkContext.isConnecting) return;
        networkContext.isConnecting = false;
        // OnOffDialog.Instance.IsOnMainMenu = false;
        // AgentUnity.Log("Close current socket!");
        if (networkContext.hasTcpClient)
        {
            if (networkContext.hasStream)
                networkContext.stream.value.Close();
            networkContext.tcpClient.value.Close();
        }
    }
}