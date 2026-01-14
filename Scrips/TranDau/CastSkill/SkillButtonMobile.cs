using UnityEngine;
using UnityEngine.EventSystems;

public class SkillButtonMobile :
    MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler,
    IDragHandler
{
    [Header("Refs")]
    public GameObject canvasSkill;     // CanvasSkill (World Space)
    public MobileSkillAim aim;           // script xoay Y
    public VirtualJoystick aimJoystick; // joystick kéo hướng
    public int Skil;

    bool holding;

    public void Start()
    {
        aimJoystick.Skil = Skil;
    }

    // Nhấn giữ
    public void OnPointerDown(PointerEventData eventData)
    {
        if (Skil == 1 && B.Instance.isCooldownSkill1 || Skil == 2 && B.Instance.isCooldownSkill2 || Skil == 3 && B.Instance.isCooldownSkill3)
        {
            return;
        }
        holding = true;
        canvasSkill.SetActive(true);
        aimJoystick.Show();
    }

    // Kéo (aim)
    public void OnDrag(PointerEventData eventData)
    {
        if (!holding) return;

        Vector2 dir = aimJoystick.Direction;
        aim.UpdateAim(dir);
    }

    // Thả tay
    public void OnPointerUp(PointerEventData eventData)
    {
        if (!holding) return;
        holding = false;

        canvasSkill.SetActive(false);
        aimJoystick.Hide();

        float angleY = aim.GetAngleY();
        CastSkill(angleY);
    }

    void CastSkill(float angleY)
    {
        if (Skil == 1)
        {
            MenuController.Instance.TungChieu1();
        }
        else if (Skil == 2)
        {
            MenuController.Instance.TungChieu2();
        }
        else if (Skil == 3)
        {
            MenuController.Instance.TungChieu3();
        }
    }
}
