using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public static MenuController Instance;
    public Button btnTungChieu;
    public Button btnTungChieu1;
    public Button btnTungChieu2;
    public Button btnDanhThuong;
    public Button btnThoat;
    public JoystickController joystick;
    public PlayerMove player1, player2;

    public PlayerMove player
    {
        get
        {
            if (B.Instance.teamId == 1)
            {
                return player1;
            }
            else
            {
                return player2;
            }
        }
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        btnTungChieu.onClick.AddListener(() =>
        {
            player.CastSkill(1);
            SendData.SendAttack(0, 3, gameObject.transform.position, 1);
        });

        btnTungChieu1.onClick.AddListener(() =>
        {
            player.CastSkill(2);
            SendData.SendAttack(0, 3, gameObject.transform.position, 2);
        });

        btnTungChieu2.onClick.AddListener(() =>
        {
            player.CastSkill(3);
            SendData.SendAttack(0, 3, gameObject.transform.position, 3);
        });

        btnDanhThuong.onClick.AddListener(() =>
        {
            player.NormalAttack();   // ← chạy anim bên trong PlayerMove
        });
        btnThoat.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("Game");
        });
    }
}