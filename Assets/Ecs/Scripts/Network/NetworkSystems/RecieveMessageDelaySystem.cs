using System.Collections;
using Entitas;
using UnityEngine;

public class RecieveMessageDelaySystem : IInitializeSystem, IExecuteSystem
{
    private Contexts _contexts;
    private float _lastRead;
    private bool _lastConfig;
    public RecieveMessageDelaySystem(Contexts contexts)
    {
        _contexts = contexts;
    }

    public void Initialize()
    {
        _contexts.network.ReplaceReceiveMessageDelay(NetworkConfig.FAST_DELAY);
        GlobalCoroutine.Invoke(DelayRoutine());
    }

    public void Execute()
    {
        _lastRead = Time.time;
    }
    
    IEnumerator DelayRoutine()
    {
        float check = 1000F / 16F / 1000F;
        float time_delay = 0.1F;
        while (true)
        {
            yield return new WaitForSeconds(time_delay);
            bool ck = Time.time - _lastRead < check;
            if (ck != _lastConfig)
            {
                _contexts.network.receiveMessageDelay.value = ck ? NetworkConfig.FAST_DELAY : NetworkConfig.NORMAL_DELAY;
                _lastConfig = ck;
            }
        }
    }
}