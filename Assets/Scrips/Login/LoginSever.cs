using UnityEngine;
using UnityEngine.UI;

public class LoginSever : MonoBehaviour
{
    public Button btnLogin, btnRegister;
    public InputField inputUserName;
    public InputField inputPassWord;
    public Toggle tgNhoMatKhau;

    private void Start()
    {
        btnLogin.onClick.AddListener(ClickLogin);
    }

    public void ClickLogin()
    {
        B.Instance.UserName = inputUserName.text;
        B.Instance.PassWord = inputPassWord.text;
        // LoadController.Instance.ShowLoadWait(true, 15);
        SendData.OnLoginGame();
    }

    private void OnEnable()
    {
        inputUserName.text = AgentUnity.GetString(KeyLocalSave.PP_USERNAME);
        inputPassWord.text = AgentUnity.GetString(KeyLocalSave.PP_PASSWORD);
    }
    
    public void View(bool val)
    {
        gameObject.SetActive(val);
    }
}
