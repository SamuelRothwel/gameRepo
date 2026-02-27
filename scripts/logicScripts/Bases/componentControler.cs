using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using coolbeats.scripts.logicScripts.Bases;
using Godot;

public interface componentController
{
    public Type type {get;}
    unitControler controler {get; set;}
    TypeRegistry subComponents { get; set; }
    Node2D self { get { return (Node2D)this; } }
    public virtual void setup() {}
}