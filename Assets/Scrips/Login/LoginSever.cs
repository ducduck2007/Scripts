using UnityEngine;
using UnityEngine.UI;

public class LoginSever : ScaleScreen
{
    public Button btnLogin, btnRegister;
    public InputField inputUserName;
    public InputField inputPassWord;
    public Toggle tgNhoMatKhau;

    public override void Start()
    {
        base.Start();
        btnLogin.onClick.AddListener(ClickLogin);
        btnRegister.onClick.AddListener(ClickRegister);
    }

    public void ClickLogin()
    {
        B.Instance.UserName = inputUserName.text;
        B.Instance.PassWord = inputPassWord.text;
        if (!AgentUnity.CheckNetWork())
        {
            ThongBaoController.Instance.PopupOneButton.ShowPopupOneButton("Thông Báo","Mạng đang bị lỗi, bạn vui lòng kiểm tra lại kết nối Wifi hoặc 3G/4G.");
            return;
        }
        SendData.OnLoginGame();
    }

    public void ClickRegister()
    {
        LoginController.Instance.RegisterView.View(true);
        View(false);
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
