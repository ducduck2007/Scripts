using UnityEngine;

public class DialogController : ManualSingleton<DialogController>
{
    private GameObject Load(string namePath)
    {
        return Resources.Load(namePath) as GameObject;
    }

    // private DialogMMSetting _dialogMMSetting;
    // public DialogMMSetting DialogMMSetting
    // {
    //     get
    //     {
    //         if (_dialogMMSetting == null)
    //         {
    //             _dialogMMSetting = AgentUnity.InstanceObject<DialogMMSetting>(Load(PathResource.MM_Setting), transform);
    //         }

    //         return _dialogMMSetting;
    //     }
    // }

    // public void ShowDialogMmSetting()
    // {
    //     DialogMMSetting.OpenDialog();
    // }
}
