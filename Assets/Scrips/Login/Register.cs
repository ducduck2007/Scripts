using UnityEngine;
using UnityEngine.UI;

public class Register : MonoBehaviour
{
    public TMPro.TMP_InputField ifUsername, ifPassword, ifRePassword;
    public Button btnRegister, btnQuayLai;

    

    private void ClickLogin()
    {
        // SoundGame.Instance.PlayButtonClickSound();
        string t = string.Empty;
        ifUsername.text = t;
        ifPassword.text = t;
        ifRePassword.text = t;

        View(false);
    }

    public void View(bool val)
    {
        gameObject.SetActive(val);
    }
}
