using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

[DisallowMultipleComponent]
public class HidePlaceholderOnFocus : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    private InputField input;
    private TMP_InputField tmpInput;
    private GameObject placeholderGO;

    private Coroutine focusCo;

    void Awake()
    {
        input = GetComponent<InputField>();
        if (input != null)
        {
            if (input.placeholder != null)
                placeholderGO = input.placeholder.gameObject;
            return;
        }

        tmpInput = GetComponent<TMP_InputField>();
        if (tmpInput != null && tmpInput.placeholder != null)
            placeholderGO = tmpInput.placeholder.gameObject;
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (placeholderGO != null) placeholderGO.SetActive(false);

        if (focusCo != null) StopCoroutine(focusCo);
        focusCo = StartCoroutine(CoForceCaretNextFrames());
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (focusCo != null)
        {
            StopCoroutine(focusCo);
            focusCo = null;
        }

        bool hasText =
            input != null ? !string.IsNullOrEmpty(input.text) :
            tmpInput != null && !string.IsNullOrEmpty(tmpInput.text);

        if (!hasText && placeholderGO != null)
            placeholderGO.SetActive(true);
    }

    private IEnumerator CoForceCaretNextFrames()
    {
        if (input != null)
        {
            input.ActivateInputField();
        }
        else if (tmpInput != null)
        {
            tmpInput.ActivateInputField();
        }

        yield return null;

        if (input != null)
        {
            input.ActivateInputField();
            input.caretPosition = input.text.Length;

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(input.transform as RectTransform);
        }
        else if (tmpInput != null)
        {
            tmpInput.ActivateInputField();
            tmpInput.ForceLabelUpdate();
            tmpInput.caretPosition = tmpInput.text.Length;

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(tmpInput.transform as RectTransform);
        }

        yield return null;

        if (input != null)
        {
            input.ActivateInputField();
            input.caretPosition = input.text.Length;
        }
        else if (tmpInput != null)
        {
            tmpInput.ActivateInputField();
            tmpInput.ForceLabelUpdate();
            tmpInput.caretPosition = tmpInput.text.Length;
        }

        focusCo = null;
    }
}
