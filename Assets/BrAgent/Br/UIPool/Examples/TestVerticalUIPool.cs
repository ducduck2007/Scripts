using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class TestVerticalUIPool : MonoBehaviour
{

    public UIPool.HorizontalOrVerticalPoolGroup _verticalPool;
    List<int> listPool = new List<int>();
    // Use this for initialization
    void Start()
    {
        for (int i = 0; i < 100; i++)
        {
            listPool.Add(i);
        }
        _verticalPool.HowToUseCellData(delegate (GameObject go, object data)
        {
            go.GetComponentInChildren<Text>().text = (int)data + "";
        });

        _verticalPool.SetAdapter(listPool.OfType<object>().ToList());
    }

}
