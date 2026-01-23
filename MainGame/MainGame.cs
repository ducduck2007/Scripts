using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainGame : ScaleScreen
{
    public Button btnChienDau, btnDauTap, btnHuyTimTran;
    public GameObject objTimTran;
    public TextMeshProUGUI txtTimeWait;

    private float timeWaiting;
    private bool isFindingMatch;

    protected override void Start()
    {
        base.Start();
        btnChienDau.onClick.AddListener(ClickChienDau);
        btnDauTap.onClick.AddListener(ClickDauTap);
        btnHuyTimTran.onClick.AddListener(ClickHuyTimTran);

        objTimTran.SetActive(false);
    }

    private void Update()
    {
        if (isFindingMatch)
        {
            timeWaiting += Time.deltaTime;
            UpdateTimerUI();
        }
    }

    private void ClickChienDau()
    {
        // SoundGame.Instance.PlayButtonClickSound();
        SendData.FindMatch();
    }

    public void TimTran()
    {
        btnChienDau.interactable = false;
        btnDauTap.interactable = false;
        isFindingMatch = true;
        timeWaiting = 0f;
        objTimTran.SetActive(true);
        UpdateTimerUI();
    }

    private void ClickDauTap()
    {
        // 1. Hủy tất cả subscription events để tránh memory leaks
        System.GC.Collect(); // Kích hoạt GC trước

        // 2. Nếu có AssetBundles, clear cache
        if (Caching.ClearCache())
        {
            Debug.Log("AssetBundle cache cleared");
        }

        // 3. Xóa các tài nguyên không sử dụng
        Resources.UnloadUnusedAssets();

        // 4. Clear PlayerPrefs nếu cần (tùy game)
        // PlayerPrefs.DeleteAll();

        // 5. Đảm bảo GC chạy triệt để
        System.GC.Collect();
        System.GC.WaitForPendingFinalizers();

        // 6. Load scene với option xóa memory
        SceneManager.LoadScene("DauTapTest");
    }

    private void ClickHuyTimTran()
    {
        // SoundGame.Instance.PlayButtonClickSound();
        SendData.CancelFindMatch(); // nếu có API hủy tìm trận

        btnChienDau.interactable = true;
        btnDauTap.interactable = true;
        isFindingMatch = false;
        objTimTran.SetActive(false);
    }

    void UpdateTimerUI()
    {
        int totalSeconds = Mathf.FloorToInt(timeWaiting);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        txtTimeWait.text = $"{minutes:00}:{seconds:00}";
    }

    public void Show(bool val = true)
    {
        gameObject.SetActive(val);
    }

    // Call từ server khi đã tìm thấy trận
    public void OnMatchFound()
    {
        isFindingMatch = false;
        SceneManager.LoadScene("Play");
    }
}
