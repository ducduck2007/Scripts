using UnityEngine;

public class DialogController : ManualSingleton<DialogController>
{
    private GameObject Load(string namePath)
    {
        return Resources.Load(namePath) as GameObject;
    }

    private DialogChonPhong _dialogChonPhong;
    public DialogChonPhong DialogChonPhong
    {
        get
        {
            if (_dialogChonPhong == null)
            {
                _dialogChonPhong = AgentUnity.InstanceObject<DialogChonPhong>(Load(PathResource.DialogChonPhong), transform);
            }

            return _dialogChonPhong;
        }
    }

    public void ShowDialogChonPhong()
    {
        DialogChonPhong.Show();
    }
}
