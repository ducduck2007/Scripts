using System.Threading;
using Entitas;
using Entitas.CodeGeneration.Attributes;

[Network, Unique]
public class PingThreadComponent : IComponent
{
    public Thread value;
}