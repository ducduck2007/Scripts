using UnityEngine;
using UnityEngine.UI;

public class Register : ScaleScreen
{
    public TMPro.TMP_InputField ifUsername, ifPassword, ifRePassword;
    public Button btnRegister, btnQuayLai;

    public override void Start()
    {
        base.Start();
        btnRegister.onClick.AddListener(ClickLogin);
        btnQuayLai.onClick.AddListener(() =>
        {
            LoginController.Instance.ServerLogin.View(true);
            View(false);
        });
    }

    private void ClickLogin()
    {
        // SoundGame.Instance.PlayButtonClickSound();
        string t = string.Empty;
        
        AgentUnity.SetString(KeyLocalSave.PP_USERNAME, ifUsername.text);
        AgentUnity.SetString(KeyLocalSave.PP_PASSWORD, ifPassword.text);
        LoginController.Instance.ServerLogin.View(true);
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
