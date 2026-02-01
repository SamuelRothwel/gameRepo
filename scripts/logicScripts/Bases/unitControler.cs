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
    public Guid ID = Guid.NewGuid();
    public float radius;
    public int priority;
    public float detectionRadius;
    public string type;
    public multiTypeRegistry<componentController> components { get; set; }
    public virtual void sendCommand(command value) {}
    public virtual void queueCommand(command value) {}
    public override void _Ready()
    {
        components = new multiTypeRegistry<componentController>();
        IEnumerable<componentController> componentList = GetChildren().OfType<componentController>();
        foreach (componentController component in componentList)
        {
            component.subComponents = new TypeRegistry<subComponent>();
            component.controler = this;
            components.Register(component, component.type);
            IEnumerable<subComponent> subComponents = component.self.GetChildren().OfType<subComponent>(); 
            foreach (subComponent sub in subComponents)
            {
                component.subComponents.Register(sub, sub.type);
            }
        }
    }
}