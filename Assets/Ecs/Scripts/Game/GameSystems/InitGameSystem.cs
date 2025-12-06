using System;
using System.Collections;
using System.Collections.Generic;
using Entitas;
using UnityEngine;

public class InitGameSystem : ReactiveSystem<NetworkEntity>
{
    private readonly Contexts _contexts;
    public InitGameSystem(Contexts contexts) : base(contexts.network)
    {
        _contexts = contexts;
    }
    
    protected override ICollector<NetworkEntity> GetTrigger(IContext<NetworkEntity> context)
    {
        return new Collector<NetworkEntity>(
            new [] {
                context.GetGroup(NetworkMatcher.AllOf(NetworkMatcher.Command, NetworkMatcher.MessageData)),
                context.GetGroup(NetworkMatcher.RequestQueue)
            },
            new [] {
                GroupEvent.Added,
                GroupEvent.Added
            }
        );
    }

    protected override bool Filter(NetworkEntity entity)
    {
        return!_contexts.game.isGameInit && entity.hasCommand && entity.hasMessageData || entity.hasRequestQueue;
    }

    protected override void Execute(List<NetworkEntity> entities)
    {
        foreach (var entity in entities)
        {
            if (entity.hasCommand && entity.hasMessageData) continue;
            OnProcessInitGame();          
        }
    }

    private void OnProcessInitGame()
    {
        if(!_contexts.network.hasRequestQueue) return;
        RequestQueueComponent requestQueue = _contexts.network.requestQueue;
        Queue<Action> queue = requestQueue.value;
        if (queue.Count > 0)
        {
            queue.Dequeue().Invoke();
            requestQueue.current++;
            // SceneLoadFunction.Instance.LoadPercentChangeInfo.UpdatePercent(requestQueue.current / (float)requestQueue.maxCount);
        }
        else
        {
            
            _contexts.game.isGameInit = true;
            _contexts.network.RemoveRequestQueue();
            // UIControl.Instance.InitGuide();
            Debug.Log("InitGameSuccess CountProcess: " + requestQueue.current + "/" + requestQueue.maxCount);
            //UIControl.Menu.MainMenu.SetInfoPlayer();
            // SceneLoadFunction.Instance.ShowLoadPercentChangeInfo(false);
//            if (UserData.Instance.CurrentStepGuide >= 26 && Event_Data.Instance.CheckEventByType(GameEventType.NAP_LAN_DAU))
//            {
//                DialogControl.MainMenu.DialogMMNapLanDau.Show();
//                DialogControl.MainMenu.DialogMMNapLanDau.SetInfo(Event_Data.Instance.firstPayGiftInfo);
//            }
            GlobalCoroutine.Invoke(LoadData());
        }
    }

    IEnumerator LoadData()
    {
        // SendData.OnDataChatTheGioi();
        yield return new WaitForSeconds(0.2f);
        
    }
}