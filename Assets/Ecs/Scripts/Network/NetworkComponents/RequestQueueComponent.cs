using System;
using System.Collections.Generic;
using Entitas;
using Entitas.CodeGeneration.Attributes;

[Network, Unique]
public class RequestQueueComponent : IComponent
{
    public Queue<Action> value;
    public int maxCount;
    public int current;
}