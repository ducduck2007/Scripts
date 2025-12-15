using UnityEngine;

public class PopupController : ManualSingleton<PopupController>
{
    private GameObject Load(string namePath)
    {
        return Resources.Load(namePath) as GameObject;
    }

    private PopupTimTran _dialogChonPhong;
    public PopupTimTran PopupTimTran
    {
        get
        {
            if (_dialogChonPhong == null)
            {
                _dialogChonPhong = AgentUnity.InstanceObject<PopupTimTran>(Load(PathResource.PopupTimTran), transform);
            }

            return _dialogChonPhong;
        }
    }

    public void ShowPopupTimTran()
    {
        PopupTimTran.Show();
    }
}
