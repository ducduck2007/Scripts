using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainGame : MonoBehaviour
{
    public Button btnChienDau;

    private void Start()
    {
        btnChienDau.onClick.AddListener(ClickChienDau);
    }
    private void ClickChienDau()
    {
        // SoundGame.Instance.PlayButtonClickSound();
        SceneManager.LoadScene("Play");
    }

    public void Show(bool val = true)
    {
        gameObject.SetActive(val);
    }
}
