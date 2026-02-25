using UnityEngine;
using UnityEngine.EventSystems;

public class VirtualJoystick : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    public GameObject BgJt;
    public RectTransform handle;
    public float radius = 80f;
    public int Skil;

    Vector2 startPos;

    public Vector2 Direction { get; private set; }
    public float Power { get; private set; }

    void Awake()
    {
        startPos = handle.anchoredPosition;
        Direction = Vector2.zero;
        Power = 0f;
    }

    public void OnPointerDown(PointerEventData eventData) => Begin(eventData);
    public void OnDrag(PointerEventData eventData) => ProcessDrag(eventData);
    public void OnPointerUp(PointerEventData eventData) => End(eventData);

    bool IsCooldown()
    {
        return (Skil == 1 && B.Instance.isCooldownSkill1) ||
               (Skil == 2 && B.Instance.isCooldownSkill2) ||
               (Skil == 3 && B.Instance.isCooldownSkill3);
    }

    public void Begin(PointerEventData eventData)
    {
        if (IsCooldown()) return;
        ProcessDrag(eventData);
    }

    public void ProcessDrag(PointerEventData eventData)
    {
        if (IsCooldown()) return;

        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out pos
        );

        Vector2 clamped = Vector2.ClampMagnitude(pos, radius);
        handle.anchoredPosition = clamped;

        float mag = clamped.magnitude;
        Power = (radius <= 0.0001f) ? 0f : Mathf.Clamp01(mag / radius);

        Direction = (mag > 0.0001f) ? (clamped / mag) : Vector2.zero;
    }

    public void End(PointerEventData eventData)
    {
        handle.anchoredPosition = startPos;
        Direction = Vector2.zero;
        Power = 0f;
    }

    public void Show()
    {
        BgJt.SetActive(true);
    }

    public void Hide()
    {
        BgJt.SetActive(false);
        handle.anchoredPosition = startPos;
        Direction = Vector2.zero;
        Power = 0f;
    }
}