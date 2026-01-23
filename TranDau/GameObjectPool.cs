using System.Collections.Generic;
using UnityEngine;

public class GameObjectPool
{
    private Queue<GameObject> pool = new Queue<GameObject>();
    private GameObject prefab;
    private Transform parent;

    public GameObjectPool(GameObject prefab, int preload, Transform parent = null)
    {
        this.prefab = prefab;
        this.parent = parent;

        for (int i = 0; i < preload; i++)
        {
            GameObject obj = GameObject.Instantiate(prefab, parent);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public GameObject Get()
    {
        GameObject obj = pool.Count > 0
            ? pool.Dequeue()
            : GameObject.Instantiate(prefab, parent);

        obj.SetActive(true);
        return obj;
    }

    public void Release(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}
