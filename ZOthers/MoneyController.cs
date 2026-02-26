using UnityEngine;

public class MoneyController : ManualSingleton<MoneyController>
{
    private GameObject Load(string namePath)
    {
        return Resources.Load(namePath) as GameObject;
    }

    private RutThuong _rutThuong;
    public RutThuong RutThuong
    {
        get
        {
            if (_rutThuong == null)
            {
                _rutThuong = AgentUnity.InstanceObject<RutThuong>(Load(PathResource.RutThuong), transform);
            }

            return _rutThuong;
        }
    }

    private NapTien _napTien;
    public NapTien NapTien
    {
        get
        {
            if (_napTien == null)
            {
                _napTien = AgentUnity.InstanceObject<NapTien>(Load(PathResource.NapTien), transform);
            }

            return _napTien;
        }
    }
}
