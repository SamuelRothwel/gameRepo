using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using coolbeats.scripts.logicScripts.Bases;
using Godot;

public interface componentController
{
    public string type {get;}
    public string state {get; set;}
    unitControler controler {get; set;}
    TypeRegistry<subComponent> subComponents { get; set; }
    Node2D self { get { return (Node2D)this; } }
}