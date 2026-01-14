using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatControlController : ManualSingleton<ChatControlController>
{
    private GameObject Load(string namePath)
    {
        return  Resources.Load(namePath) as  GameObject;
    }

    private DialogChat _chatControl;
    public DialogChat DialogChat
    {
        get
        {
            if (_chatControl == null)
            {
                _chatControl = AgentUnity.InstanceObject<DialogChat>(Load(PathResource.DialogChat), transform);
                AgentUnity.SetPositionGameObjectUI(_chatControl.transform, new Vector3(-1200F, 0), false);
            }

            return _chatControl;
        }
    }
    
}
