using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using coolbeats.scripts.logicScripts.Bases;
using coolbeats.scripts.staticScriptsAndDataStructures;
using Godot;


public partial class projectile : Node2D, Recyclable
{
    public static Dictionary<string, projectilePhysicsTemplate> physicsTemplates;
    public static Dictionary<string, projectileEffectsTemplate> effectTemplates;
    public static Dictionary<string, (string, string)> templateLookup = new Dictionary<string, (string, string)>  {["default"] = ("def", "def")} ;
    public projectilePhysicsTemplate physics {get => physicsTemplates[templateLookup[type].Item1];}
    public projectileEffectsTemplate effects {get => effectTemplates[templateLookup[type].Item2];}
    string type;
    public bool active {get; set;}
    public unitControler owner;
    public Vector2 targetCoordinates;
    Rid physicsArea;
    public float range;
    public float _distance;
    public float distance {get  { return _distance; } set { _distance = value; if (distance > range) physics.destroy(this); }}
    public double Delta;
    public privatePhysicsArea simulation;
    public void spawn(Vector2 position, float rotation, float projectileRange, string projectileType, Guid[] Targets) 
    {
        active = true;
        Position = position;
        Rotation = rotation-math.PI/2;
        type = projectileType;
        range = projectileRange;
        simulation = new privatePhysicsArea();
        TopLevel = true;
        foreach (Guid target in Targets)
        {
            foreach (Area2D hitbox in mAccess.unitManager.units[target].hitBoxes)
            {
                simulation.addArea(hitbox);
            }
        }
    }
    public override void _PhysicsProcess(double delta)
    {
        Delta = delta;
        physics.travel(this);
    }
    public void clean()
    {
        this.active = false;
        GetParent().RemoveChild(this);   
    }
}