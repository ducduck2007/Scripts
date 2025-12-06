using UnityEngine;

public class AutoSingleton<T> : MonoBehaviour where T : AutoSingleton<T>
{
    protected static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new GameObject(typeof(T).Name).AddComponent<T>();
            }    
            return _instance;
        }
    }
    
    protected virtual void Awake()
    {
        if (_instance != null && _instance.GetInstanceID() != GetInstanceID())
            Destroy(gameObject);
        else _instance = this as T;
    }

    protected virtual void OnDestroy()
    {
        if (_instance == this) _instance = null;
    }
}