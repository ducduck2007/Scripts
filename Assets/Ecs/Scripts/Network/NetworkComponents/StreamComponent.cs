using System.IO;
using Entitas;
using Entitas.CodeGeneration.Attributes;

[Network, Unique]
public class StreamComponent : IComponent
{
    public Stream value;
}