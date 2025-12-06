using Newtonsoft.Json.Linq;
using System.Collections.Generic;

public class JObjectCustom
{
    private JToken _jo;

    public static JObjectCustom From(string json)
    {
        return new JObjectCustom(JObject.Parse(json));
    }
#if UNITY_EDITOR
    public JObjectCustom(JToken jToken)
    {
        _jo = jToken; data = _jo.ToObject<Dictionary<string, object>>();
        _json = jToken.ToString();
    }
    private Dictionary<string, object> data = new Dictionary<string, object>();
    private Dictionary<string, int> dicKeyUsed = new Dictionary<string, int>();
    /*** Debug ***/
    private string _json;
    public string GetJson()
    {
        if (_json != null)
        {
            return _json;
        }
        return "";
    }
    /*** End Debug ***/
    public string GetKeyNew()
    {
        System.Text.StringBuilder builder = new System.Text.StringBuilder();
        foreach (var used in dicKeyUsed.Keys)
        {
            data.Remove(used);
        }
        foreach (var keyNotUsed in data)
        {
            builder.Append(keyNotUsed.Key).Append(" : ").Append(keyNotUsed.Value).Append("\n");
        }
        return builder.ToString();
    }
    private void AddKeyUsed(string val)
    {
        if (!dicKeyUsed.ContainsKey(val))
            dicKeyUsed.Add(val, 0);
    }
    private bool CheckKey(string key) { return data.ContainsKey(key); }
    public T GetObject<T>(string key) { if (CheckKey(key)) { AddKeyUsed(key); return _jo[key].ToObject<T>(); } else { DebugLog(key); return default(T); } }
    public int GetInt(string key) { if (CheckKey(key)) { AddKeyUsed(key); return _jo[key].ToObject<int>(); } else { DebugLog(key); return 0; } }
    public int[] GetIntArray(string key) { if (CheckKey(key)) { AddKeyUsed(key); return JArray.Parse(_jo[key].ToString()).ToObject<int[]>(); } else { DebugLog(key); return null; } }
    public long GetLong(string key) { if (CheckKey(key)) { AddKeyUsed(key); return _jo[key].ToObject<long>(); } else { DebugLog(key); return 0L; } }
    public long[] GetLongArray(string key) { if (CheckKey(key)) { AddKeyUsed(key); return JArray.Parse(_jo[key].ToString()).ToObject<long[]>(); } else { DebugLog(key); return null; } }
    public float GetFloat(string key) { if (CheckKey(key)) { AddKeyUsed(key); return _jo[key].ToObject<float>(); } else { DebugLog(key); return 0F; } }
    public float[] GetFloatArray(string key) { if (CheckKey(key)) { AddKeyUsed(key); return JArray.Parse(_jo[key].ToString()).ToObject<float[]>(); } else { DebugLog(key); return null; } }
    public double GetDouble(string key) { if (CheckKey(key)) { AddKeyUsed(key); return _jo[key].ToObject<double>(); } else { DebugLog(key); return 0; } }
    public double[] GetDoubleArray(string key) { if (CheckKey(key)) { AddKeyUsed(key); return JArray.Parse(_jo[key].ToString()).ToObject<double[]>(); } else { DebugLog(key); return null; } }
    public string GetString(string key) { if (CheckKey(key)) { AddKeyUsed(key); return _jo[key].ToString(); } else { DebugLog(key); return string.Empty; } }
    public string[] GetStringArray(string key) { if (CheckKey(key)) { AddKeyUsed(key); return JArray.Parse(_jo[key].ToString()).ToObject<string[]>(); } else { DebugLog(key); return null; } }
    public bool GetBool(string key) { if (CheckKey(key)) { AddKeyUsed(key); return _jo[key].ToObject<bool>(); } else { DebugLog(key); return false; } }
    public bool[] GetBoolArray(string key) { if (CheckKey(key)) { AddKeyUsed(key); return JArray.Parse(_jo[key].ToString()).ToObject<bool[]>(); } else { DebugLog(key); return null; } }
    public ArrayObject GetArrayObject(string key)
    {
        if (CheckKey(key)) { AddKeyUsed(key); return (ArrayObject)data[key]; } else { DebugLog(key); return null; }
    }
    public JArray GetJArray(string key)
    {
        if (CheckKey(key)) { AddKeyUsed(key); return JArray.Parse(data[key].ToString()); } else { DebugLog(key); return null; }
    }
    public JObjectCustom GetJObjectCustom(string key)
    {
        if (CheckKey(key)) { AddKeyUsed(key); return new JObjectCustom(JObject.Parse(data[key].ToString())); } else { DebugLog(key); return null; }
    }
    public T GetClass<T>(string key)
    {
        if (CheckKey(key)) { AddKeyUsed(key); return JObject.Parse(data[key].ToString()).ToObject<T>();/*return (T)data[key];*/} else { DebugLog(key); return default(T); }
    }
    public T[] GetClassArray<T>(string key)
    {
        if (CheckKey(key)) { AddKeyUsed(key); return JArray.Parse(data[key].ToString()).ToObject<T[]>(); } else { DebugLog(key); return null; }
    }
    public List<T> GetClassList<T>(string key)
    {
        if (CheckKey(key)) { AddKeyUsed(key); return JArray.Parse(data[key].ToString()).ToObject<List<T>>(); } else { DebugLog(key); return null; }
    }
    public T[][] GetClassArrayJagged<T>(string key)
    {
        if (CheckKey(key)) { AddKeyUsed(key); return JArray.Parse(data[key].ToString()).ToObject<T[][]>(); }
        else { DebugLog(key); return null; }
    }
    public T[,] GetClassArrayMatrix<T>(string key)
    {
        if (CheckKey(key)) { AddKeyUsed(key); return JArray.Parse(data[key].ToString()).ToObject<T[,]>(); }
        else { DebugLog(key); return null; }
    }
    private void DebugLog(string key)
    {
        AgentUnity.LogWarning("Not Found Key: " + key + " - Json: " + GetJson());
    }
#else
    public JObjectCustom(JToken jToken) { _jo = jToken; }
    public T GetObject<T>(string key) { return _jo[key].ToObject<T>(); }
    public int GetInt(string key) { return _jo[key].ToObject<int>(); }
    public int[] GetIntArray(string key) { return JArray.Parse(_jo[key].ToString()).ToObject<int[]>(); }
    public long GetLong(string key) { return _jo[key].ToObject<long>(); }
    public long[] GetLongArray(string key) { return JArray.Parse(_jo[key].ToString()).ToObject<long[]>(); }
    public float GetFloat(string key) { return _jo[key].ToObject<float>(); }
    public float[] GetFloatArray(string key) { return JArray.Parse(_jo[key].ToString()).ToObject<float[]>(); }
    public double GetDouble(string key) { return _jo[key].ToObject<double>(); }
    public double[] GetDoubleArray(string key) { return JArray.Parse(_jo[key].ToString()).ToObject<double[]>(); }
    public string GetString(string key) { return _jo[key].ToString(); }
    public string[] GetStringArray(string key) { return JArray.Parse(_jo[key].ToString()).ToObject<string[]>(); }
    public bool GetBool(string key) { return _jo[key].ToObject<bool>(); }
    public bool[] GetBoolArray(string key) { return JArray.Parse(_jo[key].ToString()).ToObject<bool[]>(); }
    public ArrayObject GetArrayObject(string key)
    {
        return _jo[key].ToObject<ArrayObject>();
    }
    public JArray GetJArray(string key) { return JArray.Parse(_jo[key].ToString()); }
    public JObjectCustom GetJObjectCustom(string key) { return new JObjectCustom(JObject.Parse(_jo[key].ToString())); }
    public T GetClass<T>(string key) { return JObject.Parse(_jo[key].ToString()).ToObject<T>(); }
    public T[] GetClassArray<T>(string key) { return JArray.Parse(_jo[key].ToString()).ToObject<T[]>(); }
    public List<T> GetClassList<T>(string key) { return JArray.Parse(_jo[key].ToString()).ToObject<List<T>>(); }
    public T[][] GetClassArrayJagged<T>(string key) { return JArray.Parse(_jo[key].ToString()).ToObject<T[][]>(); }
    public T[,] GetClassArrayMatrix<T>(string key) { return JArray.Parse(_jo[key].ToString()).ToObject<T[,]>(); }
    public string GetKeyNew()
    { return string.Empty; }
#endif
}
