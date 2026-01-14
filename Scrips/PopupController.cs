using UnityEngine;

public class PopupController : ManualSingleton<PopupController>
{
    private GameObject Load(string namePath)
    {
        return Resources.Load(namePath) as GameObject;
    }

    private LoiMoiKetBan _loiMoiKetBan;
    public LoiMoiKetBan LoiMoiKetBan
    {
        get
        {
            if (_loiMoiKetBan == null)
            {
                _loiMoiKetBan = AgentUnity.InstanceObject<LoiMoiKetBan>(Load(PathResource.LoiMoiKetBan), transform);
            }

            return _loiMoiKetBan;
        }
    }

    public void ShowLoiMoiKetBan()
    {
        LoiMoiKetBan.Show();
    }

    private ChonTuong _chonTuong;
    public ChonTuong ChonTuong
    {
        get
        {
            if (_chonTuong == null)
            {
                _chonTuong = AgentUnity.InstanceObject<ChonTuong>(Load(PathResource.ChonTuong), transform);
            }

            return _chonTuong;
        }
    }
}
