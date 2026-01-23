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
        if (PopupController.Instance != null)
        {
            PopupController.Instance.ChonTuong.Show(false);
        }
        routine = StartCoroutine(AnimateDots());
    }

    protected override void OnDisable()
    {
        if (routine != null)
            StopCoroutine(routine);
    }

    public void SetLoadScene(string scene)
    {
        AudioManager.Instance.StopAudioBg();
        StartCoroutine(LoadSceneAsync(scene));
    }

    IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);

        while (!op.isDone)
        {
            yield return null;
        }

        // Scene đã active xong
        // Show(false);
    }

    IEnumerator AnimateDots()
    {
        int dotCount = 0;

        while (true)
        {
            dotCount = (dotCount + 1) % 4; // 0 -> 3
            loadingText.text = "Đang vào trận" + new string('.', dotCount);
            yield return new WaitForSeconds(0.4f);
        }
    }

    public void Show(bool val = true)
    {
        if (val)
        {
            gameObject.SetActive(true);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
