using System;

[Serializable]
public class ChatWorldMessage
{
    public long fromUserId;
    public string fromDisplayName;
    public int fromLevel;
    public int fromAvatarId;
    public string content;
    public long timestamp;

    public ChatWorldMessage() { }

    public ChatWorldMessage(long fromUserId, string fromDisplayName, int fromLevel, int fromAvatarId, string content, long timestamp)
    {
        this.fromUserId = fromUserId;
        this.fromDisplayName = fromDisplayName;
        this.fromLevel = fromLevel;
        this.fromAvatarId = fromAvatarId;
        this.content = content;
        this.timestamp = timestamp;
    }
}
