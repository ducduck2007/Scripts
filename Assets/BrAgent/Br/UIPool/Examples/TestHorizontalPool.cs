using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class TestHorizontalPool : MonoBehaviour
{
    public UIPool.HorizontalPoolGroup _horiPool;
    List<int> listPool = new List<int>();
    // Use this for initialization
    void Start()
    {
        for (int i = 0; i < 100; i++)
        {
            listPool.Add(i);
        }
        // init pool
        _horiPool.HowToUseCellData(delegate (GameObject go, object data)
        {
            go.GetComponentInChildren<Text>().text = "item " + (int)data;
        });

        // init use prefab
        _horiPool.HowToUseCellPrefab(delegate (int index)
        {
            int data = (int)_horiPool.GetCellData(index);
            if (data % 2 == 0)
            {
                return _horiPool.GetCellPrefab(0);
            }
            else
            {
                return _horiPool.GetCellPrefab(1);
            }
        });

        // init size prefab
        _horiPool.HowToUseCellSize(delegate (int index)
        {
            int data = (int)_horiPool.GetCellData(index);
            if (data % 2 == 0)
            {
                return new Vector2(100, 100);
            }
            else
            {
                return new Vector2(100, 50);
            }
        });

        // 
        _horiPool.SetAdapter(listPool.OfType<object>().ToList());
    }
}
