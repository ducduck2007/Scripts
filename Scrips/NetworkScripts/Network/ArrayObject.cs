using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

public class ArrayObject
{
    private int i = 0;
    public Dictionary<int, object> data = new Dictionary<int, object>();

    public ArrayObject() { }
    public ArrayObject(string json)
    {
        try
        {
            JObject dt = JObject.Parse(json);
            data = dt.GetValue(Key.DATA).ToObject<Dictionary<int, object>>();
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogWarning(e);
        }
    }
    public void AddInt(int val)
    {
        data.Add(i++, val);
    }
    public void AddIntArray(int[] val)
    {
        data.Add(i++, val);
    }
    public void AddLong(long val)
    {
        data.Add(i++, val);
    }
    public void AddLongArray(long[] val)
    {
        data.Add(i++, val);
    }
    public void AddFloat(float val)
    {
        data.Add(i++, val);
    }
    public void AddFloatArray(float[] val)
    {
        data.Add(i++, val);
    }
    public void AddDouble(double val)
    {
        data.Add(i++, val);
    }
    public void AddDoubleArray(double[] val)
    {
        data.Add(i++, val);
    }
    public void AddString(string val)
    {
        data.Add(i++, val);
    }
    public void AddStringArray(string[] val)
    {
        data.Add(i++, val);
    }
    public void AddBool(bool val)
    {
        data.Add(i++, val);
    }
    public void AddBoolArray(bool[] val)
    {
        data.Add(i++, val);
    }
    public void AddObject(object val)
    {
        data.Add(i++, val);
    }
    public void AddObjectArray(object[] val)
    {
        data.Add(i++, val);
    }

    public string GetString()
    {
        return data[i++].ToString();
    }
    public string[] GetStringArray()
    {
        return JArray.Parse(data[i++].ToString()).ToObject<string[]>();
    }
    public int GetInt()
    {
        return Convert.ToInt32(data[i++]);
    }
    public int[] GetIntArray()
    {
        return JArray.Parse(data[i++].ToString()).ToObject<int[]>();
    }
    public long GetLong()
    {
        return Convert.ToInt64(data[i++]);
    }
    public long[] GetLongArray()
    {
        return JArray.Parse(data[i++].ToString()).ToObject<long[]>();
    }
    public bool GetBool()
    {
        return Convert.ToBoolean(data[i++]);
    }
    public bool[] GetBoolArray()
    {
        return JArray.Parse(data[i++].ToString()).ToObject<bool[]>();
    }
    public float GetFloat()
    {
        return float.Parse(data[i++].ToString());
    }
    public float[] GetFloatArray()
    {
        return JArray.Parse(data[i++].ToString()).ToObject<float[]>();
    }
    public double GetDouble()
    {
        return Convert.ToDouble(data[i++]);
    }
    public double[] GetDoubleArray()
    {
        return JArray.Parse(data[i++].ToString()).ToObject<double[]>();
    }
    public object GetObject()
    {
        return data[i++];
    }
    public object[] GetObjectArray()
    {
        return JArray.Parse(data[i++].ToString()).ToObject<object[]>();
    }
    public bool ConstainsKey(int key)
    {
        return data.ContainsKey(key);
    }
}
