using UnityEngine;

public class ManualSingleton<T> : MonoBehaviour where T : ManualSingleton<T>
{
    protected static T _instance;

    public static T Instance
    {
        get
        {
#if UNITY_EDITOR
            // if (_instance == null)
            // {
            //     Debug.LogErrorFormat("ManualSingleton: Cant not find: {0}", typeof(T).Name);
            // }
#endif
            return _instance;
        }
    }

    // protected virtual void Awake()
    // {
    //     if (_instance != null && _instance.GetInstanceID() != GetInstanceID())
    //         Destroy(gameObject);
    //     else _instance = this as T;
    // }
    
    protected bool isDuplicate = false;

    protected virtual void Awake()
    {
        if (_instance != null && _instance.GetInstanceID() != GetInstanceID())
        {
            isDuplicate = true;
            Destroy(gameObject);
            return;
        }

        _instance = this as T;
        // DontDestroyOnLoad(gameObject);
    }

    protected virtual void OnDestroy()
    {
        if (_instance == this) _instance = null;
    }
}