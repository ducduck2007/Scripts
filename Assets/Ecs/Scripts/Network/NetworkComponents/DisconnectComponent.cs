using Entitas;
using Entitas.CodeGeneration.Attributes;

[Network, Unique, Event(EventTarget.Any)]
public class DisconnectComponent : IComponent
{
    public bool value;
}