using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

[DisallowMultipleComponent]
public class FakeCaretInput : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [Header("Refs")]
    public RectTransform fakeCaret;
    public float blinkInterval = 0.5f;

    [Header("Layout")]
    public float leftPadding = 6f;
    public float rightPadding = 2f; // tránh caret vượt quá khung

    private InputField input;
    private TMP_InputField tmpInput;
    private Coroutine blinkCo;

    void Awake()
    {
        input = GetComponent<InputField>();
        tmpInput = GetComponent<TMP_InputField>();

        if (fakeCaret != null)
            fakeCaret.gameObject.SetActive(false);

        if (input != null) input.caretWidth = 0;
        if (tmpInput != null) tmpInput.caretWidth = 0;
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (fakeCaret == null) return;

        fakeCaret.gameObject.SetActive(true);
        UpdateCaretPosition();

        if (blinkCo != null) StopCoroutine(blinkCo);
        blinkCo = StartCoroutine(CoBlink());
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (blinkCo != null)
        {
            StopCoroutine(blinkCo);
            blinkCo = null;
        }

        if (fakeCaret != null)
            fakeCaret.gameObject.SetActive(false);
    }

    void Update()
    {
        if (fakeCaret != null && fakeCaret.gameObject.activeSelf)
            UpdateCaretPosition();
    }

    private void UpdateCaretPosition()
    {
        if (fakeCaret == null) return;

        // ===== TMP_InputField =====
        if (tmpInput != null && tmpInput.textComponent != null)
        {
            TMP_Text t = tmpInput.textComponent;
            string text = tmpInput.text ?? "";

            // width theo font TMP chính xác hơn preferredWidth
            float w = t.GetPreferredValues(text).x;

            // position theo textViewport/textArea
            RectTransform textRT = t.rectTransform;
            float maxW = textRT.rect.width;

            float x = Mathf.Min(leftPadding + w, maxW - rightPadding);

            Vector2 pos = fakeCaret.anchoredPosition;
            pos.x = x;
            fakeCaret.anchoredPosition = pos;
            return;
        }

        // ===== Legacy InputField =====
        if (input != null && input.textComponent != null)
        {
            Text t = input.textComponent;
            string text = input.text ?? "";

            float w = t.preferredWidth;
            RectTransform textRT = t.rectTransform;
            float maxW = textRT.rect.width;

            float x = Mathf.Min(leftPadding + w, maxW - rightPadding);

            Vector2 pos = fakeCaret.anchoredPosition;
            pos.x = x;
            fakeCaret.anchoredPosition = pos;
        }
    }

    private IEnumerator CoBlink()
    {
        while (true)
        {
            fakeCaret.gameObject.SetActive(!fakeCaret.gameObject.activeSelf);
            yield return new WaitForSecondsRealtime(blinkInterval);
        }
    }
}
