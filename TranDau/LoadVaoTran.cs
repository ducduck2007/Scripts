using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadVaoTran : ScaleScreen
{
    public TextMeshProUGUI loadingText;

    private Coroutine routine;

    protected override void OnEnable()
    {
        base.OnEnable();

        // Reset gate mỗi lần vào trận
        PlayLoadGate.Reset();

        if (PopupController.Instance != null)
            PopupController.Instance.ChonTuong.Show(false);

        routine = StartCoroutine(AnimateDots());
    }

    protected override void OnDisable()
    {
        if (routine != null) StopCoroutine(routine);
        routine = null;
    }

    public void SetLoadScene(string scene)
    {
        AudioManager.Instance.StopAudioBg();
        StartCoroutine(LoadSceneAsyncControlled(scene));
    }

    private IEnumerator LoadSceneAsyncControlled(string sceneName)
    {
        // Load scene nhưng CHƯA activate
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        // Chờ load tới ngưỡng 0.9 (Unity load xong assets, chuẩn bị activate)
        while (op.progress < 0.9f)
        {
            yield return null;
        }

        // ===== Phase: cho GC dọn + 1-2 frame rảnh (giảm spike) =====
        // (Không phải cure OOM, nhưng giảm peak)
        System.GC.Collect();
        yield return null;
        yield return null;

        // Activate scene
        op.allowSceneActivation = true;

        while (!op.isDone)
            yield return null;

        // Scene active xong. Lúc này TranDauControl.Start sẽ chạy.
        // Nhưng snapshot vẫn đang bị gate (Ready=false).
        // TranDauControl sẽ là nơi MarkReady + Flush.
    }

    private IEnumerator AnimateDots()
    {
        int dotCount = 0;
        while (true)
        {
            dotCount = (dotCount + 1) % 4;
            if (loadingText != null)
                loadingText.text = "Đang vào trận" + new string('.', dotCount);

            yield return new WaitForSeconds(0.4f);
        }
    }

    public void Show(bool val = true)
    {
        if (val) gameObject.SetActive(true);
        else Destroy(gameObject);
    }
}
