using System.Collections;
using UnityEngine;

public class SceneTransititon : MonoBehaviour
{
    public static SceneTransititon Instance;

    public Animator transition;
    public float fadeTime = 1f;

    private bool isLoading = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadNextScene(string sceneName)
    {
        if (isLoading) return;
        StartCoroutine(LoadScene(sceneName));
    }

    IEnumerator LoadScene(string sceneName)
    {
    if (isLoading) yield break;
    isLoading = true;

    transition.SetTrigger("StartFade");

    yield return new WaitForSeconds(fadeTime);


    ThongBaoController.Instance.LoadVaoTran.SetLoadScene(sceneName);

    transition.SetTrigger("EndFade");

    isLoading = false;
    }
}
