using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BaseDialog : MonoBehaviour
{
    [SerializeField] protected Button _btnClose;
    [SerializeField] protected Button _btnExit;

    public GameObject bg;
    public Transform tranScale;

    [SerializeField] protected TextMeshProUGUI txtTitle;

    protected virtual void Awake()
    {
        if (bg != null)
            AgentUnity.ScaleBg(bg);

        if (tranScale != null)
            AgentUnity.ScaleTranform(tranScale);

        if (_btnClose != null)
            _btnClose.onClick.AddListener(Close);

        if (_btnExit != null)
            _btnExit.onClick.AddListener(Close);
    }

    protected void Open()
    {
        gameObject.SetActive(true);
        transform.SetAsLastSibling();
    }

    protected void Close()
    {
        gameObject.SetActive(false); // KHÔNG Destroy
    }

    public void Show(bool val = true)
    {
        gameObject.SetActive(val);
    }
}