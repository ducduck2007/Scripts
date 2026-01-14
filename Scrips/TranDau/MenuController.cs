using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro; // Thêm namespace TMPro

public class MenuController : MonoBehaviour
{
    public static MenuController Instance;
    public Button btnTungChieu1;
    public Button btnTungChieu2;
    public Button btnTungChieu3;
    public Button btnDanhThuong;
    public Button btnThoat;
    public JoystickController joystick;
    public Image imgChieu1;
    public Image imgChieu2;
    public Image imgChieu3;
    public TMP_Text txtCooldown1; // Thêm text cho chiêu 1
    public TMP_Text txtCooldown2; // Thêm text cho chiêu 2
    public TMP_Text txtCooldown3; // Thêm text cho chiêu 3

    // Biến để lưu thời gian cooldown và các coroutine
    private float cooldownTime = 3f;
    private Coroutine cooldownCoroutine1;
    private Coroutine cooldownCoroutine2;
    private Coroutine cooldownCoroutine3;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Ẩn các image và text, reset fill amount
        imgChieu1.gameObject.SetActive(false);
        imgChieu2.gameObject.SetActive(false);
        imgChieu3.gameObject.SetActive(false);

        txtCooldown1.gameObject.SetActive(false);
        txtCooldown2.gameObject.SetActive(false);
        txtCooldown3.gameObject.SetActive(false);

        imgChieu1.fillAmount = 0f;
        imgChieu2.fillAmount = 0f;
        imgChieu3.fillAmount = 0f;

        btnTungChieu1.onClick.AddListener(TungChieu1);

        btnTungChieu2.onClick.AddListener(TungChieu2);

        btnTungChieu3.onClick.AddListener(TungChieu3);

        btnDanhThuong.onClick.AddListener(() =>
        {
            TranDauControl.Instance.playerMove.NormalAttack();   // ← chạy anim bên trong PlayerMove
        });
        btnThoat.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("Game");
        });
    }

    public void TungChieu1()
    {
        TranDauControl.Instance.playerMove.CastSkill(1);
        btnTungChieu1.interactable = false;
        SendData.SendAttack(0, 3, gameObject.transform.position, 1);
        StartCooldown(imgChieu1, 1);
    }

    public void TungChieu2()
    {
        TranDauControl.Instance.playerMove.CastSkill(2);
        btnTungChieu2.interactable = false;
        SendData.SendAttack(0, 3, gameObject.transform.position, 2);
        StartCooldown(imgChieu2, 2);
    }

    public void TungChieu3()
    {
        TranDauControl.Instance.playerMove.CastSkill(3);
        btnTungChieu3.interactable = false;
        SendData.SendAttack(0, 3, gameObject.transform.position, 3);
        StartCooldown(imgChieu3, 3);
    }

    // Phương thức để bắt đầu cooldown
    private void StartCooldown(Image cooldownImage, int skillNumber)
    {
        // Dừng coroutine cũ nếu đang chạy
        switch (skillNumber)
        {
            case 1:
                if (cooldownCoroutine1 != null)
                    StopCoroutine(cooldownCoroutine1);
                cooldownCoroutine1 = StartCoroutine(CooldownCoroutine(cooldownImage, 1));
                B.Instance.isCooldownSkill1 = true;
                break;
            case 2:
                if (cooldownCoroutine2 != null)
                    StopCoroutine(cooldownCoroutine2);
                cooldownCoroutine2 = StartCoroutine(CooldownCoroutine(cooldownImage, 2));
                B.Instance.isCooldownSkill2 = true;
                break;
            case 3:
                if (cooldownCoroutine3 != null)
                    StopCoroutine(cooldownCoroutine3);
                cooldownCoroutine3 = StartCoroutine(CooldownCoroutine(cooldownImage, 3));
                B.Instance.isCooldownSkill3 = true;
                break;
        }
    }

    // Coroutine để xử lý fill amount và hiển thị thời gian
    private System.Collections.IEnumerator CooldownCoroutine(Image cooldownImage, int skill)
    {
        // Lấy text tương ứng
        TMP_Text cooldownText = GetCooldownText(skill);

        // Hiển thị image và text
        cooldownImage.gameObject.SetActive(true);
        cooldownText.gameObject.SetActive(true);

        // Đặt fill amount thành 1 (không xoay theo fill amount nữa)
        cooldownImage.fillAmount = 1f;

        float timer = cooldownTime;

        while (timer > 0f)
        {
            timer -= Time.deltaTime;

            // Cập nhật text với thời gian còn lại (làm tròn lên)
            int secondsLeft = Mathf.CeilToInt(timer);
            cooldownText.text = secondsLeft.ToString();

            yield return null;
        }

        // Ẩn image và text khi hoàn thành
        cooldownImage.gameObject.SetActive(false);
        cooldownText.gameObject.SetActive(false);

        // Reset fill amount và text
        cooldownImage.fillAmount = 0f;
        cooldownText.text = "";

        // Kích hoạt lại button tương ứng
        switch (skill)
        {
            case 1:
                btnTungChieu1.interactable = true;
                B.Instance.isCooldownSkill1 = false;
                break;
            case 2:
                btnTungChieu2.interactable = true;
                B.Instance.isCooldownSkill2 = false;
                break;
            case 3:
                btnTungChieu3.interactable = true;
                B.Instance.isCooldownSkill3 = false;
                break;
        }
    }

    // Phương thức phụ để lấy text cooldown tương ứng
    private TMP_Text GetCooldownText(int skill)
    {
        switch (skill)
        {
            case 1: return txtCooldown1;
            case 2: return txtCooldown2;
            case 3: return txtCooldown3;
            default: return null;
        }
    }

    // Phương thức để hủy tất cả coroutine khi đối tượng bị hủy
    private void OnDestroy()
    {
        if (cooldownCoroutine1 != null)
            StopCoroutine(cooldownCoroutine1);
        if (cooldownCoroutine2 != null)
            StopCoroutine(cooldownCoroutine2);
        if (cooldownCoroutine3 != null)
            StopCoroutine(cooldownCoroutine3);
    }
}