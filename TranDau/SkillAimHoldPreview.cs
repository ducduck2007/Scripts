using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class SkillAimHoldPreview : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public int skillIndex = 1;
    public float delay = 0.25f;

    private Coroutine _co;
    private bool _aimShown;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_co != null) StopCoroutine(_co);
        _co = StartCoroutine(CoWaitThenShow());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        StopAndHide();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StopAndHide();
    }

    private IEnumerator CoWaitThenShow()
    {
        yield return new WaitForSeconds(delay);

        // Sau 0.5s:
        // - Nếu user click nhanh (auto=1) thì đã PointerUp trước đó => coroutine bị Stop rồi.
        // - Nếu còn giữ (auto=0) => mới show aim canvas theo logic hiện tại.
        if (MenuController.Instance != null)
        {
            MenuController.Instance.ShowAimCanvasForSkill(skillIndex);
            _aimShown = true;
        }

        _co = null;
    }

    private void StopAndHide()
    {
        if (_co != null)
        {
            StopCoroutine(_co);
            _co = null;
        }

        if (_aimShown)
        {
            _aimShown = false;
            if (MenuController.Instance != null)
                MenuController.Instance.HideAllAimCanvases();
        }
    }
}