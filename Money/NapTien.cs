using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class NapTien : ScaleScreen
{
    public Button btnClose, btnVip, btnTinhNang;

    protected override void Start()
    {
        base.Start();

        if (btnClose)
            btnClose.onClick.AddListener(() =>
            {
                AudioManager.Instance.AudioClick();
                Show(false);
            });

        if (btnVip) btnVip.onClick.AddListener(ClickTinhNangAn);
        if (btnTinhNang) btnTinhNang.onClick.AddListener(ClickTinhNangAn);

        HookAllItemNapClick();
    }

    public void Show(bool val = true)
    {
        gameObject.SetActive(val);
    }

    private void HookAllItemNapClick()
    {
        var content = transform.Find("BG/Giua/ChucNang/Inapp/Viewport/Content");
        if (content == null)
        {
            Debug.LogWarning("[NapTien] Không tìm thấy Content: ChucNang/Nap/Viewport/Content");
            return;
        }

        for (int i = 0; i < content.childCount; i++)
        {
            var item = content.GetChild(i).gameObject;

            // đảm bảo có Graphic để nhận raycast
            var g = item.GetComponent<Graphic>();
            if (g == null) g = item.GetComponentInChildren<Graphic>(true);
            if (g != null) g.raycastTarget = true;

            var clicker = item.GetComponent<ItemNapClick>();
            if (clicker == null) clicker = item.AddComponent<ItemNapClick>();
            clicker.owner = this;
        }
    }

    private class ItemNapClick : MonoBehaviour, IPointerClickHandler
    {
        public NapTien owner;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (owner == null) return;
            AudioManager.Instance.AudioClick();
            owner.ClickDichVuChuaKetNoi();
        }
    }
}
