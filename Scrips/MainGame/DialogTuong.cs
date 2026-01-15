using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogTuong : ScaleScreen
{
    public Button btnExit;
    public TextMeshProUGUI txtSl;

    [Header("List UI")]
    public Transform content;
    public ItemTuong itemPrefab;

    private readonly List<ItemTuong> spawned = new();

    protected override void Start()
    {
        base.Start();
        btnExit.onClick.AddListener(SetExit);
    }

    private void SetExit()
    {
        AudioManager.Instance.AudioClick();
        Show(false);
    }

    public void Show(bool val = true)
    {
        gameObject.SetActive(val);
    }

    public void SetData(List<CommandGetDanhSachLoaiTuongSystem.LoaiTuongDto> list)
    {
        // Debug.Log($"DialogTuong.SetData: list={(list == null ? 0 : list.Count)} content={(content ? content.name : "NULL")} prefab={(itemPrefab ? itemPrefab.name : "NULL")}");
        ClearItems();

        int count = (list == null) ? 0 : list.Count;

        if (txtSl != null) txtSl.text = $"{count}/100";

        if (list == null) return;

        foreach (var t in list)
        {
            var item = Instantiate(itemPrefab, content);
            // Debug.Log("Spawn item: " + t.ten);

            item.idTuong = t.id;
            if (item.txtName != null) item.txtName.text = t.ten;

            spawned.Add(item);
        }

        // đếm số item đã spawn thật sự (phòng trường hợp prefab null bị bỏ qua), thì đặt txtSl sau vòng foreach
        // if (txtSl != null) txtSl.text = $"{spawned.Count}/100";
    }

    private void ClearItems()
    {
        for (int i = 0; i < spawned.Count; i++)
        {
            if (spawned[i] != null) Destroy(spawned[i].gameObject);
        }
        spawned.Clear();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        SendData.OnGetDanhSachLoaiTuong(); // ✅ yêu cầu server trả danh sách
    }

}
