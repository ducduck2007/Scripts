#if NETFX_CORE || WINDOWS_PHONE// || UNITY_EDITOR
using System;
using System.Linq;
using System.Text;

public class Message
{
    public int command;
    public byte[] data = new byte[0];
    public Message(int command)
    {
        this.command = command;
    }
    public Message(byte[] data)
    {
        command = (int)data[0];
        byte[] dataTmp = new byte[data.Length - 3];
        dataTmp = data.Skip(3).ToArray();
        this.data = dataTmp;
    }

    public void writeUTF(String content)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(content);
        byte[] dataTmp = new byte[data.Length + bytes.Length + 2];
        data.CopyTo(dataTmp, 0);
        short size = (short)bytes.Length;
        byte[] sizeBytes = BitConverter.GetBytes(size);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(sizeBytes);
        sizeBytes.CopyTo(dataTmp, dataTmp.Length - 2 - bytes.Length);
        bytes.CopyTo(dataTmp, dataTmp.Length - bytes.Length);
        this.data = dataTmp;

    }
    public void writeInt(int value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes);
        byte[] dataTmp = new byte[data.Length + bytes.Length];
        data.CopyTo(dataTmp, 0);
        bytes.CopyTo(dataTmp, dataTmp.Length - bytes.Length);
        this.data = dataTmp;
    }
    public void writeFloat(float value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        byte[] byteArray = { bytes[3], bytes[2], bytes[1], bytes[0] };
        byte[] dataTmp = new byte[data.Length + bytes.Length];
        data.CopyTo(dataTmp, 0);
        byteArray.CopyTo(dataTmp, dataTmp.Length - bytes.Length);
        this.data = dataTmp;
    }
    public void writeByte(int value)
    {
        byte[] bytes = new byte[1];
        bytes[0] = (byte)value;
        byte[] dataTmp = new byte[data.Length + 1];
        data.CopyTo(dataTmp, 0);
        bytes.CopyTo(dataTmp, dataTmp.Length - bytes.Length);
        this.data = dataTmp;
    }
    public void writeShort(short value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes);
        byte[] dataTmp = new byte[data.Length + bytes.Length];
        data.CopyTo(dataTmp, 0);
        bytes.CopyTo(dataTmp, dataTmp.Length - bytes.Length);
        this.data = dataTmp;
    }
    public void writeBoolean(bool value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes);
        byte[] dataTmp = new byte[data.Length + bytes.Length];
        data.CopyTo(dataTmp, 0);
        bytes.CopyTo(dataTmp, dataTmp.Length - bytes.Length);
        this.data = dataTmp;
    }
    public void writeLong(long value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes);
        byte[] dataTmp = new byte[data.Length + bytes.Length];
        data.CopyTo(dataTmp, 0);
        bytes.CopyTo(dataTmp, dataTmp.Length - bytes.Length);
        this.data = dataTmp;
    }
    public void writeUnsignedShort(ushort value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes);
        byte[] dataTmp = new byte[data.Length + bytes.Length];
        data.CopyTo(dataTmp, 0);
        bytes.CopyTo(dataTmp, dataTmp.Length - bytes.Length);
        data = dataTmp;
    }
    public void write(byte[] bytes, int startIndex, int endIndex)
    {
        byte[] dataTmp = new byte[data.Length + bytes.Length];
        data.CopyTo(dataTmp, 0);
        bytes.CopyTo(dataTmp, dataTmp.Length - bytes.Length);
        this.data = dataTmp;
    }
    public string readUTF()
    {
        byte[] sizebytes = new byte[2];
        byte[] dataTmp = new byte[this.data.Length - 2];
        sizebytes = this.data.Take(2).ToArray();
        dataTmp = this.data.Skip(2).ToArray();
        this.data = dataTmp;
        short size = BitConverter.ToInt16(new byte[2] { sizebytes[1], sizebytes[0] }, 0);
        byte[] dataString = new byte[size];
        dataString = this.data.Take(size).ToArray();
        dataTmp = this.data.Skip(size).ToArray();
        this.data = dataTmp;
        return Encoding.UTF8.GetString(dataString, 0, size);
    }
    public int readInt()
    {
        byte[] dataInt = new byte[4];
        dataInt = this.data.Take(4).ToArray();
        byte[] dataTmp = this.data.Skip(4).ToArray();
        this.data = dataTmp;
        return BitConverter.ToInt32(new byte[4] { dataInt[3], dataInt[2], dataInt[1], dataInt[0] }, 0);
    }
    public float readFloat()
    {
        byte[] dataFloat = new byte[4];
        dataFloat = this.data.Take(4).ToArray();
        byte[] dataTmp = this.data.Skip(4).ToArray();
        this.data = dataTmp;
        byte[] byteArray = { dataFloat[3], dataFloat[2], dataFloat[1], dataFloat[0] };
        return System.BitConverter.ToSingle(byteArray, 0);
    }
    public long readLong()
    {
        byte[] dataLong = new byte[8];
        dataLong = this.data.Take(8).ToArray();
        byte[] dataTmp = this.data.Skip(8).ToArray();
        this.data = dataTmp;
        return BitConverter.ToInt64(new byte[8] { dataLong[7], dataLong[6], dataLong[5], dataLong[4], dataLong[3], dataLong[2], dataLong[1], dataLong[0] }, 0);
    }
    public short readShort()
    {
        byte[] dataShort = new byte[2];
        dataShort = this.data.Take(2).ToArray();
        byte[] dataTmp = this.data.Skip(2).ToArray();
        this.data = dataTmp;
        return BitConverter.ToInt16(new byte[2] { dataShort[1], dataShort[0] }, 0);
    }
    public sbyte readByte()
    {
        byte[] dataByte = new byte[1];
        dataByte = this.data.Take(1).ToArray();
        byte[] dataTmp = this.data.Skip(1).ToArray();
        this.data = dataTmp;
        return (sbyte)dataByte[0];
    }
    public bool readBoolean()
    {
        byte[] dataBoolean = new byte[1];
        dataBoolean = this.data.Take(1).ToArray();
        byte[] dataTmp = this.data.Skip(1).ToArray();
        this.data = dataTmp;
        return Convert.ToBoolean(dataBoolean[0]);
    }
    public ushort readUnsignedShort()
    {
        byte[] dataShort = new byte[2];
        dataShort = data.Take(2).ToArray();
        byte[] dataTmp = data.Skip(2).ToArray();
        data = dataTmp;
        return BitConverter.ToUInt16(new byte[2] { dataShort[1], dataShort[0] }, 0);
    }
    public void read(byte[] bytes, int startIndex, int endIndex)
    {
        bytes = data.Take(endIndex).ToArray();
        byte[] dataTmp = data.Skip(endIndex).ToArray();
        this.data = dataTmp;
    }
    public override bool Equals(object obj)
    {
        if (this == obj)
            return true;
        if (obj == null)
            return false;
        if (this.GetType() != obj.GetType())
            return false;
        Message other = (Message)obj;
        return other.command == command;
    }

    public override int GetHashCode()
    {
        // Which is preferred?

        return command;

        //return this.FooId.GetHashCode();
    }
    public sbyte[] toSbyteArray(byte[] byteArray)
    {
        sbyte[] sbytes = new sbyte[byteArray.Length];
        System.Buffer.BlockCopy(byteArray, 0, sbytes, 0, byteArray.Length);
        return sbytes;
    }
    public byte[] getData()
    {
        byte[] dataTmp = new byte[data.Length + 3];
        byte[] dataCMD = new byte[1];
        dataCMD[0] = (byte)command;
        dataCMD.CopyTo(dataTmp, 0);
        short size = (short)data.Length;
        byte[] bytesSize = BitConverter.GetBytes(size);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(bytesSize);
        bytesSize.CopyTo(dataTmp, 1);
        data.CopyTo(dataTmp, 3);
        return dataTmp;
    }
}
#else
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System;

