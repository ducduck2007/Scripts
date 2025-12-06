using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class GetDataKeyLocal
{
    protected static GetDataKeyLocal instance;

    internal static GetDataKeyLocal Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new GetDataKeyLocal();
            }

            return instance;
        }
    }

    public Dictionary<string, object> Data = new Dictionary<string, object>();
    private JObject _jo;

    public T GetObject<T>(string jsonString, string key)
    {
        _jo = JObject.Parse(jsonString);
        Data = _jo.GetValue(Key.DATA).ToObject<Dictionary<string, object>>();
        if (Data.ContainsKey(key)) return _jo.GetValue(Key.DATA)[key].ToObject<T>();
        else
        {
            return default(T);
        }
    }

    public T GetClass<T>(string jsonString)
    {
        _jo = JObject.Parse(jsonString);
        return _jo[Key.DATA].ToObject<T>();
    }

    public T GetClass<T>(string jsonString, string key)
    {
        _jo = JObject.Parse(jsonString);
        Data = _jo.GetValue(Key.DATA).ToObject<Dictionary<string, object>>();
        if (Data.ContainsKey(key)) return JObject.Parse(Data[key].ToString()).ToObject<T>();
        else
        {
            return default(T);
        }
    }

    public T[] GetClassArray<T>(string jsonString, string key)
    {
        _jo = JObject.Parse(jsonString);
        Data = _jo.GetValue(Key.DATA).ToObject<Dictionary<string, object>>();
        if (Data.ContainsKey(key)) return JArray.Parse(Data[key].ToString()).ToObject<T[]>();
        else
        {
            AgentUnity.LogError("NULL NO DATA");
            return null;
        }
    }

    public List<T> GetClassList<T>(string jsonString, string key)
    {
        _jo = JObject.Parse(jsonString);
        Data = _jo.GetValue(Key.DATA).ToObject<Dictionary<string, object>>();
        if (Data.ContainsKey(key)) return JArray.Parse(Data[key].ToString()).ToObject<List<T>>();
        else
        {
            return null;
        }
    }

    public T[,] GetClassArrayMatrix<T>(string jsonString, string key)
    {
        _jo = JObject.Parse(jsonString);
        Data = _jo.GetValue(Key.DATA).ToObject<Dictionary<string, object>>();
        if (Data.ContainsKey(key)) return JArray.Parse(Data[key].ToString()).ToObject<T[,]>();
        else
        {
            return null;
        }
    }

    public bool GetBool(string jsonString, string key)
    {
        _jo = JObject.Parse(jsonString);
        Data = _jo.GetValue(Key.DATA).ToObject<Dictionary<string, object>>();
        if (Data.ContainsKey(key)) return Convert.ToBoolean(Data[key]);
        else return false;
    }

    public string GetString(string jsonString, string key)
    {
        _jo = JObject.Parse(jsonString);
        Data = _jo.GetValue(Key.DATA).ToObject<Dictionary<string, object>>();
        if (Data.ContainsKey(key)) return Data[key].ToString();
        else return string.Empty;
    }

    public long GetLong(string jsonString, string key)
    {
        _jo = JObject.Parse(jsonString);
        Data = _jo.GetValue(Key.DATA).ToObject<Dictionary<string, object>>();
        if (Data.ContainsKey(key)) return Convert.ToInt64(Data[key]);
        else return 0;
    }

    public int GetInt(string jsonString, string key)
    {
        _jo = JObject.Parse(jsonString);
        Data = _jo.GetValue(Key.DATA).ToObject<Dictionary<string, object>>();
        if (Data.ContainsKey(key)) return Convert.ToInt32(Data[key]);
        else return 0;
    }

    public int[] GetIntArray(string jsonString, string key)
    {
        _jo = JObject.Parse(jsonString);
        Data = _jo.GetValue(Key.DATA).ToObject<Dictionary<string, object>>();
        if (Data.ContainsKey(key)) return JArray.Parse(Data[key].ToString()).ToObject<int[]>();
        else return null;
    }

    private int lengMinData = 10;
    
    // public void GetDataInfoCmd60()
    // {
    //     try
    //     {
    //         string json = AgentUnity.ReadFile(KeyLocalSave.PP_TEXT_CHAT_NHANH_INFO);
    //         ChatDataBase.Instance.ListTextChatNhanh.Clear();
    //         ChatDataBase.Instance.ListTextChatNhanh = GetClassList<TextChatNhanh>(json, "textInfo");
    //         ChatDataBase.Instance.ListTextChatNhanh.Sort((t1, t2) => t1.index.CompareTo(t2.index));
    //         if (OnOffDialog.Instance.isOnChatNhanh)
    //         {
    //             ChatController.Instance.Chat.chatNhanh.SetData();
    //         }
    //     }
    //     catch (Exception e)
    //     {
    //         SendData.OnTextChatNhanhInfo();
    //         AgentUnity.LogError(e);
    //     }
    // }
    
    

}