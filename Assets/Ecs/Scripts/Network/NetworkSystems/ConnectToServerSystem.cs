using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using Entitas;
using UnityEngine;

public class ConnectToServerSystem : ReactiveSystem<NetworkEntity>
{
    private NetworkContext _networkContext;
    private NetworkEntity _entityMessage;
    
    public ConnectToServerSystem(Contexts contexts) : base(contexts.network)
    {
        _networkContext = contexts.network;
    }

    protected override ICollector<NetworkEntity> GetTrigger(IContext<NetworkEntity> context)
    {
        return context.CreateCollector(NetworkMatcher.MessageData);
    }

    protected override bool Filter(NetworkEntity entity)
    {
        return !_networkContext.isConnecting && entity.hasMessageData;
    }

    protected override void Execute(List<NetworkEntity> entities)
    {
        Debug.Log("Execute");
        _entityMessage = entities.SingleEntity();
        if (_networkContext.hasConnectionThread && _networkContext.connectionThread.value.IsAlive) return;
        _networkContext.ReplaceTcpClient(new TcpClient());
        _networkContext.ReplaceConnectionThread(new Thread(RunConnect));
        _networkContext.connectionThread.value.Start();
    }
    
    public void RunConnect()
    {
        try
        {
            Debug.Log("RunConnect IP = " + Res.IP + ", PORT = " + Res.PORT);
            TcpClient client = _networkContext.tcpClient.value;
            client.LingerState = new LingerOption(true, 2);
            client.ReceiveBufferSize = 512000;
//            client.ReceiveBufferSize = 999000000;
            client.SendBufferSize = 512000;
            client.NoDelay = true;
            //          IP Server test
            // Res.IP = "125.212.229.92";
            // Res.PORT = 4468;
            client.Connect(Res.IP, Res.PORT);
            Connect();
            if (_networkContext.isConnecting)
            {
                Debug.Log("Connected");
                DispathcerUtility.Invoke(() =>
                {
                    _networkContext.ReplaceStream(client.GetStream());
                    _networkContext.stream.value.WriteTimeout = 2;
                    
                    _networkContext.ReplaceRecieveMessageThread(new Thread(ReceiveMessageThread));
                    _networkContext.recieveMessageThread.value.Start();
                    
                    _networkContext.ReplacePingThread(new Thread(PingThread));
                    _networkContext.pingThread.value.Start();

                    if (!_entityMessage.hasMessageData) return;
                    SendData.SendMessage(_entityMessage.messageData.value);
                    _entityMessage.Destroy();
                });
            }
            else 
            {
                Debug.LogError("Connect server failure");
                DispathcerUtility.Invoke(() => _networkContext.ReplaceDisconnect(true));
            }
        }
        catch (Exception e)
        {
            // LoginAccount.IsNotConnectServer = true;
            // DispathcerUtility.Invoke(() => _networkContext.ReplaceDisconnect(true));
            AgentUnity.LogError(e);
            // try
            // {
            //     UIControl.Instance.ShowKetNoiMang();
            //     SuperDialog.Instance.PopupOneButton.ShowPopupOneButton("Thông Báo","Server đang bảo trì.");
            //     AgentUnity.LogError("Connect to Server Failed [" + ex+"]");
            // }
            // catch (Exception e)
            // {
            //     AgentUnity.LogError(e);
            // }
        }
    }
    
    private void Connect()
    {
        try
        {
#if UNITY_EDITOR
            Debug.Log("========try connect=========== : " + Res.IP + " - " + Res.PORT);
#endif
            int currentRetrySecond = 0;
            while (!_networkContext.isConnecting)
            {
                if (currentRetrySecond < NetworkConfig.MAX_RETRY_SECOND)
                {
                    currentRetrySecond++;
                    if(_networkContext.tcpClient.value.Connected)
                        DispathcerUtility.Invoke(() => _networkContext.isConnecting = true);
                    Thread.Sleep(100);
                }
                else break;
            }
        }
        catch (Exception e)
        {
            AgentUnity.LogError(e);
        }
    }
    
    private void ReceiveMessageThread()
    {
        TcpClient client = _networkContext.tcpClient.value;
        Stream stream = client.GetStream();
        int lengthDataMsg = 0;
        bool isHead = true;
        while (_networkContext.isConnecting)
        {
            if (client.Available > 0)
            {
                if (!stream.CanRead)
                {
                    continue;
                }
                if (isHead)
                {
                    if (client.Available < NetworkConfig.SIZE_CONTROL)
                    {
                        if (!_networkContext.isConnecting)
                        { break; }
                        continue;
                    }
                    byte[] lengthControl = new byte[NetworkConfig.SIZE_CONTROL];
                    stream.Read(lengthControl, 0, NetworkConfig.SIZE_CONTROL);
                    lengthDataMsg = NetworkUtility.BytesToInt(lengthControl, 0);
#if UNITY_EDITOR
                    if (lengthDataMsg > client.ReceiveBufferSize)
                    {
                        Debug.LogWarning("Buffer size receive Length very big: " + lengthDataMsg + " ReceiveBufferSize: " + client.ReceiveBufferSize);
                        lengthDataMsg = 0;
                        DispathcerUtility.Invoke(() => _networkContext.ReplaceDisconnect(true));
                    }
#endif
                    isHead = false;
                }

                if (!isHead)
                {
                    if (client.Available < lengthDataMsg)
                    {

                        Thread.Sleep(5); 
                        // Debug.Log("WAIT: " + _client.Available + "wait: " + lengthDataMsg);
                        continue;
                    }
                    byte[] msgByte = new byte[lengthDataMsg];
                    stream.Read(msgByte, 0, lengthDataMsg);
                    lengthDataMsg = 0;
                    isHead = true;
                    DispathcerUtility.Invoke(() =>
                    {
                        NetworkEntity entity = _networkContext.CreateEntity();
                        entity.AddByteData(msgByte);
                        entity.isRecieve = true;
                    });
                }
            }
            else
            {
                bool keepConnect = true;
                if (client.Client.Poll(0, SelectMode.SelectRead))
                {
                    if (!client.Connected)
                        keepConnect = false;
                    else
                    {
                        byte[] b = new byte[1];
                        try
                        {
                            if (client.Client.Receive(b, SocketFlags.Peek) == 0)
                            {
                                keepConnect = false;
                            }
                        }
                        catch { keepConnect = false; }
                    }
                }
                if (!keepConnect)
                {
                    DispathcerUtility.Invoke(() =>
                    {
                        _networkContext.ReplaceDisconnect(true);
                    });
                }
                Thread.Sleep(_networkContext.receiveMessageDelay.value);
            }
        }
    }
    
    /// <summary>
    /// Ping to keep connect from server
    /// </summary>
    private void PingThread()
    {
        while (_networkContext.isConnecting)
        {
            Thread.Sleep(NetworkConfig.TIME_PING);
            // Message msg = new Message(CMD.PING_PONG);
            // if (_networkContext.isConnecting)
            // {
            //     DispathcerUtility.Invoke(() => SendData.SendMessage(msg));
            // }
        }
    }
}