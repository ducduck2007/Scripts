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
        ClearItems();

        int count = (list == null) ? 0 : list.Count;
        if (txtSl != null) txtSl.text = $"{count}/100";

        if (content == null)
        {
            Debug.LogError("DialogTuong: content is NULL (chưa gán Content của ScrollView).");
            return;
        }

        if (itemPrefab == null)
        {
            Debug.LogError("DialogTuong: itemPrefab is NULL (chưa gán prefab ItemTuong).");
            return;
        }

        if (list == null) return;

        foreach (var t in list)
        {
            var item = Instantiate(itemPrefab, content);
            item.Init(t.id, t.ten);
            spawned.Add(item);
        }

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
        SendData.OnGetDanhSachLoaiTuong();
    }

}
