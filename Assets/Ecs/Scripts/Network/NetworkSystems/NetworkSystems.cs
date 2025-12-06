public sealed class NetworkSystems : Feature
{
    public NetworkSystems(Contexts contexts)
    {
        Add(new ConnectToServerSystem(contexts));  // 👈 PHẢI TRƯỚC
        Add(new SendMessageTcpSystem(contexts));  // 👈 SAU

        Add(new NetworkEventSystems(contexts));
        Add(new CommandSystems(contexts));
        Add(new DispatcherSystem(contexts));
        Add(new RecieveMessageDelaySystem(contexts));
        Add(new ReceiveMessageTcpSystem(contexts));
    }
}