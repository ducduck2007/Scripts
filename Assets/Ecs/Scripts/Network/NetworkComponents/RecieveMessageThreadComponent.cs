using System.Threading;
using Entitas;
using Entitas.CodeGeneration.Attributes;

[Network, Unique]
public class RecieveMessageThreadComponent : IComponent
{
    public Thread value;
}