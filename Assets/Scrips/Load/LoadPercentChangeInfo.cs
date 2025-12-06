using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class LoadPercentChangeInfo : MonoBehaviour
{
    public Image _imgPercentLoad;
    public TextMeshProUGUI _txtContent;
    public GameObject bgIos, bgAndroid;

    public void Start()
    {
#if UNITY_IOS
        bgIos.gameObject.SetActive(true);
        bgAndroid.gameObject.SetActive(false);
#else
        bgIos.gameObject.SetActive(false);
        bgAndroid.gameObject.SetActive(true);
#endif
    }

    internal void ShowLoadPercent(bool isShow)
    {
        gameObject.SetActive(isShow);
        if (isShow)
        {
            int random = Random.Range(0,4);
            //Debug.LogError("random = " +random);
            //imgBg.sprite = PathResource.GetSpriteBgLoading(random);
            SetPercent(0F);
            ShowTextRandom();
            StartCoroutine(CheckChangeTextShow());
        }
        else this.StopAllCoroutines();
    }

    private IEnumerator CheckChangeTextShow()
    {
        while (gameObject.activeInHierarchy)
        {
            yield return new WaitForSeconds(2.5F);
            ShowTextRandom();
        }

        yield return null;
    }


    private void ShowTextRandom()
    {
        // if (B.Instance.ListTextLoadInfo.Count > 0)
        // {
        //     _txtContent.text = B.Instance.ListTextLoadInfo[Random.Range(0, B.Instance.ListTextLoadInfo.Count)].info;
        // }
        // else
        {
            _txtContent.text = "";
        }
    }


    internal void UpdatePercent(float percent)
    {
        SetPercent(percent);
    }

    private void SetPercent(float val)
    {
        _imgPercentLoad.fillAmount = val;

        switch (val)
        {
            case 0.5f:
                break;
            case 1:
                //SendData.OnTokenFireBase(B.Instance.tokenFireBase);
                Destroy(gameObject, 0.5f);
                break;
        }
    }
    
    internal void ShowLoadImage()
    {
        gameObject.SetActive(true);
        _imgPercentLoad.transform.parent.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        _imgPercentLoad.transform.parent.gameObject.SetActive(false);
        // if (UserData.Instance.CurrentStepGuide < 26)
        // {
        //     UIControl.Instance.InitGuide();
        //     SendData.OnJoinScene(TypeKhuVuc.HUONG_DAN);
        // }
        // else
        // {
        //     SendData.OnJoinScene(TypeKhuVuc.NOI_THANH);   
        // }
        //
        // if (UserData.Instance.stepSauHuongDan < 96 && UserData.Instance.stepSauHuongDan > 26)
        // {
        //     if (UserData.Instance.stepSauHuongDan > C.STEP_GUIDE_LVL16)
        //     {
        //         MenuController.Instance.MainMenu.SetClickMapQuocChien();
        //         GuideQcController.Instance.GuidePlayerController.gameObject.SetActive(true);
        //         GlobalCoroutine.InvokeDelay(1.6f, () =>
        //         {
        //             GuidePlayerController.Instance.SetCountClick(0);
        //             GuidePlayerController.Instance.StartMissionGuide();
        //         });
        //     }
        //     else
        //     {
        //         if (OnOffDialog.Instance.IsOnGuidePlayController)
        //         {
        //             GuidePlayerController.Instance.gameObject.SetActive(true);
        //         }
        //         else
        //         {
        //             UIControl.Instance.InitGuide();
        //         }
        //         GuidePlayerController.Instance.SetCountClick(0);
        //         GuidePlayerController.Instance.StartMissionGuide();
        //     }
        // }
        // if (AudioManager.Instance.GetMusicConfig()) SoundBG.Instance.PlayInGameBG();
    }

}