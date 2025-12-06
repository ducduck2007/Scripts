using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class TestGripUIPool : MonoBehaviour
{
    public UIPool.GridPoolGroup _gripPool;
    List<int> listPool = new List<int>();
    // Use this for initialization
    void Start()
    {
        for (int i = 0; i < 100; i++)
        {
            listPool.Add(i);
        }
        // init pool
        _gripPool.HowToUseCellData(delegate (GameObject go, object data)
        {
            go.GetComponentInChildren<Text>().text = "item " + (int)data;
        });

        // 
        _gripPool.SetAdapter(listPool.OfType<object>().ToList());
    }

}
