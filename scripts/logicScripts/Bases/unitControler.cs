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
    public Queue<command> commandList = new Queue<command>();
    public multiTypeRegistry<componentController> components { get; set; }
    public command activeCommand;
    bool _selected;
    public bool selected { get { return _selected; } set { _selected = value; QueueRedraw(); }}
    public float maxHP;
    float _HP;
    public float HP { get { return _HP; } set { _HP = value; if (_HP <= 0) {die();} else { healthBar.Value = _HP; } }}
    ProgressBar healthBar;
    
    public virtual void Next()
    {
        if (commandList.Any())
        {
            activateCommand(commandList.Dequeue());
        }
        else
        {
            activateCommand(new command("idle"));
        }
    }
    public virtual void queueCommand(command com)
    {
        if (activeCommand.state == "idle")
        {
            activateCommand(com);
        } else
        {
            commandList.Enqueue(com);
        }
    }
    
    public virtual void sendCommand(command com)
    {
        commandList.Clear();
        activateCommand(com);
    }
    public virtual void activateCommand(command com) {}

    public override void _Ready()
    {
        healthBar = (ProgressBar)mAccess.entityManager.getEntity("healthBar");
        healthBar.MaxValue = maxHP;
        healthBar.Position = healthBar.Position + new Vector2(0, -radius*5) ;
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
    public void die()
    {
        QueueFree();
    }
    public override void _Draw()
    {
        if (selected)
        {
            DrawCircle(new Vector2(0,0), radius, new Color(0,0,0));
        }
    }
}