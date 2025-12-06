using UnityEngine;

namespace UIPool
{
    public class PoolObject
    {
        public int index { get; set; }

        public string prefabName { get; set; }

        public bool isAvailable { get; set; }

        public GameObject gameObj { get; set; }

        public PoolObject()
        {
            index = -1;
            prefabName = "";
            isAvailable = false;//true: out of bounds, false: in bounds
            gameObj = null;
        }

        public void RecycleObject()
        {
            index = -1;
            isAvailable = true;
            if (gameObj != null)
            {
                gameObj.SetActive(false);
            }
        }
    }

}