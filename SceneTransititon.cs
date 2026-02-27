using System.Collections;
using UnityEngine;

public class SceneTransititon : MonoBehaviour
{
    public static SceneTransititon Instance;

    public Animator transition;

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
        StartCoroutine(LoadScene(sceneName));
    }

    IEnumerator LoadScene(string sceneName)
    {
        transition.SetTrigger("StartFade");

        yield return new WaitForSeconds(1f);

        ThongBaoController.Instance.LoadVaoTran.SetLoadScene(sceneName);

        transition.SetTrigger("EndFade");
    }
}
