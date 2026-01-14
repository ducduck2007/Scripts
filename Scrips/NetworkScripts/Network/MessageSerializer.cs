using System.Text;
using Newtonsoft.Json;

public static class MessageSerializer
{
    public static byte[] Serialize(Message msg)
    {
        string json = JsonConvert.SerializeObject(msg);
        return Encoding.UTF8.GetBytes(json);
    }

    public static Message Deserialize(byte[] data)
    {
        string json = Encoding.UTF8.GetString(data);
        return JsonConvert.DeserializeObject<Message>(json);
    }
}