public class Message
{
    public int cmd = -1;
    public Dictionary<string, object> data = new Dictionary<string, object>();
    private JObject _jo;
    public Message(Message msg)
    {
        cmd = msg.cmd;
        data = msg.data;
    }
    public Message(int cmd)
    {
        this.cmd = cmd;
    }
    /*** Debug ***/
    private string _json;
    public string GetJson()
    {
        if (_json != null)
        {
            return _json;
        }
        return string.Empty;
    }
    /*** End Debug ***/

    public Message(string json)
    {
        try
        {
//#if UNITY_EDITOR
            _json = json; //UnityEngine.Debug.Log(json);
//#endif
            _jo = JObject.Parse(json);
            cmd = _jo.GetValue(Key.CMD).ToObject<int>();
            data = _jo.GetValue(Key.DATA).ToObject<Dictionary<string, object>>();
        }
        catch (Exception e)
        {
             UnityEngine.Debug.LogError(e);
        }
    }
    public void PutInt(string key, int val)
    {
        data.Add(key, val);
    }
    public void PutIntArray(string key, int[] val)
    {
        data.Add(key, val);
    }
    public void PutLong(string key, long val)
    {
        data.Add(key, val);
    }
    public void PutLongArray(string key, long[] val)
    {
        data.Add(key, val);
    }
    public void PutObject(string key, object val)
    {
        data.Add(key, val);
    }
    public void PutString(string key, string val)
    {
        data.Add(key, val);
    }
    public void PutStringArray(string key, string[] val)
    {
        data.Add(key, val);
    }
    public void PutFloat(string key, float val)
    {
        data.Add(key, val);
    }
    public void PutFloatArray(string key, float[] val)
    {
        data.Add(key, val);
    }
    public void PutDouble(string key, double val)
    {
        data.Add(key, val);
    }
    public void PutDoubleArray(string key, double[] val)
    {
        data.Add(key, val);
    }
    public void PutBool(string key, bool val)
    {
        data.Add(key, val);
    }
    public void PutArrayObject(string key, ArrayObject val)
    {
        data.Add(key, val);
    }
#if UNITY_EDITOR
    private Dictionary<string, int> dicKeyUsed = new Dictionary<string, int>();

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
    public T GetObject<T>(string key)
    {
        if (CheckKey(key)) { AddKeyUsed(key); return _jo.GetValue(Key.DATA)[key].ToObject<T>(); }
        else { DebugLog(key); return default(T); }
    }
    public int GetInt(string key)
    {
        if (CheckKey(key)) { AddKeyUsed(key); return Convert.ToInt32(data[key]); }
        else { DebugLog(key); return 0; }
    }
    public int[] GetIntArray(string key)
    {
        if (CheckKey(key)) { AddKeyUsed(key); return JArray.Parse(data[key].ToString()).ToObject<int[]>(); }
        else { DebugLog(key); return null; }
    }
    public long GetLong(string key)
    {
        if (CheckKey(key)) { AddKeyUsed(key); return Convert.ToInt64(data[key]); }
        else { DebugLog(key); return 0; }
    }
    public long[] GetLongArray(string key)
    {
        if (CheckKey(key)) { AddKeyUsed(key); return JArray.Parse(data[key].ToString()).ToObject<long[]>(); }
        else { DebugLog(key); return null; }
    }
    public object GetObject(string key)
    {
        if (CheckKey(key)) { AddKeyUsed(key); return data[key]; }
        else { DebugLog(key); return null; }
    }
    public object[] GetObjectArray(string key)
    {
        if (CheckKey(key)) { AddKeyUsed(key); return JArray.Parse(data[key].ToString()).ToObject<object[]>(); }
        else { DebugLog(key); return null; }
    }
    public string GetString(string key)
    {
        if (CheckKey(key)) { AddKeyUsed(key); return data[key].ToString(); }
        else { DebugLog(key); return string.Empty; }
    }
    public string[] GetStringArray(string key)
    {
        if (CheckKey(key)) { AddKeyUsed(key); return JArray.Parse(data[key].ToString()).ToObject<string[]>(); }
        else { DebugLog(key); return null; }
    }
    public float GetFloat(string key)
    {
        if (CheckKey(key)) { AddKeyUsed(key); return float.Parse(data[key].ToString()); }
        else { DebugLog(key); return 0F; }
    }
    public float[] GetFloatArray(string key)
    {
        if (CheckKey(key)) { AddKeyUsed(key); return JArray.Parse(data[key].ToString()).ToObject<float[]>(); }
        else { DebugLog(key); return null; }
    }
    public double GetDouble(string key)
    {
        if (CheckKey(key)) { AddKeyUsed(key); return Convert.ToDouble(data[key]); }
        else { DebugLog(key); return 0; }
    }
    public double[] GetDoubleArray(string key)
    {
        if (CheckKey(key)) { AddKeyUsed(key); return JArray.Parse(data[key].ToString()).ToObject<double[]>(); }
        else { DebugLog(key); return null; }
    }
    public bool GetBool(string key)
    {
        if (CheckKey(key)) { AddKeyUsed(key); return Convert.ToBoolean(data[key]); }
        else { DebugLog(key); return false; }
    }
    public bool[] GetBoolArray(string key)
    {
        if (CheckKey(key)) { AddKeyUsed(key); return JArray.Parse(data[key].ToString()).ToObject<bool[]>(); }
        else { DebugLog(key); return null; }
    }
    public ArrayObject GetArrayObject(string key)
    {
        if (CheckKey(key)) { AddKeyUsed(key); return JObject.Parse(data[key].ToString()).ToObject<ArrayObject>(); }
        else { DebugLog(key); return null; }
    }
    public JArray GetJArray(string key)
    {
        if (CheckKey(key)) { AddKeyUsed(key); return JArray.Parse(data[key].ToString()); }
        else { DebugLog(key); return null; }
    }
    public JObjectCustom GetJObjectCustom(string key)
    {
        if (CheckKey(key)) { AddKeyUsed(key); return new JObjectCustom(JObject.Parse(data[key].ToString())); }
        else { DebugLog(key); return null; }
    }
    public T GetClass<T>(string key)
    {
        if (CheckKey(key)) { AddKeyUsed(key); return JObject.Parse(data[key].ToString()).ToObject<T>();/*return (T)data[key];*/}
        else { DebugLog(key); return default(T); }
    }
    public T[] GetClassArray<T>(string key)
    {
        if (CheckKey(key)) { AddKeyUsed(key); return JArray.Parse(data[key].ToString()).ToObject<T[]>(); }
        else { DebugLog(key); return null; }
    }
    public List<T> GetClassList<T>(string key)
    {
        if (CheckKey(key)) { AddKeyUsed(key); return JArray.Parse(data[key].ToString()).ToObject<List<T>>(); }
        else { DebugLog(key); return null; }
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
    public JToken GetJToken(string key)
    {
        if (CheckKey(key)) { AddKeyUsed(key); return _jo.GetValue(Key.DATA)[key]; } else { DebugLog(key); return null; }
    }
    private void DebugLog(string key)
    {
        AgentUnity.LogWarning("Not Found Key: " + key + " - Json: " + GetJson());
    }
#else
    public T GetObject<T>(string key) { return _jo.GetValue(Key.DATA)[key].ToObject<T>(); }
    public int GetInt(string key) { return Convert.ToInt32(data[key]); }
    public int[] GetIntArray(string key) { return JArray.Parse(data[key].ToString()).ToObject<int[]>(); }
    public long GetLong(string key) { return Convert.ToInt64(data[key]); }
    public long[] GetLongArray(string key) { return JArray.Parse(data[key].ToString()).ToObject<long[]>(); }
    public object GetObject(string key) { return data[key]; }
    public object[] GetObjectArray(string key) { return JArray.Parse(data[key].ToString()).ToObject<object[]>(); }
    public string GetString(string key) { return data[key].ToString(); }
    public string[] GetStringArray(string key) { return JArray.Parse(data[key].ToString()).ToObject<string[]>(); }
    public float GetFloat(string key) { return float.Parse(data[key].ToString()); }
    public float[] GetFloatArray(string key) { return JArray.Parse(data[key].ToString()).ToObject<float[]>(); }
    public double GetDouble(string key) { return Convert.ToDouble(data[key]); }
    public double[] GetDoubleArray(string key) { return JArray.Parse(data[key].ToString()).ToObject<double[]>(); }
    public bool GetBool(string key) { return Convert.ToBoolean(data[key]); }
    public bool[] GetBoolArray(string key) { return JArray.Parse(data[key].ToString()).ToObject<bool[]>(); }
    public ArrayObject GetArrayObject(string key) { return (ArrayObject)data[key]; }
    public JArray GetJArray(string key) { return JArray.Parse(data[key].ToString()); }
    public JObjectCustom GetJObjectCustom(string key) { return new JObjectCustom(JObject.Parse(data[key].ToString())); }
    public T GetClass<T>(string key) { return JObject.Parse(data[key].ToString()).ToObject<T>(); }
    public T[] GetClassArray<T>(string key) { return JArray.Parse(data[key].ToString()).ToObject<T[]>(); }
    public List<T> GetClassList<T>(string key) { return JArray.Parse(data[key].ToString()).ToObject<List<T>>(); }
    public T[][] GetClassArrayJagged<T>(string key) { return JArray.Parse(data[key].ToString()).ToObject<T[][]>(); }
    public T[,] GetClassArrayMatrix<T>(string key) { return JArray.Parse(data[key].ToString()).ToObject<T[,]>(); }
    public JToken GetJToken(string key) { return _jo.GetValue(Key.DATA)[key]; }
#endif
    public T GetClass<T>()
    {
        return _jo[Key.DATA].ToObject<T>();
    }
    public bool ConstainsKey(string key) { return data.ContainsKey(key); }
}
#endif
