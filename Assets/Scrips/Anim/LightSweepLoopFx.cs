using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class LightSweepLoopFx : MonoBehaviour
{
    [SerializeField] private float delayTime;
    [SerializeField] private float fxDuration;
    [SerializeField] private float fxPos;

//    private void Start()
//    {
//        StartCoroutine(MovingLoop(0.1f));
//    }

    private void OnEnable()
    {
        StartCoroutine(MovingLoop(delayTime));
    }

    private IEnumerator MovingLoop(float waitTime)
    {
        while (true)
        {
            transform.localPosition = new Vector3(-fxPos, 0, 0);
            yield return new WaitForSeconds(waitTime);
            transform.DOLocalMove(new Vector3(fxPos, 0, 0), fxDuration);
        }
    }
}