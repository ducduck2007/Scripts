using System.Threading;
using Entitas;
using Entitas.CodeGeneration.Attributes;

[Network, Unique]
public class ConnectionThreadComponent : IComponent
{
    public Thread value;
}