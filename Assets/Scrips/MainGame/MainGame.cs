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

    public override void Start()
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
        SceneManager.LoadScene("DauTap");
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
