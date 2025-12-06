using UnityEngine;

public class ServiceWrapper<T>
{
    public static T instance;
}

public partial class Service
{
    public static void Set<T>(T t)
    {
        if (IsSet<T>())
        {
            Debug.Log("Service has setted: " + typeof(T));
        }
        else
        {
            ServiceWrapper<T>.instance = t;
        }
    }

    public static bool IsSet<T>()
    {
        return ServiceWrapper<T>.instance != null;
    }

    public static T Get<T>()
    {
        if (!IsSet<T>())
        {
            Debug.LogError("Service has not setted: " + typeof(T));
        }

        return ServiceWrapper<T>.instance;
    }

    public static void Unset<T>()
    {
        ServiceWrapper<T>.instance = default;
    }
}