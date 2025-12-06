using System.Net.Sockets;
using Entitas;
using Entitas.CodeGeneration.Attributes;

[Network, Unique]
public class TcpClientComponent : IComponent
{
    public TcpClient value;
}