using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class HidePlaceholderOnFocus : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    private InputField _input;
    private TMP_InputField _tmpInput;
    private GameObject _placeholderGO;

    void Awake()
    {
        _input = GetComponent<InputField>();
        if (_input != null && _input.placeholder != null)
        {
            _placeholderGO = _input.placeholder.gameObject;
            return;
        }

        _tmpInput = GetComponent<TMP_InputField>();
        if (_tmpInput != null && _tmpInput.placeholder != null)
        {
            _placeholderGO = _tmpInput.placeholder.gameObject;
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (_placeholderGO != null)
            _placeholderGO.SetActive(false);

        if (_input != null)
        {
            _input.ActivateInputField();
            _input.Select();
        }
        else if (_tmpInput != null)
        {
            _tmpInput.ActivateInputField();
            _tmpInput.Select();
        }
    }

    public void OnDeselect(BaseEventData eventData)
    {
        bool hasText = false;

        if (_input != null)
            hasText = !string.IsNullOrEmpty(_input.text);
        else if (_tmpInput != null)
            hasText = !string.IsNullOrEmpty(_tmpInput.text);

        if (!hasText && _placeholderGO != null)
            _placeholderGO.SetActive(true);
    }
}
