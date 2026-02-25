using Newtonsoft.Json.Linq;
using System.Collections.Generic;

public static class MessageExtensions
{
    public static JArray GetArrayJson(this Message msg, string key)
    {
        try
        {
            if (!msg.ConstainsKey(key)) return null;

            // Lấy data dạng object
            object data = msg.GetObject(key);
            if (data == null) return null;

            // Parse sang JArray
            return JArray.Parse(data.ToString());
        }
        catch (System.Exception e)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.LogError($"[MessageExtensions] GetArrayJson error for key '{key}': {e.Message}");
#endif
            return null;
        }
    }
}