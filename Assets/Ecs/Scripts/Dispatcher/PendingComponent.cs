using System;
using System.Collections.Generic;
using Entitas;
using Entitas.CodeGeneration.Attributes;

[Game, Unique]
public class PendingComponent : IComponent
{
    public Queue<Action> value;
}