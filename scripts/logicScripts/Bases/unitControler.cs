using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using coolbeats.scripts.logicScripts.AttachedLogic.SubComponents;
using coolbeats.scripts.logicScripts.Bases;
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
    public multiTypeRegistry components { get; set; }
    public command activeCommand;
    bool _selected;
    public bool selected { get { return _selected; } set { _selected = value; QueueRedraw(); }}
    public List<Area2D> hitBoxes;
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
    public void initiateHealthBar()
    {
        Node barAnchor = mAccess.entityManager.spawnEntity("healthBar");
        healthBar = (ProgressBar)barAnchor.GetChild(0);
        healthBar.MaxValue = maxHP;
        RemoteTransform2D transform = (RemoteTransform2D)mAccess.entityManager.getEntity("positionTransform");
        AddChild(transform);
        healthBar.GlobalPosition = Position + new Vector2(-radius, -radius*2) ;
        healthBar.Size = new Vector2(radius*2, 10);
        transform.RemotePath = barAnchor.GetPath();
        transform.ForceUpdateCache();
    }
    public override void _Ready()
    {
        components = new multiTypeRegistry();
        IEnumerable<componentController> componentList = GetChildren().OfType<componentController>();
        hitBoxes = new List<Area2D>();
        foreach (componentController component in componentList)
        {
            component.subComponents = new TypeRegistry();
            component.controler = this;
            components.Register(component, component.type);
            component.setup();
            IEnumerable<subComponent> subComponents = component.self.GetChildren().OfType<subComponent>(); 
            foreach (subComponent sub in subComponents)
            {
                component.subComponents.Register(sub, sub.type);
                sub.parent = component;
                sub.setup();
            }
        }
        initiateHealthBar();
        HP = maxHP;
    }
    public void die()
    {
        QueueFree();
    }
    public override void _Draw()
    {
        if (selected)
        {
            DrawArc(new Vector2(0,0), radius, 0, MathF.Tau, 100, new Color(0,0,0), 2);
        }
    }
}