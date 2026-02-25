using UnityEngine;

public class ScaleScreen : MonoBehaviour
{
    public GameObject bg;
    public Transform tranScale;
    public Transform contentScale;

    protected virtual void OnEnable()
    {
        Scale();
    }

    protected virtual void OnDisable()
    {

    }

    protected virtual void Start()
    {
    }

    protected void Scale()
    {
        if (bg != null)
            AgentUnity.ScaleBg(bg);

        if (tranScale != null)
            AgentUnity.ScaleTranform(tranScale);

        if (contentScale != null)
            AgentUnity.ScaleContent(contentScale);
    }

    public void ClickTinhNangAn()
    {
        ThongBaoController.Instance.Toast.ShowToast("Tính năng đang phát triển");
    }

    public void ClickDichVuChuaKetNoi()
    {
        ThongBaoController.Instance.Toast.ShowToast("Kết nối dịch vụ IAP chỉ hoạt động với phiên bản chính thức trên chợ");
    }
}
