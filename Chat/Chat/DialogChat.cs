using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class DialogChat : ScaleScreen
{
    public Button btnClose, btnExit;

    protected override void OnEnable()
    {
        base.OnEnable();
        AgentUnity.SetPositionGameObjectUI(transform, new Vector3(0, 0), true, 1f);
    }

    protected override void Start()
    {
        base.Start();
        btnExit.onClick.AddListener(SetExit);
        btnClose.onClick.AddListener(SetExit);
    }

    private void SetExit()
    {
        AudioManager.Instance.AudioClick();
        transform.DOLocalMove(new Vector3(-1200f, 0f, 0f), 1f)
         .OnComplete(() =>
         {
             Show(false);
         });
    }

    public void Show(bool val = true)
    {
        gameObject.SetActive(val);
    }
}
