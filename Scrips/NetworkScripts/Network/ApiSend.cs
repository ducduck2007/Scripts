public class ApiSend
{
    private System.Collections.Generic.Dictionary<string, object> _dic = new System.Collections.Generic.Dictionary<string, object>();
    public ApiSend(int cmd)
    {
        Put(Key.CMD, cmd);
    }
    public void Put(string key, object data) { _dic.Add(key, data); }
    public string GetJson() { return Newtonsoft.Json.JsonConvert.SerializeObject(_dic); }
}
