using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public interface subComponent
{
    public Type type {get;}
    componentController parent { get; set; }
    Node self { get { return (Node)this; } }
    public virtual void setup() {}
}