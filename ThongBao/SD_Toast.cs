using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class SD_Toast : ScaleScreen
{
    private RectTransform objTran;
    public TextMeshProUGUI txtContent;
    public GameObject obj;
    public Vector3 currentTranform;
    private void Awake()
    {
        gameObject.SetActive(false);
        objTran = obj.GetComponent<RectTransform>();
        currentTranform = objTran.position;
    }

    public void MakeToast(string message, float time = 2.5f)
    {
        txtContent.text = message;
        GameObject toastGo = AgentUnity.InstanceObject(obj, ThongBaoController.Instance.transform, currentTranform);
        toastGo.transform.localPosition = new Vector2(0, -186);
        toastGo.transform.localScale = new Vector2(1.3f, 1.3f);
        toastGo.transform.DOKill();
        toastGo.transform.DOLocalMoveY(180, time);
        toastGo.transform.DOScale(new Vector3(0.8f, 0.8f), time).OnComplete(() =>
        {
            toastGo.transform.DOKill();
            toastGo.transform.DOScale(new Vector3(0.1F,0.1F,0.1F),0.3F ).OnComplete(() =>
            {
                toastGo.transform.DOKill();
                Destroy(toastGo);
            });
        });
    }

    private IEnumerator _iEShowToast;
    public void ShowToast(string message, int valueY = 100, float time = 1.5f, float timeDestroy = 2f)
    {
        gameObject.SetActive(true);
        if (_iEShowToast != null)
        {
            StopCoroutine(_iEShowToast);
            _iEShowToast = IeShowToast(message, valueY, time, timeDestroy);
            StartCoroutine(_iEShowToast);
        }
        else
        {
            _iEShowToast = IeShowToast(message, valueY, time, timeDestroy);
            StartCoroutine(_iEShowToast);
        }
    }

    private IEnumerator IeShowToast(string message, int valueY = 100, float time = 1.5f, float timeDestroy = 2f)
    {
        txtContent.text = message;
        obj.transform.localPosition = new Vector2(0, -100);
        obj.transform.localScale = new Vector3(1.1f,1.1f);
        obj.transform.DOKill();
        obj.transform.DOLocalMoveY(valueY, time);
        obj.transform.DOScale(new Vector3(0.85f, 0.85f), time);
        yield return new WaitForSeconds(timeDestroy);
        gameObject.SetActive(false);
    }
}