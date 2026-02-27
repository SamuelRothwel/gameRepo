using Godot;
using System;
using coolbeats.scripts.managerScripts;

public partial class barracks : unitControler
{
	// Called when the node enters the scene tree for the first time.
	public barracks()
	{
		type = "barracks";
		radius = 100;
		//type = "attacker";
		detectionRadius = 150;
		//speed = 1;
		maxHP = 50;
		//sendCommand(new command("idle"));
		//QueueRedraw();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
