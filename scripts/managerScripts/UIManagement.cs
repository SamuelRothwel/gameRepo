using Godot;
using System;
using System.Collections.Generic;

public partial class UIManagement : managerNode
{
	[Export] public PackedScene MainMenu;
	[Export] public PackedScene GameUI;
	Node CurrentUI;
	public Dictionary<string, PackedScene> scenes;
	public override void setup()
	{
		scenes = new Dictionary<string, PackedScene>
		{
			{ "main", MainMenu },
			{ "game", GameUI }
		};
		changeUI("main");
	}
	public void changeUI(string name)
	{
		if (CurrentUI != null)
		{
			CurrentUI.QueueFree();
		}
		CurrentUI = scenes[name].Instantiate();
		AddChild(CurrentUI);
	}
}
