using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public static MenuController Instance;
    public Button btnTungChieu;
    public Button btnTungChieu1;
    public Button btnTungChieu2;
    public Button btnDanhThuong;
    public Toggle toggleRandom;
    public JoystickController joystick;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        btnTungChieu.onClick.AddListener(() =>
        {
            PlayerMove player = FindObjectOfType<PlayerMove>();
            player.CastSkill1();
        });

        btnTungChieu1.onClick.AddListener(() =>
        {
            PlayerMove player = FindObjectOfType<PlayerMove>();
            player.CastSkill2();  // Skill thứ 2
        });

        btnTungChieu2.onClick.AddListener(() =>
        {
            PlayerMove player = FindObjectOfType<PlayerMove>();
            player.CastSkill3();  // Skill thứ 3
        });

        btnDanhThuong.onClick.AddListener(() =>
        {
            PlayerMove player = FindObjectOfType<PlayerMove>();
            player.NormalAttack();   // ← chạy anim bên trong PlayerMove
        });
    }
}