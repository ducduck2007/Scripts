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

    void Awake()
    {
        startPos = handle.anchoredPosition;
        Direction = Vector2.zero;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (Skil == 1 && B.Instance.isCooldownSkill1 || Skil == 2 && B.Instance.isCooldownSkill2 || Skil == 3 && B.Instance.isCooldownSkill3)
        {
            return;
        }
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out pos
        );

        pos = Vector2.ClampMagnitude(pos, radius);
        handle.anchoredPosition = pos;
        Direction = pos.normalized;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        handle.anchoredPosition = startPos;
        Direction = Vector2.zero;
    }
    
    public void Show() 
    {
        // gameObject.SetActive(true);
        BgJt.SetActive(true);
    }

    public void Hide()
    {
        BgJt.SetActive(false);
        // gameObject.SetActive(false);
        handle.anchoredPosition = startPos;
        Direction = Vector2.zero;
    }
}
