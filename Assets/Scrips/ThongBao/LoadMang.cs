using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadMang : MonoBehaviour
{
    public void Show(bool val = true)
    {
        if (val)
        {
            gameObject.SetActive(true);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        // OnOffDialog.Instance.isOnLoadMang = true;
    }

    private void OnDisable()
    {
        // OnOffDialog.Instance.isOnLoadMang = false;
    }
}
