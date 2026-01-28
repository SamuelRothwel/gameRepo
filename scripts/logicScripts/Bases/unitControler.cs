using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using coolbeats.scripts.logicScripts.AttachedLogic.SubComponents;
using coolbeats.scripts.managerScripts;
using Godot;
using Microsoft.EntityFrameworkCore.Diagnostics;

public partial class unitControler : Node2D
{
    public Guid ID = new Guid();
    public float radius;
    public string state { get; set; }
    public int priority;
    public float detectionRadius;
    public string type;
    public Dictionary<string, List<componentController>> components { get; set; }
    public virtual void sendCommand(command value) {}
    public override void _Ready()
    {
        components = new Dictionary<string, List<componentController>>();
        IEnumerable<componentController> componentList = GetChildren().OfType<componentController>();
        foreach (componentController component in componentList)
        {
            component.subComponents = new TypeRegistry<subComponent>();
            component.controler = this;
            if (!components.ContainsKey(component.type))
            {
                components[component.type] = new List<componentController>();
            }
            components[component.type].Add(component);
            IEnumerable<subComponent> subComponents = component.self.GetChildren().OfType<subComponent>(); 
            foreach (subComponent sub in subComponents)
            {
                component.subComponents.Register(sub, sub.type);
            }
        }
    }
}