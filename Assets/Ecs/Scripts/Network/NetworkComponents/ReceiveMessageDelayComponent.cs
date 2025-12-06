using Entitas;
using Entitas.CodeGeneration.Attributes;

[Network, Unique]
public class ReceiveMessageDelayComponent : IComponent
{
    public int value;
}