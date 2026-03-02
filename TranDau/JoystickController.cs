using UnityEngine;
using UnityEngine.EventSystems;

public class JoystickController : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    public RectTransform bg;
    public RectTransform handle;
    public float joystickRadius = 80f;

    [HideInInspector] public Vector2 inputVector;

    [Header("Keyboard (PC/Emulator)")]
    public bool enableKeyboard = true;
    public bool includeArrowKeys = true;

    // Nếu đang drag bằng chuột/touch thì không override bằng phím
    private bool _isPointerDown = false;

    public void OnPointerDown(PointerEventData eventData)
    {
        _isPointerDown = true;
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (bg == null || handle == null) return;

        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            bg,
            eventData.position,
            eventData.pressEventCamera,
            out pos
        );

        pos = Vector2.ClampMagnitude(pos, joystickRadius);

        handle.anchoredPosition = pos;
        inputVector = (joystickRadius <= 0.0001f) ? Vector2.zero : (pos / joystickRadius);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _isPointerDown = false;
        ResetStick();
    }

    private void Update()
    {
        if (!enableKeyboard) return;
        if (_isPointerDown) return; // đang kéo thì ưu tiên kéo

        Vector2 key = ReadKeyboardVector();
        if (key.sqrMagnitude > 0.0001f)
        {
            key = Vector2.ClampMagnitude(key, 1f);
            ApplyInputVector(key);
        }
        else
        {
            // không bấm phím -> trả về giữa
            if (inputVector.sqrMagnitude > 0.0001f)
                ResetStick();
        }
    }

    private Vector2 ReadKeyboardVector()
    {
        float x = 0f;
        float y = 0f;

        if (Input.GetKey(KeyCode.A)) x -= 1f;
        if (Input.GetKey(KeyCode.D)) x += 1f;
        if (Input.GetKey(KeyCode.S)) y -= 1f;
        if (Input.GetKey(KeyCode.W)) y += 1f;

        if (includeArrowKeys)
        {
            if (Input.GetKey(KeyCode.LeftArrow)) x -= 1f;
            if (Input.GetKey(KeyCode.RightArrow)) x += 1f;
            if (Input.GetKey(KeyCode.DownArrow)) y -= 1f;
            if (Input.GetKey(KeyCode.UpArrow)) y += 1f;
        }

        return new Vector2(x, y);
    }

    private void ApplyInputVector(Vector2 v)
    {
        if (bg == null || handle == null) return;

        inputVector = v;
        handle.anchoredPosition = v * joystickRadius;
    }

    private void ResetStick()
    {
        if (handle != null) handle.anchoredPosition = Vector2.zero;
        inputVector = Vector2.zero;
    }
}