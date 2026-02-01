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
	public Sprite2D[] _components;
	public CircularEnumerator<Sprite2D> gunComponents;
	public double rotation = 0;
	public bool shooting = false;

    public Type type => typeof(Gun);
    public componentController parent { get; set; }

    public override void _Ready()
	{
		_components = GetChildren().Where(x => x is Sprite2D).Select(x => (Sprite2D)x).ToArray();
		gunComponents = new CircularEnumerator<Sprite2D>(ref _components);
		animator = GetChild<AnimationPlayer>(0);
		animator.SpeedScale = guns[gunType].firerate;
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
}
