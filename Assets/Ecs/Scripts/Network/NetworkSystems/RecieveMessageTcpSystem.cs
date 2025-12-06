using System.Collections.Generic;
using System.Text;
using Entitas;
using UnityEngine;

public class ReceiveMessageTcpSystem : ReactiveSystem<NetworkEntity>
{
    private NetworkContext _networkContext;
    
    public ReceiveMessageTcpSystem(Contexts contexts) : base(contexts.network)
    {
        _networkContext = contexts.network;
    }

    protected override ICollector<NetworkEntity> GetTrigger(IContext<NetworkEntity> context)
    {
        return context.CreateCollector(NetworkMatcher.AllOf(NetworkMatcher.Recieve, NetworkMatcher.ByteData));
    }

    protected override bool Filter(NetworkEntity entity)
    {
        return _networkContext.isConnecting && entity.isRecieve && entity.hasByteData;
    }

    protected override void Execute(List<NetworkEntity> entities)
    {
        foreach (var entity in entities)
        {
            byte[] data = entity.byteData.value;
            Message msg = new Message(Encoding.UTF8.GetString(data));
#if UNITY_EDITOR
           // Debug.Log("Read: " + msg.cmd+" "+msg.GetJson());
            Debug.Log("Read: " + msg.cmd);
#endif
            if (msg.cmd != -1)
            {
                NetworkEntity messageEntity = _networkContext.CreateEntity();
                messageEntity.AddMessageData(msg);
                messageEntity.AddCommand(msg.cmd);
            }
            else Debug.LogWarning($"Read data error {msg.GetJson()} ");
            entity.Destroy();
        }
    }
}