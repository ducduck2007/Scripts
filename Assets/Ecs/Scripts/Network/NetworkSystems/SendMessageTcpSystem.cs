using System;
using System.Collections.Generic;
using System.Text;
using Entitas;
using Newtonsoft.Json;
using UnityEngine;

public class SendMessageTcpSystem : ReactiveSystem<NetworkEntity>
{
    private readonly NetworkContext _networkContext;
    
    public SendMessageTcpSystem(Contexts contexts) : base(contexts.network)
    {
        _networkContext = contexts.network;
    }

    protected override ICollector<NetworkEntity> GetTrigger(IContext<NetworkEntity> context)
    {
        return context.CreateCollector(NetworkMatcher.AllOf(NetworkMatcher.Send, NetworkMatcher.MessageData));
    }

    protected override bool Filter(NetworkEntity entity)
    {
        return _networkContext.isConnecting && _networkContext.hasStream && entity.isSend && entity.hasMessageData;
    }

    protected override void Execute(List<NetworkEntity> entities)
    {
        foreach (var entity in entities)
        {
            Message msg = entity.messageData.value;
            byte[] b = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(msg));
            byte[] size = NetworkUtility.IntToBytes(b.Length);
            byte[] data = new byte[b.Length + size.Length];
            Array.Copy(size, data, size.Length);
            Array.Copy(b, 0, data, NetworkConfig.SIZE_CONTROL, b.Length);
            _networkContext.stream.value.Write(data, 0, data.Length);
    #if UNITY_EDITOR
            // if (msg.cmd != CMD.PING_PONG)
            {
                Debug.Log("Send: " + msg.cmd);
            }
    #endif
            entity.Destroy();
        }
    }
}