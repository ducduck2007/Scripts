using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

public class ThongBaoController : ManualSingleton<ThongBaoController>
{
    private GameObject Load(string namePath)
    {
        return Resources.Load(namePath) as GameObject;
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
    
}
