using UnityEngine;
using UnityEngine.EventSystems;

public class SkillButtonMobile :
    MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler,
    IDragHandler
{
    [Header("Refs")]
    public VirtualJoystick aimJoystick;
    public int Skil;

    [Header("Fallback (nếu không tìm được canvas/aim runtime)")]
    public GameObject canvasSkillFallback;
    public MobileSkillAim aimFallback;

    [Header("Debug")]
    public bool debugLog = true;

    [Header("Click-only threshold")]
    [SerializeField] private float clickOnlyPowerThreshold = 0.05f;

    private bool holding;

    private GameObject _canvasRuntime;
    private MobileSkillAim _aimRuntime;

    void Start()
    {
        if (aimJoystick != null) aimJoystick.Skil = Skil;
        ResolveCanvasByType("Start");
    }

    private PlayerMove GetLocalPlayerMove()
    {
        if (TranDauControl.Instance == null) return null;
        return TranDauControl.Instance.playerMove;
    }

    private int GetTypeBySkillSlot()
    {
        long myId = (UserData.Instance != null) ? UserData.Instance.UserID : 0;
        if (B.Instance == null) return 0;
        return B.Instance.GetSkillType(myId, Skil);
    }

    private void ResolveCanvasByType(string where)
    {
        int type = GetTypeBySkillSlot();

        var pm = GetLocalPlayerMove();
        if (pm != null)
        {
            _canvasRuntime = pm.GetAimCanvasByType(type);
            _aimRuntime = (_canvasRuntime != null)
                ? _canvasRuntime.GetComponentInChildren<MobileSkillAim>(true)
                : null;
        }

        if (_canvasRuntime == null) _canvasRuntime = canvasSkillFallback;
        if (_aimRuntime == null) _aimRuntime = aimFallback;

        if (debugLog)
        {
            if (type <= 0) Debug.LogWarning($"[SkillButtonMobile] {where} slot={Skil}: type=0 (chưa nhận từ server?)");
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if ((Skil == 1 && B.Instance.isCooldownSkill1) ||
            (Skil == 2 && B.Instance.isCooldownSkill2) ||
            (Skil == 3 && B.Instance.isCooldownSkill3))
            return;

        holding = true;

        ResolveCanvasByType("PointerDown");

        var pm = GetLocalPlayerMove();
        if (pm != null) pm.DisableAllAimCanvases();

        if (_canvasRuntime != null) _canvasRuntime.SetActive(true);

        if (aimJoystick != null)
        {
            aimJoystick.Show();
            aimJoystick.Begin(eventData);
        }

        if (_aimRuntime != null)
        {
            _aimRuntime.UpdateAim(Vector2.up, 0f);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!holding) return;

        if (aimJoystick != null) aimJoystick.ProcessDrag(eventData);

        if (_aimRuntime != null && aimJoystick != null)
        {
            _aimRuntime.UpdateAim(aimJoystick.Direction, aimJoystick.Power);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!holding) return;
        holding = false;

        Vector2 uiDir = Vector2.up;
        float power = 0f;

        if (aimJoystick != null)
        {
            uiDir = aimJoystick.Direction;
            power = aimJoystick.Power;
        }

        var pm = GetLocalPlayerMove();

        // CLICK-ONLY: không kéo joystick => KHÔNG set aim override (để PlayerMove tự auto-aim)
        if (power < clickOnlyPowerThreshold || uiDir.sqrMagnitude < 0.0001f)
        {
            if (pm != null) pm.ClearSkillAimOverride();
        }
        else
        {
            Vector3 worldDir = new Vector3(uiDir.x, 0f, uiDir.y);
            if (worldDir.sqrMagnitude > 0.0001f) worldDir.Normalize();

            if (pm != null) pm.SetSkillAimOverride(worldDir);
        }

        if (aimJoystick != null)
        {
            aimJoystick.End(eventData);
            aimJoystick.Hide();
        }

        CastSkill(0f);

        if (_canvasRuntime != null) _canvasRuntime.SetActive(false);

        if (_aimRuntime != null) _aimRuntime.ResetSmallToCenter();
    }

    void CastSkill(float angleY)
    {
        if (Skil == 1) MenuController.Instance.TungChieu1();
        else if (Skil == 2) MenuController.Instance.TungChieu2();
        else if (Skil == 3) MenuController.Instance.TungChieu3();
    }
}
