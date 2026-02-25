using System;
using System.Collections.Generic;

public static class ChatWorldDataBase
{
    public const int MAX_CACHE = 200;

    public static readonly List<ChatWorldMessage> Messages = new List<ChatWorldMessage>(256);

    public static event Action<ChatWorldMessage> OnMessageAdded;

    public static void Clear()
    {
        Messages.Clear();
    }

    public static void Add(ChatWorldMessage m)
    {
        if (m == null) return;

        Messages.Add(m);

        // trim cache
        if (Messages.Count > MAX_CACHE)
        {
            int remove = Messages.Count - MAX_CACHE;
            if (remove > 0) Messages.RemoveRange(0, remove);
        }

        OnMessageAdded?.Invoke(m);
    }
}
