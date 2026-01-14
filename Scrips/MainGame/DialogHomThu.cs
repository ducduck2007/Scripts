using UnityEngine;
using UnityEngine.UI;

public class DialogHomThu : ScaleScreen
{
    public Button btnExit;

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    protected override void Start()
    {
        base.Start();
        btnExit.onClick.AddListener(SetExit);
    }

    private void SetExit()
    {
        AudioManager.Instance.AudioClick();
        Show(false);
    }

    public void Show(bool val = true)
    {
        gameObject.SetActive(val);
    }
}
