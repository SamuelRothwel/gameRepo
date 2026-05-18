using coolbeats.scripts.logicScripts.Bases;
using coolbeats.scripts.staticScriptsAndDataStructures;
using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

public partial class Gun : Node2D, subComponent
{
	public static Dictionary<string, gunTemplate> guns;
	public double heat = 0;
	public double Delta;
	public string gunType = "Minigun";
	public AnimationPlayer animator;
	public DynamicAnimator dynamicAnimator;
	public double rotation = 0;
	public bool shooting = false;
    public Type type => typeof(Gun);
    public componentController parent { get; set; }
    public Guid target;	

    public override void _Ready()
	{
		animator = GetChild<AnimationPlayer>(0);
		animator.SpeedScale = guns[gunType].firerate;
	}
	public void setup()
	{
		GunComponentRing componentRing = new GunComponentRing
		{
			Name = "GunComponentRing",
			gun = this,
			parent = parent
		};
		AddChild(componentRing);
		componentRing.setup();
		parent.subComponents.Register(componentRing, componentRing.type);
	}
    public void fire(Guid t)
	{
		target = t;
		shooting = true;
	}
	public void swap(string newType)
	{
		gunType = newType;
		animator.SpeedScale = guns[gunType].firerate;
	}
	public override void _Process(double delta)
	{
		Delta = delta;
		guns[gunType].process(this);
	}
	public void spawnBullet()
	{
		guns[gunType].spawnBullet(this);
	}
	public void handleAnimationEvent(string eventName)
	{
		guns[gunType].handleAnimationEvent(this, eventName);
	}
}

public partial class GunComponentRing : Node, subComponent
{
	public Gun gun;
	public Sprite2D[] components;
	public CircularEnumerator<Sprite2D> circularComponents;
	public Type type => typeof(GunComponentRing);
	public componentController parent { get; set; }

	public void setup()
	{
		components = gun.GetChildren().OfType<Sprite2D>().ToArray();
		circularComponents = new CircularEnumerator<Sprite2D>(ref components);
	}
}
